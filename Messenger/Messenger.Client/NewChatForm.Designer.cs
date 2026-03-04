using System.Windows.Forms;

namespace Messenger.Client
{
    partial class NewChatForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.PictureBox picSearch;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDepartment;
        private System.Windows.Forms.TabPage tabPrivate;
        private System.Windows.Forms.TabPage tabGroup;
        private System.Windows.Forms.ListBox lstDepartments;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Label lblChatName;
        private System.Windows.Forms.TextBox txtChatName;
        private System.Windows.Forms.CheckedListBox chkUsers;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;

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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabDepartment = new System.Windows.Forms.TabPage();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.lstDepartments = new System.Windows.Forms.ListBox();
            this.tabPrivate = new System.Windows.Forms.TabPage();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.tabGroup = new System.Windows.Forms.TabPage();
            this.lblChatName = new System.Windows.Forms.Label();
            this.txtChatName = new System.Windows.Forms.TextBox();
            this.chkUsers = new System.Windows.Forms.CheckedListBox();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            this.panelMain.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabDepartment.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.tabPrivate.SuspendLayout();
            this.tabGroup.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();

            // NewChatForm
            this.ClientSize = new System.Drawing.Size(550, 650);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Создать новый чат";
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 46);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            // panelMain
            this.panelMain.BackColor = System.Drawing.Color.FromArgb(45, 45, 58);
            this.panelMain.Controls.Add(this.panelHeader);
            this.panelMain.Controls.Add(this.tabControl);
            this.panelMain.Controls.Add(this.panelFooter);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(20);
            this.panelMain.Size = new System.Drawing.Size(550, 650);

            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Controls.Add(this.lblSubtitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(20, 20);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(510, 90);

            // lblTitle
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.lblTitle.Location = new System.Drawing.Point(0, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(510, 30);
            this.lblTitle.Text = "Создать новый чат";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblSubtitle
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(180, 180, 200);
            this.lblSubtitle.Location = new System.Drawing.Point(0, 50);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(510, 25);
            this.lblSubtitle.Text = "Выберите тип чата и участников";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // tabControl
            this.tabControl.Controls.Add(this.tabDepartment);
            this.tabControl.Controls.Add(this.tabPrivate);
            this.tabControl.Controls.Add(this.tabGroup);
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(20, 120);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(510, 430);
            this.tabControl.TabIndex = 1;
            this.tabControl.BackColor = System.Drawing.Color.FromArgb(45, 45, 58);
            this.tabControl.ForeColor = System.Drawing.Color.White;

            // tabDepartment
            this.tabDepartment.BackColor = System.Drawing.Color.FromArgb(45, 45, 58);
            this.tabDepartment.Controls.Add(this.txtSearch);
            this.tabDepartment.Controls.Add(this.picSearch);
            this.tabDepartment.Controls.Add(this.lstDepartments);
            this.tabDepartment.Location = new System.Drawing.Point(4, 26);
            this.tabDepartment.Name = "tabDepartment";
            this.tabDepartment.Padding = new System.Windows.Forms.Padding(15);
            this.tabDepartment.Size = new System.Drawing.Size(502, 400);
            this.tabDepartment.Text = "Отдел";
            this.tabDepartment.ForeColor = System.Drawing.Color.White;

            // txtSearch
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.ForeColor = System.Drawing.Color.Gray;
            this.txtSearch.Location = new System.Drawing.Point(15, 15);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(440, 18);
            this.txtSearch.Text = "Поиск отдела...";

            // picSearch
            this.picSearch.BackColor = System.Drawing.Color.Transparent;
            this.picSearch.Location = new System.Drawing.Point(455, 15);
            this.picSearch.Name = "picSearch";
            this.picSearch.Size = new System.Drawing.Size(20, 20);
            this.picSearch.TabIndex = 1;
            this.picSearch.TabStop = false;

            // lstDepartments
            this.lstDepartments.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstDepartments.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstDepartments.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstDepartments.IntegralHeight = false;
            this.lstDepartments.ItemHeight = 60;
            this.lstDepartments.Location = new System.Drawing.Point(15, 45);
            this.lstDepartments.Name = "lstDepartments";
            this.lstDepartments.Size = new System.Drawing.Size(460, 320);
            this.lstDepartments.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.lstDepartments.ForeColor = System.Drawing.Color.White;

            // tabPrivate
            this.tabPrivate.BackColor = System.Drawing.Color.FromArgb(45, 45, 58);
            this.tabPrivate.Controls.Add(this.lstUsers);
            this.tabPrivate.Location = new System.Drawing.Point(4, 26);
            this.tabPrivate.Name = "tabPrivate";
            this.tabPrivate.Padding = new System.Windows.Forms.Padding(15);
            this.tabPrivate.Size = new System.Drawing.Size(502, 400);
            this.tabPrivate.Text = "Личный";
            this.tabPrivate.ForeColor = System.Drawing.Color.White;

            // lstUsers
            this.lstUsers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstUsers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstUsers.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstUsers.IntegralHeight = false;
            this.lstUsers.ItemHeight = 70;
            this.lstUsers.Location = new System.Drawing.Point(15, 15);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(460, 355);
            this.lstUsers.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.lstUsers.ForeColor = System.Drawing.Color.White;

            // tabGroup
            this.tabGroup.BackColor = System.Drawing.Color.FromArgb(45, 45, 58);
            this.tabGroup.Controls.Add(this.lblChatName);
            this.tabGroup.Controls.Add(this.txtChatName);
            this.tabGroup.Controls.Add(this.chkUsers);
            this.tabGroup.Location = new System.Drawing.Point(4, 26);
            this.tabGroup.Name = "tabGroup";
            this.tabGroup.Padding = new System.Windows.Forms.Padding(15);
            this.tabGroup.Size = new System.Drawing.Size(502, 400);
            this.tabGroup.Text = "Групповой";
            this.tabGroup.ForeColor = System.Drawing.Color.White;

            // lblChatName
            this.lblChatName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblChatName.ForeColor = System.Drawing.Color.White;
            this.lblChatName.Location = new System.Drawing.Point(15, 15);
            this.lblChatName.Name = "lblChatName";
            this.lblChatName.Size = new System.Drawing.Size(120, 20);
            this.lblChatName.Text = "Название чата:";

            // txtChatName
            this.txtChatName.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.txtChatName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtChatName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtChatName.ForeColor = System.Drawing.Color.White;
            this.txtChatName.Location = new System.Drawing.Point(15, 38);
            this.txtChatName.Name = "txtChatName";
            this.txtChatName.Size = new System.Drawing.Size(460, 25);

            // chkUsers
            this.chkUsers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUsers.CheckOnClick = true;
            this.chkUsers.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkUsers.IntegralHeight = false;
            this.chkUsers.Location = new System.Drawing.Point(15, 75);
            this.chkUsers.Name = "chkUsers";
            this.chkUsers.Size = new System.Drawing.Size(460, 295);
            this.chkUsers.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.chkUsers.ForeColor = System.Drawing.Color.White;

            // panelFooter
            this.panelFooter.BackColor = System.Drawing.Color.FromArgb(45, 45, 58);
            this.panelFooter.Controls.Add(this.btnCreate);
            this.panelFooter.Controls.Add(this.btnCancel);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(20, 550);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(510, 80);

            // btnCreate
            this.btnCreate.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCreate.BackColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnCreate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreate.Enabled = false;
            this.btnCreate.FlatAppearance.BorderSize = 0;
            this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreate.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnCreate.ForeColor = System.Drawing.Color.Black;
            this.btnCreate.Location = new System.Drawing.Point(290, 22);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(180, 40);
            this.btnCreate.Text = "Создать чат";
            this.btnCreate.UseVisualStyleBackColor = false;

            // btnCancel
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnCancel.Location = new System.Drawing.Point(90, 22);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(180, 40);
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = false;

            this.Controls.Add(this.panelMain);
            this.panelMain.ResumeLayout(false);
            this.panelHeader.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabDepartment.ResumeLayout(false);
            this.tabDepartment.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.tabPrivate.ResumeLayout(false);
            this.tabGroup.ResumeLayout(false);
            this.tabGroup.PerformLayout();
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}