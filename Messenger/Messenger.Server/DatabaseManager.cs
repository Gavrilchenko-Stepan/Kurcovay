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
            // Создаем папку Data если её нет
            string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDirectory))
                Directory.CreateDirectory(dataDirectory);

            dbPath = Path.Combine(dataDirectory, "messenger.db");
            connectionString = $"Data Source={dbPath};Version=3;Foreign Keys=True;";
        }

        public void InitializeDatabase()
        {
            bool dbExists = File.Exists(dbPath);

            connection = new SQLiteConnection(connectionString);
            connection.Open();

            if (!dbExists)
            {
                CreateTables();
                Console.WriteLine("База данных создана.");
            }

            // Всегда пытаемся заполнить данными (проверка внутри метода)
            SeedDatabase();
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
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void Close()
        {
            if (connection != null && connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        #region User Operations

        public User AuthenticateUser(string username, string password)
        {
            lock (dbLock)
            {
                string query = @"
            SELECT u.id, u.username, u.password, u.full_name, 
                   u.department_id, u.position, d.name as department_name 
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
                            Console.WriteLine($"✅ Найден пользователь: {username}");
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
                            Console.WriteLine($"❌ Пользователь {username} не найден или неверный пароль");
                        }
                    }
                }
                return null;
            }
        }

        public bool RegisterUser(string username, string password, string fullName, int departmentId, string position)
        {
            lock (dbLock)
            {
                try
                {
                    string query = @"
                        INSERT INTO users (username, password, full_name, department_id, position)
                        VALUES (@username, @password, @fullName, @departmentId, @position)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@fullName", fullName);
                        cmd.Parameters.AddWithValue("@departmentId", departmentId);
                        cmd.Parameters.AddWithValue("@position", position ?? (object)DBNull.Value);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch
                {
                    return false;
                }
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

        #endregion

        #region Department Operations

        public List<Department> GetAllDepartments()
        {
            var departments = new List<Department>();

            lock (dbLock)
            {
                string query = "SELECT id, name, description FROM departments ORDER BY name";
                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }

            return departments;
        }

        public int AddDepartment(string name, string description)
        {
            lock (dbLock)
            {
                string query = "INSERT INTO departments (name, description) VALUES (@name, @desc); SELECT last_insert_rowid();";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", description ?? (object)DBNull.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int? GetDepartmentIdByName(string departmentName)
        {
            lock (dbLock)
            {
                string query = "SELECT id FROM departments WHERE name = @name";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", departmentName);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : (int?)null;
                }
            }
        }

        #endregion

        #region Chat Operations

        public List<Chat> GetUserChats(int userId)
        {
            var chats = new List<Chat>();

            lock (dbLock)
            {
                string query = @"
            SELECT 
                c.id,
                c.name,
                c.type,
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
                SELECT u.id, u.username, u.full_name, u.department_id, u.position, 
                       d.name as department_name, us.is_online, us.last_seen
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
                            Department = reader.GetString(5),
                            Position = reader.IsDBNull(4) ? null : reader.GetString(4),
                            IsOnline = !reader.IsDBNull(6) && reader.GetBoolean(6),
                            LastSeen = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                        });
                    }
                }
            }

            return users;
        }

        public Chat CreateChat(string name, ChatType type, List<int> participants, int createdBy)
        {
            lock (dbLock)
            {
                string query = @"
                    INSERT INTO chats (name, type, created_by)
                    VALUES (@name, @type, @createdBy);
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@type", type.ToString());
                    cmd.Parameters.AddWithValue("@createdBy", createdBy);

                    int chatId = Convert.ToInt32(cmd.ExecuteScalar());

                    // Добавляем создателя как участника, если его нет в списке
                    if (!participants.Contains(createdBy))
                        participants.Add(createdBy);

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
                        Type = type,
                        Participants = GetChatParticipants(chatId)
                    };
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

        #endregion

        #region Message Operations

        public List<Message> GetChatMessages(int chatId, int userId)
        {
            var messages = new List<Message>();

            lock (dbLock)
            {
                string query = @"
                    SELECT m.id, m.chat_id, m.sender_id, m.text, m.sent_at, m.is_read,
                           u.full_name
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
                                SenderName = reader.GetString(6),
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

        public void MarkMessagesAsRead(int chatId, int userId, int lastReadMessageId)
        {
            lock (dbLock)
            {
                string query = @"
                    INSERT OR REPLACE INTO user_chat_read (user_id, chat_id, last_read_message_id, read_at)
                    VALUES (@userId, @chatId, @lastMsg, CURRENT_TIMESTAMP)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@chatId", chatId);
                    cmd.Parameters.AddWithValue("@lastMsg", lastReadMessageId);
                    cmd.ExecuteNonQuery();
                }

                // Отмечаем сами сообщения как прочитанные
                string updateQuery = @"
                    UPDATE messages 
                    SET is_read = 1 
                    WHERE chat_id = @chatId AND id <= @lastMsg";

                using (var cmd = new SQLiteCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@chatId", chatId);
                    cmd.Parameters.AddWithValue("@lastMsg", lastReadMessageId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        public void SeedDatabase()
        {
            lock (dbLock)
            {
                try
                {
                    // Проверяем, есть ли уже отделы
                    var checkDept = new SQLiteCommand("SELECT COUNT(*) FROM departments", connection);
                    long deptCount = (long)checkDept.ExecuteScalar();

                    if (deptCount == 0)
                    {
                        // Добавляем отделы
                        string depts = @"
                    INSERT INTO departments (name, description) VALUES 
                    ('ИТ отдел', 'Информационные технологии и разработка'),
                    ('Производство', 'Производственный цех и технологи'),
                    ('HR отдел', 'Отдел кадров и рекрутинг'),
                    ('Бухгалтерия', 'Финансовый отдел'),
                    ('Отдел продаж', 'Продажи и работа с клиентами'),
                    ('Логистика', 'Склад и доставка')";

                        using (var cmd = new SQLiteCommand(depts, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("✅ Добавлены отделы");
                    }

                    // Проверяем, есть ли уже пользователи
                    var checkUser = new SQLiteCommand("SELECT COUNT(*) FROM users", connection);
                    long userCount = (long)checkUser.ExecuteScalar();

                    if (userCount == 0)
                    {
                        // Добавляем пользователей (пароль = логин)
                        string users = @"
                    INSERT INTO users (username, password, full_name, department_id, position) VALUES 
                    ('ivanov', 'ivanov', 'Иванов Иван Иванович', 1, 'Разработчик'),
                    ('petrov', 'petrov', 'Петров Петр Петрович', 1, 'Тимлид'),
                    ('sidorov', 'sidorov', 'Сидоров Сидор Сидорович', 2, 'Инженер'),
                    ('smirnova', 'smirnova', 'Смирнова Анна Сергеевна', 3, 'HR-менеджер'),
                    ('kuznetsov', 'kuznetsov', 'Кузнецов Дмитрий Владимирович', 4, 'Бухгалтер'),
                    ('sokolov', 'sokolov', 'Соколов Максим Андреевич', 5, 'Менеджер'),
                    ('vasiliev', 'vasiliev', 'Васильев Алексей Николаевич', 6, 'Логист')";

                        using (var cmd = new SQLiteCommand(users, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("✅ Добавлены пользователи");
                    }

                    // Создаем чаты отделов
                    var checkChats = new SQLiteCommand("SELECT COUNT(*) FROM chats", connection);
                    long chatCount = (long)checkChats.ExecuteScalar();

                    if (chatCount == 0)
                    {
                        // Чаты отделов
                        for (int i = 1; i <= 6; i++)
                        {
                            string deptName;
                            switch (i)
                            {
                                case 1:
                                    deptName = "ИТ отдел";
                                    break;
                                case 2:
                                    deptName = "Производство";
                                    break;
                                case 3:
                                    deptName = "HR отдел";
                                    break;
                                case 4:
                                    deptName = "Бухгалтерия";
                                    break;
                                case 5:
                                    deptName = "Отдел продаж";
                                    break;
                                case 6:
                                    deptName = "Логистика";
                                    break;
                                default:
                                    deptName = $"Отдел {i}";
                                    break;
                            }

                            var chatCmd = new SQLiteCommand(
                                "INSERT INTO chats (name, type, created_by) VALUES (@name, 'Department', 1)",
                                connection);
                            chatCmd.Parameters.AddWithValue("@name", deptName);
                            chatCmd.ExecuteNonQuery();

                            // Получаем ID чата
                            long chatId = connection.LastInsertRowId;

                            // Добавляем всех пользователей этого отдела в чат
                            var partCmd = new SQLiteCommand(
                                "INSERT INTO chat_participants (chat_id, user_id) SELECT @chatId, id FROM users WHERE department_id = @deptId",
                                connection);
                            partCmd.Parameters.AddWithValue("@chatId", chatId);
                            partCmd.Parameters.AddWithValue("@deptId", i);
                            partCmd.ExecuteNonQuery();
                        }

                        // Общий чат
                        var generalCmd = new SQLiteCommand(
                            "INSERT INTO chats (name, type, created_by) VALUES ('Общий чат', 'Group', 1)",
                            connection);
                        generalCmd.ExecuteNonQuery();
                        long generalChatId = connection.LastInsertRowId;

                        // Добавляем всех пользователей в общий чат
                        var allUsersCmd = new SQLiteCommand(
                            "INSERT INTO chat_participants (chat_id, user_id) SELECT @chatId, id FROM users",
                            connection);
                        allUsersCmd.Parameters.AddWithValue("@chatId", generalChatId);
                        allUsersCmd.ExecuteNonQuery();

                        Console.WriteLine("✅ Созданы чаты");
                    }

                    // Финальная статистика
                    var userFinal = new SQLiteCommand("SELECT COUNT(*) FROM users", connection);
                    long finalUsers = (long)userFinal.ExecuteScalar();

                    var chatFinal = new SQLiteCommand("SELECT COUNT(*) FROM chats", connection);
                    long finalChats = (long)chatFinal.ExecuteScalar();

                    Console.WriteLine($"📊 В базе: {finalUsers} пользователей, {finalChats} чатов");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка при заполнении базы: {ex.Message}");
                }
            }
        }

        #region Дополнительные методы для чатов

        /// <summary>
        /// Получить список пользователей для создания личного чата
        /// </summary>
        public List<User> GetAvailableUsersForChat(int currentUserId)
        {
            var users = new List<User>();
            lock (dbLock)
            {
                string query = @"
            SELECT u.id, u.username, u.full_name, u.department_id, u.position, 
                   d.name as department_name, us.is_online, us.last_seen
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

        /// <summary>
        /// Создать личный чат между двумя пользователями
        /// </summary>
        public Chat CreatePrivateChat(int user1Id, int user2Id)
        {
            lock (dbLock)
            {
                // Проверяем, существует ли уже такой чат
                string checkQuery = @"
            SELECT c.id 
            FROM chats c
            JOIN chat_participants cp1 ON c.id = cp1.chat_id
            JOIN chat_participants cp2 ON c.id = cp2.chat_id
            WHERE c.type = 'Private' 
              AND cp1.user_id = @user1Id 
              AND cp2.user_id = @user2Id
              AND cp1.user_id != cp2.user_id";

                using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@user1Id", user1Id);
                    checkCmd.Parameters.AddWithValue("@user2Id", user2Id);

                    var existingId = checkCmd.ExecuteScalar();
                    if (existingId != null)
                    {
                        // Чат уже существует
                        int chatId = Convert.ToInt32(existingId);
                        return new Chat
                        {
                            Id = chatId,
                            Name = GetChatName(chatId, user1Id),
                            Type = ChatType.Private,
                            Participants = GetChatParticipants(chatId)
                        };
                    }
                }

                // Создаем новый чат
                string insertQuery = @"
            INSERT INTO chats (name, type, created_by) 
            VALUES (@name, 'Private', @createdBy);
            SELECT last_insert_rowid();";

                string chatName = $"Чат {user1Id}-{user2Id}"; // Временное имя

                using (var cmd = new SQLiteCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@name", chatName);
                    cmd.Parameters.AddWithValue("@createdBy", user1Id);

                    int chatId = Convert.ToInt32(cmd.ExecuteScalar());

                    // Добавляем участников
                    string partQuery = "INSERT INTO chat_participants (chat_id, user_id) VALUES (@chatId, @userId)";

                    using (var partCmd = new SQLiteCommand(partQuery, connection))
                    {
                        partCmd.Parameters.AddWithValue("@chatId", chatId);
                        partCmd.Parameters.AddWithValue("@userId", user1Id);
                        partCmd.ExecuteNonQuery();

                        partCmd.Parameters["@userId"].Value = user2Id;
                        partCmd.ExecuteNonQuery();
                    }

                    return new Chat
                    {
                        Id = chatId,
                        Name = GetChatName(chatId, user1Id),
                        Type = ChatType.Private,
                        Participants = GetChatParticipants(chatId)
                    };
                }
            }
        }

        /// <summary>
        /// Получить название чата для отображения
        /// </summary>
        private string GetChatName(int chatId, int currentUserId)
        {
            string query = @"
        SELECT u.full_name 
        FROM users u
        JOIN chat_participants cp ON u.id = cp.user_id
        WHERE cp.chat_id = @chatId AND u.id != @currentUserId
        LIMIT 1";

            using (var cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@chatId", chatId);
                cmd.Parameters.AddWithValue("@currentUserId", currentUserId);

                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? "Личный чат";
            }
        }

        #endregion

        #region Методы для поиска

        /// <summary>
        /// Поиск сообщений в чате
        /// </summary>
        public List<Message> SearchMessages(int chatId, string searchText)
        {
            var messages = new List<Message>();
            lock (dbLock)
            {
                string query = @"
            SELECT m.id, m.chat_id, m.sender_id, m.text, m.sent_at, m.is_read,
                   u.full_name
            FROM messages m
            JOIN users u ON m.sender_id = u.id
            WHERE m.chat_id = @chatId AND m.text LIKE @searchText
            ORDER BY m.sent_at DESC
            LIMIT 50";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@chatId", chatId);
                    cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Message
                            {
                                Id = reader.GetInt32(0),
                                ChatId = reader.GetInt32(1),
                                SenderId = reader.GetInt32(2),
                                SenderName = reader.GetString(6),
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

        #endregion

        #region Методы для статистики

        /// <summary>
        /// Получить количество непрочитанных сообщений
        /// </summary>
        public int GetTotalUnreadCount(int userId)
        {
            lock (dbLock)
            {
                string query = @"
            SELECT SUM(
                (SELECT COUNT(*) FROM messages m 
                 WHERE m.chat_id = c.id 
                 AND m.id > COALESCE(
                     (SELECT last_read_message_id FROM user_chat_read 
                      WHERE user_id = @userId AND chat_id = c.id), 0))
            ) as total_unread
            FROM chats c
            JOIN chat_participants cp ON c.id = cp.chat_id
            WHERE cp.user_id = @userId";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        /// <summary>
        /// Получить онлайн пользователей в отделе
        /// </summary>
        public int GetOnlineUsersInDepartment(int departmentId)
        {
            lock (dbLock)
            {
                string query = @"
            SELECT COUNT(*) 
            FROM users u
            JOIN user_status us ON u.id = us.user_id
            WHERE u.department_id = @deptId AND us.is_online = 1";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        #endregion
    }



    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
