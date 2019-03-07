using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;

namespace CloudSecDemo
{
	public partial class Server : Form
	{
		string tag;
		string connectionString;
		ServiceManager serviceManager;
		public Server()
		{
			InitializeComponent();
			tag = ConfigurationManager.AppSettings["initTag"].ToString();
			//if (tag == "1")
			//	button4.Visible = false;
			connectionString = ConfigurationManager.ConnectionStrings["FirstConnection"].ToString();
			int listenPort = int.Parse(ConfigurationManager.AppSettings["Port"].ToString());
			serviceManager = new ServiceManager(listenPort, connectionString);
			serviceManager.ReturnMsg += new ServiceManager.ReturnMsgDelegate(UpdateInfoDisp);
		}

		delegate void Delegate(string value);
		public void UpdateInfoDisp(string value)
		{
			if (listBox1.InvokeRequired)
			{
				Delegate d = new Delegate(UpdateInfoDisp);
				listBox1.Invoke(d, new object[] { value });
			}
			else
				listBox1.Items.Add(value);
		}

		private void SetAppSettingConf(string key, string value)
		{
			Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			cfa.AppSettings.Settings[key].Value = value;
			cfa.Save();
			ConfigurationManager.RefreshSection("appSettings");
		}

		private void button4_Click(object sender, EventArgs e) //初始化
		{
			serviceManager.InitProcess();
			SetAppSettingConf("initTag", "1");
			button4.Visible = false;
		}

		private void button1_Click(object sender, EventArgs e) //启动
		{
			serviceManager.Start();
			listBox1.Items.Add(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ": 服务器启动成功");
			button1.Visible = false;
		}

		private void button2_Click(object sender, EventArgs e) //获取云端文件列表
		{
			listBox2.Items.Clear();
			listBox2.Items.Add("云端文件路径：");
			DataBaseManager dbm = new DataBaseManager(connectionString);
			List<string> cloudFiles = dbm.GetCloudFiles();
			foreach(var i in cloudFiles)
			{
				listBox2.Items.Add("./ServerFiles/" + i);
			}
		}
	}
}
