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
            stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            isConnected = true;
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
            if (packet.Command != CommandType.GetChats)
                server.Log($"Команда: {packet.Command} от пользователя {User?.Username ?? "unknown"}");
            switch (packet.Command)
            {
                case CommandType.Login: HandleLogin(packet); break;
                case CommandType.GetChats: HandleGetChats(); break;
                case CommandType.GetMessages: HandleGetMessages(packet); break;
                case CommandType.SendMessage: HandleSendMessage(packet); break;
                case CommandType.GetDepartments: HandleGetDepartments(); break;
                case CommandType.GetAvailableUsers: HandleGetAvailableUsers(packet); break;
                case CommandType.CreatePrivateChat: HandleCreatePrivateChat(packet); break;
                case CommandType.CreateGroupChat: HandleCreateGroupChat(packet); break;
                case CommandType.Logout: Disconnect(); break;
            }
        }

        private void HandleLogin(NetworkPacket packet)
        {
            server.Log("HandleLogin started");
            var data = packet.Data as JsonElement?;
            if (!data.HasValue)
            {
                server.Log("Login data is null");
                return;
            }

            try
            {
                string username = data.Value.GetProperty("username").GetString();
                string password = data.Value.GetProperty("password").GetString();
                server.Log($"Login attempt: {username}");

                User = db.AuthenticateUser(username, password);
                server.Log($"AuthenticateUser returned: {(User != null ? "user" : "null")}");

                if (User != null)
                {
                    db.UpdateUserStatus(User.Id, true);
                    User.IsOnline = true;
                    SendPacket(new NetworkPacket { Command = CommandType.LoginResponse, Data = new { success = true, user = User } });
                    server.BroadcastToDepartment(User.Department, new NetworkPacket { Command = CommandType.UserStatusChanged, Data = User }, User.Id);
                    server.Log($"Пользователь {User.FullName} вошёл");
                }
                else
                {
                    SendPacket(new NetworkPacket { Command = CommandType.LoginResponse, Data = new { success = false, message = "Неверный логин или пароль" } });
                    server.Log("Login failed: invalid credentials");
                }
            }
            catch (Exception ex)
            {
                server.Log($"Exception in HandleLogin: {ex.Message}");
            }
        }

        private void HandleGetChats()
        {
            if (User == null) return;
            var chats = db.GetUserChats(User.Id);
            SendPacket(new NetworkPacket { Command = CommandType.ChatsList, Data = chats });
        }

        private void HandleGetMessages(NetworkPacket packet)
        {
            if (User == null) return;
            var jsonElement = (JsonElement)packet.Data;
            int chatId = jsonElement.GetInt32(); // получаем число напрямую
            var msgs = db.GetChatMessages(chatId, User.Id);
            SendPacket(new NetworkPacket { Command = CommandType.MessagesList, Data = msgs });
        }

        private void HandleSendMessage(NetworkPacket packet)
        {
            if (User == null) return;
            // Здесь packet.Data – это JsonElement, содержащий объект Message
            var jsonElement = (JsonElement)packet.Data;
            string json = jsonElement.GetRawText();
            var msg = JsonSerializer.Deserialize<Message>(json);
            msg.SenderId = User.Id;
            msg.SenderName = User.FullName;
            msg.SentAt = DateTime.Now;
            int id = db.SaveMessage(msg);
            msg.Id = id;
            server.BroadcastToChat(msg.ChatId, new NetworkPacket { Command = CommandType.NewMessage, Data = msg }, User.Id);
            server.Log($"Сообщение от {User.FullName} в чат {msg.ChatId}");
        }

        private void HandleGetDepartments()
        {
            if (User == null) return;
            var depts = db.GetAllDepartments();
            Console.WriteLine($"HandleGetDepartments: sending {depts.Count} departments");
            SendPacket(new NetworkPacket { Command = CommandType.DepartmentsList, Data = depts });
        }

        private void HandleGetAvailableUsers(NetworkPacket packet)
        {
            if (User == null) return;
            var jsonElement = (JsonElement)packet.Data;
            int uid = jsonElement.GetInt32(); // получаем число
            var users = db.GetAvailableUsersForChat(uid);
            SendPacket(new NetworkPacket { Command = CommandType.AvailableUsersList, Data = users });
        }

        private void HandleCreatePrivateChat(NetworkPacket packet)
        {
            if (User == null) return;
            var jsonElement = (JsonElement)packet.Data;
            string json = jsonElement.GetRawText();
            var data = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            int otherId = data["otherUserId"];
            var chat = db.CreatePrivateChat(User.Id, otherId);
            SendPacket(new NetworkPacket { Command = CommandType.ChatCreated, Data = chat });
            // обновить списки чатов у обоих
            var chats1 = db.GetUserChats(User.Id);
            var chats2 = db.GetUserChats(otherId);
            SendPacket(new NetworkPacket { Command = CommandType.ChatsList, Data = chats1 });
            server.BroadcastToUser(otherId, new NetworkPacket { Command = CommandType.ChatsList, Data = chats2 });
            server.Log($"Создан личный чат между {User.Id} и {otherId}");
        }

        private void HandleCreateGroupChat(NetworkPacket packet)
        {
            if (User == null) return;
            var jsonElement = (JsonElement)packet.Data;
            string json = jsonElement.GetRawText();
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            string name = data["name"].ToString();
            // participants может быть JsonElement или списком, поэтому десериализуем отдельно
            var participants = JsonSerializer.Deserialize<List<int>>(data["participants"].ToString());
            var chat = db.CreateGroupChat(name, participants, User.Id);
            SendPacket(new NetworkPacket { Command = CommandType.ChatCreated, Data = chat });
            // всем участникам обновить списки
            foreach (var uid in participants)
            {
                var chats = db.GetUserChats(uid);
                server.BroadcastToUser(uid, new NetworkPacket { Command = CommandType.ChatsList, Data = chats });
            }
            server.Log($"Создан групповой чат '{name}'");
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
                    server.BroadcastToDepartment(User.Department, new NetworkPacket { Command = CommandType.UserStatusChanged, Data = User }, User.Id);
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
