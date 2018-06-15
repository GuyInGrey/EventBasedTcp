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

            Console.WriteLine("Enter the IP address to connect to: ");
            var ip = Console.ReadLine();

            invalidPort:;

            Console.WriteLine("Enter the port to connect to: ");
            var portString = Console.ReadLine();

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
                        var input = Console.ReadLine();
                        
                        if (input.ToLower().StartsWith("send "))
                        {
                            var toSend = input.Substring(5, input.Length - 5);

                            Console.WriteLine("Sending message: " + toSend);
                            client.SendMessage(toSend);
                            Console.WriteLine("Sent.");
                        }
                        else if (input.ToLower() == "stop")
                        {
                            Console.WriteLine("Disconnecting...");
                            client.Dispose(false);
                            Console.WriteLine("Disconnected. Press any key to continue.");
                            Console.ReadLine();
                        }
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