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
            networkClient.OnPacketReceived += OnPacketReceived;

            LoadData();
        }

        private void LoadData()
        {
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetChatInfo, Data = chatId });
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetAvailableUsers, Data = currentUserId });
        }

        private void OnPacketReceived(NetworkPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NetworkPacket>(OnPacketReceived), packet);
                return;
            }

            switch (packet.Command)
            {
                case Shared.CommandType.ChatInfo:
                    currentChat = JsonSerializer.Deserialize<Chat>(packet.Data.ToString());
                    participants = currentChat.Participants;
                    UpdateParticipantsList();
                    break;
                case Shared.CommandType.AvailableUsersList:
                    allUsers = JsonSerializer.Deserialize<List<User>>(packet.Data.ToString());
                    UpdateAvailableUsers();
                    break;
                case Shared.CommandType.ChatUpdated:
                    // Обновить данные после изменений
                    LoadData();
                    break;
            }
        }

        private void UpdateParticipantsList()
        {
            lstParticipants.Items.Clear();
            foreach (var user in participants.OrderBy(u => u.FullName))
                lstParticipants.Items.Add(user.FullName);
        }

        private void UpdateAvailableUsers()
        {
            if (participants == null) return;
            var available = allUsers.Where(u => !participants.Any(p => p.Id == u.Id)).ToList();
            lstAvailable.Items.Clear();
            foreach (var user in available)
                lstAvailable.Items.Add(user.FullName);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (lstAvailable.SelectedItem == null) return;
            var selectedName = lstAvailable.SelectedItem.ToString();
            var user = allUsers.First(u => u.FullName == selectedName);
            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.AddChatParticipant,
                Data = new { chatId, userId = user.Id }
            });
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstParticipants.SelectedItem == null) return;
            var selectedName = lstParticipants.SelectedItem.ToString();
            var user = participants.First(u => u.FullName == selectedName);
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
}
