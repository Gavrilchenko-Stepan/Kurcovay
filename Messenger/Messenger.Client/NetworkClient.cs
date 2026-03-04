using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Messenger.Shared;

namespace Messenger.Client
{
    public class NetworkClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private bool isConnected;
        private Thread receiveThread;

        // Добавляем свойство ServerIP
        public string ServerIP { get; private set; }

        public event Action<NetworkPacket> OnPacketReceived;
        public event Action OnDisconnected;

        public bool IsConnected => isConnected;

        public async Task<bool> Connect(string serverIP, int port = 8888)
        {
            try
            {
                ServerIP = serverIP; // Сохраняем IP сервера
                client = new TcpClient();
                await client.ConnectAsync(serverIP, port);

                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                isConnected = true;

                receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReceiveLoop()
        {
            while (isConnected && client.Connected)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        string json = reader.ReadLine();
                        if (!string.IsNullOrEmpty(json))
                        {
                            Console.WriteLine($"📨 Получено: {json.Substring(0, Math.Min(100, json.Length))}...");
                            var packet = JsonSerializer.Deserialize<NetworkPacket>(json);
                            OnPacketReceived?.Invoke(packet);
                        }
                    }
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка в ReceiveLoop: {ex.Message}");
                    Disconnect();
                    break;
                }
            }
        }

        public void SendPacket(NetworkPacket packet)
        {
            try
            {
                if (isConnected && client.Connected)
                {
                    string json = JsonSerializer.Serialize(packet);
                    writer.WriteLine(json);
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            isConnected = false;

            try
            {
                if (isConnected)
                {
                    SendPacket(new NetworkPacket { Command = CommandType.Logout });
                }

                reader?.Close();
                writer?.Close();
                stream?.Close();
                client?.Close();
            }
            catch { }

            OnDisconnected?.Invoke();
        }
    }
}
