using System;
using EventBasedTCP;

namespace ConsoleApp1
{
    public class Program
    {
        static Server server;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Server...");

            server = new Server(Server.GetLocalIPAddress(), 13001);
            server.ClientConnected += Server_ClientConnected;
            server.MessageReceived += Server_MessageReceived;
            server.ClientDisconnected += Server_ClientDisconnected;
            var rickroll = new ResponseEvent()
            {
                Content = "never gunna give you up",
                Mode = ContentMode.Contains,
                Event = Rickroll,
            };
            server.Responses.Add(rickroll);

            Console.WriteLine("Server started.");
            Console.WriteLine("Listing on IP address: " + server.Address);
            Console.WriteLine("On port: " + server.Port);

            while (!server.IsDisposed)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "listclients":
                        Console.WriteLine(server.ConnectedClients.Count + " Client(s) Connected\n-----------------------");
                        foreach (var client in server.ConnectedClients)
                        {
                            Console.WriteLine(client.ConnectAddress);
                        }
                        Console.WriteLine("-----------------------");
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

                Console.WriteLine();
            }
        }

        public static void Rickroll(MessageReceivedEventArgs e)
        {
            e.Client.SendMessage("never gunna let you down");
        }

        private static void Server_ClientDisconnected(object sender, ClientToggleEventArgs e)
        {
            Console.WriteLine("Client Disconnected: " + e.ConnectedClient.ConnectAddress);
        }

        private static void Server_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Received Message: " + e.Client.ConnectAddress + " : " + e.Message);

            var toRespond = Reverse(e.Message);

            Console.WriteLine("Returning Message: " + toRespond);
            e.Client.SendMessage(toRespond);
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