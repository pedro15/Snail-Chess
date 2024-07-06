using SnailChess.Client;
using System;

namespace SnailChess.Cmd
{
    internal sealed class CmdStream : IClientStream
    {
        public string GetInput()
        {
            string line = Console.ReadLine();
            
            if (line.Equals("cls"))
            {
                Console.Clear();
            }else 
            {
                return line;
            }

            return string.Empty;
        }

        public void Write(string _message)
        {
            Console.Write(_message);
        }
    }
}