using System;
using System.IO;
using System.Linq;
using EventBasedTCP;

namespace ConsoleApp1
{
    public class Program
    {
        static Server server;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Server...");

            server = new Server("127.0.0.1", 13001);
            server.ClientConnected += Server_ClientConnected;
            server.MessageReceived += Server_MessageReceived;
            server.ClientDisconnected += Server_ClientDisconnected;

            Console.WriteLine("Server started.");
            Console.WriteLine("Listing on IP address: " + server.ListenAddress);
            Console.WriteLine("On port: " + server.Port);

            while (!server.IsDisposed)
            {
                var input = Console.ReadLine();

                switch (input)
                {
                    case "listclients":
                        Console.WriteLine($"\n{0} Clients Connected\n-----------------------", server.ConnectedClients.Count);
                        foreach (var client in server.ConnectedClients)
                        {
                            Console.WriteLine(client.ConnectAddress);
                        }
                        Console.WriteLine("-----------------------\n");
                        break;
                    case "stop":
                        Console.WriteLine("Disposing Server...");
                        server.Dispose();
                        Console.WriteLine("Server closed. Press any key to exit.");
                        Console.Read();
                        break;
                    default:
                        Console.WriteLine("Invalid Command: " + input);
                        break;
                }
            }
        }

        private static void Server_ClientDisconnected(object sender, ClientToggleEventArgs e)
        {
            Console.WriteLine("Client Disconnected: " + e.ConnectedClient.ConnectAddress);
        }

        private static void Server_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Received Message: " + (sender as Client).ConnectAddress + " : " + e.Message);
            var toRespond = Reverse(e.Message);

            Console.WriteLine("Returning Message: " + toRespond);
            (sender as Client).SendMessage(toRespond);
        }

        public static string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private static void Server_ClientConnected(object sender, ClientToggleEventArgs e)
        {
            Console.WriteLine("Client Connected: " + e.ConnectedClient.ConnectAddress);
        }
    }
}