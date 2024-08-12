namespace SnailChess.Client.Internal.UCI.Commands
{
    [Command("setoption")]
	internal sealed class SetOptionCommand : ICommand
	{
        private readonly UCIClient Client;
        internal SetOptionCommand(UCIClient _client) 
        {
            Client = _client;
        }

		public void Execute(string[] _args)
		{
            string id = Client.GetParamValue<string>("name" , _args, string.Empty);
            if (!string.IsNullOrEmpty(id))
            {
                if (id.ToLower().Equals("level") && Client.GetParamValue<int>("value", _args, -1) is int target_level)
                {
                    Client.SetLevelIndex(target_level);
                }
            }
        }

		public void Init()
		{

		}
	}
}