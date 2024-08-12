using System;
using System.Globalization;
using Newtonsoft.Json;
using SnailChess.AI.Evaluation;
using SnailChess.AI.Personalities;
using SnailChess.Tuner.Core;
using SnailChess.Tuner.Data;

namespace SnailChess.Tuner
{
    public static class Program 
    {
        private static bool running = true;
        private static TunerController controller;  
        private static EvaluationParams evalParams;

        private static Tuple<string,Action>[] options_menu = new Tuple<string, Action>[]
        {
            new Tuple<string, Action>("Display Option Menu", WriteOptions),
            new Tuple<string, Action>("Enable/Disable detailed logs" , EnableDisableLogs),
            new Tuple<string, Action>("Calculate best K", CalculateBestK),
            new Tuple<string, Action>("Start tuning" , StartTuning),
            new Tuple<string, Action>("Run tests" , RunTests),
            new Tuple<string, Action>("Change datset limit", ChangeDatasetLimit),
            new Tuple<string, Action>("Print eval params", PrintEvalParams),
            new Tuple<string, Action>("Quit", Quit)
        };

        public static void Main(string[] _args)
        {
            WriteHeader();
            
            evalParams = BotPersonality.MAX.evaluationParams;
            controller = new TunerController();

            if (DataLoader.LoadParams(out EvaluationParams loaded_params))
            {
                evalParams = loaded_params;
                Console.WriteLine($"[Info] Using evaluation params from file = {DataLoader.FILE_NAME_EVAL_PARAMS}");
            }
            
            double best_k_loaded = DataLoader.LoadBestK();
            if (best_k_loaded > 0)
            {
                Console.WriteLine($"[Info] Best K found from file, using value = {best_k_loaded}");
                controller.best_k = best_k_loaded;
            }
            
            Console.WriteLine("[Info] Loading Positions...");
            int positions_count = controller.LoadPositions(in evalParams);

            if (positions_count < 1)
            {
                Console.WriteLine($"[Error] dataset EPD not found '{DataLoader.FILE_NAME_DATA_SET}'");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"[Info] Positions loaded, total: {positions_count} | total params: {controller.tune_params.Length}");
            Console.Write("\n");
            WriteOptions();

            string cmd;
            while(running)
            {
                cmd = Console.ReadLine().Trim();
                if (int.TryParse(cmd , out int result))
                {
                    int option_index = result - 1;
                    if (option_index >= 0 && option_index < options_menu.Length)
                    {
                        options_menu[option_index].Item2.Invoke();
                    }else 
                    {
                        Console.WriteLine("[Info] Unknown option");
                    }
                }
            }
        }

        static string CurrentDateStringFileFormat() => DateTime.Now.ToString("dd-MM-yyy_HH-mm-ss");
        
        private static void Quit()
        {   
            running = false;
        }

        private static void StartTuning() 
        {
            Console.WriteLine("[Info] Tuning started!");
            evalParams = controller.TuneEvaluation((EvaluationParams eval_params_improved) => 
            {
                SaveParams(eval_params_improved, $"session_params_{CurrentDateStringFileFormat()}.json" , DataLoader.FOLDER_NAME_TUNER_DATA);
            });
            Console.WriteLine("[Info] Tuning finished!");
        }

        private static void CalculateBestK()
        {
            Console.WriteLine("[Info] Computing optimal K...");
            double best_k = controller.ProcessOptimalK();
            Console.WriteLine($"[Info] Optimal K is: {best_k}");
            DataLoader.SaveBestK(best_k);
            Console.WriteLine($"[Info] Optimal K saved to file: {DataLoader.FILE_NAME_BESTK}");
        }

        

        private static void SaveParams(EvaluationParams _evalParams, string _fileName, string _directory = "")
        {
            if (!string.IsNullOrEmpty(_fileName) && DataLoader.IsValidFileName(_fileName))
            {
                DataLoader.SaveParams(_evalParams, _fileName, _directory);
                Console.WriteLine($"[Info] Saved params correctly to {_fileName}");
            }else 
            {
                Console.WriteLine("[Error] You must specify a valid file name");
            }
        }

        private static void WriteHeader()
        {
            Console.WriteLine("############ ChessClub-Tuner ###########");
            Console.WriteLine("Utility to tune AI evaluation parameters");
            Console.WriteLine("----------------------------------------\n");
        }

        private static void PrintEvalParams()
        {
            Console.WriteLine(JsonConvert.SerializeObject(evalParams, Formatting.Indented));
        }

        private static void WriteOptions()
        {
            for (int i = 0; i < options_menu.Length; i++)
            {
                Console.WriteLine($"{i+1}. {options_menu[i].Item1}");
            }
            Console.Write("\n");
        }

        private static void RunTests()
        {
            Console.WriteLine("[Info] Running tests...");
            bool tests_results = controller.RunTests();
            if (tests_results)
            {
                Console.WriteLine("[Info] Tests completed successfully!");
            }else 
            {
                Console.WriteLine("[Warning] Tests failed");
            }
        }

        private static void EnableDisableLogs()
        {
            Console.WriteLine($"[Info] Enable/Disable Logs (t/f) = ({controller.detailed_logs})");
            char ch = (char)Console.Read();
            if (ch == 't')
            {
                controller.detailed_logs = true;
                Console.WriteLine("[Info] Logs enabled");
            }else if (ch =='f')
            {
                controller.detailed_logs = false;
                Console.WriteLine("[Info] Logs disabled");
            }else 
            {
                Console.WriteLine("[Info] No changes made");
            }
        }

        private static void ChangeDatasetLimit()
        {
            Console.WriteLine($"[Info] Change DatasetLimit ({controller.DATASET_LIMIT})");
            Console.Write("[Info] Enter new dataset limit: ");
            if (int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int val) && val > 0)
            {
                controller.DATASET_LIMIT = val;
                Console.WriteLine($"[Info] New Dataset limit is: {val}");
            }else 
            {
                Console.WriteLine("[Info] Invalid value. No changes made");
            }
        }
    }
}