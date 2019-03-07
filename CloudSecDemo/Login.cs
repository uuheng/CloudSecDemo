using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CloudSecDemo
{
	public partial class Login : Form
	{
		int maxInput;
		string ipString;
		int port;
		string workPath;

		public ClientManager ClientManager;

		public Login()
		{
			InitializeComponent();
			label3.Text = "";
			maxInput = 10;
			ipString = ConfigurationManager.AppSettings["ServerIP"].ToString();
			port = int.Parse(ConfigurationManager.AppSettings["Port"].ToString());
		}

		private void button1_Click(object sender, EventArgs e)
		{
			label3.Text = "正在连接...";
			ClientManager = new ClientManager(ipString, port);
			string userName = textBox1.Text.Trim();
			string userPass = textBox2.Text.Trim();
			byte status = CheckInput(userName, userPass);
			if (status == DefindedCode.OK)
			{
				try {
					status = ClientManager.LoginProcess(userName, userPass);
				}
				catch
				{
					label3.Text = "请检查网络连接";
					ClientManager = null;
					return;
				}
				switch (status)
				{
					case DefindedCode.LOGSUCCESS:
						DialogResult = DialogResult.OK;
						label3.Text = "登录成功 正在同步...";
						break;
					case DefindedCode.PASSERROR:
						label3.Text = "密码错误";
						break;
					case DefindedCode.USERMISS:
						label3.Text = "用户不存在";
						break;
					default:
						break;
				}
			}
			else if (status == DefindedCode.TOOLONG)
				label3.Text = "输入过长";
		}

		private byte CheckInput(string userName, string userPass)
		{
			if (userName.Length > maxInput || userPass.Length > maxInput)
				return DefindedCode.TOOLONG;
			return DefindedCode.OK;
		}
	}
}
