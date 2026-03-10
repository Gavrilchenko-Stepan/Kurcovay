using Messenger.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Messenger.Client
{
    public partial class EditUserForm : Form
    {
        private User editingUser;
        private List<Department> departments;
        private NetworkClient networkClient;
        private bool isNewUser;

        public EditUserForm(User user, List<Department> depts, NetworkClient client)
        {
            InitializeComponent();
            editingUser = user;
            departments = depts;
            networkClient = client;
            isNewUser = (user == null);

            LoadDepartments();

            if (!isNewUser)
            {
                lblTitle.Text = "Редактирование пользователя";
                txtUsername.Text = editingUser.Username;
                txtFullName.Text = editingUser.FullName;
                txtPosition.Text = editingUser.Position;
                chkIsAdmin.Checked = editingUser.IsAdmin;
                cmbDepartment.SelectedValue = editingUser.DepartmentId;
                // Пароль оставляем пустым – не менять, если не введён
            }
            else
            {
                lblTitle.Text = "Добавление пользователя";
            }
        }

        private void LoadDepartments()
        {
            cmbDepartment.DataSource = departments;
            cmbDepartment.DisplayMember = "Name";
            cmbDepartment.ValueMember = "Id";
            cmbDepartment.SelectedIndex = -1;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Введите логин.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО.");
                return;
            }
            if (cmbDepartment.SelectedItem == null)
            {
                MessageBox.Show("Выберите отдел.");
                return;
            }
            if (isNewUser && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Введите пароль для нового пользователя.");
                return;
            }

            var user = new User
            {
                Id = editingUser?.Id ?? 0,
                Username = txtUsername.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                DepartmentId = (int)cmbDepartment.SelectedValue,
                Position = txtPosition.Text.Trim(),
                IsAdmin = chkIsAdmin.Checked
            };

            var packet = new NetworkPacket();
            if (isNewUser)
            {
                packet.Command = Shared.CommandType.AddUser;
                packet.Data = new { user, password = txtPassword.Text };
            }
            else
            {
                packet.Command = Shared.CommandType.UpdateUser;
                var data = new Dictionary<string, object>
                {
                    { "user", user },
                    { "password", string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text }
                };
                packet.Data = data;
            }
            networkClient.SendPacket(packet);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
