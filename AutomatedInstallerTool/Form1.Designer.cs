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
            installButton = new Button();
            button1 = new Button();
            renamePC = new TextBox();
            refreshList = new Button();
            SuspendLayout();
            // 
            // checkedListBox1
            // 
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new Point(14, 16);
            checkedListBox1.Margin = new Padding(3, 4, 3, 4);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new Size(404, 488);
            checkedListBox1.TabIndex = 0;
            // 
            // installButton
            // 
            installButton.Location = new Point(153, 544);
            installButton.Margin = new Padding(3, 4, 3, 4);
            installButton.Name = "installButton";
            installButton.Size = new Size(126, 37);
            installButton.TabIndex = 2;
            installButton.Text = "Install";
            installButton.UseVisualStyleBackColor = true;
            installButton.Click += installButton_Click;
            // 
            // button1
            // 
            button1.Location = new Point(293, 544);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(126, 37);
            button1.TabIndex = 3;
            button1.Text = "Run .bat/.ps1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // renamePC
            // 
            renamePC.Location = new Point(14, 511);
            renamePC.Name = "renamePC";
            renamePC.PlaceholderText = "Insert new computer name here, will execute during 'Install'";
            renamePC.Size = new Size(405, 27);
            renamePC.TabIndex = 4;
            // 
            // refreshList
            // 
            refreshList.Location = new Point(14, 544);
            refreshList.Margin = new Padding(3, 4, 3, 4);
            refreshList.Name = "refreshList";
            refreshList.Size = new Size(126, 37);
            refreshList.TabIndex = 1;
            refreshList.Text = "Refresh List";
            refreshList.UseVisualStyleBackColor = true;
            refreshList.Click += refreshList_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(437, 600);
            Controls.Add(renamePC);
            Controls.Add(button1);
            Controls.Add(installButton);
            Controls.Add(refreshList);
            Controls.Add(checkedListBox1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox checkedListBox1;
        private Button installButton;
        private Button button1;
        private TextBox renamePC;
        private Button refreshList;
    }
}
