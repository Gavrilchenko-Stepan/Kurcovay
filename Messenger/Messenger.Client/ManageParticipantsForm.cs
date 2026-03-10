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
                        UpdateAvailableUsers();
                        break;

                    case Shared.CommandType.ChatUpdated:
                        // После изменений обновляем данные
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
                    lstParticipants.Items.Add(user.FullName);
            }
        }

        private void UpdateAvailableUsers()
        {
            lstAvailable.Items.Clear();
            if (allUsers == null || participants == null) return;

            var available = allUsers.Where(u => !participants.Any(p => p.Id == u.Id)).ToList();
            foreach (var user in available.OrderBy(u => u.FullName))
                lstAvailable.Items.Add(user.FullName);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (lstAvailable.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для добавления.");
                return;
            }

            var selectedName = lstAvailable.SelectedItem.ToString();
            var user = allUsers.FirstOrDefault(u => u.FullName == selectedName);
            if (user == null) return;

            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.AddChatParticipant,
                Data = new { chatId, userId = user.Id }
            });
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstParticipants.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.");
                return;
            }

            var selectedName = lstParticipants.SelectedItem.ToString();
            var user = participants.FirstOrDefault(u => u.FullName == selectedName);
            if (user == null) return;

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

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Отписка от события при закрытии формы (предотвращает утечки памяти)
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            networkClient.OnPacketReceived -= OnPacketReceived;
            base.OnFormClosing(e);
        }
    }
}
