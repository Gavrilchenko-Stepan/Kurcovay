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

namespace Messenger.Server
{
    public class ClientHandler
    {
        private TcpClient client;
        private MessengerServer server;
        private DatabaseManager db;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private bool isConnected;

        public User User { get; private set; }

        public ClientHandler(TcpClient client, MessengerServer server, DatabaseManager db)
        {
            this.client = client;
            this.server = server;
            this.db = db;
            this.stream = client.GetStream();
            this.reader = new StreamReader(stream, Encoding.UTF8);
            this.writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            this.isConnected = true;
        }

        public void HandleClient()
        {
            try
            {
                while (isConnected && client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        string json = reader.ReadLine();
                        if (!string.IsNullOrEmpty(json))
                        {
                            var packet = JsonSerializer.Deserialize<NetworkPacket>(json);
                            ProcessPacket(packet);
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                server.Log($"Ошибка клиента: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        private void ProcessPacket(NetworkPacket packet)
        {
            server.Log($"Команда: {packet.Command} от пользователя {User?.Username ?? "unknown"}");

            switch (packet.Command)
            {
                case CommandType.Login:
                    HandleLogin(packet);
                    break;

                case CommandType.GetChats:
                    HandleGetChats();
                    break;

                case CommandType.GetMessages:
                    HandleGetMessages(packet);
                    break;

                case CommandType.SendMessage:
                    HandleSendMessage(packet);
                    break;

                case CommandType.CreateChat:
                    HandleCreateChat(packet);
                    break;

                case CommandType.GetDepartments:
                    HandleGetDepartments();
                    break;

                case CommandType.Logout:
                    Disconnect();
                    break;
            }
        }

        private void HandleLogin(NetworkPacket packet)
        {
            var data = packet.Data as JsonElement?;
            if (!data.HasValue) return;

            string username = data.Value.GetProperty("username").GetString();
            string password = data.Value.GetProperty("password").GetString();

            User = db.AuthenticateUser(username, password);

            if (User != null)
            {
                db.UpdateUserStatus(User.Id, true);
                User.IsOnline = true;

                SendPacket(new NetworkPacket
                {
                    Command = CommandType.LoginResponse,
                    Data = new { success = true, user = User }
                });

                server.BroadcastToDepartment(User.Department, new NetworkPacket
                {
                    Command = CommandType.UserStatusChanged,
                    Data = User
                }, User.Id);

                server.Log($"Пользователь {User.FullName} вошел в систему");
            }
            else
            {
                SendPacket(new NetworkPacket
                {
                    Command = CommandType.LoginResponse,
                    Data = new { success = false, message = "Неверный логин или пароль" }
                });
            }
        }

        private void HandleGetChats()
        {
            if (User == null) return;

            var chats = db.GetUserChats(User.Id);
            SendPacket(new NetworkPacket
            {
                Command = CommandType.ChatsList,
                Data = chats
            });
        }

        private void HandleGetMessages(NetworkPacket packet)
        {
            if (User == null) return;

            int chatId = Convert.ToInt32(packet.Data);
            var messages = db.GetChatMessages(chatId, User.Id);

            SendPacket(new NetworkPacket
            {
                Command = CommandType.MessagesList,
                Data = messages
            });
        }

        private void HandleSendMessage(NetworkPacket packet)
        {
            if (User == null) return;

            var message = JsonSerializer.Deserialize<Message>(packet.Data.ToString());
            message.SenderId = User.Id;
            message.SenderName = User.FullName;
            message.SentAt = DateTime.Now;

            int messageId = db.SaveMessage(message);
            message.Id = messageId;

            server.BroadcastToChat(message.ChatId, new NetworkPacket
            {
                Command = CommandType.NewMessage,
                Data = message
            }, User.Id);

            server.Log($"Сообщение от {User.FullName} в чат #{message.ChatId}");
        }

        private void HandleCreateChat(NetworkPacket packet)
        {
            if (User == null) return;

            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(packet.Data.ToString());
                string chatName = data["name"].ToString();
                string chatTypeString = data["type"].ToString();
                var participants = JsonSerializer.Deserialize<List<int>>(data["participants"].ToString());

                if (!Enum.TryParse<ChatType>(chatTypeString, out ChatType chatType))
                {
                    server.Log($"Ошибка: неверный тип чата '{chatTypeString}'");
                    return;
                }

                var newChat = db.CreateChat(chatName, chatType, participants, User.Id);

                SendPacket(new NetworkPacket
                {
                    Command = CommandType.ChatCreated,
                    Data = newChat
                });

                server.Log($"Создан новый чат '{chatName}' типа {chatType}");
            }
            catch (Exception ex)
            {
                server.Log($"Ошибка создания чата: {ex.Message}");
            }
        }

        private void HandleGetDepartments()
        {
            if (User == null) return;

            var departments = db.GetAllDepartments();
            SendPacket(new NetworkPacket
            {
                Command = CommandType.DepartmentsList,
                Data = departments
            });
        }

        public void SendPacket(NetworkPacket packet)
        {
            try
            {
                string json = JsonSerializer.Serialize(packet);
                writer.WriteLine(json);
            }
            catch (Exception ex)
            {
                server.Log($"Ошибка отправки: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            try
            {
                if (User != null)
                {
                    db.UpdateUserStatus(User.Id, false);
                    User.IsOnline = false;

                    server.BroadcastToDepartment(User.Department, new NetworkPacket
                    {
                        Command = CommandType.UserStatusChanged,
                        Data = User
                    }, User.Id);

                    server.Log($"Пользователь {User.FullName} вышел");
                }

                isConnected = false;
                reader?.Close();
                writer?.Close();
                stream?.Close();
                client?.Close();

                server.RemoveClient(this);
            }
            catch (Exception ex)
            {
                server.Log($"Ошибка при отключении: {ex.Message}");
            }
        }
    }
}
