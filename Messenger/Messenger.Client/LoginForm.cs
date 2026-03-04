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
using Messenger.Shared;

namespace Messenger.Client
{
    public partial class LoginForm : Form
    {
        private NetworkClient networkClient;

        public User CurrentUser { get; private set; }
        public NetworkClient NetworkClient => networkClient;

        public LoginForm()
        {
            InitializeComponent();
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.StartPosition = FormStartPosition.CenterScreen;

            networkClient = new NetworkClient();
            networkClient.OnPacketReceived += OnPacketReceived;
            networkClient.OnDisconnected += OnDisconnected;

            this.AcceptButton = btnLogin;
            CreateLogo();
        }

        private void CreateLogo()
        {
            var bmp = new Bitmap(120, 120);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(63, 81, 181));

                using (var font = new Font("Segoe UI", 48, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("M", font, brush, new Rectangle(0, 0, 120, 120), sf);
                }
            }
            picLogo.Image = bmp;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServerIP.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowError("Заполните все поля");
                return;
            }

            btnLogin.Enabled = false;
            lblStatus.Text = "Подключение к серверу...";
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Visible = true;
            lblError.Visible = false;

            bool connected = await networkClient.Connect(txtServerIP.Text);

            if (!connected)
            {
                ShowError("Не удалось подключиться к серверу");
                btnLogin.Enabled = true;
                return;
            }

            lblStatus.Text = "Отправка данных...";

            networkClient.SendPacket(new NetworkPacket
            {
                Command = Shared.CommandType.Login,
                Data = new
                {
                    username = txtUsername.Text,
                    password = txtPassword.Text
                }
            });
        }

        private void OnPacketReceived(NetworkPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NetworkPacket>(OnPacketReceived), packet);
                return;
            }

            if (packet.Command == Shared.CommandType.LoginResponse)
            {
                // Получаем JsonElement
                var jsonElement = (JsonElement)packet.Data;

                // Извлекаем значения через JsonElement
                bool success = jsonElement.GetProperty("success").GetBoolean();

                if (success)
                {
                    // Получаем пользователя как JsonElement
                    var userElement = jsonElement.GetProperty("user");

                    // Конвертируем в строку и десериализуем
                    string userJson = userElement.GetRawText();
                    CurrentUser = JsonSerializer.Deserialize<User>(userJson);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    string message = jsonElement.GetProperty("message").GetString();
                    ShowError(message);
                    networkClient.Disconnect();
                    btnLogin.Enabled = true;
                }
            }
        }

        private void OnDisconnected()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnDisconnected));
                return;
            }

            if (!IsDisposed)
            {
                ShowError("Соединение с сервером потеряно");
                btnLogin.Enabled = true;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
            lblStatus.Visible = false;
        }
    }
}
