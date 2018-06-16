using System;
using System.Linq;
using EventBasedTCP;

namespace ChatClient
{
    public class Program
    {
        static Client client;
        
        static void Main(string[] args)
        {
            failedToConnect:;

            Console.Write("Enter the IP address to connect to:\n> ");
            var ip = Console.ReadLine();

            invalidPort:;

            Console.Write("Enter the port to connect to:\n> ");
            var portString = Console.ReadLine();
            Console.WriteLine();

            if (int.TryParse(portString, out var port))
            {
                try
                {
                    client = new Client(ip, port);
                    if (client.FailedConnect)
                    {
                        Console.WriteLine("Failed to connect!");
                        goto failedToConnect;
                    }
                    client.MessageReceived += Client_MessageReceived;
                    Console.WriteLine("Client connected.");

                    while (!client.IsDisposed)
                    {
                        Console.Write("> ");
                        var input = Console.ReadLine();

                        if (client.IsDisposed)
                        {
                            Console.WriteLine("The server closed. This client has disposed. Press any key to close...");
                            Console.ReadLine();
                        }
                        else
                        {

                            if (input.ToLower().StartsWith("send "))
                            {
                                var toSend = input.Substring(5, input.Length - 5);

                                Console.WriteLine("Sending message: \n     " + toSend + "\n");
                                client.SendMessage(toSend);
                                Console.WriteLine("Sent message.");
                            }
                            else if (input.ToLower() == "stop")
                            {
                                Console.WriteLine("Disconnecting...");
                                client.Dispose(false);
                                Console.WriteLine("Disconnected. Press any key to continue.");
                                Console.ReadLine();
                            }
                        }

                        Console.WriteLine();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Invalid Port. ");
                goto invalidPort;
            }
        }

        private static void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Received message from server: " + e.Message);
        }
    }
}