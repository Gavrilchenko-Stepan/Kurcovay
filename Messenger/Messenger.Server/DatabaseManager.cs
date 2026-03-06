using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messenger.Shared;

namespace Messenger.Server
{
    public class DatabaseManager
    {
        private SQLiteConnection connection;
        private readonly object dbLock = new object();
        private readonly string connectionString;
        private readonly string dbPath;

        public DatabaseManager()
        {
            string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDirectory))
                Directory.CreateDirectory(dataDirectory);

            dbPath = Path.Combine(dataDirectory, "messenger.db");
            Console.WriteLine($"Путь к БД: {dbPath}");
            connectionString = $"Data Source={dbPath};Version=3;Foreign Keys=True;";
        }

        public void InitializeDatabase()
        {
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            CreateTables();
            EnsureFirstUser();
        }

        private void CreateTables()
        {
            string sql = @"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS departments (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    description TEXT,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    full_name TEXT NOT NULL,
                    department_id INTEGER NOT NULL,
                    position TEXT,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (department_id) REFERENCES departments(id) ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS user_status (
                    user_id INTEGER PRIMARY KEY,
                    is_online BOOLEAN DEFAULT 0,
                    last_seen DATETIME,
                    FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS chats (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    type TEXT NOT NULL CHECK(type IN ('Department', 'Private', 'Group')),
                    created_by INTEGER,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(created_by) REFERENCES users(id) ON DELETE SET NULL
                );

                CREATE TABLE IF NOT EXISTS chat_participants (
                    chat_id INTEGER NOT NULL,
                    user_id INTEGER NOT NULL,
                    joined_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY(chat_id, user_id),
                    FOREIGN KEY(chat_id) REFERENCES chats(id) ON DELETE CASCADE,
                    FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS messages (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    chat_id INTEGER NOT NULL,
                    sender_id INTEGER NOT NULL,
                    text TEXT NOT NULL,
                    sent_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    is_read BOOLEAN DEFAULT 0,
                    FOREIGN KEY(chat_id) REFERENCES chats(id) ON DELETE CASCADE,
                    FOREIGN KEY(sender_id) REFERENCES users(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS user_chat_read (
                    user_id INTEGER NOT NULL,
                    chat_id INTEGER NOT NULL,
                    last_read_message_id INTEGER NOT NULL,
                    read_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY(user_id, chat_id),
                    FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE,
                    FOREIGN KEY(chat_id) REFERENCES chats(id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_messages_chat ON messages(chat_id, sent_at);
                CREATE INDEX IF NOT EXISTS idx_chat_participants_user ON chat_participants(user_id);
                CREATE INDEX IF NOT EXISTS idx_user_status_online ON user_status(is_online);
            ";
            using (var cmd = new SQLiteCommand(sql, connection))
                cmd.ExecuteNonQuery();
        }

        private void EnsureFirstUser()
        {
            lock (dbLock)
            {
                if ((long)new SQLiteCommand("SELECT COUNT(*) FROM users", connection).ExecuteScalar() == 0)
                {
                    new SQLiteCommand("INSERT INTO departments (name) VALUES ('Администрация')", connection).ExecuteNonQuery();
                    long deptId = (long)new SQLiteCommand("SELECT id FROM departments WHERE name='Администрация'", connection).ExecuteScalar();

                    using (var cmd = new SQLiteCommand("INSERT INTO users (username, password, full_name, department_id) VALUES (@u, @p, @f, @d)", connection))
                    {
                        cmd.Parameters.AddWithValue("@u", "admin");
                        cmd.Parameters.AddWithValue("@p", "admin");
                        cmd.Parameters.AddWithValue("@f", "Главный администратор");
                        cmd.Parameters.AddWithValue("@d", deptId);
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Создан пользователь admin/admin для первого входа.");
                }
            }
        }

        public void Close()
        {
            if (connection?.State != System.Data.ConnectionState.Closed)
                connection?.Close();
        }

        // ---------- User Operations ----------
        public User AuthenticateUser(string username, string password)
        {
            lock (dbLock)
            {
                Console.WriteLine($"🔍 AuthenticateUser: ищем {username} с паролем {password}");

                // Проверим, есть ли вообще такой пользователь
                using (var checkCmd = new SQLiteCommand("SELECT * FROM users WHERE username = @username", connection))
                {
                    checkCmd.Parameters.AddWithValue("@username", username);
                    using (var checkReader = checkCmd.ExecuteReader())
                    {
                        if (checkReader.Read())
                        {
                            Console.WriteLine($"✅ Пользователь {username} найден в БД. Пароль в БД: {checkReader["password"]}");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Пользователь {username} не найден в БД");
                        }
                    }
                }

                string query = @"
            SELECT u.id, u.username, u.password, u.full_name, u.department_id, u.position, d.name as department_name 
            FROM users u
            JOIN departments d ON u.department_id = d.id
            WHERE u.username = @username AND u.password = @password";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"✅ Успешная аутентификация: {username}");
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                FullName = reader.GetString(3),
                                DepartmentId = reader.GetInt32(4),
                                Position = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Department = reader.GetString(6),
                                IsOnline = false
                            };
                        }
                        else
                        {
                            Console.WriteLine($"❌ Неверный пароль для {username}");
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
                string query = @"INSERT OR REPLACE INTO user_status (user_id, is_online, last_seen) VALUES (@id, @online, CURRENT_TIMESTAMP)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@online", isOnline);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<User> GetAvailableUsersForChat(int currentUserId)
        {
            var users = new List<User>();
            lock (dbLock)
            {
                string query = @"
                    SELECT u.id, u.username, u.full_name, u.department_id, u.position, d.name as department_name, us.is_online, us.last_seen
                    FROM users u
                    JOIN departments d ON u.department_id = d.id
                    LEFT JOIN user_status us ON u.id = us.user_id
                    WHERE u.id != @currentUserId
                    ORDER BY us.is_online DESC, u.full_name";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                FullName = reader.GetString(2),
                                DepartmentId = reader.GetInt32(3),
                                Position = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Department = reader.GetString(5),
                                IsOnline = !reader.IsDBNull(6) && reader.GetBoolean(6),
                                LastSeen = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                            });
                        }
                    }
                }
            }
            return users;
        }

        // ---------- Department Operations ----------
        public List<Department> GetAllDepartments()
        {
            var depts = new List<Department>();
            lock (dbLock)
            {
                string query = "SELECT id, name, description FROM departments ORDER BY name";
                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        depts.Add(new Department
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return depts;
        }

        // ---------- Chat Operations ----------
        public List<Chat> GetUserChats(int userId)
        {
            var chats = new List<Chat>();
            lock (dbLock)
            {
                string query = @"
            SELECT c.id, c.name, c.type,
                (SELECT text FROM messages WHERE chat_id = c.id ORDER BY sent_at DESC LIMIT 1) as last_message,
                (SELECT sent_at FROM messages WHERE chat_id = c.id ORDER BY sent_at DESC LIMIT 1) as last_time,
                (SELECT COUNT(*) FROM messages m 
                 WHERE m.chat_id = c.id 
                   AND m.id > COALESCE((SELECT last_read_message_id FROM user_chat_read WHERE user_id = @uid AND chat_id = c.id),0)
                   AND m.sender_id != @uid) as unread
            FROM chats c
            JOIN chat_participants cp ON c.id = cp.chat_id
            WHERE cp.user_id = @uid
            ORDER BY last_time DESC";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var chat = new Chat
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Type = (ChatType)Enum.Parse(typeof(ChatType), reader.GetString(2)),
                                LastMessage = reader.IsDBNull(3) ? null : reader.GetString(3),
                                LastMessageTime = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                                UnreadCount = reader.GetInt32(5)
                            };
                            chats.Add(chat);
                        }
                    }
                }
                foreach (var chat in chats)
                    chat.Participants = GetChatParticipants(chat.Id);
            }
            return chats;
        }

        private List<User> GetChatParticipants(int chatId)
        {
            var users = new List<User>();
            string query = @"
                SELECT u.id, u.username, u.full_name, u.department_id, u.position, d.name as department_name, us.is_online, us.last_seen
                FROM users u
                JOIN chat_participants cp ON u.id = cp.user_id
                JOIN departments d ON u.department_id = d.id
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
                            FullName = reader.GetString(2),
                            DepartmentId = reader.GetInt32(3),
                            Position = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Department = reader.GetString(5),
                            IsOnline = !reader.IsDBNull(6) && reader.GetBoolean(6),
                            LastSeen = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                        });
                    }
                }
            }
            return users;
        }

        public Chat CreatePrivateChat(int user1Id, int user2Id)
        {
            lock (dbLock)
            {
                // Check if already exists
                string check = @"
                    SELECT c.id FROM chats c
                    JOIN chat_participants cp1 ON c.id = cp1.chat_id
                    JOIN chat_participants cp2 ON c.id = cp2.chat_id
                    WHERE c.type='Private' AND cp1.user_id=@u1 AND cp2.user_id=@u2 AND cp1.user_id!=cp2.user_id";
                using (var cmd = new SQLiteCommand(check, connection))
                {
                    cmd.Parameters.AddWithValue("@u1", user1Id);
                    cmd.Parameters.AddWithValue("@u2", user2Id);
                    var existing = cmd.ExecuteScalar();
                    if (existing != null)
                    {
                        int chatId = Convert.ToInt32(existing);
                        return new Chat { Id = chatId, Type = ChatType.Private, Participants = GetChatParticipants(chatId) };
                    }
                }

                string insert = "INSERT INTO chats (name, type, created_by) VALUES (@name, 'Private', @by); SELECT last_insert_rowid();";
                string name = $"private_{user1Id}_{user2Id}";
                int newId;
                using (var cmd = new SQLiteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@by", user1Id);
                    newId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string part = "INSERT INTO chat_participants (chat_id, user_id) VALUES (@cid, @uid)";
                using (var cmd = new SQLiteCommand(part, connection))
                {
                    cmd.Parameters.AddWithValue("@cid", newId);
                    cmd.Parameters.AddWithValue("@uid", user1Id);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters["@uid"].Value = user2Id;
                    cmd.ExecuteNonQuery();
                }

                return new Chat { Id = newId, Type = ChatType.Private, Participants = GetChatParticipants(newId) };
            }
        }

        public Chat CreateGroupChat(string name, List<int> participants, int createdBy)
        {
            lock (dbLock)
            {
                string insert = "INSERT INTO chats (name, type, created_by) VALUES (@name, 'Group', @by); SELECT last_insert_rowid();";
                int newId;
                using (var cmd = new SQLiteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@by", createdBy);
                    newId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (!participants.Contains(createdBy))
                    participants.Add(createdBy);

                string part = "INSERT INTO chat_participants (chat_id, user_id) VALUES (@cid, @uid)";
                using (var cmd = new SQLiteCommand(part, connection))
                {
                    foreach (var uid in participants)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@cid", newId);
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.ExecuteNonQuery();
                    }
                }
                return new Chat { Id = newId, Type = ChatType.Group, Participants = GetChatParticipants(newId) };
            }
        }

        public bool UserHasAccessToChat(int userId, int chatId)
        {
            lock (dbLock)
            {
                string query = "SELECT COUNT(*) FROM chat_participants WHERE chat_id = @cid AND user_id = @uid";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@cid", chatId);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // ---------- Message Operations ----------
        public List<Message> GetChatMessages(int chatId, int userId)
        {
            var msgs = new List<Message>();
            lock (dbLock)
            {
                string query = @"
            SELECT m.id, m.chat_id, m.sender_id, m.text, m.sent_at, m.is_read, 
                   u.full_name, d.name as department_name
            FROM messages m
            JOIN users u ON m.sender_id = u.id
            JOIN departments d ON u.department_id = d.id
            WHERE m.chat_id = @cid
            ORDER BY m.sent_at ASC LIMIT 100";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@cid", chatId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            msgs.Add(new Message
                            {
                                Id = reader.GetInt32(0),
                                ChatId = reader.GetInt32(1),
                                SenderId = reader.GetInt32(2),
                                Text = reader.GetString(3),
                                SentAt = reader.GetDateTime(4),
                                IsRead = reader.GetBoolean(5),
                                SenderName = reader.GetString(6),
                                SenderDepartment = reader.GetString(7) // новое поле
                            });
                        }
                    }
                }
            }
            return msgs;
        }

        public int SaveMessage(Message message)
        {
            lock (dbLock)
            {
                string query = "INSERT INTO messages (chat_id, sender_id, text, sent_at) VALUES (@cid, @sid, @txt, @dt); SELECT last_insert_rowid();";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@cid", message.ChatId);
                    cmd.Parameters.AddWithValue("@sid", message.SenderId);
                    cmd.Parameters.AddWithValue("@txt", message.Text);
                    cmd.Parameters.AddWithValue("@dt", message.SentAt);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void MarkMessagesAsRead(int chatId, int userId, int lastReadId)
        {
            lock (dbLock)
            {
                string query = @"INSERT OR REPLACE INTO user_chat_read (user_id, chat_id, last_read_message_id, read_at) VALUES (@uid, @cid, @last, CURRENT_TIMESTAMP);
                                 UPDATE messages SET is_read=1 WHERE chat_id=@cid AND id<=@last;";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@cid", chatId);
                    cmd.Parameters.AddWithValue("@last", lastReadId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
