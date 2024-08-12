using System.Text;
using System.Collections.Generic;

namespace SnailChess.Client.Internal.Common.Commands
{
    [Command("help" , "displays list of available commands")]
	internal sealed class HelpCommand : ICommand
	{
        private readonly CommonClient Client;
        public HelpCommand(CommonClient _client) => Client = _client;

        public void Init(){ }

		public void Execute(string[] _args)
		{
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();

            KeyValuePair<string,string>[] commandsInfo = Client.GetCommandsInfo();

            for (int i = 0; i < commandsInfo.Length; i++)
            {
                KeyValuePair<string,string> current_info = commandsInfo[i];
                if (!string.IsNullOrEmpty(current_info.Value))
                {
                    builder.AppendFormat("{0,-10}{1,10}\n" , current_info.Key, current_info.Value);
                }
            }

            builder.AppendLine();
            Client.stream.Write(builder.ToString());
		}
	}
}