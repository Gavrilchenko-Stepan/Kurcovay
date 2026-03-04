using System.Drawing;
using System.Windows.Forms;

namespace Messenger.Client
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.Panel panelLine1;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Panel panelLine2;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Panel panelLine3;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblError;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.panelLine1 = new System.Windows.Forms.Panel();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.panelLine2 = new System.Windows.Forms.Panel();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.panelLine3 = new System.Windows.Forms.Panel();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblError = new System.Windows.Forms.Label();

            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();

            // LoginForm
            this.ClientSize = new System.Drawing.Size(420, 560);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Вход в систему";
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 46);          // тёмный фон
            this.Font = new System.Drawing.Font("Segoe UI", 10F);

            // panelMain
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(45, 45, 58); // панель чуть светлее
            this.panelMain.Controls.Add(this.picLogo);
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Controls.Add(this.lblSubtitle);
            this.panelMain.Controls.Add(this.lblServer);
            this.panelMain.Controls.Add(this.txtServerIP);
            this.panelMain.Controls.Add(this.panelLine1);
            this.panelMain.Controls.Add(this.lblUsername);
            this.panelMain.Controls.Add(this.txtUsername);
            this.panelMain.Controls.Add(this.panelLine2);
            this.panelMain.Controls.Add(this.lblPassword);
            this.panelMain.Controls.Add(this.txtPassword);
            this.panelMain.Controls.Add(this.panelLine3);
            this.panelMain.Controls.Add(this.btnLogin);
            this.panelMain.Controls.Add(this.btnCancel);
            this.panelMain.Controls.Add(this.lblStatus);
            this.panelMain.Controls.Add(this.lblError);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(30);
            this.panelMain.Size = new System.Drawing.Size(420, 560);

            // picLogo
            this.picLogo.BackColor = System.Drawing.Color.FromArgb(0, 229, 255); // акцентный цвет
            this.picLogo.Location = new System.Drawing.Point(150, 30);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(120, 120);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;

            // lblTitle
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255); // акцент
            this.lblTitle.Location = new System.Drawing.Point(30, 160);
            this.lblTitle.Size = new System.Drawing.Size(360, 40);
            this.lblTitle.Text = "Добро пожаловать";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblSubtitle
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(180, 180, 200); // сероватый
            this.lblSubtitle.Location = new System.Drawing.Point(30, 200);
            this.lblSubtitle.Size = new System.Drawing.Size(360, 30);
            this.lblSubtitle.Text = "Корпоративный мессенджер";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblServer
            this.lblServer.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblServer.ForeColor = System.Drawing.Color.White;
            this.lblServer.Location = new System.Drawing.Point(30, 250);
            this.lblServer.Size = new System.Drawing.Size(100, 20);
            this.lblServer.Text = "Сервер:";

            // txtServerIP
            this.txtServerIP.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.txtServerIP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServerIP.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtServerIP.ForeColor = System.Drawing.Color.White;
            this.txtServerIP.Location = new System.Drawing.Point(30, 273);
            this.txtServerIP.Size = new System.Drawing.Size(360, 20);
            this.txtServerIP.Text = "127.0.0.1";

            // panelLine1
            this.panelLine1.BackColor = System.Drawing.Color.FromArgb(0, 229, 255); // акцентная линия
            this.panelLine1.Location = new System.Drawing.Point(30, 295);
            this.panelLine1.Size = new System.Drawing.Size(360, 1);

            // lblUsername
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.White;
            this.lblUsername.Location = new System.Drawing.Point(30, 310);
            this.lblUsername.Size = new System.Drawing.Size(100, 20);
            this.lblUsername.Text = "Логин:";

            // txtUsername
            this.txtUsername.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtUsername.ForeColor = System.Drawing.Color.White;
            this.txtUsername.Location = new System.Drawing.Point(30, 333);
            this.txtUsername.Size = new System.Drawing.Size(360, 20);
            this.txtUsername.Text = "admin";

            // panelLine2
            this.panelLine2.BackColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.panelLine2.Location = new System.Drawing.Point(30, 355);
            this.panelLine2.Size = new System.Drawing.Size(360, 1);

            // lblPassword
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.White;
            this.lblPassword.Location = new System.Drawing.Point(30, 370);
            this.lblPassword.Size = new System.Drawing.Size(100, 20);
            this.lblPassword.Text = "Пароль:";

            // txtPassword
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPassword.ForeColor = System.Drawing.Color.White;
            this.txtPassword.Location = new System.Drawing.Point(30, 393);
            this.txtPassword.Size = new System.Drawing.Size(360, 20);
            this.txtPassword.Text = "admin";
            this.txtPassword.UseSystemPasswordChar = true;

            // panelLine3
            this.panelLine3.BackColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.panelLine3.Location = new System.Drawing.Point(30, 415);
            this.panelLine3.Size = new System.Drawing.Size(360, 1);

            // btnLogin
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.Black;
            this.btnLogin.Location = new System.Drawing.Point(30, 440);
            this.btnLogin.Size = new System.Drawing.Size(170, 45);
            this.btnLogin.Text = "Войти";
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;

            // btnCancel
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnCancel.Location = new System.Drawing.Point(220, 440);
            this.btnCancel.Size = new System.Drawing.Size(170, 45);
            this.btnCancel.Text = "Отмена";
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;

            // lblStatus
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(180, 180, 200);
            this.lblStatus.Location = new System.Drawing.Point(30, 500);
            this.lblStatus.Size = new System.Drawing.Size(360, 25);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Visible = false;

            // lblError
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblError.ForeColor = System.Drawing.Color.Red;
            this.lblError.Location = new System.Drawing.Point(30, 500);
            this.lblError.Size = new System.Drawing.Size(360, 25);
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblError.Visible = false;

            this.Controls.Add(this.panelMain);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
        }
    }
}