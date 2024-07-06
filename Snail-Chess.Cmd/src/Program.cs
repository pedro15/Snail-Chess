using System.Reflection;
using System;
using System.IO;
using SnailChess.Client;


namespace SnailChess.Cmd
{
    public static class Program
    {
        static void Main(string[] _args)
        {
            const int BUFFER_SIZE = 1024;
            Stream inStream = Console.OpenStandardInput(BUFFER_SIZE);
            Console.SetIn(new StreamReader(inStream));

            JITUtility.InitializeJIT(Assembly.GetAssembly(typeof(Core.ChessBoard)));
            JITUtility.InitializeJIT(Assembly.GetAssembly(typeof(AI.Search.SearchEngine)));

            ClientManager clientManager = new ClientManager();
            clientManager.Init(new CmdStream());
            clientManager.Run();
        }
    }
}