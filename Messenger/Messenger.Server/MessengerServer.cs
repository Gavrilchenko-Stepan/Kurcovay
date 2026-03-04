using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Messenger.Shared;

namespace Messenger.Server
{
    public class MessengerServer
    {
        private TcpListener tcpListener;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private DatabaseManager db;
        private bool isRunning = false;
        private readonly object clientsLock = new object();

        public MessengerServer()
        {
            db = new DatabaseManager();
        }

        public void Start()
        {
            try
            {
                // Инициализация БД
                db.InitializeDatabase();

                // Запуск TCP сервера
                int port = 8888;
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                isRunning = true;

                Log($"Сервер запущен на порту {port}");
                Log($"Локальный IP: {GetLocalIPAddress()}");
                Log("Ожидание подключений...\n");

                var acceptThread = new Thread(AcceptClients);
                acceptThread.IsBackground = true;
                acceptThread.Start();
            }
            catch (Exception ex)
            {
                Log($"Ошибка запуска: {ex.Message}");
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    var tcpClient = tcpListener.AcceptTcpClient();
                    var clientHandler = new ClientHandler(tcpClient, this, db);

                    lock (clientsLock)
                    {
                        clients.Add(clientHandler);
                    }

                    var clientThread = new Thread(clientHandler.HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start();

                    Log($"Новый клиент подключен. Всего: {clients.Count}");
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Log($"Ошибка принятия клиента: {ex.Message}");
                }
            }
        }

        public void BroadcastToChat(int chatId, NetworkPacket packet, int excludeUserId = -1)
        {
            lock (clientsLock)
            {
                foreach (var client in clients)
                {
                    if (client.User != null && client.User.Id != excludeUserId)
                    {
                        if (db.UserHasAccessToChat(client.User.Id, chatId))
                        {
                            client.SendPacket(packet);
                        }
                    }
                }
            }
        }

        public void BroadcastToDepartment(string department, NetworkPacket packet, int excludeUserId = -1)
        {
            lock (clientsLock)
            {
                foreach (var client in clients)
                {
                    if (client.User != null &&
                        client.User.Department == department &&
                        client.User.Id != excludeUserId)
                    {
                        client.SendPacket(packet);
                    }
                }
            }
        }

        public void BroadcastToUser(int userId, NetworkPacket packet)
        {
            lock (clientsLock)
            {
                var client = clients.FirstOrDefault(c => c.User?.Id == userId);
                if (client != null)
                {
                    client.SendPacket(packet);
                }
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            lock (clientsLock)
            {
                clients.Remove(client);
                Log($"Клиент отключен. Осталось: {clients.Count}");
            }
        }

        public void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public void Stop()
        {
            isRunning = false;

            lock (clientsLock)
            {
                foreach (var client in clients)
                {
                    client.Disconnect();
                }
                clients.Clear();
            }

            tcpListener?.Stop();
            db.Close();
            Log("Сервер остановлен");
        }
    }
}
