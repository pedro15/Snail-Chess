using System.Reflection;
using System.Collections.Generic;

namespace SnailChess.Client.Internal
{
    internal abstract class ClientBase : IClient
    {
        public IClientStream stream {get; private set;} = null;
        private Dictionary<string,ICommand> commands = new Dictionary<string, ICommand>();
        private List<KeyValuePair<string,string>> commandsInfo = new List<KeyValuePair<string, string>>();
        private bool isRunning = false;
        protected virtual void OnInit(){ }
        protected abstract ICommand[] GetCommands();
        public KeyValuePair<string,string>[] GetCommandsInfo() => commandsInfo.ToArray();
        
        public void Init(IClientStream _clientStream)
        {
            stream = _clientStream;
            commandsInfo.Clear();
            commands.Clear();

            ICommand[] loaded_commands = GetCommands();
            for (int i = 0; i < loaded_commands.Length; i++)
            {
                ICommand current_command = loaded_commands[i];
                if (current_command != null)
                {
                    CommandAttribute attr = current_command.GetType().GetCustomAttribute<CommandAttribute>();
                    if (attr != null)
                    {
                        if (!commands.ContainsKey(attr.name))
                        {
                            current_command.Init();

                            commands.Add(attr.name, current_command);
                            commandsInfo.Add(new KeyValuePair<string, string>(attr.name, attr.description));
                        }
                    }
                }
            }
            
            OnInit();
        }

        public K GetParamValue<K>(string _key, string[] _args , K _defaultvalue = default(K))
        {
            for (int i = 0; i < _args.Length; i++)
            {
                if (_args[i] == _key)
                {
                    try
                    {
                        if ((i + 1) is int next_index && next_index < _args.Length)
                        {
                            return (K)System.Convert.ChangeType(_args[next_index], typeof(K));
                        }
                    }catch
                    {
                        break;
                    }
                }
            }
            
            return _defaultvalue;
        }

        public void ExecuteCommand(string[] _args , out ICommand _command)
        {
            if (_args == null || _args.Length == 0) 
            {
                _command = null;
                return;
            }

            if (commands.TryGetValue(_args[0], out _command))
            {
                _command.Execute(_args);   
            }else 
            {
                stream.Write($"Command not found: {_args[0]}\n");
            }
        }
        
        public void Run()
        {
            if (isRunning) return;
            isRunning = true;
            string cmd;
            string[] args;
            ICommand command = null;
            while(isRunning)
            {
                cmd = stream.GetInput();
                if (string.IsNullOrEmpty(cmd)) continue;
                args = cmd.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                ExecuteCommand(args, out command);
            }
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}