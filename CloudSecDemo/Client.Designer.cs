namespace CloudSecDemo
{
	partial class Client
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.listView1 = new System.Windows.Forms.ListView();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.button8 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.button4 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.tabControl1.Location = new System.Drawing.Point(0, 2);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(796, 444);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.listView1);
			this.tabPage1.Controls.Add(this.button3);
			this.tabPage1.Controls.Add(this.button2);
			this.tabPage1.Controls.Add(this.textBox1);
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(788, 415);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Auto";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(52, 29);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(172, 15);
			this.label2.TabIndex = 7;
			this.label2.Text = "请选择一个本地文件夹：";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(52, 120);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 15);
			this.label1.TabIndex = 4;
			this.label1.Text = "文件列表：";
			// 
			// listView1
			// 
			this.listView1.BackColor = System.Drawing.SystemColors.InactiveBorder;
			this.listView1.Location = new System.Drawing.Point(55, 138);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(619, 248);
			this.listView1.TabIndex = 3;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.List;
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.button3.Location = new System.Drawing.Point(599, 61);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 25);
			this.button3.TabIndex = 2;
			this.button3.Text = "确定";
			this.button3.UseVisualStyleBackColor = false;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.button2.Location = new System.Drawing.Point(508, 61);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(76, 25);
			this.button2.TabIndex = 1;
			this.button2.Text = "选择";
			this.button2.UseVisualStyleBackColor = false;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.SystemColors.InactiveBorder;
			this.textBox1.Location = new System.Drawing.Point(55, 63);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(427, 25);
			this.textBox1.TabIndex = 0;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.button8);
			this.tabPage2.Controls.Add(this.button7);
			this.tabPage2.Controls.Add(this.listBox1);
			this.tabPage2.Controls.Add(this.label4);
			this.tabPage2.Controls.Add(this.button4);
			this.tabPage2.Controls.Add(this.button1);
			this.tabPage2.Controls.Add(this.textBox2);
			this.tabPage2.Controls.Add(this.label3);
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(788, 415);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "specific";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// button8
			// 
			this.button8.Location = new System.Drawing.Point(499, 102);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(92, 39);
			this.button8.TabIndex = 7;
			this.button8.Text = "刷新";
			this.button8.UseVisualStyleBackColor = true;
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(646, 102);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(92, 39);
			this.button7.TabIndex = 6;
			this.button7.Text = "下载";
			this.button7.UseVisualStyleBackColor = true;
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 15;
			this.listBox1.Location = new System.Drawing.Point(46, 162);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(692, 229);
			this.listBox1.TabIndex = 5;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(43, 122);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(82, 15);
			this.label4.TabIndex = 4;
			this.label4.Text = "文件列表：";
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(646, 47);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(92, 39);
			this.button4.TabIndex = 3;
			this.button4.Text = "上传";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(499, 47);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(92, 39);
			this.button1.TabIndex = 2;
			this.button1.Text = "选择";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(46, 56);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(394, 25);
			this.textBox2.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(43, 21);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(82, 15);
			this.label3.TabIndex = 0;
			this.label3.Text = "选择文件：";
			// 
			// Client
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.InactiveCaption;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.tabControl1);
			this.ForeColor = System.Drawing.SystemColors.Desktop;
			this.Name = "Client";
			this.Text = "Client";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Client_FormClosing);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Label label4;
	}
}

