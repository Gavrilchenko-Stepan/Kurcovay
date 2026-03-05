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
    public partial class NewChatForm : Form
    {
        private int currentUserId;
        private string currentDepartment;
        private NetworkClient networkClient;
        private List<Department> departments = new List<Department>();
        private List<User> availableUsers = new List<User>();
        private ImageList avatarImageList;

        public NewChatForm(int userId, string department, NetworkClient client)
        {
            InitializeComponent();
            ApplyFuturisticStyle();
            currentUserId = userId;
            currentDepartment = department;
            networkClient = client;
            networkClient.OnPacketReceived += OnPacketReceived;
            this.Load += NewChatForm_Load;
            picSearch.Paint += PicSearch_Paint;

            btnCreate.Click += BtnCreate_Click;
            btnCancel.Click += BtnCancel_Click;
            lstDepartments.SelectedIndexChanged += LstDepartments_SelectedIndexChanged;
            lvPrivateUsers.SelectedIndexChanged += LvPrivateUsers_SelectedIndexChanged;
            lvGroupUsers.ItemChecked += LvGroupUsers_ItemChecked;
            txtChatName.TextChanged += TxtChatName_TextChanged;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            avatarImageList = new ImageList();
            avatarImageList.ImageSize = new Size(40, 40);
            avatarImageList.ColorDepth = ColorDepth.Depth32Bit;
        }

        private void ApplyFuturisticStyle()
        {
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.ForeColor = Color.White;
            panelMain.BackColor = Color.FromArgb(45, 45, 58);
            panelHeader.BackColor = Color.FromArgb(20, 20, 30);
            lblTitle.ForeColor = Color.FromArgb(0, 229, 255);
            lblSubtitle.ForeColor = Color.FromArgb(180, 180, 200);
            tabControl.BackColor = Color.FromArgb(45, 45, 58);
            tabControl.ForeColor = Color.White;
            txtSearch.BackColor = Color.FromArgb(60, 60, 80);
            txtSearch.ForeColor = Color.White;
            lstDepartments.BackColor = Color.FromArgb(60, 60, 80);
            lstDepartments.ForeColor = Color.White;
            txtChatName.BackColor = Color.FromArgb(60, 60, 80);
            txtChatName.ForeColor = Color.White;
            btnCreate.BackColor = Color.FromArgb(0, 229, 255);
            btnCreate.ForeColor = Color.Black;
            btnCancel.BackColor = Color.Transparent;
            btnCancel.ForeColor = Color.FromArgb(0, 229, 255);
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
        }

        private void NewChatForm_Load(object sender, EventArgs e)
        {
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetDepartments });
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetAvailableUsers, Data = currentUserId });
        }

        private void OnPacketReceived(NetworkPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NetworkPacket>(OnPacketReceived), packet);
                return;
            }

            Console.WriteLine($"NewChatForm получил: {packet.Command}");

            switch (packet.Command)
            {
                case Shared.CommandType.DepartmentsList:
                    var jsonDept = (JsonElement)packet.Data;
                    string json = jsonDept.GetRawText();
                    departments = JsonSerializer.Deserialize<List<Department>>(json);
                    UpdateDepartmentsList();
                    break;
                case Shared.CommandType.AvailableUsersList:
                    var jsonUsers = (JsonElement)packet.Data;
                    string usersJson = jsonUsers.GetRawText();
                    availableUsers = JsonSerializer.Deserialize<List<User>>(usersJson);
                    UpdateUsersList();
                    break;
                case Shared.CommandType.ChatCreated:
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
            }
        }

        private void UpdateDepartmentsList()
        {
            lstDepartments.Items.Clear();
            foreach (var dept in departments.Where(d => d.Name != currentDepartment))
                lstDepartments.Items.Add(dept);
            if (lstDepartments.Items.Count > 0)
                lstDepartments.SelectedIndex = 0;
            UpdateCreateButton();
        }

        private void UpdateUsersList()
        {
            lvPrivateUsers.Items.Clear();
            lvGroupUsers.Items.Clear();

            foreach (var user in availableUsers.Where(u => u.Id != currentUserId))
            {
                var itemPrivate = new ListViewItem(user.FullName);
                itemPrivate.Tag = user;
                lvPrivateUsers.Items.Add(itemPrivate);

                var itemGroup = new ListViewItem(user.FullName);
                itemGroup.Tag = user;
                lvGroupUsers.Items.Add(itemGroup);
            }

            UpdateCreateButton();
        }

        private void UpdateCreateButton()
        {
            if (tabControl.SelectedTab == tabDepartment)
                btnCreate.Enabled = lstDepartments.SelectedItem != null;
            else if (tabControl.SelectedTab == tabPrivate)
                btnCreate.Enabled = lvPrivateUsers.SelectedItems.Count > 0;
            else
                btnCreate.Enabled = !string.IsNullOrWhiteSpace(txtChatName.Text) && lvGroupUsers.CheckedItems.Count > 0;
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e) => UpdateCreateButton();
        private void LstDepartments_SelectedIndexChanged(object sender, EventArgs e) => UpdateCreateButton();
        private void LvPrivateUsers_SelectedIndexChanged(object sender, EventArgs e) => UpdateCreateButton();
        private void LvGroupUsers_ItemChecked(object sender, ItemCheckedEventArgs e) => UpdateCreateButton();
        private void TxtChatName_TextChanged(object sender, EventArgs e) => UpdateCreateButton();

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabDepartment && lstDepartments.SelectedItem != null)
            {
                var dept = (Department)lstDepartments.SelectedItem;
                var participants = availableUsers.Where(u => u.DepartmentId == dept.Id).Select(u => u.Id).ToList();
                if (!participants.Contains(currentUserId))
                    participants.Add(currentUserId);
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.CreateGroupChat,
                    Data = new { name = dept.Name, participants }
                });
            }
            else if (tabControl.SelectedTab == tabPrivate && lvPrivateUsers.SelectedItems.Count > 0)
            {
                var user = (User)lvPrivateUsers.SelectedItems[0].Tag;
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.CreatePrivateChat,
                    Data = new { otherUserId = user.Id }
                });
            }
            else if (tabControl.SelectedTab == tabGroup)
            {
                var participants = new List<int>();
                foreach (ListViewItem item in lvGroupUsers.CheckedItems)
                {
                    if (item.Tag is User u)
                        participants.Add(u.Id);
                }
                participants.Add(currentUserId);
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.CreateGroupChat,
                    Data = new { name = txtChatName.Text, participants }
                });
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e) => Close();

        private void PicSearch_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.Gray, 2))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawEllipse(pen, 2, 2, 12, 12);
                e.Graphics.DrawLine(pen, 11, 11, 16, 16);
            }
        }
    }
}
