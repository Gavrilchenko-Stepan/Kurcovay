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
            lstChats.DrawMode = DrawMode.Normal;
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
            lstMessages.DrawMode = DrawMode.Normal; // временно, чтобы проверить
            lstMessages.DrawItem -= LstMessages_DrawItem; // отписываемся, если подписка была
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
                    refreshTimer = new Timer { Interval = 30000 };
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
                            messages[msgs.First().ChatId] = msgs;
                            if (currentChat != null && msgs.First().ChatId == currentChat.Id)
                                DisplayMessages();
                        }
                        break;
                    case Shared.CommandType.NewMessage:
                        var jsonElemNewMsg = (JsonElement)packet.Data;
                        string jsonNewMsg = jsonElemNewMsg.GetRawText();
                        var newMsg = JsonSerializer.Deserialize<Shared.Message>(jsonNewMsg);
                        Console.WriteLine($"Получено новое сообщение: {newMsg.Text} в чат {newMsg.ChatId}");
                        HandleNewMessage(newMsg);
                        break;
                    case Shared.CommandType.UserStatusChanged:
                        var jsonElemUser = (JsonElement)packet.Data;
                        string jsonUser = jsonElemUser.GetRawText();
                        var user = JsonSerializer.Deserialize<User>(jsonUser);
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
            lstChats.Items.Clear();
            var sorted = chats.OrderByDescending(c => c.LastMessageTime).ToList();
            foreach (var chat in sorted)
            {
                lstChats.Items.Add(chat);
                Console.WriteLine($"Добавлен в lstChats: {chat.Name}");
            }
            lstChats.Refresh();
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

            // Вместо switch-выражения используем if-else или обычный switch
            if (chat.Type == ChatType.Private)
                lblChatInfo.Text = "Личный чат";
            else if (chat.Type == ChatType.Group)
                lblChatInfo.Text = $"Групповой чат • {chat.Participants.Count} уч.";
            else
                lblChatInfo.Text = $"Чат • {chat.Participants.Count} уч.";

            chat.UnreadCount = 0;
            UpdateChatsList();
            btnSend.Enabled = true;

            if (!messages.ContainsKey(chat.Id))
                networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetMessages, UserId = currentUser.Id, Data = chat.Id });
            else
                DisplayMessages();
        }

        private void DisplayMessages()
        {
            lstMessages.Items.Clear();
            if (messages.ContainsKey(currentChat.Id))
            {
                foreach (var msg in messages[currentChat.Id])
                    lstMessages.Items.Add(msg);
                if (lstMessages.Items.Count > 0)
                    lstMessages.TopIndex = lstMessages.Items.Count - 1;
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
                }
            }
            Console.WriteLine($"Сообщение добавлено в список: {msg.Text}");
            lstMessages.Items.Add(msg);
            lstMessages.Refresh();
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
            lstChats.Invalidate();
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

            // аватар-круг
            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 10, e.Bounds.Y + 10, 40, 40);
            }

            int x = e.Bounds.X + 60;
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(chat.Name, font, Brushes.White, x, e.Bounds.Y + 15);

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
            if (e.Index < 0 || !(lstMessages.Items[e.Index] is Shared.Message msg)) return;
            e.DrawBackground();

            bool isMy = msg.SenderId == currentUser.Id;
            int maxWidth = 400;
            int x = isMy ? e.Bounds.Width - maxWidth - 20 : e.Bounds.X + 20;

            Color bgColor = isMy ? Color.FromArgb(0, 229, 255, 80) : Color.FromArgb(60, 60, 80);
            Color borderColor = isMy ? Color.FromArgb(0, 229, 255) : Color.Gray;

            Rectangle msgRect = new Rectangle(x, e.Bounds.Y + 2, maxWidth, e.Bounds.Height - 4);
            using (var brush = new SolidBrush(bgColor))
            using (var pen = new Pen(borderColor))
            {
                e.Graphics.FillRectangle(brush, msgRect);
                e.Graphics.DrawRectangle(pen, msgRect);
            }

            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                e.Graphics.DrawString(msg.SenderName, font, Brushes.White, x + 5, e.Bounds.Y + 5);
            using (var font = new Font("Segoe UI", 10))
                e.Graphics.DrawString(msg.Text, font, Brushes.White, x + 5, e.Bounds.Y + 25);
            string tm = msg.SentAt.ToString("HH:mm");
            using (var font = new Font("Segoe UI", 8))
            {
                var sz = e.Graphics.MeasureString(tm, font);
                e.Graphics.DrawString(tm, font, Brushes.Gray, x + maxWidth - sz.Width - 10, e.Bounds.Y + 45);
            }
            e.DrawFocusRectangle();
        }
    }
}
