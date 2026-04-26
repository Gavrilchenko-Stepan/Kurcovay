namespace Messenger.Client
{
    partial class ViewParticipantsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblChatName;
        private System.Windows.Forms.Label lblParticipantCount;
        private System.Windows.Forms.ListView listViewParticipants;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colPosition;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Button btnClose;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblChatName = new System.Windows.Forms.Label();
            this.lblParticipantCount = new System.Windows.Forms.Label();
            this.listViewParticipants = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPosition = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.lblTitle.Location = new System.Drawing.Point(10, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(394, 26);
            this.lblTitle.TabIndex = 7;
            this.lblTitle.Text = "Участники чата";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblChatName
            // 
            this.lblChatName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblChatName.ForeColor = System.Drawing.Color.White;
            this.lblChatName.Location = new System.Drawing.Point(10, 38);
            this.lblChatName.Name = "lblChatName";
            this.lblChatName.Size = new System.Drawing.Size(257, 22);
            this.lblChatName.TabIndex = 6;
            this.lblChatName.Text = "Чат";
            // 
            // lblParticipantCount
            // 
            this.lblParticipantCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblParticipantCount.ForeColor = System.Drawing.Color.Gray;
            this.lblParticipantCount.Location = new System.Drawing.Point(274, 42);
            this.lblParticipantCount.Name = "lblParticipantCount";
            this.lblParticipantCount.Size = new System.Drawing.Size(129, 17);
            this.lblParticipantCount.TabIndex = 5;
            this.lblParticipantCount.Text = "Участников: 0";
            this.lblParticipantCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // listViewParticipants
            // 
            this.listViewParticipants.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(58)))));
            this.listViewParticipants.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colPosition,
            this.colStatus});
            this.listViewParticipants.FullRowSelect = true;
            this.listViewParticipants.GridLines = true;
            this.listViewParticipants.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewParticipants.HideSelection = false;
            this.listViewParticipants.Location = new System.Drawing.Point(15, 87);
            this.listViewParticipants.Name = "listViewParticipants";
            this.listViewParticipants.Size = new System.Drawing.Size(462, 288);
            this.listViewParticipants.TabIndex = 3;
            this.listViewParticipants.UseCompatibleStateImageBehavior = false;
            this.listViewParticipants.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Участник";
            this.colName.Width = 180;
            // 
            // colPosition
            // 
            this.colPosition.Text = "Должность";
            this.colPosition.Width = 180;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Статус";
            this.colStatus.Width = 100;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.btnClose.Location = new System.Drawing.Point(391, 393);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 26);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // ViewParticipantsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(497, 431);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.listViewParticipants);
            this.Controls.Add(this.lblParticipantCount);
            this.Controls.Add(this.lblChatName);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ViewParticipantsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Участники чата";
            this.ResumeLayout(false);

        }
    }

        #endregion
}