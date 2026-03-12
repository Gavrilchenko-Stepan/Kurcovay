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
        private Font boldFont11;
        private Font normalFont9;
        private Font smallFont8;
        private Font iconFont;
        private bool _isUpdatingList = false;
        private Font messageSenderFont;
        private Font messageTextFont;
        private Font messageTimeFont;
        private Font dateFont;

        public MainForm()
        {
            InitializeComponent();

            // Настройка шрифтов для сообщений
            messageSenderFont = new Font("Segoe UI", 9, FontStyle.Bold);
            messageTextFont = new Font("Segoe UI", 10);
            messageTimeFont = new Font("Segoe UI", 8);
            dateFont = new Font("Segoe UI", 9, FontStyle.Bold);

            // Двойная буферизация для списка чатов
            typeof(ListBox).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, lstChats, new object[] { true });

            // Настройка поля ввода
            txtMessage.Multiline = true;
            txtMessage.WordWrap = true;
            txtMessage.ScrollBars = ScrollBars.Vertical;
            txtMessage.AcceptsReturn = true;
            txtMessage.TextChanged += TxtMessage_TextChanged;

            // Шрифты для списка чатов
            boldFont11 = new Font("Segoe UI", 11, FontStyle.Bold);
            normalFont9 = new Font("Segoe UI", 9);
            smallFont8 = new Font("Segoe UI", 8);
            try { iconFont = new Font("Segoe UI Emoji", 16); }
            catch { iconFont = new Font("Segoe UI", 16); }

            // Настройка списка чатов (фиксированная высота элементов)
            lstChats.DrawMode = DrawMode.OwnerDrawFixed;
            lstChats.DrawItem += LstChats_DrawItem;

            // Настройка списка сообщений (динамическая высота элементов)
            lstMessages.DrawMode = DrawMode.OwnerDrawVariable;
            lstMessages.MeasureItem += LstMessages_MeasureItem;
            lstMessages.DrawItem += LstMessages_DrawItem;
            lstMessages.KeyDown += LstMessages_KeyDown;

            // Применение стиля
            ApplyFuturisticStyle();

            // Подписка на события
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            this.btnNewChat.Click += BtnNewChat_Click;
            this.btnLogout.Click += BtnLogout_Click;
            this.btnSend.Click += BtnSend_Click;
            this.txtMessage.KeyDown += TxtMessage_KeyDown;
            this.lstChats.SelectedIndexChanged += LstChats_SelectedIndexChanged;
            this.txtSearchChats.TextChanged += TxtSearchChats_TextChanged;
            this.txtSearchChats.Enter += TxtSearchChats_Enter;
            this.txtSearchChats.Leave += TxtSearchChats_Leave;
            this.btnAdminPanel.Click += BtnAdminPanel_Click;
        }

        // ========== Стилизация ==========
        private void ApplyFuturisticStyle()
        {
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.ForeColor = Color.White;

            panelTop.BackColor = Color.FromArgb(20, 20, 30);
            panelTopGradient.BackColor = Color.FromArgb(20, 20, 30);

            picUserAvatar.BackColor = Color.FromArgb(0, 229, 255);
            lblUserName.ForeColor = Color.White;
            lblUserDepartment.ForeColor = Color.FromArgb(180, 180, 200);
            lblUserStatus.ForeColor = Color.FromArgb(76, 175, 80);

            btnNewChat.BackColor = Color.Transparent;
            btnNewChat.FlatStyle = FlatStyle.Flat;
            btnNewChat.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
            btnNewChat.ForeColor = Color.FromArgb(0, 229, 255);
            btnNewChat.Text = "➕ Новый чат";

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

        // ========== Загрузка формы ==========
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
                    refreshTimer = new Timer { Interval = 3000 };
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
            lblUserStatus.Text = currentUser.IsAdmin ? "● Онлайн (Админ)" : "● Онлайн";
            lblUserStatus.ForeColor = Color.FromArgb(76, 175, 80);
            lblConnectionStatus.Text = "● Подключено к серверу";
            lblConnectionStatus.ForeColor = Color.FromArgb(76, 175, 80);
            lblServerInfo.Text = $"Сервер: {networkClient.ServerIP}:8888";

            // Показываем кнопку админ-панели только администратору
            btnAdminPanel.Visible = currentUser.IsAdmin;

            string initials = "?";
            var parts = currentUser.FullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                initials = parts[0][0].ToString().ToUpper();
            else if (parts.Length >= 2)
                initials = (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();

            Bitmap bmp = new Bitmap(40, 40);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(63, 81, 181));
                using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(initials, font, Brushes.White, new Rectangle(0, 0, 40, 40), sf);
                }
            }
            picUserAvatar.Image = bmp;
        }

        // ========== Сетевые методы ==========
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

                        // Преобразование имён личных чатов
                        foreach (var chat in chats)
                        {
                            if (chat.Type == ChatType.Private && chat.Participants != null)
                            {
                                var other = chat.Participants.FirstOrDefault(p => p.Id != currentUser.Id);
                                if (other != null)
                                    chat.Name = other.FullName;
                            }
                        }

                        UpdateChatsList();
                        UpdateTotalUsers();
                        ApplySearchFilter();

                        // Проверка на удаление текущего чата
                        if (currentChat != null && !chats.Any(c => c.Id == currentChat.Id))
                        {
                            currentChat = null;
                            lblChatName.Text = "Выберите чат";
                            lblChatInfo.Text = "";
                            lstMessages.Items.Clear();
                            btnSend.Enabled = false;
                        }
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

                        if (newChat.Type == ChatType.Private && newChat.Participants != null)
                        {
                            var other = newChat.Participants.FirstOrDefault(p => p.Id != currentUser.Id);
                            if (other != null)
                                newChat.Name = other.FullName;
                        }

                        if (!chats.Any(c => c.Id == newChat.Id))
                        {
                            chats.Add(newChat);
                            currentChat = newChat;
                            UpdateChatsList();
                        }
                        break;

                    case Shared.CommandType.ChatUpdated:
                        var jsonChatUpdated = (JsonElement)packet.Data;
                        string jsonUpdated = jsonChatUpdated.GetRawText();
                        var updatedChat = JsonSerializer.Deserialize<Chat>(jsonUpdated);

                        var existing = chats.FirstOrDefault(c => c.Id == updatedChat.Id);
                        if (existing != null)
                        {
                            existing.Name = updatedChat.Name;
                            existing.Participants = updatedChat.Participants;
                            if (currentChat?.Id == updatedChat.Id)
                            {
                                currentChat.Participants = updatedChat.Participants;
                                UpdateCurrentChatHeader();
                            }
                            UpdateChatsList();
                        }
                        break;

                    case Shared.CommandType.MessageDeleted:
                        int deletedId = ((JsonElement)packet.Data).GetInt32();
                        HandleMessageDeleted(deletedId);
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

        // ========== Обновление списка чатов ==========
        private void UpdateChatsList()
        {
            if (_isUpdatingList) return;
            _isUpdatingList = true;

            int topIndex = lstChats.TopIndex;

            lstChats.BeginUpdate();
            try
            {
                lstChats.Items.Clear();
                var sorted = chats.OrderByDescending(c => c.LastMessageTime).ToList();
                foreach (var chat in sorted)
                    lstChats.Items.Add(chat);

                if (currentChat != null)
                {
                    // Пытаемся восстановить выделение текущего чата
                    for (int i = 0; i < lstChats.Items.Count; i++)
                    {
                        if (((Chat)lstChats.Items[i]).Id == currentChat.Id)
                        {
                            lstChats.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    // Явно сбрасываем интерфейс в состояние "нет выбранного чата"
                    lstChats.SelectedIndex = -1;
                    lblChatName.Text = "Выберите чат";
                    lblChatInfo.Text = "";
                    lstMessages.Items.Clear();
                    btnSend.Enabled = false;
                }

                // Восстанавливаем позицию прокрутки
                if (topIndex >= 0 && topIndex < lstChats.Items.Count)
                    lstChats.TopIndex = topIndex;
                else if (lstChats.Items.Count > 0)
                    lstChats.TopIndex = 0;
            }
            finally
            {
                lstChats.EndUpdate();
                _isUpdatingList = false;
            }
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
            if (_isUpdatingList) return;

            if (!(lstChats.SelectedItem is Chat chat)) return;
            currentChat = chat;

            chat.UnreadCount = 0;
            UpdateChatsList();
            btnSend.Enabled = true;

            if (messages.ContainsKey(chat.Id))
                messages.Remove(chat.Id);
            lstMessages.Items.Clear();
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetMessages, UserId = currentUser.Id, Data = chat.Id });

            UpdateCurrentChatHeader();

            if (picChatAvatar.Image != null)
            {
                picChatAvatar.Image.Dispose();
                picChatAvatar.Image = null;
            }

            int size = 50;
            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.Clear(Color.FromArgb(63, 81, 181));

                string text = "?";
                if (currentChat.Type == ChatType.Private && currentChat.Participants != null)
                {
                    var other = currentChat.Participants.FirstOrDefault(p => p.Id != currentUser.Id);
                    if (other != null && !string.IsNullOrWhiteSpace(other.FullName))
                    {
                        var parts = other.FullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 1)
                            text = parts[0][0].ToString().ToUpper();
                        else if (parts.Length >= 2)
                            text = (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(currentChat.Name))
                {
                    text = currentChat.Name[0].ToString().ToUpper();
                }

                using (Font font = new Font("Segoe UI", 20, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (size - textSize.Width) / 2;
                    float y = (size - textSize.Height) / 2;
                    g.DrawString(text, font, Brushes.White, x, y);
                }
            }
            picChatAvatar.Image = bmp;
        }

        // ========== Отображение сообщений ==========
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
                    SendReadReceipt(msg.ChatId, msg.Id);
                }
            }
        }

        private void UpdateUserStatus(User user)
        {
            Console.WriteLine($"UpdateUserStatus: получили статус для {user.FullName} (Id={user.Id}) - онлайн={user.IsOnline}");
            bool updated = false;
            foreach (var chat in chats)
            {
                var p = chat.Participants?.FirstOrDefault(x => x.Id == user.Id);
                if (p != null)
                {
                    Console.WriteLine($"  найден в чате {chat.Name}, старый статус {p.IsOnline}, новый {user.IsOnline}");
                    p.IsOnline = user.IsOnline;
                    p.LastSeen = user.LastSeen;
                    updated = true;
                }
            }
            if (updated)
            {
                Console.WriteLine("  Обновляем список чатов...");
                UpdateChatsList();
            }
            else
            {
                Console.WriteLine("  Участник не найден ни в одном чате");
            }

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
                int onlineCount = currentChat.Participants?.Count(p => p.IsOnline) ?? 0;
                if (currentChat.Type == ChatType.Group)
                    lblChatInfo.Text = $"Групповой чат • {currentChat.Participants.Count} уч. • {onlineCount} онлайн";
                else
                    lblChatInfo.Text = $"Чат • {currentChat.Participants.Count} уч. • {onlineCount} онлайн";
            }
        }

        // ========== Отправка сообщения ==========
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

            if (!messages.ContainsKey(msg.ChatId))
                messages[msg.ChatId] = new List<Shared.Message>();
            messages[msg.ChatId].Add(msg);
            lstMessages.Items.Add(msg);
            lstMessages.TopIndex = lstMessages.Items.Count - 1;

            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.SendMessage, UserId = currentUser.Id, Data = msg });
            txtMessage.Clear();
        }

        private void BtnSend_Click(object sender, EventArgs e) => SendMessage();

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                SendMessage();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && e.Shift)
            {
                // Вставляем новую строку в позицию курсора
                int pos = txtMessage.SelectionStart;
                txtMessage.Text = txtMessage.Text.Insert(pos, Environment.NewLine);
                txtMessage.SelectionStart = pos + Environment.NewLine.Length;
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        // ========== Создание нового чата ==========
        private void BtnNewChat_Click(object sender, EventArgs e)
        {
            using (var form = new NewChatForm(currentUser.Id, currentUser.Department, networkClient, currentUser.IsAdmin))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine("Чат создан");
                }
            }
        }

        // ========== Админ-панель ==========
        private void BtnAdminPanel_Click(object sender, EventArgs e)
        {
            using (var form = new AdminPanelForm(networkClient, currentUser.Id))
            {
                form.ShowDialog();
            }
        }

        // ========== Выход ==========
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

        // ========== Поиск чатов ==========
        private void TxtSearchChats_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearchChats.Text.ToLower().Trim();

            int topIndex = lstChats.TopIndex;

            lstChats.BeginUpdate();
            try
            {
                if (string.IsNullOrWhiteSpace(search) || search == "поиск чатов...")
                {
                    UpdateChatsList();
                    return;
                }
                else
                {
                    lstChats.Items.Clear();
                    var filtered = chats.Where(c => c.Name.ToLower().Contains(search)).ToList();
                    foreach (var c in filtered)
                        lstChats.Items.Add(c);
                }
            }
            finally
            {
                if (currentChat != null)
                {
                    for (int i = 0; i < lstChats.Items.Count; i++)
                    {
                        if (((Chat)lstChats.Items[i]).Id == currentChat.Id)
                        {
                            lstChats.SelectedIndex = i;
                            break;
                        }
                    }
                }
                if (topIndex >= 0 && topIndex < lstChats.Items.Count)
                    lstChats.TopIndex = topIndex;
                lstChats.EndUpdate();
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

        // ========== Отрисовка списка чатов ==========
        private void LstChats_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(lstChats.Items[e.Index] is Chat chat)) return;

            e.DrawBackground();
            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color backColor = selected ? Color.FromArgb(80, 80, 100) : Color.FromArgb(45, 45, 58);

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            string icon = chat.Type == ChatType.Private ? "👤" : "👥";
            e.Graphics.DrawString(icon, iconFont, Brushes.Gray, e.Bounds.X + 10, e.Bounds.Y + 10);

            int x = e.Bounds.X + 50;
            e.Graphics.DrawString(chat.Name, boldFont11, Brushes.White, x, e.Bounds.Y + 15);

            if (!string.IsNullOrEmpty(chat.LastMessage))
            {
                string last = chat.LastMessage.Length > 30 ? chat.LastMessage.Substring(0, 27) + "..." : chat.LastMessage;
                e.Graphics.DrawString(last, normalFont9, Brushes.Gray, x, e.Bounds.Y + 40);
            }

            if (chat.LastMessageTime > DateTime.MinValue)
            {
                string tm = chat.LastMessageTime.ToString("HH:mm");
                var sz = e.Graphics.MeasureString(tm, smallFont8);
                e.Graphics.DrawString(tm, smallFont8, Brushes.Gray, e.Bounds.Right - sz.Width - 20, e.Bounds.Y + 15);
            }

            if (chat.UnreadCount > 0)
            {
                string cnt = chat.UnreadCount.ToString();
                var sz = e.Graphics.MeasureString(cnt, smallFont8);
                int badgeSize = Math.Max(18, (int)sz.Width + 8);
                Rectangle rect = new Rectangle(e.Bounds.Right - badgeSize - 15, e.Bounds.Y + 35, badgeSize, 18);
                using (var brush = new SolidBrush(Color.FromArgb(244, 67, 54)))
                    e.Graphics.FillEllipse(brush, rect);
                e.Graphics.DrawString(cnt, smallFont8, Brushes.White, rect.X + (rect.Width - sz.Width) / 2, rect.Y + 1);
            }

            if (chat.Type == ChatType.Private && chat.Participants != null)
            {
                var other = chat.Participants.FirstOrDefault(p => p.Id != currentUser?.Id);
                if (other != null)
                {
                    string status = other.IsOnline ? "● Онлайн" : "● Офлайн";
                    Color statusColor = other.IsOnline ? Color.FromArgb(76, 175, 80) : Color.Gray;
                    var nameWidth = e.Graphics.MeasureString(chat.Name, boldFont11).Width;
                    using (var brush = new SolidBrush(statusColor))
                        e.Graphics.DrawString(status, smallFont8, brush, x + nameWidth + 10, e.Bounds.Y + 18);
                }
            }

            e.DrawFocusRectangle();
        }

        // ========== Отрисовка сообщений ==========
        private void LstMessages_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Защита: если currentUser ещё не установлен, не рисуем детали
            if (currentUser == null) return;

            object item = lstMessages.Items[e.Index];
            if (item == null) return;

            // Фон элемента
            bool isSelected = (e.State & DrawItemState.Selected) != 0;
            Color backColor = item is string
                ? Color.FromArgb(30, 30, 46)
                : (isSelected ? Color.FromArgb(80, 80, 100) : Color.FromArgb(30, 30, 46));

            using (var backBrush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(backBrush, e.Bounds);

            // Разделитель даты (строка)
            if (item is string dateStr)
            {
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var brush = new SolidBrush(Color.FromArgb(180, 180, 200)))
                {
                    e.Graphics.DrawString(dateStr, dateFont, brush, e.Bounds, sf);
                }
                return;
            }

            // Сообщение
            if (!(item is Shared.Message msg)) return;

            bool isMy = msg.SenderId == currentUser.Id;
            const int maxWidth = 400;
            const int padding = 10;
            int x = isMy ? e.Bounds.Right - maxWidth - 20 : e.Bounds.Left + 20;
            int textWidth = maxWidth - 2 * padding;

            // Рисуем фон пузыря
            Rectangle bubbleRect = new Rectangle(x, e.Bounds.Y + 2, maxWidth, e.Bounds.Height - 4);
            using (var path = GetRoundedRect(bubbleRect, 10))
            {
                Color bgColor = isMy ? Color.FromArgb(0, 229, 255, 80) : Color.FromArgb(60, 60, 80);
                Color borderColor = isMy ? Color.FromArgb(0, 229, 255) : Color.Gray;
                using (var brush = new SolidBrush(bgColor))
                using (var pen = new Pen(borderColor, 1))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            }

            int currentY = e.Bounds.Y + 5;

            // Имя отправителя
            string senderDisplay = string.IsNullOrEmpty(msg.SenderDepartment)
                ? msg.SenderName
                : $"{msg.SenderName} ({msg.SenderDepartment})";
            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(senderDisplay, messageSenderFont,
                    brush, new RectangleF(x + padding, currentY, textWidth, 100));
            }
            SizeF senderSize = e.Graphics.MeasureString(senderDisplay, messageSenderFont, textWidth);
            currentY += (int)senderSize.Height + 5;

            // Текст сообщения
            using (var brush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(msg.Text, messageTextFont,
                    brush, new RectangleF(x + padding, currentY, textWidth, 1000));
            }
            SizeF textSize = e.Graphics.MeasureString(msg.Text, messageTextFont, textWidth);
            currentY += (int)textSize.Height + 5;

            // Время
            string timeStr = msg.SentAt.ToString("HH:mm");
            using (var brush = new SolidBrush(Color.Gray))
            {
                SizeF timeSize = e.Graphics.MeasureString(timeStr, messageTimeFont);
                float timeX = bubbleRect.Right - padding - timeSize.Width;
                float timeY = bubbleRect.Bottom - timeSize.Height - 5;
                e.Graphics.DrawString(timeStr, messageTimeFont, brush, timeX, timeY);
            }
        }

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
            var data = new Dictionary<string, int> { { "chatId", chatId }, { "lastReadMessageId", lastReadId } };
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.MessagesRead, UserId = currentUser.Id, Data = data });
        }

        private void UpdateCurrentChatHeader()
        {
            if (currentChat == null) return;

            string chatName = currentChat.Name;
            string chatDepartment = "";

            if (currentChat.Type == ChatType.Private)
            {
                var other = currentChat.Participants?.FirstOrDefault(p => p.Id != currentUser.Id);
                if (other != null)
                {
                    chatName = string.IsNullOrEmpty(other.Position) ? other.FullName : $"{other.FullName} ({other.Position})";
                    chatDepartment = other.Department ?? "";
                    string status = other.IsOnline ? "● Онлайн" : "● Офлайн";
                    lblChatInfo.Text = $"Личный чат • {status}";
                }
                else
                {
                    lblChatInfo.Text = "Личный чат";
                }
            }
            else if (currentChat.Type == ChatType.Group)
            {
                int onlineCount = currentChat.Participants?.Count(p => p.IsOnline) ?? 0;
                int totalParticipants = currentChat.Participants?.Count ?? 0;
                lblChatInfo.Text = $"Групповой чат • {totalParticipants} уч. • {onlineCount} онлайн";
            }
            else // Department
            {
                int onlineCount = currentChat.Participants?.Count(p => p.IsOnline) ?? 0;
                int totalParticipants = currentChat.Participants?.Count ?? 0;
                lblChatInfo.Text = $"Чат отдела • {totalParticipants} уч. • {onlineCount} онлайн";
            }

            btnManageParticipants.Visible = (currentUser != null && currentUser.IsAdmin &&
                (currentChat.Type == ChatType.Department || currentChat.Type == ChatType.Group));

            lblChatName.Text = chatName;
            lblChatDepartment.Text = chatDepartment;
        }

        private void BtnManageParticipants_Click(object sender, EventArgs e)
        {
            if (currentChat?.Type == ChatType.Department || currentChat?.Type == ChatType.Group)
            {
                using (var form = new ManageParticipantsForm(currentChat.Id, currentUser.Id, networkClient))
                {
                    form.ShowDialog();
                }
            }
        }

        private void LstMessages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lstMessages.SelectedItem != null)
            {
                if (lstMessages.SelectedItem is Shared.Message msg)
                {
                    Console.WriteLine($"DeleteMessage: selected message Id={msg.Id}, SenderId={msg.SenderId}, ChatId={msg.ChatId}");

                    if (msg.SenderId == currentUser.Id || currentUser.IsAdmin)
                    {
                        var result = MessageBox.Show("Удалить сообщение?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            networkClient.SendPacket(new NetworkPacket
                            {
                                Command = Shared.CommandType.DeleteMessage,
                                Data = msg.Id
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("Вы можете удалять только свои сообщения.", "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите сообщение для удаления.", "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                e.Handled = true;
            }
        }

        private void HandleMessageDeleted(int messageId)
        {
            foreach (var kv in messages)
            {
                var msgList = kv.Value;
                var msg = msgList.FirstOrDefault(m => m.Id == messageId);
                if (msg != null)
                {
                    msgList.Remove(msg);
                    if (kv.Key == currentChat?.Id)
                    {
                        DisplayMessages();
                    }
                    break;
                }
            }
        }

        private void ApplySearchFilter()
        {
            string search = txtSearchChats.Text.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(search) || search == "поиск чатов...")
            {
                // Если поиск пуст, показываем все чаты (уже отображены)
                return;
            }

            // Фильтруем список чатов по имени
            var filtered = chats.Where(c => c.Name.ToLower().Contains(search)).ToList();

            lstChats.BeginUpdate();
            try
            {
                lstChats.Items.Clear();
                foreach (var chat in filtered)
                    lstChats.Items.Add(chat);
            }
            finally
            {
                lstChats.EndUpdate();
            }
        }

        private void LstMessages_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || lstMessages.Items[e.Index] == null) return;

            // Разделитель даты (строка)
            if (lstMessages.Items[e.Index] is string)
            {
                e.ItemHeight = 30;
                return;
            }

            // Сообщение
            if (lstMessages.Items[e.Index] is Shared.Message msg)
            {
                const int maxWidth = 400;          // ширина "пузыря"
                const int padding = 10;             // отступы внутри пузыря
                int textWidth = maxWidth - 2 * padding;

                int y = 5; // верхний отступ

                // Имя отправителя
                string senderDisplay = string.IsNullOrEmpty(msg.SenderDepartment)
                    ? msg.SenderName
                    : $"{msg.SenderName} ({msg.SenderDepartment})";
                SizeF senderSize = e.Graphics.MeasureString(senderDisplay, messageSenderFont, textWidth);
                y += (int)senderSize.Height + 5;

                // Текст сообщения
                SizeF textSize = e.Graphics.MeasureString(msg.Text, messageTextFont, textWidth);
                y += (int)textSize.Height + 5;

                // Время
                string timeStr = msg.SentAt.ToString("HH:mm");
                SizeF timeSize = e.Graphics.MeasureString(timeStr, messageTimeFont);
                y += (int)timeSize.Height + 5;

                e.ItemHeight = y;
            }
        }

        private void TxtMessage_TextChanged(object sender, EventArgs e)
        {
            // Вычисляем требуемую высоту поля с учётом переноса текста
            int width = txtMessage.ClientSize.Width;
            if (width <= 0) return;

            Size proposedSize = new Size(width, int.MaxValue);
            TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
            Size textSize = TextRenderer.MeasureText(txtMessage.Text, txtMessage.Font, proposedSize, flags);

            int newHeight = textSize.Height + 8; // небольшой отступ
            const int minHeight = 40;
            const int maxHeight = 150;

            if (newHeight < minHeight) newHeight = minHeight;
            if (newHeight > maxHeight) newHeight = maxHeight;

            if (txtMessage.Height != newHeight)
            {
                // Изменяем высоту поля
                txtMessage.Height = newHeight;

                // Изменяем высоту панели (учитываем отступы: 10 сверху + 10 снизу)
                int panelNewHeight = newHeight + 20;
                panelMessageInput.Height = panelNewHeight;

                // Перемещаем кнопку по вертикали в центр панели
                btnSend.Top = (panelNewHeight - btnSend.Height) / 2;
            }
        }
    }
}
