namespace CloudSecDemo
{
	partial class Server
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button4 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Location = new System.Drawing.Point(2, 3);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(796, 444);
			this.tabControl1.TabIndex = 1;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.listBox1);
			this.tabPage2.Controls.Add(this.button4);
			this.tabPage2.Controls.Add(this.label2);
			this.tabPage2.Controls.Add(this.button1);
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(788, 415);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "云";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 15;
			this.listBox1.Location = new System.Drawing.Point(50, 136);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(669, 229);
			this.listBox1.TabIndex = 4;
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(455, 46);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(118, 40);
			this.button4.TabIndex = 3;
			this.button4.Text = "初始化";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label2.Location = new System.Drawing.Point(46, 66);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(149, 20);
			this.label2.TabIndex = 2;
			this.label2.Text = "云端实时信息：";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(601, 46);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(118, 40);
			this.button1.TabIndex = 0;
			this.button1.Text = "启动";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.button2);
			this.tabPage1.Controls.Add(this.listBox2);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(788, 415);
			this.tabPage1.TabIndex = 2;
			this.tabPage1.Text = "文件列表";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// listBox2
			// 
			this.listBox2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.listBox2.FormattingEnabled = true;
			this.listBox2.ItemHeight = 15;
			this.listBox2.Location = new System.Drawing.Point(35, 130);
			this.listBox2.Name = "listBox2";
			this.listBox2.Size = new System.Drawing.Size(675, 229);
			this.listBox2.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label1.Location = new System.Drawing.Point(29, 57);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(149, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "云端文件列表：";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(600, 45);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(110, 43);
			this.button2.TabIndex = 2;
			this.button2.Text = "刷新";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// Server
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.tabControl1);
			this.Name = "Server";
			this.Text = "Server";
			this.tabControl1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.ListBox listBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button2;
	}
}