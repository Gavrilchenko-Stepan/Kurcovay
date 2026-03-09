namespace Messenger.Client
{
    partial class ManageParticipantsForm
    {
        private System.ComponentModel.IContainer components = null;

        // Элементы управления
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblParticipants;
        private System.Windows.Forms.ListBox lstParticipants;
        private System.Windows.Forms.Label lblAvailable;
        private System.Windows.Forms.ListBox lstAvailable;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClose;

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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblParticipants = new System.Windows.Forms.Label();
            this.lstParticipants = new System.Windows.Forms.ListBox();
            this.lblAvailable = new System.Windows.Forms.Label();
            this.lstAvailable = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(281, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Управление участниками чата";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblParticipants
            this.lblParticipants.AutoSize = true;
            this.lblParticipants.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblParticipants.ForeColor = System.Drawing.Color.White;
            this.lblParticipants.Location = new System.Drawing.Point(20, 60);
            this.lblParticipants.Name = "lblParticipants";
            this.lblParticipants.Size = new System.Drawing.Size(170, 19);
            this.lblParticipants.TabIndex = 1;
            this.lblParticipants.Text = "Текущие участники:";

            // lstParticipants
            this.lstParticipants.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.lstParticipants.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstParticipants.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstParticipants.ForeColor = System.Drawing.Color.White;
            this.lstParticipants.FormattingEnabled = true;
            this.lstParticipants.ItemHeight = 17;
            this.lstParticipants.Location = new System.Drawing.Point(20, 85);
            this.lstParticipants.Name = "lstParticipants";
            this.lstParticipants.Size = new System.Drawing.Size(250, 238);
            this.lstParticipants.TabIndex = 2;

            // lblAvailable
            this.lblParticipants.AutoSize = true;
            this.lblAvailable.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblAvailable.ForeColor = System.Drawing.Color.White;
            this.lblAvailable.Location = new System.Drawing.Point(300, 60);
            this.lblAvailable.Name = "lblAvailable";
            this.lblAvailable.Size = new System.Drawing.Size(165, 19);
            this.lblAvailable.TabIndex = 3;
            this.lblAvailable.Text = "Доступные пользователи:";

            // lstAvailable
            this.lstAvailable.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            this.lstAvailable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstAvailable.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstAvailable.ForeColor = System.Drawing.Color.White;
            this.lstAvailable.FormattingEnabled = true;
            this.lstAvailable.ItemHeight = 17;
            this.lstAvailable.Location = new System.Drawing.Point(300, 85);
            this.lstAvailable.Name = "lstAvailable";
            this.lstAvailable.Size = new System.Drawing.Size(250, 238);
            this.lstAvailable.TabIndex = 4;

            // btnAdd
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.Black;
            this.btnAdd.Location = new System.Drawing.Point(20, 340);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(120, 35);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "Добавить →";
            this.btnAdd.UseVisualStyleBackColor = false;

            // btnRemove
            this.btnRemove.BackColor = System.Drawing.Color.Transparent;
            this.btnRemove.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRemove.ForeColor = System.Drawing.Color.FromArgb(0, 229, 255);
            this.btnRemove.Location = new System.Drawing.Point(160, 340);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(120, 35);
            this.btnRemove.TabIndex = 6;
            this.btnRemove.Text = "← Удалить";
            this.btnRemove.UseVisualStyleBackColor = false;

            // btnClose
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(255, 80, 80);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(255, 80, 80);
            this.btnClose.Location = new System.Drawing.Point(430, 340);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 35);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = false;

            // ManageParticipantsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(45, 45, 58);
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstAvailable);
            this.Controls.Add(this.lblAvailable);
            this.Controls.Add(this.lstParticipants);
            this.Controls.Add(this.lblParticipants);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManageParticipantsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Управление участниками";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}