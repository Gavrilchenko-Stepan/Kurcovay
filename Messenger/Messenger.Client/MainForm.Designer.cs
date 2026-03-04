namespace Messenger.Client
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelTopGradient;
        private System.Windows.Forms.PictureBox picUserAvatar;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblUserDepartment;
        private System.Windows.Forms.Label lblUserStatus;
        private System.Windows.Forms.Button btnNewChat;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnLogout;

        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelLeftHeader;
        private System.Windows.Forms.Label lblChats;
        private System.Windows.Forms.TextBox txtSearchChats;
        private System.Windows.Forms.PictureBox picSearch;
        private System.Windows.Forms.ListBox lstChats;
        private System.Windows.Forms.Panel panelLeftFooter;
        private System.Windows.Forms.Label lblTotalUsers;

        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelChatHeader;
        private System.Windows.Forms.PictureBox picChatAvatar;
        private System.Windows.Forms.Label lblChatName;
        private System.Windows.Forms.Label lblChatInfo;
        private System.Windows.Forms.ListBox lstMessages;
        private System.Windows.Forms.Panel panelMessageInput;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;

        private System.Windows.Forms.Panel panelStatusBar;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label lblServerInfo;

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
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelTopGradient = new System.Windows.Forms.Panel();
            this.picUserAvatar = new System.Windows.Forms.PictureBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblUserDepartment = new System.Windows.Forms.Label();
            this.lblUserStatus = new System.Windows.Forms.Label();
            this.btnNewChat = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelLeftHeader = new System.Windows.Forms.Panel();
            this.lblChats = new System.Windows.Forms.Label();
            this.txtSearchChats = new System.Windows.Forms.TextBox();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.lstChats = new System.Windows.Forms.ListBox();
            this.panelLeftFooter = new System.Windows.Forms.Panel();
            this.lblTotalUsers = new System.Windows.Forms.Label();
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelChatHeader = new System.Windows.Forms.Panel();
            this.picChatAvatar = new System.Windows.Forms.PictureBox();
            this.lblChatName = new System.Windows.Forms.Label();
            this.lblChatInfo = new System.Windows.Forms.Label();
            this.lstMessages = new System.Windows.Forms.ListBox();
            this.panelMessageInput = new System.Windows.Forms.Panel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.panelStatusBar = new System.Windows.Forms.Panel();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.lblServerInfo = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelTopGradient.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picUserAvatar)).BeginInit();
            this.panelLeft.SuspendLayout();
            this.panelLeftHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.panelLeftFooter.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.panelChatHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picChatAvatar)).BeginInit();
            this.panelMessageInput.SuspendLayout();
            this.panelStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(30)))));
            this.panelTop.Controls.Add(this.panelTopGradient);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1200, 70);
            this.panelTop.TabIndex = 2;
            // 
            // panelTopGradient
            // 
            this.panelTopGradient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(30)))));
            this.panelTopGradient.Controls.Add(this.picUserAvatar);
            this.panelTopGradient.Controls.Add(this.lblUserName);
            this.panelTopGradient.Controls.Add(this.lblUserDepartment);
            this.panelTopGradient.Controls.Add(this.lblUserStatus);
            this.panelTopGradient.Controls.Add(this.btnNewChat);
            this.panelTopGradient.Controls.Add(this.btnSettings);
            this.panelTopGradient.Controls.Add(this.btnLogout);
            this.panelTopGradient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTopGradient.Location = new System.Drawing.Point(0, 0);
            this.panelTopGradient.Name = "panelTopGradient";
            this.panelTopGradient.Padding = new System.Windows.Forms.Padding(10, 0, 20, 0);
            this.panelTopGradient.Size = new System.Drawing.Size(1200, 70);
            this.panelTopGradient.TabIndex = 0;
            // 
            // picUserAvatar
            // 
            this.picUserAvatar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.picUserAvatar.Location = new System.Drawing.Point(20, 15);
            this.picUserAvatar.Name = "picUserAvatar";
            this.picUserAvatar.Size = new System.Drawing.Size(40, 40);
            this.picUserAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picUserAvatar.TabIndex = 0;
            this.picUserAvatar.TabStop = false;
            // 
            // lblUserName
            // 
            this.lblUserName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblUserName.ForeColor = System.Drawing.Color.White;
            this.lblUserName.Location = new System.Drawing.Point(70, 15);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(300, 20);
            this.lblUserName.TabIndex = 1;
            this.lblUserName.Text = "Загрузка...";
            // 
            // lblUserDepartment
            // 
            this.lblUserDepartment.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblUserDepartment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.lblUserDepartment.Location = new System.Drawing.Point(70, 35);
            this.lblUserDepartment.Name = "lblUserDepartment";
            this.lblUserDepartment.Size = new System.Drawing.Size(200, 15);
            this.lblUserDepartment.TabIndex = 2;
            // 
            // lblUserStatus
            // 
            this.lblUserStatus.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblUserStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblUserStatus.Location = new System.Drawing.Point(280, 35);
            this.lblUserStatus.Name = "lblUserStatus";
            this.lblUserStatus.Size = new System.Drawing.Size(100, 15);
            this.lblUserStatus.TabIndex = 3;
            this.lblUserStatus.Text = "● Не в сети";

            // btnNewChat
            this.btnNewChat.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnNewChat.BackColor = System.Drawing.Color.Transparent;
            this.btnNewChat.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnNewChat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewChat.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnNewChat.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnNewChat.Location = new System.Drawing.Point(950, 20);
            this.btnNewChat.Size = new System.Drawing.Size(120, 30);
            this.btnNewChat.Text = "➕ Новый чат";
            this.btnNewChat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNewChat.Cursor = System.Windows.Forms.Cursors.Hand;

            // btnSettings
            this.btnSettings.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnSettings.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnSettings.Location = new System.Drawing.Point(1080, 15);
            this.btnSettings.Size = new System.Drawing.Size(40, 40);
            this.btnSettings.Text = "⚙";
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;

            // btnLogout
            this.btnLogout.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnLogout.BackColor = System.Drawing.Color.Transparent;
            this.btnLogout.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(255, 80, 80);
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnLogout.ForeColor = System.Drawing.Color.FromArgb(255, 80, 80);
            this.btnLogout.Location = new System.Drawing.Point(1130, 15);
            this.btnLogout.Size = new System.Drawing.Size(40, 40);
            this.btnLogout.Text = "🚪";
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            // 
            // panelLeft
            // 
            this.panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(58)))));
            this.panelLeft.Controls.Add(this.panelLeftHeader);
            this.panelLeft.Controls.Add(this.lstChats);
            this.panelLeft.Controls.Add(this.panelLeftFooter);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 70);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(350, 605);
            this.panelLeft.TabIndex = 1;
            // 
            // panelLeftHeader
            // 
            this.panelLeftHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(58)))));
            this.panelLeftHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLeftHeader.Controls.Add(this.lblChats);
            this.panelLeftHeader.Controls.Add(this.txtSearchChats);
            this.panelLeftHeader.Controls.Add(this.picSearch);
            this.panelLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLeftHeader.Location = new System.Drawing.Point(0, 0);
            this.panelLeftHeader.Name = "panelLeftHeader";
            this.panelLeftHeader.Size = new System.Drawing.Size(350, 80);
            this.panelLeftHeader.TabIndex = 0;
            // 
            // lblChats
            // 
            this.lblChats.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblChats.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.lblChats.Location = new System.Drawing.Point(15, 10);
            this.lblChats.Name = "lblChats";
            this.lblChats.Size = new System.Drawing.Size(100, 25);
            this.lblChats.TabIndex = 0;
            this.lblChats.Text = "ЧАТЫ";
            // 
            // txtSearchChats
            // 
            this.txtSearchChats.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(80)))));
            this.txtSearchChats.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearchChats.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearchChats.ForeColor = System.Drawing.Color.Gray;
            this.txtSearchChats.Location = new System.Drawing.Point(15, 45);
            this.txtSearchChats.Name = "txtSearchChats";
            this.txtSearchChats.Size = new System.Drawing.Size(300, 18);
            this.txtSearchChats.TabIndex = 1;
            this.txtSearchChats.Text = "Поиск чатов...";
            // 
            // picSearch
            // 
            this.picSearch.BackColor = System.Drawing.Color.Transparent;
            this.picSearch.Location = new System.Drawing.Point(315, 43);
            this.picSearch.Name = "picSearch";
            this.picSearch.Size = new System.Drawing.Size(20, 20);
            this.picSearch.TabIndex = 2;
            this.picSearch.TabStop = false;
            // 
            // lstChats
            // 
            this.lstChats.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(58)))));
            this.lstChats.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstChats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstChats.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstChats.ForeColor = System.Drawing.Color.White;
            this.lstChats.IntegralHeight = false;
            this.lstChats.ItemHeight = 70;
            this.lstChats.Location = new System.Drawing.Point(0, 0);
            this.lstChats.Name = "lstChats";
            this.lstChats.Size = new System.Drawing.Size(350, 575);
            this.lstChats.TabIndex = 1;
            // 
            // panelLeftFooter
            // 
            this.panelLeftFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.panelLeftFooter.Controls.Add(this.lblTotalUsers);
            this.panelLeftFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLeftFooter.Location = new System.Drawing.Point(0, 575);
            this.panelLeftFooter.Name = "panelLeftFooter";
            this.panelLeftFooter.Size = new System.Drawing.Size(350, 30);
            this.panelLeftFooter.TabIndex = 2;
            // 
            // lblTotalUsers
            // 
            this.lblTotalUsers.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotalUsers.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.lblTotalUsers.Location = new System.Drawing.Point(15, 5);
            this.lblTotalUsers.Name = "lblTotalUsers";
            this.lblTotalUsers.Size = new System.Drawing.Size(320, 20);
            this.lblTotalUsers.TabIndex = 0;
            this.lblTotalUsers.Text = "Всего пользователей: 0";
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.panelRight.Controls.Add(this.panelChatHeader);
            this.panelRight.Controls.Add(this.lstMessages);
            this.panelRight.Controls.Add(this.panelMessageInput);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(350, 70);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(10);
            this.panelRight.Size = new System.Drawing.Size(850, 605);
            this.panelRight.TabIndex = 0;
            // 
            // panelChatHeader
            // 
            this.panelChatHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(58)))));
            this.panelChatHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelChatHeader.Controls.Add(this.picChatAvatar);
            this.panelChatHeader.Controls.Add(this.lblChatName);
            this.panelChatHeader.Controls.Add(this.lblChatInfo);
            this.panelChatHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelChatHeader.Location = new System.Drawing.Point(10, 10);
            this.panelChatHeader.Name = "panelChatHeader";
            this.panelChatHeader.Size = new System.Drawing.Size(830, 70);
            this.panelChatHeader.TabIndex = 0;
            // 
            // picChatAvatar
            // 
            this.picChatAvatar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.picChatAvatar.Location = new System.Drawing.Point(15, 10);
            this.picChatAvatar.Name = "picChatAvatar";
            this.picChatAvatar.Size = new System.Drawing.Size(50, 50);
            this.picChatAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picChatAvatar.TabIndex = 0;
            this.picChatAvatar.TabStop = false;
            // 
            // lblChatName
            // 
            this.lblChatName.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblChatName.ForeColor = System.Drawing.Color.White;
            this.lblChatName.Location = new System.Drawing.Point(75, 15);
            this.lblChatName.Name = "lblChatName";
            this.lblChatName.Size = new System.Drawing.Size(400, 25);
            this.lblChatName.TabIndex = 1;
            this.lblChatName.Text = "Выберите чат";
            // 
            // lblChatInfo
            // 
            this.lblChatInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblChatInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.lblChatInfo.Location = new System.Drawing.Point(75, 40);
            this.lblChatInfo.Name = "lblChatInfo";
            this.lblChatInfo.Size = new System.Drawing.Size(400, 20);
            this.lblChatInfo.TabIndex = 2;
            // 
            // lstMessages
            // 
            this.lstMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMessages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.lstMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstMessages.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstMessages.ForeColor = System.Drawing.Color.White;
            this.lstMessages.IntegralHeight = false;
            this.lstMessages.ItemHeight = 60;
            this.lstMessages.Location = new System.Drawing.Point(10, 90);
            this.lstMessages.Name = "lstMessages";
            this.lstMessages.Size = new System.Drawing.Size(830, 435);
            this.lstMessages.TabIndex = 1;
            // 
            // panelMessageInput
            // 
            this.panelMessageInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMessageInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(58)))));
            this.panelMessageInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMessageInput.Controls.Add(this.txtMessage);
            this.panelMessageInput.Controls.Add(this.btnSend);
            this.panelMessageInput.Location = new System.Drawing.Point(10, 535);
            this.panelMessageInput.Name = "panelMessageInput";
            this.panelMessageInput.Size = new System.Drawing.Size(830, 60);
            this.panelMessageInput.TabIndex = 2;
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(80)))));
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtMessage.ForeColor = System.Drawing.Color.White;
            this.txtMessage.Location = new System.Drawing.Point(15, 10);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(655, 40);
            this.txtMessage.TabIndex = 0;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Enabled = false;
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSend.ForeColor = System.Drawing.Color.Black;
            this.btnSend.Location = new System.Drawing.Point(680, 5);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(140, 50);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Отправить";
            this.btnSend.UseVisualStyleBackColor = false;
            // 
            // panelStatusBar
            // 
            this.panelStatusBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(30)))));
            this.panelStatusBar.Controls.Add(this.lblConnectionStatus);
            this.panelStatusBar.Controls.Add(this.lblServerInfo);
            this.panelStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatusBar.Location = new System.Drawing.Point(0, 675);
            this.panelStatusBar.Name = "panelStatusBar";
            this.panelStatusBar.Size = new System.Drawing.Size(1200, 25);
            this.panelStatusBar.TabIndex = 3;
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblConnectionStatus.Location = new System.Drawing.Point(10, 5);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(200, 15);
            this.lblConnectionStatus.TabIndex = 0;
            this.lblConnectionStatus.Text = "● Не подключено";
            // 
            // lblServerInfo
            // 
            this.lblServerInfo.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblServerInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblServerInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(200)))));
            this.lblServerInfo.Location = new System.Drawing.Point(980, 5);
            this.lblServerInfo.Name = "lblServerInfo";
            this.lblServerInfo.Size = new System.Drawing.Size(200, 15);
            this.lblServerInfo.TabIndex = 1;
            this.lblServerInfo.Text = "Сервер: не подключен";
            this.lblServerInfo.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // MainForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelStatusBar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "MainForm";
            this.Text = "Корпоративный мессенджер";
            this.panelTop.ResumeLayout(false);
            this.panelTopGradient.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picUserAvatar)).EndInit();
            this.panelLeft.ResumeLayout(false);
            this.panelLeftHeader.ResumeLayout(false);
            this.panelLeftHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.panelLeftFooter.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.panelChatHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picChatAvatar)).EndInit();
            this.panelMessageInput.ResumeLayout(false);
            this.panelMessageInput.PerformLayout();
            this.panelStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}