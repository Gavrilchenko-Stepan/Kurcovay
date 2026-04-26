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
    public partial class ViewParticipantsForm : Form
    {
        private int chatId;
        private int currentUserId;
        private NetworkClient networkClient;
        private Chat currentChat;

        public ViewParticipantsForm(int chatId, int currentUserId, NetworkClient networkClient)
        {
            InitializeComponent();
            this.chatId = chatId;
            this.currentUserId = currentUserId;
            this.networkClient = networkClient;
            this.networkClient.OnPacketReceived += OnPacketReceived;
            this.Load += ViewParticipantsForm_Load;
        }

        private void ViewParticipantsForm_Load(object sender, EventArgs e)
        {
            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.GetChatInfo,
                Data = chatId
            });
        }

        private void OnPacketReceived(NetworkPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NetworkPacket>(OnPacketReceived), packet);
                return;
            }

            if (packet.Command == Shared.CommandType.ChatInfo)
            {
                var jsonElem = (JsonElement)packet.Data;
                string json = jsonElem.GetRawText();
                currentChat = JsonSerializer.Deserialize<Chat>(json);
                if (currentChat != null)
                {
                    currentChat.Participants = currentChat.Participants ?? new List<User>();
                    UpdateParticipantsList();
                }
            }
        }

        private void UpdateParticipantsList()
        {
            listViewParticipants.Items.Clear();
            foreach (var user in currentChat.Participants.OrderBy(u => u.FullName))
            {
                string status = user.IsOnline ? "● Онлайн" : "● Офлайн";
                Color statusColor = user.IsOnline ? Color.Green : Color.Gray;
                ListViewItem item = new ListViewItem(user.FullName);
                item.SubItems.Add(user.Position ?? "");
                item.SubItems.Add(status);
                item.Tag = user;
                item.ForeColor = statusColor;
                listViewParticipants.Items.Add(item);
            }

            // Информация о чате
            lblChatName.Text = currentChat.Name;
            lblParticipantCount.Text = $"Участников: {currentChat.Participants.Count}";
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            networkClient.OnPacketReceived -= OnPacketReceived;
            base.OnFormClosing(e);
        }
    }
}
