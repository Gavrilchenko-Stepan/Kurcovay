using Messenger.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Messenger.Client
{
    public partial class MainForm : Form
    {
        private User currentUser;
        private NetworkClient networkClient;
        private List<Chat> chats = new List<Chat>();
        private Chat currentChat;
        private Dictionary<int, List<Shared.Message>> messages = new Dictionary<int, List<Shared.Message>>();
        private Timer refreshTimer;

        public MainForm()
        {
            InitializeComponent();
            typeof(ListBox).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
            null, lstChats, new object[] { true });
            lstChats.DrawMode = DrawMode.OwnerDrawFixed;
            lstChats.DrawItem += LstChats_DrawItem;
            // отписываемся от события, если оно было подписано
            lstMessages.DrawItem += LstMessages_DrawItem;
            ApplyFuturisticStyle();
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            this.btnNewChat.Click += BtnNewChat_Click;
            this.btnSettings.Click += BtnSettings_Click;
            this.btnLogout.Click += BtnLogout_Click;
            this.btnSend.Click += BtnSend_Click;
            this.txtMessage.KeyDown += TxtMessage_KeyDown;
            this.lstChats.SelectedIndexChanged += LstChats_SelectedIndexChanged;
            this.txtSearchChats.TextChanged += TxtSearchChats_TextChanged;
            this.txtSearchChats.Enter += TxtSearchChats_Enter;
            this.txtSearchChats.Leave += TxtSearchChats_Leave;
        }

        private void ApplyFuturisticStyle()
        {
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.ForeColor = Color.White;

            panelTop.BackColor = Color.FromArgb(20, 20, 30);
            panelTopGradient.BackColor = Color.FromArgb(20, 20, 30); // убираем градиент

            picUserAvatar.BackColor = Color.FromArgb(0, 229, 255); // временный цвет для аватара

            lblUserName.ForeColor = Color.White;
            lblUserDepartment.ForeColor = Color.FromArgb(180, 180, 200);
            lblUserStatus.ForeColor = Color.FromArgb(76, 175, 80);

            btnNewChat.BackColor = Color.Transparent;
            btnNewChat.FlatStyle = FlatStyle.Flat;
            btnNewChat.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
            btnNewChat.ForeColor = Color.FromArgb(0, 229, 255);
            btnNewChat.Text = "➕ Новый чат";

            btnSettings.BackColor = Color.Transparent;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
            btnSettings.ForeColor = Color.FromArgb(0, 229, 255);
            btnSettings.Text = "⚙";

            btnLogout.BackColor = Color.Transparent;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(255, 80, 80);
            btnLogout.ForeColor = Color.FromArgb(255, 80, 80);
            btnLogout.Text = "🚪";

            panelLeft.BackColor = Color.FromArgb(45, 45, 58);
            panelLeftHeader.BackColor = Color.FromArgb(45, 45, 58);
            lblChats.ForeColor = Color.FromArgb(0, 229, 255);
            txtSearchChats.BackColor = Color.FromArgb(60, 60, 80);
            txtSearchChats.ForeColor = Color.White;
            txtSearchChats.BorderStyle = BorderStyle.None;

            lstChats.BackColor = Color.FromArgb(45, 45, 58);
            lstChats.ForeColor = Color.White;

            panelLeftFooter.BackColor = Color.FromArgb(30, 30, 46);
            lblTotalUsers.ForeColor = Color.FromArgb(180, 180, 200);

            panelRight.BackColor = Color.FromArgb(30, 30, 46);
            panelChatHeader.BackColor = Color.FromArgb(45, 45, 58);
            lblChatName.ForeColor = Color.White;
            lblChatInfo.ForeColor = Color.FromArgb(180, 180, 200);

            lstMessages.BackColor = Color.FromArgb(30, 30, 46);
            lstMessages.ForeColor = Color.White;

            panelMessageInput.BackColor = Color.FromArgb(45, 45, 58);
            txtMessage.BackColor = Color.FromArgb(60, 60, 80);
            txtMessage.ForeColor = Color.White;
            txtMessage.BorderStyle = BorderStyle.FixedSingle;

            btnSend.BackColor = Color.FromArgb(0, 229, 255);
            btnSend.ForeColor = Color.Black;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.FlatAppearance.BorderSize = 0;

            panelStatusBar.BackColor = Color.FromArgb(20, 20, 30);
            lblConnectionStatus.ForeColor = Color.FromArgb(180, 180, 200);
            lblServerInfo.ForeColor = Color.FromArgb(180, 180, 200);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Hide();
            ShowLoginForm();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Выйти из приложения?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            refreshTimer?.Stop();
            networkClient?.Disconnect();
        }

        private void ShowLoginForm()
        {
            using (var loginForm = new LoginForm())
            {
                var result = loginForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    currentUser = loginForm.CurrentUser;
                    networkClient = loginForm.NetworkClient;
                    networkClient.OnPacketReceived += OnPacketReceived;
                    networkClient.OnDisconnected += OnDisconnected;

                    UpdateUIAfterLogin();
                    LoadChats();
                    refreshTimer = new Timer { Interval = 5000 };
                    refreshTimer.Tick += (s, e) => LoadChats();
                    refreshTimer.Start();
                    this.Show();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void UpdateUIAfterLogin()
        {
            lblUserName.Text = currentUser.FullName;
            lblUserDepartment.Text = currentUser.Department;
            lblUserStatus.Text = "● Онлайн";
            lblUserStatus.ForeColor = Color.FromArgb(76, 175, 80);
            lblConnectionStatus.Text = "● Подключено к серверу";
            lblConnectionStatus.ForeColor = Color.FromArgb(76, 175, 80);
            lblServerInfo.Text = $"Сервер: {networkClient.ServerIP}:8888";
        }

        private void LoadChats()
        {
            if (networkClient?.IsConnected == true)
                networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetChats, UserId = currentUser.Id });
        }

        private void OnPacketReceived(NetworkPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NetworkPacket>(OnPacketReceived), packet);
                return;
            }

            try
            {
                switch (packet.Command)
                {
                    case Shared.CommandType.ChatsList:
                        var jsonElemChats = (JsonElement)packet.Data;
                        string jsonChats = jsonElemChats.GetRawText();
                        chats = JsonSerializer.Deserialize<List<Chat>>(jsonChats);
                        // Преобразуем имена приватных чатов
                        foreach (var chat in chats)
                        {
                            if (chat.Type == ChatType.Private && chat.Participants != null)
                            {
                                var other = chat.Participants.FirstOrDefault(p => p.Id != currentUser.Id);
                                if (other != null)
                                    chat.Name = other.FullName;
                            }
                        }
                        Console.WriteLine($"Получен список чатов: {chats.Count} чатов");
                        foreach (var c in chats) Console.WriteLine($"  - {c.Name} (Id={c.Id})");
                        UpdateChatsList();
                        UpdateTotalUsers();
                        break;
                    case Shared.CommandType.MessagesList:
                        var jsonElemMsgs = (JsonElement)packet.Data;
                        string jsonMsgs = jsonElemMsgs.GetRawText();
                        var msgs = JsonSerializer.Deserialize<List<Shared.Message>>(jsonMsgs);
                        if (msgs.Any())
                        {
                            int chatId = msgs.First().ChatId;
                            Console.WriteLine($"Получен список сообщений для чата {chatId}, всего {msgs.Count}");
                            messages[chatId] = msgs;
                            if (currentChat != null && chatId == currentChat.Id)
                                DisplayMessages();
                        }
                        else
                        {
                            Console.WriteLine("Получен пустой список сообщений");
                        }
                        break;
                    case Shared.CommandType.NewMessage:
                        var jsonElemNewMsg = (JsonElement)packet.Data;
                        string jsonNewMsg = jsonElemNewMsg.GetRawText();
                        var newMsg = JsonSerializer.Deserialize<Shared.Message>(jsonNewMsg);
                        Console.WriteLine($"Получено новое сообщение: '{newMsg.Text}' в чат {newMsg.ChatId}");
                        HandleNewMessage(newMsg);
                        break;
                    case Shared.CommandType.UserStatusChanged:
                        var jsonElemUser = (JsonElement)packet.Data;
                        string jsonUser = jsonElemUser.GetRawText();
                        var user = JsonSerializer.Deserialize<User>(jsonUser);
                        Console.WriteLine($"UserStatusChanged: {user.FullName} is {user.IsOnline}");
                        UpdateUserStatus(user);
                        break;
                    case Shared.CommandType.ChatCreated:
                        var jsonElemChatCreated = (JsonElement)packet.Data;
                        string jsonChatCreated = jsonElemChatCreated.GetRawText();
                        var newChat = JsonSerializer.Deserialize<Chat>(jsonChatCreated);
                        chats.Add(newChat);
                        UpdateChatsList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки: {ex.Message}");
            }
        }

        private void OnDisconnected()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnDisconnected));
                return;
            }
            lblConnectionStatus.Text = "● Отключено";
            lblConnectionStatus.ForeColor = Color.Red;
            btnSend.Enabled = false;
            refreshTimer?.Stop();
            MessageBox.Show("Соединение с сервером потеряно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateChatsList()
        {
            lstChats.BeginUpdate();
            lstChats.Items.Clear();
            var sorted = chats.OrderByDescending(c => c.LastMessageTime).ToList();
            foreach (var chat in sorted)
                lstChats.Items.Add(chat);
            lstChats.EndUpdate();
        }

        private void UpdateTotalUsers()
        {
            var ids = new HashSet<int>();
            foreach (var chat in chats)
                foreach (var u in chat.Participants)
                    ids.Add(u.Id);
            lblTotalUsers.Text = $"Всего пользователей: {ids.Count}";
        }

        private void LstChats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(lstChats.SelectedItem is Chat chat)) return;
            currentChat = chat;
            lblChatName.Text = chat.Name;

            // Остальной код (сброс unread, очистка и т.д.)
            chat.UnreadCount = 0;
            UpdateChatsList();
            btnSend.Enabled = true;

            if (messages.ContainsKey(chat.Id))
                messages.Remove(chat.Id);
            lstMessages.Items.Clear();
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetMessages, UserId = currentUser.Id, Data = chat.Id });

            // Обновляем заголовок с учётом статусов
            UpdateCurrentChatHeader();
        }

        private void DisplayMessages()
        {
            lstMessages.Items.Clear();
            if (!messages.ContainsKey(currentChat.Id)) return;

            var msgs = messages[currentChat.Id];
            DateTime? lastDate = null;
            foreach (var msg in msgs)
            {
                if (lastDate == null || msg.SentAt.Date != lastDate.Value.Date)
                {
                    lstMessages.Items.Add(msg.SentAt.ToString("d MMMM yyyy"));
                    lastDate = msg.SentAt.Date;
                }
                lstMessages.Items.Add(msg);
            }
            if (lstMessages.Items.Count > 0)
                lstMessages.TopIndex = lstMessages.Items.Count - 1;

            // Отправляем подтверждение прочтения последнего сообщения
            if (msgs.Any())
            {
                int lastId = msgs.Max(m => m.Id);
                SendReadReceipt(currentChat.Id, lastId);
            }
        }

        private void HandleNewMessage(Shared.Message msg)
        {
            if (!messages.ContainsKey(msg.ChatId))
                messages[msg.ChatId] = new List<Shared.Message>();
            messages[msg.ChatId].Add(msg);

            var chat = chats.FirstOrDefault(c => c.Id == msg.ChatId);
            if (chat != null)
            {
                chat.LastMessage = msg.Text;
                chat.LastMessageTime = msg.SentAt;
                if (currentChat?.Id != msg.ChatId)
                    chat.UnreadCount++;
                UpdateChatsList();
                if (currentChat?.Id == msg.ChatId)
                {
                    lstMessages.Items.Add(msg);
                    lstMessages.TopIndex = lstMessages.Items.Count - 1;
                    // Отправляем подтверждение прочтения этого нового сообщения
                    SendReadReceipt(msg.ChatId, msg.Id);
                }
            }
        }

        private void UpdateUserStatus(User user)
        {
            foreach (var chat in chats)
            {
                var p = chat.Participants?.FirstOrDefault(x => x.Id == user.Id);
                if (p != null)
                {
                    p.IsOnline = user.IsOnline;
                    p.LastSeen = user.LastSeen;
                }
            }
            // Перезаполняем список чатов, чтобы статусы отобразились
            UpdateChatsList();

            // Если текущий чат – личный с этим пользователем, обновляем заголовок
            if (currentChat?.Type == ChatType.Private)
            {
                var other = currentChat.Participants?.FirstOrDefault(p => p.Id == user.Id);
                if (other != null)
                {
                    string status = other.IsOnline ? "● Онлайн" : "● Офлайн";
                    lblChatInfo.Text = $"Личный чат • {status}";
                }
            }
            else if (currentChat?.Type == ChatType.Group || currentChat?.Type == ChatType.Department)
            {
                // Можно обновить количество онлайн в групповом чате
                int onlineCount = currentChat.Participants?.Count(p => p.IsOnline) ?? 0;
                if (currentChat.Type == ChatType.Group)
                    lblChatInfo.Text = $"Групповой чат • {currentChat.Participants.Count} уч. • {onlineCount} онлайн";
                else
                    lblChatInfo.Text = $"Чат • {currentChat.Participants.Count} уч. • {onlineCount} онлайн";
            }
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text) || currentChat == null) return;

            var msg = new Shared.Message
            {
                ChatId = currentChat.Id,
                SenderId = currentUser.Id,
                SenderName = currentUser.FullName,
                Text = txtMessage.Text.Trim(),
                SentAt = DateTime.Now
            };

            // Локально добавляем в список (чтобы сразу увидеть)
            if (!messages.ContainsKey(msg.ChatId))
                messages[msg.ChatId] = new List<Shared.Message>();
            messages[msg.ChatId].Add(msg);
            lstMessages.Items.Add(msg);
            lstMessages.TopIndex = lstMessages.Items.Count - 1;

            // Отправляем на сервер
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.SendMessage, UserId = currentUser.Id, Data = msg });
            txtMessage.Clear();
        }

        private void BtnSend_Click(object sender, EventArgs e) => SendMessage();
        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Control) { SendMessage(); e.SuppressKeyPress = true; }
        }

        private void BtnNewChat_Click(object sender, EventArgs e)
        {
            using (var form = new NewChatForm(currentUser.Id, currentUser.Department, networkClient))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    LoadChats();
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Настройки будут позже", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Выйти из системы?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                refreshTimer?.Stop();
                networkClient?.Disconnect();
                if (networkClient != null)
                {
                    networkClient.OnPacketReceived -= OnPacketReceived;
                    networkClient.OnDisconnected -= OnDisconnected;
                }
                this.Hide();
                ShowLoginForm();
            }
        }

        private void TxtSearchChats_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearchChats.Text.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(search) || search == "поиск чатов...")
                UpdateChatsList();
            else
            {
                var filtered = chats.Where(c => c.Name.ToLower().Contains(search)).ToList();
                lstChats.Items.Clear();
                foreach (var c in filtered) lstChats.Items.Add(c);
            }
        }
        private void TxtSearchChats_Enter(object sender, EventArgs e)
        {
            if (txtSearchChats.Text == "Поиск чатов...") { txtSearchChats.Text = ""; txtSearchChats.ForeColor = Color.White; }
        }
        private void TxtSearchChats_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchChats.Text)) { txtSearchChats.Text = "Поиск чатов..."; txtSearchChats.ForeColor = Color.Gray; }
        }

        // Отрисовка элементов списков (без эмодзи, только круги)
        private void LstChats_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(lstChats.Items[e.Index] is Chat chat)) return;
            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = selected ? Color.FromArgb(0, 229, 255, 50) : Color.FromArgb(45, 45, 58);
            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            // Выбираем иконку в зависимости от типа чата
            string icon = chat.Type == ChatType.Private ? "👤" : "👥";
            using (var iconFont = new Font("Segoe UI", 16))
            {
                e.Graphics.DrawString(icon, iconFont, Brushes.Gray, e.Bounds.X + 10, e.Bounds.Y + 10);
            }

            int x = e.Bounds.X + 50;
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(chat.Name, font, Brushes.White, x, e.Bounds.Y + 15);
            if (chat.Type == ChatType.Private && chat.Participants != null)
            {
                var other = chat.Participants.FirstOrDefault(p => p.Id != currentUser?.Id);
                if (other != null)
                {
                    string statusText = other.IsOnline ? "● Онлайн" : "● Офлайн";
                    Color statusColor = other.IsOnline ? Color.FromArgb(76, 175, 80) : Color.Gray;
                    using (var statusFont = new Font("Segoe UI", 8, FontStyle.Bold))
                    using (var brush = new SolidBrush(statusColor))
                    {
                        var nameSize = e.Graphics.MeasureString(chat.Name, new Font("Segoe UI", 11, FontStyle.Bold));
                        e.Graphics.DrawString(statusText, statusFont, brush, x + nameSize.Width + 10, e.Bounds.Y + 18);
                    }
                }
            }

            if (!string.IsNullOrEmpty(chat.LastMessage))
            {
                string last = chat.LastMessage.Length > 30 ? chat.LastMessage.Substring(0, 27) + "..." : chat.LastMessage;
                using (var font = new Font("Segoe UI", 9))
                    e.Graphics.DrawString(last, font, Brushes.Gray, x, e.Bounds.Y + 40);
            }

            if (chat.LastMessageTime > DateTime.MinValue)
            {
                string tm = chat.LastMessageTime.ToString("HH:mm");
                using (var font = new Font("Segoe UI", 8))
                {
                    var sz = e.Graphics.MeasureString(tm, font);
                    e.Graphics.DrawString(tm, font, Brushes.Gray, e.Bounds.Right - sz.Width - 20, e.Bounds.Y + 15);
                }
            }

            if (chat.UnreadCount > 0)
            {
                string cnt = chat.UnreadCount.ToString();
                using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                {
                    var sz = e.Graphics.MeasureString(cnt, font);
                    int badgeSize = Math.Max(18, (int)sz.Width + 8);
                    Rectangle rect = new Rectangle(e.Bounds.Right - badgeSize - 15, e.Bounds.Y + 35, badgeSize, 18);
                    using (var brush = new SolidBrush(Color.FromArgb(244, 67, 54)))
                        e.Graphics.FillEllipse(brush, rect);
                    e.Graphics.DrawString(cnt, font, Brushes.White, rect.X + (rect.Width - sz.Width) / 2, rect.Y + 1);
                }
            }
            e.DrawFocusRectangle();
        }

        private void LstMessages_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Разделитель даты
            if (lstMessages.Items[e.Index] is string dateStr)
            {
                e.DrawBackground();
                using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(180, 180, 200)))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(dateStr, font, brush, e.Bounds, sf);
                }
                e.DrawFocusRectangle();
                return;
            }

            if (!(lstMessages.Items[e.Index] is Shared.Message msg)) return;

            e.DrawBackground();

            bool isMy = msg.SenderId == currentUser.Id;
            int maxWidth = 400;
            int x = isMy ? e.Bounds.Width - maxWidth - 20 : e.Bounds.X + 20;
            int y = e.Bounds.Y + 4;

            Color bgColor = isMy ? Color.FromArgb(0, 229, 255, 80) : Color.FromArgb(60, 60, 80);
            Color borderColor = isMy ? Color.FromArgb(0, 229, 255) : Color.Gray;

            Rectangle msgRect = new Rectangle(x, y, maxWidth, e.Bounds.Height - 12);

            using (var brush = new SolidBrush(bgColor))
            using (var pen = new Pen(borderColor, 1))
            using (var path = GetRoundedRect(msgRect, 10))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }

            // Полное имя отправителя
            string senderDisplay = string.IsNullOrEmpty(msg.SenderDepartment)
                    ? msg.SenderName
                    : $"{msg.SenderName} ({msg.SenderDepartment})";
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(senderDisplay, font, brush, x + 10, y + 5);
            }

            // Текст сообщения
            using (var font = new Font("Segoe UI", 10))
            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(msg.Text, font, brush, x + 10, y + 25);
            }

            // Время
            string tm = msg.SentAt.ToString("HH:mm");
            using (var font = new Font("Segoe UI", 8))
            using (var brush = new SolidBrush(Color.Gray))
            {
                var sz = e.Graphics.MeasureString(tm, font);
                e.Graphics.DrawString(tm, font, brush, x + maxWidth - sz.Width - 10, y + 45);
            }

            e.DrawFocusRectangle();
        }

        // Создание скруглённого прямоугольника
        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void SendReadReceipt(int chatId, int lastReadId)
        {
            var data = new Dictionary<string, int>
            {
                { "chatId", chatId },
                { "lastReadMessageId", lastReadId }
            };

            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.MessagesRead,
                UserId = currentUser.Id,
                Data = data
            });
        }

        private void UpdateCurrentChatHeader()
        {
            if (currentChat == null) return;

            if (currentChat.Type == ChatType.Private)
            {
                var other = currentChat.Participants?.FirstOrDefault(p => p.Id != currentUser.Id);
                if (other != null)
                {
                    string status = other.IsOnline ? "● Онлайн" : "● Офлайн";
                    lblChatInfo.Text = $"Личный чат • {status}";
                }
                else
                    lblChatInfo.Text = "Личный чат";
            }
            else if (currentChat.Type == ChatType.Group)
            {
                int onlineCount = currentChat.Participants?.Count(p => p.IsOnline) ?? 0;
                lblChatInfo.Text = $"Групповой чат • {currentChat.Participants.Count} уч. • {onlineCount} онлайн";
            }
            else // Department
            {
                int onlineCount = currentChat.Participants?.Count(p => p.IsOnline) ?? 0;
                lblChatInfo.Text = $"Чат • {currentChat.Participants.Count} уч. • {onlineCount} онлайн";
            }
        }
    }
}
