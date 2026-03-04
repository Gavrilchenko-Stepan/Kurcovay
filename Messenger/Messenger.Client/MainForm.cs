using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Messenger.Shared;

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
            InitializeCustomStyles();
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeCustomStyles()
        {
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);

            // Настройка списков
            lstChats.DrawMode = DrawMode.OwnerDrawFixed;
            lstChats.DrawItem += LstChats_DrawItem;
            lstChats.ItemHeight = 70;
            lstChats.SelectedIndexChanged += LstChats_SelectedIndexChanged;

            lstMessages.DrawMode = DrawMode.OwnerDrawFixed;
            lstMessages.DrawItem += LstMessages_DrawItem;
            lstMessages.ItemHeight = 70;

            // Подписка на события кнопок
            btnSend.Click += BtnSend_Click;
            btnNewChat.Click += BtnNewChat_Click;
            btnLogout.Click += BtnLogout_Click;
            btnSettings.Click += BtnSettings_Click;

            txtMessage.KeyDown += TxtMessage_KeyDown;
            txtSearchChats.TextChanged += TxtSearchChats_TextChanged;
            txtSearchChats.Enter += TxtSearchChats_Enter;
            txtSearchChats.Leave += TxtSearchChats_Leave;

            // Таймер для обновления статусов
            refreshTimer = new Timer();
            refreshTimer.Interval = 30000; // 30 секунд
            refreshTimer.Tick += RefreshTimer_Tick;
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
                var result = MessageBox.Show("Вы действительно хотите выйти из приложения?",
                    "Подтверждение выхода",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            refreshTimer?.Stop();
            networkClient?.Disconnect();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (networkClient != null && networkClient.IsConnected)
            {
                LoadChats();
            }
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
            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.GetChats,
                UserId = currentUser.Id
            });
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
                        chats = JsonSerializer.Deserialize<List<Chat>>(packet.Data.ToString());
                        UpdateChatsList();
                        UpdateTotalUsers();
                        break;

                    case Shared.CommandType.MessagesList:
                        var msgs = JsonSerializer.Deserialize<List<Shared.Message>>(packet.Data.ToString());
                        if (msgs.Any())
                        {
                            messages[msgs.First().ChatId] = msgs;
                            if (currentChat != null && msgs.First().ChatId == currentChat.Id)
                            {
                                DisplayMessages();
                            }
                        }
                        break;

                    case Shared.CommandType.NewMessage:
                        var newMsg = JsonSerializer.Deserialize<Shared.Message>(packet.Data.ToString());
                        HandleNewMessage(newMsg);
                        break;

                    case Shared.CommandType.UserStatusChanged:
                        var user = JsonSerializer.Deserialize<User>(packet.Data.ToString());
                        UpdateUserStatus(user);
                        break;

                    case Shared.CommandType.ChatCreated:
                        var newChat = JsonSerializer.Deserialize<Chat>(packet.Data.ToString());
                        chats.Add(newChat);
                        UpdateChatsList();
                        break;

                    case Shared.CommandType.AvailableUsersList:
                        var users = JsonSerializer.Deserialize<List<User>>(packet.Data.ToString());
                        // Здесь можно обработать список пользователей для NewChatForm
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки пакета: {ex.Message}");
            }
        }

        private void OnDisconnected()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnDisconnected));
                return;
            }

            lblConnectionStatus.Text = "● Отключено от сервера";
            lblConnectionStatus.ForeColor = Color.Red;
            btnSend.Enabled = false;
            refreshTimer.Stop();

            MessageBox.Show("Соединение с сервером потеряно!", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateChatsList()
        {
            lstChats.Items.Clear();
            var sortedChats = chats.OrderByDescending(c => c.LastMessageTime).ToList();
            foreach (var chat in sortedChats)
            {
                lstChats.Items.Add(chat);
            }
        }

        private void UpdateTotalUsers()
        {
            var allUsers = new List<int>();
            foreach (var chat in chats)
            {
                foreach (var user in chat.Participants)
                {
                    if (!allUsers.Contains(user.Id))
                        allUsers.Add(user.Id);
                }
            }
            lblTotalUsers.Text = $"Всего пользователей: {allUsers.Count}";
        }

        private void LstChats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(lstChats.SelectedItem is Chat chat)) return;

            currentChat = chat;
            lblChatName.Text = chat.Name;

            if (chat.Type == ChatType.Department)
            {
                lblChatInfo.Text = $"Внутренний чат • {chat.Participants.Count} участников";
            }
            else if (chat.Type == ChatType.Private)
            {
                lblChatInfo.Text = $"Личный чат";
            }
            else
            {
                lblChatInfo.Text = $"Групповой чат • {chat.Participants.Count} участников";
            }

            chat.UnreadCount = 0;
            UpdateChatsList();
            btnSend.Enabled = true;

            if (!messages.ContainsKey(chat.Id))
            {
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.GetMessages,
                    UserId = currentUser.Id,
                    Data = chat.Id
                });
            }
            else
            {
                DisplayMessages();
            }
        }

        private void DisplayMessages()
        {
            lstMessages.Items.Clear();
            if (messages.ContainsKey(currentChat.Id))
            {
                foreach (var msg in messages[currentChat.Id])
                {
                    lstMessages.Items.Add(msg);
                }

                if (lstMessages.Items.Count > 0)
                    lstMessages.TopIndex = lstMessages.Items.Count - 1;
            }
        }

        private void LstChats_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(lstChats.Items[e.Index] is Chat chat)) return;

            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = selected ? Color.FromArgb(230, 242, 255) : Color.White;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            string icon = chat.Type == ChatType.Department ? "🏢" :
                         (chat.Type == ChatType.Private ? "👤" : "👥");

            using (var font = new Font("Segoe UI", 16))
                e.Graphics.DrawString(icon, font, Brushes.Gray, e.Bounds.X + 10, e.Bounds.Y + 15);

            int x = e.Bounds.X + 50;
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(chat.Name, font, Brushes.Black, x, e.Bounds.Y + 10);

            if (!string.IsNullOrEmpty(chat.LastMessage))
            {
                string lastMsg = chat.LastMessage.Length > 30 ?
                    chat.LastMessage.Substring(0, 27) + "..." : chat.LastMessage;
                using (var font = new Font("Segoe UI", 9))
                    e.Graphics.DrawString(lastMsg, font, Brushes.Gray, x, e.Bounds.Y + 35);
            }

            if (chat.LastMessageTime > DateTime.MinValue)
            {
                string time = chat.LastMessageTime.ToString("HH:mm");
                using (var font = new Font("Segoe UI", 8))
                {
                    var size = e.Graphics.MeasureString(time, font);
                    e.Graphics.DrawString(time, font, Brushes.Gray,
                        e.Bounds.Right - size.Width - 20, e.Bounds.Y + 10);
                }
            }

            if (chat.UnreadCount > 0)
            {
                string count = chat.UnreadCount.ToString();
                using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                {
                    var rect = new Rectangle(e.Bounds.Right - 30, e.Bounds.Y + 35, 20, 20);
                    using (var brush = new SolidBrush(Color.FromArgb(244, 67, 54)))
                        e.Graphics.FillEllipse(brush, rect);

                    var countSize = e.Graphics.MeasureString(count, font);
                    e.Graphics.DrawString(count, font, Brushes.White,
                        rect.X + (20 - countSize.Width) / 2, rect.Y + 2);
                }
            }

            e.DrawFocusRectangle();
        }

        private void LstMessages_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(lstMessages.Items[e.Index] is Shared.Message msg)) return;

            e.DrawBackground();

            bool isMyMessage = msg.SenderId == currentUser.Id;

            int maxWidth = 400;
            int x = isMyMessage ? e.Bounds.Width - maxWidth - 20 : e.Bounds.X + 20;

            Color bgColor = isMyMessage ? Color.FromArgb(220, 248, 220) : Color.White;
            Color borderColor = isMyMessage ? Color.FromArgb(76, 175, 80) : Color.LightGray;

            var msgRect = new Rectangle(x, e.Bounds.Y + 2, maxWidth, e.Bounds.Height - 4);

            using (var brush = new SolidBrush(bgColor))
            using (var pen = new Pen(borderColor))
            {
                e.Graphics.FillRectangle(brush, msgRect);
                e.Graphics.DrawRectangle(pen, msgRect);
            }

            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                e.Graphics.DrawString(msg.SenderName, font, Brushes.Black, x + 5, e.Bounds.Y + 5);

            using (var font = new Font("Segoe UI", 10))
                e.Graphics.DrawString(msg.Text, font, Brushes.Black, x + 5, e.Bounds.Y + 25);

            string time = msg.SentAt.ToString("HH:mm");
            using (var font = new Font("Segoe UI", 8))
            {
                var size = e.Graphics.MeasureString(time, font);
                e.Graphics.DrawString(time, font, Brushes.Gray,
                    x + maxWidth - size.Width - 10, e.Bounds.Y + 45);
            }

            e.DrawFocusRectangle();
        }

        private void HandleNewMessage(Shared.Message message)
        {
            if (!messages.ContainsKey(message.ChatId))
                messages[message.ChatId] = new List<Shared.Message>();

            messages[message.ChatId].Add(message);

            var chat = chats.FirstOrDefault(c => c.Id == message.ChatId);
            if (chat != null)
            {
                chat.LastMessage = message.Text;
                chat.LastMessageTime = message.SentAt;

                if (currentChat?.Id != message.ChatId)
                {
                    chat.UnreadCount++;
                }

                UpdateChatsList();

                if (currentChat?.Id == message.ChatId)
                {
                    lstMessages.Items.Add(message);
                    lstMessages.TopIndex = lstMessages.Items.Count - 1;
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        private void UpdateUserStatus(User user)
        {
            foreach (var chat in chats)
            {
                var participant = chat.Participants?.FirstOrDefault(p => p.Id == user.Id);
                if (participant != null)
                {
                    participant.IsOnline = user.IsOnline;
                    participant.LastSeen = user.LastSeen;
                }
            }
            lstChats.Invalidate();
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text) || currentChat == null)
                return;

            var message = new Shared.Message
            {
                ChatId = currentChat.Id,
                SenderId = currentUser.Id,
                SenderName = currentUser.FullName,
                Text = txtMessage.Text.Trim(),
                SentAt = DateTime.Now
            };

            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.SendMessage,
                UserId = currentUser.Id,
                Data = message
            });

            txtMessage.Clear();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Control)
            {
                SendMessage();
                e.SuppressKeyPress = true;
            }
        }

        private void BtnNewChat_Click(object sender, EventArgs e)
        {
            using (var newChatForm = new NewChatForm(currentUser.Id, currentUser.Department))
            {
                // Запрашиваем список пользователей для чата
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.GetAvailableUsers,
                    Data = currentUser.Id
                });

                // Здесь нужно будет обработать ответ и передать данные в форму
                newChatForm.ShowDialog();
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Настройки будут доступны в следующей версии", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                refreshTimer.Stop();
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
            string searchText = txtSearchChats.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "поиск чатов...")
            {
                UpdateChatsList();
                return;
            }

            var filtered = chats.Where(c => c.Name.ToLower().Contains(searchText)).ToList();
            lstChats.Items.Clear();
            foreach (var chat in filtered)
            {
                lstChats.Items.Add(chat);
            }
        }

        private void TxtSearchChats_Enter(object sender, EventArgs e)
        {
            if (txtSearchChats.Text == "Поиск чатов...")
            {
                txtSearchChats.Text = "";
                txtSearchChats.ForeColor = Color.Black;
            }
        }

        private void TxtSearchChats_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchChats.Text))
            {
                txtSearchChats.Text = "Поиск чатов...";
                txtSearchChats.ForeColor = Color.Gray;
            }
        }
    }
}
