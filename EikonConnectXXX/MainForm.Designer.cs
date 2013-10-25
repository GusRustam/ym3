namespace EikonConnectWF {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.ConnectButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.StatusTextBox = new System.Windows.Forms.TextBox();
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.TryAdfinButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.DataStatusTextBox = new System.Windows.Forms.TextBox();
            this.KillRequestButton = new System.Windows.Forms.Button();
            this.RequestDataButton2 = new System.Windows.Forms.Button();
            this.KillRequestButton2 = new System.Windows.Forms.Button();
            this.ImageButton = new System.Windows.Forms.Button();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.TimeButton = new System.Windows.Forms.Button();
            this.UpdateOrTimeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(22, 36);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 0;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Status";
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.Location = new System.Drawing.Point(76, 10);
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.ReadOnly = true;
            this.StatusTextBox.Size = new System.Drawing.Size(182, 20);
            this.StatusTextBox.TabIndex = 2;
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.Enabled = false;
            this.DisconnectButton.Location = new System.Drawing.Point(183, 36);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(75, 23);
            this.DisconnectButton.TabIndex = 3;
            this.DisconnectButton.Text = "Disconnect";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // TryAdfinButton
            // 
            this.TryAdfinButton.Location = new System.Drawing.Point(22, 229);
            this.TryAdfinButton.Name = "TryAdfinButton";
            this.TryAdfinButton.Size = new System.Drawing.Size(104, 23);
            this.TryAdfinButton.TabIndex = 3;
            this.TryAdfinButton.Text = "Request Data";
            this.TryAdfinButton.UseVisualStyleBackColor = true;
            this.TryAdfinButton.Click += new System.EventHandler(this.TryAdfinButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Data status";
            // 
            // DataStatusTextBox
            // 
            this.DataStatusTextBox.Location = new System.Drawing.Point(22, 130);
            this.DataStatusTextBox.Multiline = true;
            this.DataStatusTextBox.Name = "DataStatusTextBox";
            this.DataStatusTextBox.ReadOnly = true;
            this.DataStatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DataStatusTextBox.Size = new System.Drawing.Size(640, 93);
            this.DataStatusTextBox.TabIndex = 2;
            // 
            // KillRequestButton
            // 
            this.KillRequestButton.Location = new System.Drawing.Point(154, 229);
            this.KillRequestButton.Name = "KillRequestButton";
            this.KillRequestButton.Size = new System.Drawing.Size(104, 23);
            this.KillRequestButton.TabIndex = 3;
            this.KillRequestButton.Text = "Kill Request";
            this.KillRequestButton.UseVisualStyleBackColor = true;
            this.KillRequestButton.Click += new System.EventHandler(this.KillRequestButton_Click);
            // 
            // RequestDataButton2
            // 
            this.RequestDataButton2.Location = new System.Drawing.Point(22, 330);
            this.RequestDataButton2.Name = "RequestDataButton2";
            this.RequestDataButton2.Size = new System.Drawing.Size(104, 23);
            this.RequestDataButton2.TabIndex = 3;
            this.RequestDataButton2.Text = "Create";
            this.RequestDataButton2.UseVisualStyleBackColor = true;
            this.RequestDataButton2.Click += new System.EventHandler(this.RequestDataButton2_Click);
            // 
            // KillRequestButton2
            // 
            this.KillRequestButton2.Location = new System.Drawing.Point(132, 330);
            this.KillRequestButton2.Name = "KillRequestButton2";
            this.KillRequestButton2.Size = new System.Drawing.Size(126, 23);
            this.KillRequestButton2.TabIndex = 3;
            this.KillRequestButton2.Text = "Kill Request";
            this.KillRequestButton2.UseVisualStyleBackColor = true;
            this.KillRequestButton2.Click += new System.EventHandler(this.KillRequestButton2_Click);
            // 
            // ImageButton
            // 
            this.ImageButton.Location = new System.Drawing.Point(264, 330);
            this.ImageButton.Name = "ImageButton";
            this.ImageButton.Size = new System.Drawing.Size(75, 23);
            this.ImageButton.TabIndex = 3;
            this.ImageButton.Text = "Image";
            this.ImageButton.UseVisualStyleBackColor = true;
            this.ImageButton.Click += new System.EventHandler(this.ImageButton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.Location = new System.Drawing.Point(345, 330);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(75, 23);
            this.UpdateButton.TabIndex = 3;
            this.UpdateButton.Text = "Update";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // TimeButton
            // 
            this.TimeButton.Location = new System.Drawing.Point(426, 330);
            this.TimeButton.Name = "TimeButton";
            this.TimeButton.Size = new System.Drawing.Size(75, 23);
            this.TimeButton.TabIndex = 3;
            this.TimeButton.Text = "Time";
            this.TimeButton.UseVisualStyleBackColor = true;
            this.TimeButton.Click += new System.EventHandler(this.TimeButton_Click);
            // 
            // UpdateOrTimeButton
            // 
            this.UpdateOrTimeButton.Location = new System.Drawing.Point(507, 330);
            this.UpdateOrTimeButton.Name = "UpdateOrTimeButton";
            this.UpdateOrTimeButton.Size = new System.Drawing.Size(155, 23);
            this.UpdateOrTimeButton.TabIndex = 3;
            this.UpdateOrTimeButton.Text = "Update or Time";
            this.UpdateOrTimeButton.UseVisualStyleBackColor = true;
            this.UpdateOrTimeButton.Click += new System.EventHandler(this.UpdateOrTimeButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 365);
            this.Controls.Add(this.KillRequestButton2);
            this.Controls.Add(this.KillRequestButton);
            this.Controls.Add(this.TimeButton);
            this.Controls.Add(this.UpdateOrTimeButton);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.ImageButton);
            this.Controls.Add(this.RequestDataButton2);
            this.Controls.Add(this.TryAdfinButton);
            this.Controls.Add(this.DisconnectButton);
            this.Controls.Add(this.DataStatusTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.StatusTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ConnectButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "Test";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox StatusTextBox;
        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.Button TryAdfinButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DataStatusTextBox;
        private System.Windows.Forms.Button KillRequestButton;
        private System.Windows.Forms.Button RequestDataButton2;
        private System.Windows.Forms.Button KillRequestButton2;
        private System.Windows.Forms.Button ImageButton;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.Button TimeButton;
        private System.Windows.Forms.Button UpdateOrTimeButton;
    }
}

