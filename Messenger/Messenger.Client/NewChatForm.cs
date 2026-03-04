using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Messenger.Server;
using Messenger.Shared;

namespace Messenger.Client
{
    public partial class NewChatForm : Form
    {
        private int currentUserId;
        private string currentDepartment;
        private List<Shared.Department> departments = new List<Shared.Department>();
        private List<User> availableUsers = new List<User>();

        public NewChatForm(int userId, string department)
        {
            InitializeComponent();
            currentUserId = userId;
            currentDepartment = department;

            // Подписываемся на события
            this.Load += NewChatForm_Load;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.Enter += TxtSearch_Enter;
            txtSearch.Leave += TxtSearch_Leave;
            lstDepartments.SelectedIndexChanged += LstDepartments_SelectedIndexChanged;
            lstDepartments.DrawItem += LstDepartments_DrawItem;
            lstUsers.DrawItem += LstUsers_DrawItem;
            txtChatName.TextChanged += TxtChatName_TextChanged;
            chkUsers.DrawItem += ChkUsers_DrawItem;
            chkUsers.SelectedIndexChanged += ChkUsers_SelectedIndexChanged;
            btnCreate.Click += BtnCreate_Click;
            btnCancel.Click += BtnCancel_Click;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
        }

        private void NewChatForm_Load(object sender, EventArgs e)
        {
            LoadDepartments();
            LoadUsers();
        }

        private void LoadDepartments()
        {
            // Здесь будет загрузка отделов с сервера
            // Пока оставляем пустым - данные придут через NetworkClient
            lstDepartments.Items.Clear();
        }

        private void LoadUsers()
        {
            // Здесь будет загрузка пользователей с сервера
            lstUsers.Items.Clear();
            chkUsers.Items.Clear();
        }

        public void SetDepartments(List<Shared.Department> depts)
        {
            departments = depts;
            lstDepartments.Items.Clear();
            foreach (var dept in departments)
            {
                if (dept.Name != currentDepartment)
                    lstDepartments.Items.Add(dept);
            }
        }

        public void SetUsers(List<User> users)
        {
            availableUsers = users;
            lstUsers.Items.Clear();
            chkUsers.Items.Clear();

            foreach (var user in users)
            {
                if (user.Id != currentUserId)
                {
                    lstUsers.Items.Add(user);
                    chkUsers.Items.Add(user, false);
                }
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCreateButton();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(search) || search == "поиск отдела...")
            {
                return;
            }

            for (int i = 0; i < lstDepartments.Items.Count; i++)
            {
                if (lstDepartments.Items[i] is Shared.Department dept)
                {
                    lstDepartments.SetSelected(i, dept.Name.ToLower().Contains(search));
                }
            }
        }

        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Поиск отдела...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Поиск отдела...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void LstDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCreateButton();
        }

        private void LstDepartments_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(lstDepartments.Items[e.Index] is Shared.Department dept)) return;

            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = selected ? Color.FromArgb(230, 242, 255) : Color.White;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            using (var font = new Font("Segoe UI", 16))
                e.Graphics.DrawString("🏢", font, Brushes.Gray, e.Bounds.X + 10, e.Bounds.Y + 10);

            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(dept.Name, font, Brushes.Black, e.Bounds.X + 45, e.Bounds.Y + 10);

            using (var font = new Font("Segoe UI", 9))
            using (var brush = new SolidBrush(Color.FromArgb(76, 175, 80)))
                e.Graphics.DrawString(dept.Description ?? "Отдел", font, brush, e.Bounds.X + 45, e.Bounds.Y + 35);

            e.DrawFocusRectangle();
        }

        private void LstUsers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(lstUsers.Items[e.Index] is User user)) return;

            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = selected ? Color.FromArgb(230, 242, 255) : Color.White;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 10, e.Bounds.Y + 10, 40, 40);
            }

            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(user.FullName, font, Brushes.Black, e.Bounds.X + 60, e.Bounds.Y + 15);

            using (var font = new Font("Segoe UI", 9))
            {
                Color statusColor = user.IsOnline ? Color.FromArgb(76, 175, 80) : Color.Gray;
                string status = user.IsOnline ? "● Онлайн" : "● Офлайн";
                using (var brush = new SolidBrush(statusColor))
                    e.Graphics.DrawString(status, font, brush, e.Bounds.X + 60, e.Bounds.Y + 38);
            }

            e.DrawFocusRectangle();
        }

        private void TxtChatName_TextChanged(object sender, EventArgs e)
        {
            UpdateCreateButton();
        }

        private void ChkUsers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(chkUsers.Items[e.Index] is User user)) return;

            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool checked_ = chkUsers.GetItemChecked(e.Index);

            Color backColor = selected ? Color.FromArgb(230, 242, 255) : Color.White;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            Rectangle checkRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 15, 20, 20);
            using (var pen = new Pen(Color.FromArgb(63, 81, 181), 2))
            {
                e.Graphics.DrawRectangle(pen, checkRect);
                if (checked_)
                {
                    using (var font = new Font("Segoe UI", 14))
                    using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
                        e.Graphics.DrawString("✓", font, brush, checkRect.X + 2, checkRect.Y - 2);
                }
            }

            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 40, e.Bounds.Y + 10, 30, 30);
            }

            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                e.Graphics.DrawString(user.FullName, font, Brushes.Black, e.Bounds.X + 80, e.Bounds.Y + 15);

            e.DrawFocusRectangle();
        }

        private void ChkUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCreateButton();
        }

        private void UpdateCreateButton()
        {
            if (tabControl.SelectedTab == tabDepartment)
            {
                btnCreate.Enabled = lstDepartments.SelectedItem != null;
            }
            else if (tabControl.SelectedTab == tabPrivate)
            {
                btnCreate.Enabled = lstUsers.SelectedItem != null;
            }
            else if (tabControl.SelectedTab == tabGroup)
            {
                btnCreate.Enabled = !string.IsNullOrWhiteSpace(txtChatName.Text) &&
                                    chkUsers.CheckedItems.Count > 0;
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabDepartment && lstDepartments.SelectedItem != null)
            {
                var selectedDept = (Shared.Department)lstDepartments.SelectedItem;
                // Отправляем запрос на создание чата с отделом
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (tabControl.SelectedTab == tabPrivate && lstUsers.SelectedItem != null)
            {
                var selectedUser = (User)lstUsers.SelectedItem;
                // Отправляем запрос на создание личного чата
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (tabControl.SelectedTab == tabGroup)
            {
                var selectedUsers = new List<int>();
                foreach (var item in chkUsers.CheckedItems)
                {
                    if (item is User user)
                        selectedUsers.Add(user.Id);
                }
                // Отправляем запрос на создание группового чата
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
