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
    public partial class AdminPanelForm : Form
    {
        private NetworkClient networkClient;
        private int currentUserId;  // добавлено
        private List<User> allUsers;
        private List<Department> departments;

        public AdminPanelForm(NetworkClient client, int currentUserId) // изменён конструктор
        {
            InitializeComponent();
            networkClient = client;
            this.currentUserId = currentUserId;
            networkClient.OnPacketReceived += OnPacketReceived;
            LoadData();
        }

        private void LoadData()
        {
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetAllUsers });
            networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.GetDepartments });
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
                    case Shared.CommandType.AllUsersList:
                        var jsonElem = (JsonElement)packet.Data;
                        string json = jsonElem.GetRawText();
                        allUsers = JsonSerializer.Deserialize<List<User>>(json);
                        UpdateTree(allUsers);
                        break;
                    case Shared.CommandType.DepartmentsList:
                        var jsonDept = (JsonElement)packet.Data;
                        string jsonDeptStr = jsonDept.GetRawText();
                        departments = JsonSerializer.Deserialize<List<Department>>(jsonDeptStr);
                        break;
                    case Shared.CommandType.Error:
                        string error = packet.Data.ToString();
                        MessageBox.Show($"Ошибка: {error}");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки: {ex.Message}");
            }
        }

        private void UpdateTree(List<User> users)
        {
            tvUsers.Nodes.Clear();
            var usersByDept = users.GroupBy(u => u.Department).OrderBy(g => g.Key);
            foreach (var group in usersByDept)
            {
                TreeNode deptNode = new TreeNode(group.Key);
                deptNode.ForeColor = Color.White;
                foreach (var user in group.OrderBy(u => u.FullName))
                {
                    string display = user.FullName;
                    if (!string.IsNullOrEmpty(user.Position))
                        display += $" ({user.Position})";
                    if (user.IsAdmin)
                        display += " [Admin]";
                    TreeNode userNode = new TreeNode(display);
                    userNode.Tag = user;
                    userNode.ForeColor = Color.White;
                    deptNode.Nodes.Add(userNode);
                }
                tvUsers.Nodes.Add(deptNode);
            }
            tvUsers.ExpandAll();
        }

        private void FilterTree(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                UpdateTree(allUsers);
                return;
            }
            var filtered = allUsers.Where(u => u.FullName.ToLower().Contains(searchText.ToLower()) ||
                                               (u.Position != null && u.Position.ToLower().Contains(searchText.ToLower()))).ToList();
            UpdateTree(filtered);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterTree(txtSearch.Text);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            FilterTree(txtSearch.Text);
        }

        private void TvUsers_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is User user)
            {
                EditUser(user);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new EditUserForm(null, departments, networkClient))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (tvUsers.SelectedNode?.Tag is User user)
            {
                EditUser(user);
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования.");
            }
        }

        private void EditUser(User user)
        {
            using (var form = new EditUserForm(user, departments, networkClient))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (tvUsers.SelectedNode?.Tag is User user)
            {
                if (user.Id == currentUserId)  // исправлено
                {
                    MessageBox.Show("Нельзя удалить самого себя.");
                    return;
                }
                var result = MessageBox.Show($"Удалить пользователя {user.FullName}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    networkClient.SendPacket(new NetworkPacket { Command = Shared.CommandType.DeleteUser, Data = user.Id });
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.");
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            networkClient.OnPacketReceived -= OnPacketReceived;
            base.OnFormClosing(e);
        }
    }
}
