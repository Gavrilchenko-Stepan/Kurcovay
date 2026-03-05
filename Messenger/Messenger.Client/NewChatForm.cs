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

        public NewChatForm(int userId, string department, NetworkClient client)
        {
            InitializeComponent();
            lstDepartments.DrawMode = DrawMode.Normal;
            lstUsers.DrawMode = DrawMode.Normal;
            chkUsers.DrawMode = DrawMode.Normal;
            ApplyFuturisticStyle();
            currentUserId = userId;
            currentDepartment = department;
            networkClient = client;
            networkClient.OnPacketReceived += OnPacketReceived;
            this.Load += NewChatForm_Load;
            picSearch.Paint += PicSearch_Paint;
            lstDepartments.SelectedIndexChanged += LstDepartments_SelectedIndexChanged;
            lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;
            chkUsers.SelectedIndexChanged += ChkUsers_SelectedIndexChanged;
            txtChatName.TextChanged += TxtChatName_TextChanged;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
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
            lstUsers.BackColor = Color.FromArgb(60, 60, 80);
            lstUsers.ForeColor = Color.White;
            txtChatName.BackColor = Color.FromArgb(60, 60, 80);
            txtChatName.ForeColor = Color.White;
            chkUsers.BackColor = Color.FromArgb(60, 60, 80);
            chkUsers.ForeColor = Color.White;
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

            Console.WriteLine($"NewChatForm получил: {packet.Command}"); // отладка

            switch (packet.Command)
            {
                case Shared.CommandType.DepartmentsList:
                    var jsonElement = (JsonElement)packet.Data;
                    string json = jsonElement.GetRawText();
                    departments = JsonSerializer.Deserialize<List<Department>>(json);
                    Console.WriteLine($"DepartmentsList: получено {departments.Count} отделов");
                    UpdateDepartmentsList();
                    break;
                case Shared.CommandType.AvailableUsersList:
                    var jsonUsers = (JsonElement)packet.Data;
                    string usersJson = jsonUsers.GetRawText();
                    availableUsers = JsonSerializer.Deserialize<List<User>>(usersJson);
                    UpdateUsersList();
                    break;
                case Shared.CommandType.ChatCreated:
                    var jsonChat = (JsonElement)packet.Data;
                    string chatJson = jsonChat.GetRawText();
                    var newChat = JsonSerializer.Deserialize<Chat>(chatJson);
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
            }
        }

        private void UpdateDepartmentsList()
        {
            Console.WriteLine($"UpdateDepartmentsList: departments.Count = {departments.Count}");
            foreach (var d in departments)
                Console.WriteLine($"  отдел: {d.Name}, currentDepartment = {currentDepartment}");
            lstDepartments.Items.Clear();
            foreach (var dept in departments.Where(d => d.Name != currentDepartment))
                lstDepartments.Items.Add(dept);
            if (lstDepartments.Items.Count > 0)
                lstDepartments.SelectedIndex = 0;
            UpdateCreateButton();
        }

        private void UpdateUsersList()
        {
            Console.WriteLine($"UpdateUsersList: availableUsers.Count = {availableUsers.Count}");
            if (availableUsers.Count == 0) return;
            lstUsers.Items.Clear();
            chkUsers.Items.Clear();
            foreach (var user in availableUsers.Where(u => u.Id != currentUserId))
            {
                lstUsers.Items.Add(user);
                chkUsers.Items.Add(user, false);
            }
            Console.WriteLine($"lstUsers.Items.Count = {lstUsers.Items.Count}");
            if (lstDepartments.Items.Count > 0)
                lstDepartments.SelectedIndex = 0;
            UpdateCreateButton();
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e) => UpdateCreateButton();
        private void LstDepartments_SelectedIndexChanged(object sender, EventArgs e) => UpdateCreateButton();
        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e) => UpdateCreateButton();
        private void TxtChatName_TextChanged(object sender, EventArgs e) => UpdateCreateButton();
        private void ChkUsers_SelectedIndexChanged(object sender, EventArgs e) => UpdateCreateButton();

        private void UpdateCreateButton()
        {
            if (tabControl.SelectedTab == tabDepartment)
                btnCreate.Enabled = lstDepartments.SelectedItem != null;
            else if (tabControl.SelectedTab == tabPrivate)
                btnCreate.Enabled = lstUsers.SelectedItem != null;
            else
                btnCreate.Enabled = !string.IsNullOrWhiteSpace(txtChatName.Text) && chkUsers.CheckedItems.Count > 0;
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabDepartment && lstDepartments.SelectedItem != null)
            {
                // Для простоты создаём групповой чат с участниками из выбранного отдела
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
            else if (tabControl.SelectedTab == tabPrivate && lstUsers.SelectedItem != null)
            {
                var user = (User)lstUsers.SelectedItem;
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.CreatePrivateChat,
                    Data = new { otherUserId = user.Id }
                });
            }
            else if (tabControl.SelectedTab == tabGroup)
            {
                var participants = new List<int>();
                foreach (var item in chkUsers.CheckedItems)
                    if (item is User u) participants.Add(u.Id);
                networkClient.SendPacket(new NetworkPacket
                {
                    Command = Shared.CommandType.CreateGroupChat,
                    Data = new { name = txtChatName.Text, participants }
                });
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e) => Close();

        // Отрисовка списков (без эмодзи, только круги)
        /*private void LstDepartments_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            if (!(lstDepartments.Items[e.Index] is Department dept))
            {
                e.DrawBackground();
                e.DrawFocusRectangle();
                return;
            }

            e.DrawBackground();
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color back = sel ? Color.FromArgb(0, 229, 255, 50) : Color.FromArgb(60, 60, 80);
            using (var brush = new SolidBrush(back))
                e.Graphics.FillRectangle(brush, e.Bounds);

            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 10, e.Bounds.Y + 10, 40, 40);
            }

            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(dept.Name, font, Brushes.White, e.Bounds.X + 60, e.Bounds.Y + 15);
            using (var font = new Font("Segoe UI", 9))
                e.Graphics.DrawString(dept.Description ?? "Отдел", font, Brushes.Gray, e.Bounds.X + 60, e.Bounds.Y + 38);
            e.DrawFocusRectangle();
        }

        private void LstUsers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            if (!(lstUsers.Items[e.Index] is User user))
            {
                e.DrawBackground();
                e.DrawFocusRectangle();
                return;
            }

            e.DrawBackground();
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color back = sel ? Color.FromArgb(0, 229, 255, 50) : Color.FromArgb(60, 60, 80);
            using (var brush = new SolidBrush(back))
                e.Graphics.FillRectangle(brush, e.Bounds);

            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 10, e.Bounds.Y + 10, 40, 40);
            }

            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(user.FullName, font, Brushes.White, e.Bounds.X + 60, e.Bounds.Y + 10);

            string status = user.IsOnline ? "● Онлайн" : "● Офлайн";
            Color statusColor = user.IsOnline ? Color.FromArgb(76, 175, 80) : Color.Gray;
            using (var font = new Font("Segoe UI", 8))
            using (var brush = new SolidBrush(statusColor))
                e.Graphics.DrawString(status, font, brush, e.Bounds.X + 60, e.Bounds.Y + 35);

            e.DrawFocusRectangle();
        }

        private void ChkUsers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            if (!(chkUsers.Items[e.Index] is User user))
            {
                e.DrawBackground();
                e.DrawFocusRectangle();
                return;
            }

            e.DrawBackground();
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool chk = chkUsers.GetItemChecked(e.Index);
            Color back = sel ? Color.FromArgb(0, 229, 255, 50) : Color.FromArgb(60, 60, 80);
            using (var brush = new SolidBrush(back))
                e.Graphics.FillRectangle(brush, e.Bounds);

            Rectangle checkRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 15, 20, 20);
            using (var pen = new Pen(Color.FromArgb(0, 229, 255), 2))
            {
                e.Graphics.DrawRectangle(pen, checkRect);
                if (chk)
                {
                    using (var font = new Font("Segoe UI", 14))
                    using (var brush = new SolidBrush(Color.FromArgb(0, 229, 255)))
                        e.Graphics.DrawString("✓", font, brush, checkRect.X + 2, checkRect.Y - 2);
                }
            }

            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 40, e.Bounds.Y + 8, 30, 30);
            }

            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                e.Graphics.DrawString(user.FullName, font, Brushes.White, e.Bounds.X + 80, e.Bounds.Y + 12);

            e.DrawFocusRectangle();
        }*/

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
