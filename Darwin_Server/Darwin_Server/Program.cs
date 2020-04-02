using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    class Program
    {

        private static Thread threadConsole;

        static void Main(string[] args)
        {
            threadConsole = new Thread(new ThreadStart(ConsoleThread));
            threadConsole.Start();
            
            NetworkConfig.InitNetwork();
            Console.WriteLine("Network Was Initialized.");

            ClientManager.OnClientCountChange += APIController.UpdateServer;
            APIController.UpdateServer();
            
        }

        private static void ConsoleThread()
        {
            while(true)
            {
                string command = Console.ReadLine();

                if(string.IsNullOrEmpty(command) == false)
                    ProcessLine(command);
            }
        }

        private static void ProcessLine(string input)
        {
            string s = input.ToLower();

            switch(s)
            {
                case ("clear"):
                    Console.Clear();
                    break;
            }
        }

    }
}
