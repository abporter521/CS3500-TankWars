namespace TankWars
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.hostLabel = new System.Windows.Forms.Label();
            this.playerLabel = new System.Windows.Forms.Label();
            this.playerText = new System.Windows.Forms.TextBox();
            this.hostText = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hostLabel
            // 
            this.hostLabel.AutoSize = true;
            this.hostLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hostLabel.Location = new System.Drawing.Point(12, 20);
            this.hostLabel.Name = "hostLabel";
            this.hostLabel.Size = new System.Drawing.Size(99, 20);
            this.hostLabel.TabIndex = 0;
            this.hostLabel.Text = "Host Name:";
            // 
            // playerLabel
            // 
            this.playerLabel.AutoSize = true;
            this.playerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerLabel.Location = new System.Drawing.Point(241, 20);
            this.playerLabel.Name = "playerLabel";
            this.playerLabel.Size = new System.Drawing.Size(110, 20);
            this.playerLabel.TabIndex = 1;
            this.playerLabel.Text = "Player Name:";
            // 
            // playerText
            // 
            this.playerText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerText.Location = new System.Drawing.Point(357, 20);
            this.playerText.Name = "playerText";
            this.playerText.Size = new System.Drawing.Size(100, 26);
            this.playerText.TabIndex = 2;
            // 
            // hostText
            // 
            this.hostText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hostText.Location = new System.Drawing.Point(110, 17);
            this.hostText.Name = "hostText";
            this.hostText.Size = new System.Drawing.Size(113, 26);
            this.hostText.TabIndex = 3;
            this.hostText.Text = "localhost";
            // 
            // connectButton
            // 
            this.connectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connectButton.Location = new System.Drawing.Point(481, 17);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(91, 32);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.hostText);
            this.Controls.Add(this.playerText);
            this.Controls.Add(this.playerLabel);
            this.Controls.Add(this.hostLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label hostLabel;
        private System.Windows.Forms.Label playerLabel;
        private System.Windows.Forms.TextBox playerText;
        private System.Windows.Forms.TextBox hostText;
        private System.Windows.Forms.Button connectButton;
    }
}

