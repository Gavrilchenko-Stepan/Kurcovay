using Messenger.Shared;
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

namespace Messenger.Client
{
    public partial class ManageParticipantsForm : Form
    {
        private int chatId;
        private int currentUserId;
        private NetworkClient networkClient;
        private Chat currentChat;
        private List<User> allUsers;
        private List<User> participants;

        public ManageParticipantsForm(int chatId, int currentUserId, NetworkClient client)
        {
            InitializeComponent();
            this.chatId = chatId;
            this.currentUserId = currentUserId;
            this.networkClient = client;
            this.networkClient.OnPacketReceived += OnPacketReceived;

            // Подписка на события кнопок
            this.btnAdd.Click += BtnAdd_Click;
            this.btnRemove.Click += BtnRemove_Click;
            this.btnClose.Click += BtnClose_Click;

            // Настройка прокрутки для списка участников
            lstParticipants.IntegralHeight = false;
            lstParticipants.ScrollAlwaysVisible = true;
            lstParticipants.HorizontalScrollbar = true;

            LoadData();
        }

        private void LoadData()
        {
            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.GetChatInfo,
                Data = chatId
            });
            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.GetAvailableUsers,
                Data = currentUserId
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
                    case Shared.CommandType.ChatInfo:
                        var jsonElem = (JsonElement)packet.Data;
                        string json = jsonElem.GetRawText();
                        currentChat = JsonSerializer.Deserialize<Chat>(json);
                        participants = currentChat.Participants;
                        UpdateParticipantsList();
                        break;

                    case Shared.CommandType.AvailableUsersList:
                        var jsonElem2 = (JsonElement)packet.Data;
                        string json2 = jsonElem2.GetRawText();
                        allUsers = JsonSerializer.Deserialize<List<User>>(json2);
                        UpdateAvailableTree();
                        break;

                    case Shared.CommandType.ChatUpdated:
                        LoadData();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки пакета: {ex.Message}");
            }
        }

        private void UpdateParticipantsList()
        {
            lstParticipants.Items.Clear();
            if (participants != null)
            {
                foreach (var user in participants.OrderBy(u => u.FullName))
                {
                    string display = string.IsNullOrEmpty(user.Position)
                        ? user.FullName
                        : $"{user.FullName} ({user.Position})";
                    lstParticipants.Items.Add(display);
                    // можно хранить объект через Tag, но ListBox неудобен; оставим пока как строки
                    // для удаления потом будем искать по имени, но это не очень надёжно. Лучше хранить объект.
                    // Временно так; позже можно добавить свойство для хранения User.
                }
            }
        }

        private void UpdateAvailableTree()
        {
            tvAvailable.Nodes.Clear();
            if (allUsers == null || participants == null) return;

            // Исключаем уже состоящих в чате
            var available = allUsers.Where(u => !participants.Any(p => p.Id == u.Id)).ToList();

            // Группируем по отделам
            var usersByDept = available.GroupBy(u => u.Department).OrderBy(g => g.Key);

            foreach (var group in usersByDept)
            {
                TreeNode deptNode = new TreeNode(group.Key);
                deptNode.ForeColor = Color.White;
                foreach (var user in group.OrderBy(u => u.FullName))
                {
                    string display = string.IsNullOrEmpty(user.Position)
                        ? user.FullName
                        : $"{user.FullName} ({user.Position})";
                    TreeNode userNode = new TreeNode(display);
                    userNode.Tag = user; // сохраняем объект User
                    userNode.ForeColor = Color.White;
                    deptNode.Nodes.Add(userNode);
                }
                tvAvailable.Nodes.Add(deptNode);
            }
            tvAvailable.ExpandAll();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (tvAvailable.SelectedNode?.Tag is User user)
            {
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.AddChatParticipant,
                    Data = new { chatId, userId = user.Id }
                });
            }
            else
            {
                MessageBox.Show("Выберите пользователя для добавления.");
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstParticipants.SelectedItem != null)
            {
                // Пока удаляем по имени – это временное решение, пока не храним объекты.
                // Лучше хранить в lstParticipants объекты User, используя свойство DisplayMember.
                // Для простоты оставим так, но потом можно доработать.
                string selectedName = lstParticipants.SelectedItem.ToString();
                var user = participants.FirstOrDefault(u => u.FullName == selectedName);
                if (user != null)
                {
                    if (user.Id == currentUserId)
                    {
                        MessageBox.Show("Нельзя удалить себя из чата.");
                        return;
                    }
                    networkClient.SendPacket(new NetworkPacket
                    {
                        Command = Shared.CommandType.RemoveChatParticipant,
                        Data = new { chatId, userId = user.Id }
                    });
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.");
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TvAvailable_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is User user)
            {
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.AddChatParticipant,
                    Data = new { chatId, userId = user.Id }
                });
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            networkClient.OnPacketReceived -= OnPacketReceived;
            base.OnFormClosing(e);
        }
    }
}
