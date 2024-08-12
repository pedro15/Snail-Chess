using System.Text;
using SnailChess.Client.Internal;
using SnailChess.Client.Internal.Common;

namespace SnailChess.Client
{
	public sealed class ClientManager : IClient
	{
		private CommonClient commonClient;
		public void Init(IClientStream _clientStream)
		{
			commonClient = new CommonClient();
			commonClient.Init(_clientStream);
			
			string welcome_header = $"{ClientConstants.ENGINE_NAME} v{ClientConstants.VERSION}";
			StringBuilder builder = new StringBuilder();
			builder.Append(welcome_header);
			builder.AppendLine();
			_clientStream.Write(builder.ToString());
		}

		public void Run()
		{
			if (commonClient != null) commonClient.Run();
		}

		public void Stop()
		{
			if (commonClient != null) commonClient.Stop();
        }
	}
}