using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SnailChess.AI.Evaluation;
using SnailChess.Core;

namespace SnailChess.Tuner.Data
{
    internal static class DataLoader
    {
        internal const int RANDOM_SEED = 45408093;
        internal static readonly ushort THREAD_COUNT;


        internal const float OUTCOME_WIN     =  1.0f;
        internal const float OUTCOME_LOSS    =    0f;
        internal const float OUTCOME_DRAW    =  0.5f;
        internal const float OUTCOME_INVALID = -1.0f;

        internal const string FOLDER_NAME_TUNER_DATA = "tuner_data";
        internal const string FILE_NAME_DATA_SET = "dataset.epd";
        internal const string FILE_NAME_BESTK = "best_k.txt";
        internal const string FILE_NAME_EVAL_PARAMS = "eval_params.json";

        static DataLoader()
        {
            THREAD_COUNT = Convert.ToUInt16(Math.Max(1, Environment.ProcessorCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetProgramPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetFilePath(string _filename, string _directory = "")
        {
            if (!string.IsNullOrEmpty(_directory))
            {
                string directory_path = Path.Combine(GetProgramPath(), _directory);
                if (!Directory.Exists(directory_path)) Directory.CreateDirectory(directory_path);

                return Path.Combine(GetProgramPath(), _directory , $"{_filename}");
            }else 
            {
                return Path.Combine(GetProgramPath(), $"{_filename}");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double LoadBestK()
        {
            string best_k_file_path = GetFilePath(FILE_NAME_BESTK);
            if (File.Exists(best_k_file_path))
            {
                string best_k_str = File.ReadAllText(best_k_file_path);
                if(double.TryParse(best_k_str, out double best_k))
                    return best_k;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SaveBestK(double _best_k)
        {
            string best_k_file_path = GetFilePath(FILE_NAME_BESTK);
            if (File.Exists(best_k_file_path)) File.Delete(best_k_file_path);

            File.WriteAllText(best_k_file_path, _best_k.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool LoadParams(out EvaluationParams _params)
        {
            string params_path = GetFilePath(FILE_NAME_EVAL_PARAMS);
            if (File.Exists(params_path))
            {
                _params = JsonConvert.DeserializeObject<EvaluationParams>(File.ReadAllText(params_path), new JsonSerializerSettings() { Formatting = Formatting.Indented });
                return true;
            }
            
            _params = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SaveParams(EvaluationParams _params, string _filename, string _directory = "")
        {
            string params_path = GetFilePath(_filename, _directory);
            if (File.Exists(params_path)) File.Delete(params_path);

            string params_json = JsonConvert.SerializeObject(_params, Formatting.Indented);
            File.WriteAllText(params_path, params_json);
        }

        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TunerPosition[] LoadEPD(Dictionary<string,int> _paramKeyIndexesDb)
        {
            string epd_path = GetFilePath(FILE_NAME_DATA_SET);
            List<TunerPosition> positions = new List<TunerPosition>();

            if (!File.Exists(epd_path)) return positions.ToArray();
            string[] lines = File.ReadAllText(epd_path).Split('\n' , StringSplitOptions.RemoveEmptyEntries);

            ThreadPositionRange[] threads_limits = new ThreadPositionRange[THREAD_COUNT]; 

            int thread_index;
            int step = lines.Length / THREAD_COUNT;
            int start_index = 0, end_index = step;

            for (thread_index = 0; thread_index < THREAD_COUNT; thread_index++, start_index += step, end_index = start_index + step)
            {
                if (thread_index == THREAD_COUNT - 1 && end_index < lines.Length) end_index = lines.Length;
                threads_limits[thread_index] = new ThreadPositionRange(start_index, end_index);
            }

            TunerPosition[] WorkerLoadEPD(int start, int end)
            {
                Random worker_random = new Random(RANDOM_SEED);
                ChessBoard board = new ChessBoard();
                List<TunerPosition> worker_positions = new List<TunerPosition>();
                string fen;
                string[] line_content;
                float outcome;
                
                for (int i = start; i < end; i++)
                {
                    line_content = lines[i].Split("\"");
                    fen = line_content[0];

                    if (!Notation.IsValidFEN(fen)) continue;
                    board.LoadPosition(Notation.ParseFEN(fen));

                    TunerEvalImpl.ExtractEvalFeatures(in board, _paramKeyIndexesDb, out TunerParamData[] eval_param_data, out
                            byte game_phase, out byte sideToMove);
                    
                    outcome = ParseGameOutcome(line_content[1], in sideToMove);
                    if (outcome == OUTCOME_INVALID) continue;
                    
                    worker_positions.Add(new TunerPosition(fen, outcome, game_phase, sideToMove, eval_param_data));
                }
                
                return worker_positions.ToArray();
            }

            Task<TunerPosition[]>[] workers = new Task<TunerPosition[]>[THREAD_COUNT];
            for (thread_index = 0; thread_index < THREAD_COUNT; thread_index++)
            {
                ThreadPositionRange worker_limits = threads_limits[thread_index];
                workers[thread_index] = Task.Run(() => WorkerLoadEPD(worker_limits.start, worker_limits.end));
            }

            Task.WaitAll(workers);

            for (thread_index = 0; thread_index < THREAD_COUNT; thread_index++)
            {
                positions.AddRange(workers[thread_index].Result);
                workers[thread_index].Dispose();
            }

            // free memory before returning processed data
            GC.Collect();

            return positions.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ParseGameOutcome(string _outcome, in byte _sideToMove)
        {
            if (_outcome.Equals("1-0"))
            {
                // White wins
                if (_sideToMove != Piece.White)
                {
                    return OUTCOME_LOSS;
                }
                return OUTCOME_WIN; 
            }
            else if (_outcome.Equals("0-1"))
            {
                // Black wins
                if (_sideToMove != Piece.Black)
                {
                    return OUTCOME_LOSS;
                }
                return OUTCOME_WIN;
            }
            else if (_outcome.Equals("1/2-1/2"))
                return OUTCOME_DRAW;

            return OUTCOME_INVALID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsValidFileName(string _fileName)
        {
            return Path.IsPathFullyQualified(Path.Combine(GetProgramPath(), _fileName));
        }
    }
}