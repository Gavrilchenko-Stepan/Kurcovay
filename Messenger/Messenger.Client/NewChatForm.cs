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
        private NetworkClient networkClient;

        public NewChatForm(int userId, string department)
        {
            InitializeComponent();
            InitializeForm();

            currentUserId = userId;
            currentDepartment = department;
        }

        private void InitializeForm()
        {
            this.Font = new Font("Segoe UI", 10F);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Настройка списков
            lstDepartments.DrawMode = DrawMode.OwnerDrawFixed;
            lstDepartments.DrawItem += LstDepartments_DrawItem;
            lstDepartments.ItemHeight = 60;
            lstDepartments.SelectedIndexChanged += LstDepartments_SelectedIndexChanged;

            lstUsers.DrawMode = DrawMode.OwnerDrawFixed;
            lstUsers.DrawItem += LstUsers_DrawItem;
            lstUsers.ItemHeight = 70;
            lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;

            chkUsers.DrawMode = DrawMode.OwnerDrawFixed;
            chkUsers.DrawItem += ChkUsers_DrawItem;
            chkUsers.ItemHeight = 60;
            chkUsers.SelectedIndexChanged += ChkUsers_SelectedIndexChanged;

            // Подписка на события
            this.Load += NewChatForm_Load;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.Enter += TxtSearch_Enter;
            txtSearch.Leave += TxtSearch_Leave;
            txtChatName.TextChanged += TxtChatName_TextChanged;
            btnCreate.Click += BtnCreate_Click;
            btnCancel.Click += BtnCancel_Click;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            // Отрисовка иконки поиска
            picSearch.Paint += PicSearch_Paint;
        }

        private void NewChatForm_Load(object sender, EventArgs e)
        {
            LoadDepartments();
            LoadUsers();
        }

        private void LoadDepartments()
        {
            // В реальном приложении здесь будет загрузка с сервера
            departments = new List<Shared.Department>
            {
                new Shared.Department { Id = 1, Name = "ИТ отдел", Description = "Информационные технологии" },
                new Shared.Department { Id = 2, Name = "Производство", Description = "Производственный отдел" },
                new Shared.Department { Id = 3, Name = "HR отдел", Description = "Отдел кадров" },
                new Shared.Department { Id = 4, Name = "Бухгалтерия", Description = "Финансовый отдел" },
                new Shared.Department { Id = 5, Name = "Отдел продаж", Description = "Продажи" },
                new Shared.Department { Id = 6, Name = "Логистика", Description = "Склад и доставка" }
            };

            UpdateDepartmentsList(departments);
        }

        private void LoadUsers()
        {
            // В реальном приложении здесь будет загрузка с сервера
            availableUsers = new List<User>
            {
                new User { Id = 1, FullName = "Иванов Иван", Department = "ИТ отдел", IsOnline = true },
                new User { Id = 2, FullName = "Петров Петр", Department = "ИТ отдел", IsOnline = true },
                new User { Id = 3, FullName = "Сидоров Сидор", Department = "Производство", IsOnline = false },
                new User { Id = 4, FullName = "Смирнова Анна", Department = "HR отдел", IsOnline = true },
                new User { Id = 5, FullName = "Кузнецов Дмитрий", Department = "Бухгалтерия", IsOnline = false },
                new User { Id = 6, FullName = "Соколов Максим", Department = "Отдел продаж", IsOnline = true }
            };

            UpdateUsersList(availableUsers);
        }

        public void SetDepartments(List<Shared.Department> depts)
        {
            departments = depts;
            UpdateDepartmentsList(departments);
        }

        public void SetUsers(List<User> users)
        {
            availableUsers = users;
            UpdateUsersList(availableUsers);
        }

        private void UpdateDepartmentsList(List<Shared.Department> depts)
        {
            lstDepartments.Items.Clear();
            foreach (var dept in depts.Where(d => d.Name != currentDepartment))
            {
                lstDepartments.Items.Add(dept);
            }
        }

        private void UpdateUsersList(List<User> users)
        {
            lstUsers.Items.Clear();
            chkUsers.Items.Clear();

            foreach (var user in users.Where(u => u.Id != currentUserId))
            {
                lstUsers.Items.Add(user);
                chkUsers.Items.Add(user, false);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCreateButton();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(search) || search == "поиск отдела...")
            {
                UpdateDepartmentsList(departments);
                return;
            }

            var filtered = departments.Where(d =>
                d.Name.ToLower().Contains(search) ||
                (d.Description != null && d.Description.ToLower().Contains(search))
            ).ToList();

            UpdateDepartmentsList(filtered);
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

        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCreateButton();
        }

        private void TxtChatName_TextChanged(object sender, EventArgs e)
        {
            UpdateCreateButton();
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
                MessageBox.Show($"Создан чат с отделом: {selectedDept.Name}", "Успех");
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (tabControl.SelectedTab == tabPrivate && lstUsers.SelectedItem != null)
            {
                var selectedUser = (User)lstUsers.SelectedItem;
                MessageBox.Show($"Создан личный чат с пользователем: {selectedUser.FullName}", "Успех");
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (tabControl.SelectedTab == tabGroup)
            {
                var selectedUsers = new List<int>();
                var selectedNames = new List<string>();

                foreach (var item in chkUsers.CheckedItems)
                {
                    if (item is User user)
                    {
                        selectedUsers.Add(user.Id);
                        selectedNames.Add(user.FullName);
                    }
                }

                MessageBox.Show($"Создан групповой чат '{txtChatName.Text}' с участниками: {string.Join(", ", selectedNames)}", "Успех");
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LstDepartments_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(lstDepartments.Items[e.Index] is Shared.Department dept)) return;

            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = selected ? Color.FromArgb(230, 242, 255) : Color.White;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            // Иконка отдела
            using (var font = new Font("Segoe UI", 16))
                e.Graphics.DrawString("🏢", font, Brushes.Gray, e.Bounds.X + 10, e.Bounds.Y + 12);

            // Название отдела
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(dept.Name, font, Brushes.Black, e.Bounds.X + 45, e.Bounds.Y + 10);

            // Описание
            if (!string.IsNullOrEmpty(dept.Description))
            {
                using (var font = new Font("Segoe UI", 9))
                using (var brush = new SolidBrush(Color.FromArgb(76, 175, 80)))
                {
                    e.Graphics.DrawString(dept.Description, font, brush, e.Bounds.X + 45, e.Bounds.Y + 32);
                }
            }

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

            // Аватар
            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 10, e.Bounds.Y + 10, 40, 40);
            }

            // Имя
            using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
                e.Graphics.DrawString(user.FullName, font, Brushes.Black, e.Bounds.X + 60, e.Bounds.Y + 12);

            // Отдел и статус
            string status = user.IsOnline ? "● Онлайн" : "● Офлайн";
            Color statusColor = user.IsOnline ? Color.FromArgb(76, 175, 80) : Color.Gray;

            using (var font = new Font("Segoe UI", 9))
            {
                e.Graphics.DrawString(user.Department, font, Brushes.Gray, e.Bounds.X + 60, e.Bounds.Y + 35);

                var deptSize = e.Graphics.MeasureString(user.Department, font);
                using (var brush = new SolidBrush(statusColor))
                {
                    e.Graphics.DrawString(status, font, brush,
                        e.Bounds.X + 60 + deptSize.Width + 10, e.Bounds.Y + 35);
                }
            }

            e.DrawFocusRectangle();
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

            // Чекбокс
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

            // Аватар
            using (var brush = new SolidBrush(Color.FromArgb(63, 81, 181)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, e.Bounds.X + 40, e.Bounds.Y + 8, 30, 30);
            }

            // Имя
            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
                e.Graphics.DrawString(user.FullName, font, Brushes.Black, e.Bounds.X + 80, e.Bounds.Y + 12);

            // Отдел
            using (var font = new Font("Segoe UI", 8))
                e.Graphics.DrawString(user.Department, font, Brushes.Gray, e.Bounds.X + 80, e.Bounds.Y + 32);

            e.DrawFocusRectangle();
        }
    }
}
