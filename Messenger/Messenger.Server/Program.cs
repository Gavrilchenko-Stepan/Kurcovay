using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Messenger.Shared;
using CommandType = Messenger.Shared.CommandType;
using System.Text.Json;
using System.Data.SQLite;

namespace Messenger.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Messenger Server";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ╔═══════════════════════════════════════╗
  ║     ЛОКАЛЬНЫЙ КОРПОРАТИВНЫЙ           ║
  ║         MESSENGER - СЕРВЕР            ║
  ╚═══════════════════════════════════════╝
");
            Console.ResetColor();

            var server = new MessengerServer();
            server.Start();

            Console.WriteLine("\nНажмите 'Q' для остановки сервера...");
            while (Console.ReadKey().Key != ConsoleKey.Q) { }

            server.Stop();
        }
    }
}
