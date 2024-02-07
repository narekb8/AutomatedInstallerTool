namespace WinFormsTest
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            checkedListBox1 = new CheckedListBox();
            refreshList = new Button();
            installButton = new Button();
            button1 = new Button();
            SuspendLayout();
            // 
            // checkedListBox1
            // 
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new Point(12, 12);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new Size(354, 382);
            checkedListBox1.TabIndex = 0;
            // 
            // refreshList
            // 
            refreshList.Location = new Point(12, 408);
            refreshList.Name = "refreshList";
            refreshList.Size = new Size(110, 28);
            refreshList.TabIndex = 1;
            refreshList.Text = "Refresh List";
            refreshList.UseVisualStyleBackColor = true;
            refreshList.Click += refreshList_Click;
            // 
            // installButton
            // 
            installButton.Location = new Point(134, 408);
            installButton.Name = "installButton";
            installButton.Size = new Size(110, 28);
            installButton.TabIndex = 2;
            installButton.Text = "Install";
            installButton.UseVisualStyleBackColor = true;
            installButton.Click += installButton_Click;
            // 
            // button1
            // 
            button1.Location = new Point(256, 408);
            button1.Name = "button1";
            button1.Size = new Size(110, 28);
            button1.TabIndex = 3;
            button1.Text = "Install";
            button1.UseVisualStyleBackColor = true;
            button1.Click += this.button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(382, 450);
            Controls.Add(button1);
            Controls.Add(installButton);
            Controls.Add(refreshList);
            Controls.Add(checkedListBox1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private CheckedListBox checkedListBox1;
        private Button refreshList;
        private Button installButton;
        private Button button1;
    }
}
