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

                // Запускаем поток для принятия клиентов
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
                        // Проверяем, имеет ли доступ к чату
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
            Log("Сервер остановлен");
        }
    }

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

                // Уведомить всех в отделе о новом онлайн пользователе
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

            // Сохраняем в БД
            int messageId = db.SaveMessage(message);
            message.Id = messageId;

            // Отправляем всем в чате
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

            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(packet.Data.ToString());
            string chatName = data["name"].ToString();
            string chatType = data["type"].ToString();
            var participants = JsonSerializer.Deserialize<List<int>>(data["participants"].ToString());

            var newChat = db.CreateChat(chatName, chatType, participants, User.Department);

            SendPacket(new NetworkPacket
            {
                Command = CommandType.ChatCreated,
                Data = newChat
            });
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

                    // Уведомить всех в отделе
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

    public class DatabaseManager
    {
        private SQLiteConnection connection;
        private readonly object dbLock = new object();

        public void InitializeDatabase()
        {
            string dbPath = "messenger.db";
            bool dbExists = File.Exists(dbPath);

            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            if (!dbExists)
            {
                CreateTables();
                AddTestData();
                Console.WriteLine("База данных создана и заполнена тестовыми данными");
            }
        }

        private void CreateTables()
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE,
                    password TEXT,
                    full_name TEXT,
                    department TEXT,
                    position TEXT
                );

                CREATE TABLE IF NOT EXISTS user_status (
                    user_id INTEGER PRIMARY KEY,
                    is_online BOOLEAN DEFAULT 0,
                    last_seen DATETIME,
                    FOREIGN KEY(user_id) REFERENCES users(id)
                );

                CREATE TABLE IF NOT EXISTS departments (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE,
                    description TEXT
                );

                CREATE TABLE IF NOT EXISTS chats (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT,
                    type TEXT,
                    department TEXT,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS chat_participants (
                    chat_id INTEGER,
                    user_id INTEGER,
                    joined_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY(chat_id, user_id),
                    FOREIGN KEY(chat_id) REFERENCES chats(id),
                    FOREIGN KEY(user_id) REFERENCES users(id)
                );

                CREATE TABLE IF NOT EXISTS messages (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    chat_id INTEGER,
                    sender_id INTEGER,
                    text TEXT,
                    sent_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    is_read BOOLEAN DEFAULT 0,
                    FOREIGN KEY(chat_id) REFERENCES chats(id),
                    FOREIGN KEY(sender_id) REFERENCES users(id)
                );

                CREATE TABLE IF NOT EXISTS user_chat_read (
                    user_id INTEGER,
                    chat_id INTEGER,
                    last_read_message_id INTEGER,
                    read_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY(user_id, chat_id),
                    FOREIGN KEY(user_id) REFERENCES users(id),
                    FOREIGN KEY(chat_id) REFERENCES chats(id)
                );";

            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void AddTestData()
        {
            lock (dbLock)
            {
                // Добавляем отделы
                string depts = @"
                    INSERT INTO departments (name, description) VALUES
                    ('ИТ отдел', 'Информационные технологии'),
                    ('Производство', 'Производственный отдел'),
                    ('HR отдел', 'Отдел кадров'),
                    ('Бухгалтерия', 'Финансовый отдел');";

                using (var cmd = new SQLiteCommand(depts, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем пользователей (пароль = username)
                string users = @"
                    INSERT INTO users (username, password, full_name, department, position) VALUES
                    ('admin', 'admin', 'Иванов Иван', 'ИТ отдел', 'Разработчик'),
                    ('petrov', 'petrov', 'Петров Петр', 'ИТ отдел', 'Тимлид'),
                    ('sidorov', 'sidorov', 'Сидоров Сидор', 'Производство', 'Инженер'),
                    ('smirnova', 'smirnova', 'Смирнова Анна', 'HR отдел', 'HR-менеджер');";

                using (var cmd = new SQLiteCommand(users, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Создаем чаты
                string chats = @"
                    INSERT INTO chats (name, type, department) VALUES
                    ('ИТ отдел', 'Department', 'ИТ отдел'),
                    ('Производство', 'Department', 'Производство'),
                    ('Общий чат', 'Group', NULL);";

                using (var cmd = new SQLiteCommand(chats, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем участников в чаты
                string participants = @"
                    INSERT INTO chat_participants (chat_id, user_id) VALUES
                    (1, 1), (1, 2),  -- ИТ отдел
                    (2, 3),           -- Производство
                    (3, 1), (3, 2), (3, 3); -- Общий чат";

                using (var cmd = new SQLiteCommand(participants, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем тестовые сообщения
                string messages = @"
                    INSERT INTO messages (chat_id, sender_id, text, sent_at) VALUES
                    (1, 1, 'Всем привет!', datetime('now', '-1 hour')),
                    (1, 2, 'Привет!', datetime('now', '-50 minutes')),
                    (1, 1, 'Как дела?', datetime('now', '-40 minutes')),
                    (3, 3, 'Коллеги, всем привет из производства!', datetime('now', '-30 minutes'));";

                using (var cmd = new SQLiteCommand(messages, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            lock (dbLock)
            {
                string query = "SELECT * FROM users WHERE username = @username AND password = @password";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                FullName = reader.GetString(3),
                                Department = reader.GetString(4),
                                Position = reader.GetString(5)
                            };
                        }
                    }
                }
                return null;
            }
        }

        public void UpdateUserStatus(int userId, bool isOnline)
        {
            lock (dbLock)
            {
                string query = @"
                    INSERT OR REPLACE INTO user_status (user_id, is_online, last_seen) 
                    VALUES (@userId, @isOnline, CURRENT_TIMESTAMP)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@isOnline", isOnline);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Chat> GetUserChats(int userId)
        {
            var chats = new List<Chat>();

            lock (dbLock)
            {
                string query = @"
                    SELECT 
                        c.*,
                        (SELECT text FROM messages 
                         WHERE chat_id = c.id 
                         ORDER BY sent_at DESC LIMIT 1) as last_message,
                        (SELECT sent_at FROM messages 
                         WHERE chat_id = c.id 
                         ORDER BY sent_at DESC LIMIT 1) as last_time,
                        (SELECT COUNT(*) FROM messages m 
                         WHERE m.chat_id = c.id 
                         AND m.id > COALESCE(
                             (SELECT last_read_message_id FROM user_chat_read 
                              WHERE user_id = @userId AND chat_id = c.id), 0)
                        ) as unread
                    FROM chats c
                    JOIN chat_participants cp ON c.id = cp.chat_id
                    WHERE cp.user_id = @userId
                    ORDER BY last_time DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chats.Add(new Chat
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2) == "Department" ? ChatType.Department : ChatType.Group,
                                LastMessage = reader.IsDBNull(5) ? null : reader.GetString(5),
                                LastMessageTime = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                                UnreadCount = reader.GetInt32(7)
                            });
                        }
                    }
                }

                // Загружаем участников для каждого чата
                foreach (var chat in chats)
                {
                    chat.Participants = GetChatParticipants(chat.Id);
                }
            }

            return chats;
        }

        private List<User> GetChatParticipants(int chatId)
        {
            var users = new List<User>();

            string query = @"
                SELECT u.*, us.is_online, us.last_seen
                FROM users u
                JOIN chat_participants cp ON u.id = cp.user_id
                LEFT JOIN user_status us ON u.id = us.user_id
                WHERE cp.chat_id = @chatId";

            using (var cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@chatId", chatId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = reader.GetString(3),
                            Department = reader.GetString(4),
                            Position = reader.GetString(5),
                            IsOnline = !reader.IsDBNull(6) && reader.GetBoolean(6),
                            LastSeen = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                        });
                    }
                }
            }

            return users;
        }

        public List<Message> GetChatMessages(int chatId, int userId)
        {
            var messages = new List<Message>();

            lock (dbLock)
            {
                string query = @"
                    SELECT m.*, u.full_name
                    FROM messages m
                    JOIN users u ON m.sender_id = u.id
                    WHERE m.chat_id = @chatId
                    ORDER BY m.sent_at ASC
                    LIMIT 100";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@chatId", chatId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Message
                            {
                                Id = reader.GetInt32(0),
                                ChatId = reader.GetInt32(1),
                                SenderId = reader.GetInt32(2),
                                SenderName = reader.GetString(7),
                                Text = reader.GetString(3),
                                SentAt = reader.GetDateTime(4),
                                IsRead = reader.GetBoolean(5)
                            });
                        }
                    }
                }
            }

            return messages;
        }

        public int SaveMessage(Message message)
        {
            lock (dbLock)
            {
                string query = @"
                    INSERT INTO messages (chat_id, sender_id, text, sent_at)
                    VALUES (@chatId, @senderId, @text, @sentAt);
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@chatId", message.ChatId);
                    cmd.Parameters.AddWithValue("@senderId", message.SenderId);
                    cmd.Parameters.AddWithValue("@text", message.Text);
                    cmd.Parameters.AddWithValue("@sentAt", message.SentAt);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public bool UserHasAccessToChat(int userId, int chatId)
        {
            lock (dbLock)
            {
                string query = "SELECT COUNT(*) FROM chat_participants WHERE chat_id = @chatId AND user_id = @userId";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@chatId", chatId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public Chat CreateChat(string name, string type, List<int> participants, string department)
        {
            lock (dbLock)
            {
                string query = "INSERT INTO chats (name, type, department) VALUES (@name, @type, @dept); SELECT last_insert_rowid();";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@dept", department);

                    int chatId = Convert.ToInt32(cmd.ExecuteScalar());

                    foreach (var userId in participants)
                    {
                        string partQuery = "INSERT INTO chat_participants (chat_id, user_id) VALUES (@chatId, @userId)";
                        using (var partCmd = new SQLiteCommand(partQuery, connection))
                        {
                            partCmd.Parameters.AddWithValue("@chatId", chatId);
                            partCmd.Parameters.AddWithValue("@userId", userId);
                            partCmd.ExecuteNonQuery();
                        }
                    }

                    return new Chat
                    {
                        Id = chatId,
                        Name = name,
                        Type = type == "Department" ? ChatType.Department : ChatType.Group,
                        Participants = GetChatParticipants(chatId)
                    };
                }
            }
        }

        public List<string> GetAllDepartments()
        {
            var departments = new List<string>();

            lock (dbLock)
            {
                string query = "SELECT name FROM departments ORDER BY name";
                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        departments.Add(reader.GetString(0));
                    }
                }
            }

            return departments;
        }
    }
}
