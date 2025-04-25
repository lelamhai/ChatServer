using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace ChatServer
{
    internal class Program
    {
        static List<TcpClient> clients = new List<TcpClient>();
        static object _lock = new object();

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server is running...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                lock (_lock) clients.Add(client);
                Console.WriteLine("Client connected!");

                Thread t = new Thread(() => HandleClient(client));
                t.Start();
            }
        }

        static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int byteCount;

            try
            {
                while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine("Received: " + message);

                    Broadcast(message, client);
                }
            }
            catch { }

            lock (_lock) clients.Remove(client);
            client.Close();
            Console.WriteLine("Client disconnected.");
        }

        static void Broadcast(string message, TcpClient sender)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            lock (_lock)
            {
                foreach (var client in clients)
                {
                    if (client != sender)
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
    }
}
