using System.Drawing;
using System.Windows.Forms;

namespace Messenger.Client
{
    partial class ManageParticipantsForm
    {
        private System.ComponentModel.IContainer components = null;

        // Элементы управления
        private Label lblTitle;
        private Label lblParticipants;
        private ListBox lstParticipants;
        private Label lblAvailable;
        private TreeView tvAvailable;   // заменяем ListBox на TreeView
        private Button btnAdd;
        private Button btnRemove;
        private Button btnClose;

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
            this.lblTitle = new Label();
            this.lblParticipants = new Label();
            this.lstParticipants = new ListBox();
            this.lblAvailable = new Label();
            this.tvAvailable = new TreeView();   // новый TreeView
            this.btnAdd = new Button();
            this.btnRemove = new Button();
            this.btnClose = new Button();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(0, 229, 255);
            this.lblTitle.Location = new Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(281, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Управление участниками чата";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // lblParticipants
            this.lblParticipants.AutoSize = true;
            this.lblParticipants.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblParticipants.ForeColor = Color.White;
            this.lblParticipants.Location = new Point(20, 60);
            this.lblParticipants.Name = "lblParticipants";
            this.lblParticipants.Size = new Size(170, 19);
            this.lblParticipants.TabIndex = 1;
            this.lblParticipants.Text = "Текущие участники:";

            // lstParticipants
            this.lstParticipants.BackColor = Color.FromArgb(60, 60, 80);
            this.lstParticipants.BorderStyle = BorderStyle.None;
            this.lstParticipants.Font = new Font("Segoe UI", 10F);
            this.lstParticipants.ForeColor = Color.White;
            this.lstParticipants.FormattingEnabled = true;
            this.lstParticipants.IntegralHeight = false;
            this.lstParticipants.ItemHeight = 17;
            this.lstParticipants.Location = new Point(20, 85);
            this.lstParticipants.Name = "lstParticipants";
            this.lstParticipants.Size = new Size(250, 300);
            this.lstParticipants.TabIndex = 2;

            // lblAvailable
            this.lblAvailable.AutoSize = true;
            this.lblAvailable.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblAvailable.ForeColor = Color.White;
            this.lblAvailable.Location = new Point(300, 60);
            this.lblAvailable.Name = "lblAvailable";
            this.lblAvailable.Size = new Size(165, 19);
            this.lblAvailable.TabIndex = 3;
            this.lblAvailable.Text = "Доступные пользователи:";

            // tvAvailable
            this.tvAvailable.BackColor = Color.FromArgb(60, 60, 80);
            this.tvAvailable.BorderStyle = BorderStyle.None;
            this.tvAvailable.Font = new Font("Segoe UI", 10F);
            this.tvAvailable.ForeColor = Color.White;
            this.tvAvailable.Location = new Point(300, 85);
            this.tvAvailable.Name = "tvAvailable";
            this.tvAvailable.Size = new Size(250, 300);
            this.tvAvailable.TabIndex = 4;
            this.tvAvailable.NodeMouseDoubleClick += TvAvailable_NodeMouseDoubleClick; // обработчик двойного клика

            // btnAdd
            this.btnAdd.BackColor = Color.FromArgb(0, 229, 255);
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdd.ForeColor = Color.Black;
            this.btnAdd.Location = new Point(20, 400);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new Size(120, 35);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "Добавить →";
            this.btnAdd.UseVisualStyleBackColor = false;

            // btnRemove
            this.btnRemove.BackColor = Color.Transparent;
            this.btnRemove.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
            this.btnRemove.FlatStyle = FlatStyle.Flat;
            this.btnRemove.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnRemove.ForeColor = Color.FromArgb(0, 229, 255);
            this.btnRemove.Location = new Point(160, 400);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new Size(120, 35);
            this.btnRemove.TabIndex = 6;
            this.btnRemove.Text = "← Удалить";
            this.btnRemove.UseVisualStyleBackColor = false;

            // btnClose
            this.btnClose.BackColor = Color.Transparent;
            this.btnClose.FlatAppearance.BorderColor = Color.FromArgb(255, 80, 80);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClose.ForeColor = Color.FromArgb(255, 80, 80);
            this.btnClose.Location = new Point(430, 400);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(120, 35);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = false;

            // ManageParticipantsForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(45, 45, 58);
            this.ClientSize = new Size(584, 461);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.tvAvailable);
            this.Controls.Add(this.lblAvailable);
            this.Controls.Add(this.lstParticipants);
            this.Controls.Add(this.lblParticipants);
            this.Controls.Add(this.lblTitle);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManageParticipantsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Управление участниками";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}