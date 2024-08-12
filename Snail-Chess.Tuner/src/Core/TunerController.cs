using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using SnailChess.AI.Evaluation;
using SnailChess.Tuner.Data;
using SnailChess.Core;

namespace SnailChess.Tuner.Core
{
    public sealed class TunerController 
    {
        private const byte   MAX_RETRIES = 3;
        private const int    K_PRECISION = 10;
        internal int DATASET_LIMIT = 45000;
        private TunerPosition[] positions;
        internal TunerParam[] tune_params;
        private Task<double>[] threadEvalPool;
        private ThreadPositionRange[] threadRanges;
        private int threads_dataset_count = -1;

        internal double best_k = 2.0;
        internal bool detailed_logs = false;

        public TunerController()
        {
            threadEvalPool = new Task<double>[DataLoader.THREAD_COUNT];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitThreads(bool _limitedData = false)
        {
            threadRanges = new ThreadPositionRange[DataLoader.THREAD_COUNT];
            threads_dataset_count = _limitedData ? Math.Min(positions.Length, DATASET_LIMIT) : positions.Length;

            int step = threads_dataset_count / DataLoader.THREAD_COUNT, start = 0, end = start + step;
            for (ushort thread_index = 0; thread_index < DataLoader.THREAD_COUNT; thread_index++)
            {
                if (thread_index == DataLoader.THREAD_COUNT -1)
                    end = threads_dataset_count; 

                threadRanges[thread_index] = new ThreadPositionRange(start, end);
                start += step;
                end = start + step;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Sigmoid(double _x)
        {
            return 1.0 / (1.0 + Math.Pow(10, -_x / 400));
        }

        internal int LoadPositions(in EvaluationParams _params)
        {
            Dictionary<string,int> indexes_db;
            tune_params = TunerEvalImpl.ExtractParamsFromEval(in _params, out indexes_db);
            positions = DataLoader.LoadEPD(indexes_db);
            DATASET_LIMIT = positions.Length;
            return positions.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double ProcessOptimalK()
        {
            best_k = CalculateOptimalK();
            return best_k;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateOptimalK()
        {
            InitThreads(_limitedData: false);
            double start = 0.0, end = 10, step = 1.0;
            double curr, error;
            double best = MeanSquaredError(start);

            Console.WriteLine($"[Tuner][Optimal_K] Initial K value: {best}");

            for (int i = 0; i < K_PRECISION; i++)
            {
                curr = start - step;
                while(curr < end)
                {
                    curr = curr + step;
                    error = MeanSquaredError(curr);
                    if (error <= best)
                    {
                        best = error;
                        start = curr;
                    }
                }

                end   = start + step;
                start = start - step;
                step  = step / 10.0;

                Console.WriteLine($"[Tuner][Optimal_K][{i+1}/{K_PRECISION}] K value: {start}");
            }

            return start;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double WorkerComputeError(int _start, int _end, double _k)
        {
            float outcome;
            int score;
            double sigmoid;
            double local_error = 0;
            TunerPosition tune_position;
            for (int position_index = _start; position_index < _end; position_index++)
            {
                tune_position = positions[position_index];
                outcome = tune_position.outcome;
                score = TunerEvaluator.Evaluate(in tune_params, in tune_position.params_data, tune_position.gamePhase, tune_position.sideToMove);
                sigmoid = Sigmoid(_k * score);
                local_error += Math.Pow(outcome - sigmoid, 2);
            }
            return local_error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double MeanSquaredError (double _k )
        {
            byte task_index;

            for (task_index = 0; task_index < threadRanges.Length; task_index++)
            {
                ThreadPositionRange current_range = threadRanges[task_index];
                threadEvalPool[task_index] = Task.Run(() => WorkerComputeError(current_range.start, current_range.end, _k));
            }

            Task.WaitAll(threadEvalPool);

            double error = 0.0;
            for (task_index = 0; task_index < threadEvalPool.Length; task_index++)
                error += threadEvalPool[task_index].Result;

            return error / threads_dataset_count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EvaluationParams TuneEvaluation(Action<EvaluationParams> _onParamsImproved)
        {
            const string TUNER_PARAM_FORMAT = "[Tuner][Info][ {0} ] Tuning param [{1}] = {2,-26} error: {3}";
            const int ADJUST_VALUE = 1;

            InitThreads(_limitedData: true);
            int epoch = 0, retries, og_value, param_index;
            bool improving = true;
            double best_error = MeanSquaredError(best_k);
            double new_error;

            while(improving)
            {
                epoch++;
                improving = false;
                Console.WriteLine($"[Tuner][Info] Epoch= {epoch}");

                for (param_index = 0; param_index < tune_params.Length; param_index++)
                {
                    if (!tune_params[param_index].IsValid) continue;
                    
                    retries = 0;
                    og_value = tune_params[param_index].value;
                    
                    Console.WriteLine(TUNER_PARAM_FORMAT, epoch, $"{param_index+1}/{tune_params.Length}", tune_params[param_index].name, best_error);

                    // adjust current parameter
                    tune_params[param_index].value += ADJUST_VALUE;

                    // recalculate error
                    new_error = MeanSquaredError(best_k);

                    check_error:
                    if (detailed_logs)
                        Console.WriteLine($"[Tuner][Info] New error: {new_error} | new value: {tune_params[param_index].value}");
                    // parameter adjustments reduced square error
                    if (new_error < best_error)
                    {
                        improving = true;
                        best_error = new_error;
                        if (detailed_logs)
                            Console.WriteLine("[Tuner][Success] Improved!");
                    }
                    // parameter adjustments didn't reduce square error
                    else if (retries < MAX_RETRIES)
                    {
                        retries++;
                        if (detailed_logs)
                            Console.WriteLine($"[Tuner][Warning] Error didn't improved, adjusting param... [{retries}/{MAX_RETRIES}]");
                        tune_params[param_index].value -= ADJUST_VALUE;
                        new_error = MeanSquaredError(best_k);
                        goto check_error;
                    }else 
                    {
                        if (detailed_logs)
                            Console.WriteLine("[Tuner][Warning] Error didn't improved after all retries, adjusting param to its default value");
                        tune_params[param_index].value = og_value;
                    }
                }
                
                if (_onParamsImproved != null) 
                    _onParamsImproved.Invoke(TunerEvalImpl.TransformParamsToEval(in tune_params)); 
            }

            return TunerEvalImpl.TransformParamsToEval(in tune_params);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool RunTests()
        {
            EvaluationController evaluation_controller = new EvaluationController(_use_cache: false);
            evaluation_controller.LoadParams(TunerEvalImpl.TransformParamsToEval(in tune_params));
            ChessBoard board = new ChessBoard();

            int eval_tuner, eval_engine;
            for (int i = 0; i < positions.Length; i++)
            {
                if (detailed_logs)
                    Console.Write($"[Tuner][Test] Testing position ... [{i+1}/{positions.Length}]");
                eval_tuner = TunerEvaluator.Evaluate(in tune_params, positions[i].params_data,  positions[i].gamePhase, positions[i].sideToMove);
                
                board.LoadPosition(Notation.ParseFEN(positions[i].fen));
                eval_engine = evaluation_controller.Evaluate(in board);

                if (eval_tuner != eval_engine)
                {
                    Console.Write("\n");
                    Console.WriteLine($"[Tuner][Test] FAIL: Evaluation does not match in position: {positions[i].fen}");
                    Console.WriteLine($"[Tuner][Info] Tuner Eval: {eval_tuner} != Engine Eval: {eval_engine}");
                    return false;
                }else if (detailed_logs)
                {
                    Console.Write(" ... OK\n");
                }
            }
            
            return true;
        }

    }
}