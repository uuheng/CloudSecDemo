using System;
using System.Windows.Forms;
using System.Configuration;
using System.Threading;
using System.IO;

namespace CloudSecDemo
{
	public partial class Client : Form
	{

		private string workPath = string.Empty;

		private ClientManager clientManager;
		private FileWatcher fw;
		public Client(ClientManager clientManager)
		{
			InitializeComponent();
			textBox1.ReadOnly = true; //设置只读属性
			workPath = ConfigurationManager.AppSettings["TargetDir"];
			textBox1.Text = "";
			this.clientManager = clientManager;
			if (!string.IsNullOrEmpty(workPath) && Directory.Exists(workPath))
			{
				textBox1.Text = workPath;
				textBox1.Enabled = false;
				button2.Enabled = false;
				button3.Enabled = false;
				Thread th = new Thread(SyncTh);
				th.IsBackground = true;
				th.Start();
				fw = new FileWatcher(workPath, "*.*");
				fw.SendEvent += new FileWatcher.DelegateEventHander(clientManager.AnalysesEvent);
				fw.Start();
			}
			UpdateFileList2();
			clientManager.ReturnMsg += new ClientManager.DelegateEventHander(UpdateFileList2);
		}
		private void SyncTh()
		{
			clientManager.SyncProcess();
		}

		private void SetAppSettingConf(string key, string value)
		{
			Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			cfa.AppSettings.Settings[key].Value = value;
			cfa.Save();
			ConfigurationManager.RefreshSection("appSettings");
		}
		//specific
		private void button1_Click(object sender, EventArgs e)  //浏览文件
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			string filePath;
			//fileDialog.Filter="文本文档(*txt)|".txt;对文件类型进行筛选
			if (fileDialog.ShowDialog() == DialogResult.OK)
			{
				filePath = fileDialog.FileName;     //文件路径
				//fileName = fileDialog.SafeFileName; //文件名                
				textBox2.Text = filePath;
			}
		}
		private void button4_Click(object sender, EventArgs e)  //上传
		{
			string filePath = textBox2.Text.Trim();
			Thread th = new Thread(UploadTh);
			th.IsBackground = true;
			th.Start(filePath);
			UpdateFileList1();
		}
		private void UploadTh(object fp)
		{
			string filePath = fp as string;
			clientManager.UploadFileProcess(filePath);
		}
		private void button8_Click(object sender, EventArgs e)
		{
			UpdateFileList1();
		}
		private void button7_Click(object sender, EventArgs e)
		{
			string fileName = listBox1.SelectedItem.ToString().Trim();
			Thread th = new Thread(DownloadTh);
			th.IsBackground = true;
			th.Start(fileName);
		}
		private void DownloadTh(object obj)
		{
			string fileName = obj as string;
			byte res = clientManager.DownloadFileProcess(fileName);
			if (res == DefindedCode.ERROR)
				MessageBox.Show("下载失败");
			else
				MessageBox.Show("下载完成");
			return;
		}

		//auto
		private void button2_Click(object sender, EventArgs e)  //指定工作目录
		{
			//选择目标目录
			FolderBrowserDialog dialog = new FolderBrowserDialog
			{
				Description = "请选择目标目录"
			};

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				workPath = dialog.SelectedPath;
				textBox1.Text = workPath.Trim();
			}
		}

		private void button3_Click(object sender, EventArgs e)  //启动客户端
		{
			if (string.IsNullOrEmpty(workPath))
			{
				MessageBox.Show("未指定目标文件夹");
				return;
			}
			clientManager.workPath = workPath;
			SetAppSettingConf("TargetDir", workPath);
			
			clientManager.SyncProcess();
			//修改配置文件
			fw = new FileWatcher(workPath, "*.*");
			fw.SendEvent += new FileWatcher.DelegateEventHander(clientManager.AnalysesEvent);
			fw.Start();
			button3.Enabled = false;
			button2.Enabled = false;
		}
		

		private void button5_Click(object sender, EventArgs e)  //刷新列表
		{
			UpdateFileList2();
		}

		private void button6_Click(object sender, EventArgs e)
		{
			var items = listView1.SelectedItems;
			string downloadFile = items[0].Text;
			Thread th = new Thread(DownloadTh);
			th.IsBackground = true;
			th.Start(downloadFile);
		}

		private void Client_FormClosing(object sender, FormClosingEventArgs e)
		{
			clientManager.LogoutProcess();
			clientManager = null;
		}

		private void UpdateFileList1()
		{
			string[] fileList = clientManager.GetFileListProcess();
			if (fileList == null)
			{
				MessageBox.Show("获取文件列表失败");
				return;
			}
			listBox1.Items.Clear();
			int len = fileList.Length;
			for(int i=0; i<len; i+=2)
			{
				listBox1.Items.Add(fileList[i]);
			}
		}

		private void UpdateFileList2()
		{
			string[] fileList = clientManager.GetFileListProcess();
			if (fileList == null)
			{
				MessageBox.Show("获取文件列表失败");
				return;
			}
			ClearListView("");
			int len = fileList.Length;
			for(int i=0; i<len; i+=2)
			{
				UpdateListView(fileList[i]);
			}
		}
		private delegate void Delegate(string value);
		private void ClearListView(string arg)
		{
			if (listView1.InvokeRequired)
			{
				Delegate d = new Delegate(ClearListView);
				listView1.Invoke(d, new object[] { arg });
			}
			else
				listView1.Clear();
		}
		private void UpdateListView(string value)
		{
			if (listView1.InvokeRequired)
			{
				Delegate d = new Delegate(UpdateListView);
				listView1.Invoke(d, new object[] { value });
			}
			else
				listView1.Items.Add(value);
		}

	}
}
