using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;
using System.IO;

namespace CloudSecDemo
{
	class ServiceManager
	{
		
		private TcpListener tcpListener;
		private int Port;
		private string serFilePath = "./ServerFiles";
		const int resLength = 1025;
		const int reqLength = 256;
		public HashSet<string> onlineUser;

		public ServiceManager(int p)  //以端口号为参数的构造函数
		{
			onlineUser = new HashSet<string>();
			onlineUser.Add("admin");
			Port = p;
		}  

		public void Start()   //启动
		{
			try
			{
				tcpListener = new TcpListener(IPAddress.IPv6Any, Port);
				tcpListener.Start();
				Thread th = new Thread(ListenTh);  //分出负责监听连接的线程
				th.IsBackground = true;
				th.Start();
				//infoDispText.AppendText(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + "服务器启动成功\r\n");
				//Start.Visible = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("启动失败，检查端口是否被占用。错误信息： \r\n" + ex.ToString());
			}
		} 

		private void ListenTh()  //监听连接
		{
			while (true)
			{
				TcpClient client = tcpListener.AcceptTcpClient();

				if (client.Connected)
				{
					Thread recvTh = new Thread(RecvClient);  //为每个客户单独分出线程处理
					recvTh.IsBackground = true;
					recvTh.Start(client);
				}
			}
		}

		public void InitProcess()
		{
			DataBaseManager dbm = new DataBaseManager();
			dbm.InitProcess();
			DirectoryInfo dir = new DirectoryInfo(serFilePath);
			FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  
			foreach (FileSystemInfo i in fileinfo)
			{
				if (i is DirectoryInfo)
				{
					DirectoryInfo subdir = new DirectoryInfo(i.FullName);
					subdir.Delete(true);
				}
				else
				{
					File.Delete(i.FullName);
				}
			}
		}

		private void RecvClient(object obj)
		{
			bool hasLogin = false;
			TcpClient client = obj as TcpClient;
			NetworkStream networkStream = client.GetStream();
			byte[] requestMsg = new byte[reqLength];
			byte[] responseMsg = new byte[resLength];
			string userName = string.Empty;
			int readLength = networkStream.Read(requestMsg, 0, reqLength);
			int userNameLength = BitConverter.ToInt32(requestMsg, 1);
			if (userNameLength > 10)
			{
				MessageBox.Show("User name too long");
				return;
			}
			userName = Encoding.Default.GetString(requestMsg, 5, userNameLength);
			if (readLength <= 0)
				return;
			if (onlineUser.Contains(userName))
				hasLogin = true;
			if (requestMsg[0] == 0x70)
			{
				//登录请求
				LoginRequest(ref userName,ref hasLogin, ref requestMsg, ref responseMsg);
			}
			if (!CheckLogin(hasLogin, ref responseMsg))
				networkStream.Write(responseMsg, 0, responseMsg.Length);
			else if (requestMsg[0] == 0x71)
			{
				//注销操作
				LogoutRequest(ref userName, ref hasLogin, ref responseMsg);
				networkStream.Write(responseMsg, 0, responseMsg.Length);
			}
			else if (requestMsg[0] == 0x72)
			{
				//拉取列表请求
				GetListRequest(ref userName, ref responseMsg);
				networkStream.Write(responseMsg, 0, responseMsg.Length);
			}
			else if (requestMsg[0] == 0x73)
			{
				//上传文件
				RecvUploadFile(ref userName, ref requestMsg, ref responseMsg, ref networkStream);
			}
			else if (requestMsg[0] == 0x74)
			{
				//下载文件
				SendDownloadFile(ref userName, ref requestMsg, ref responseMsg, ref networkStream);
			}
			else if (requestMsg[0] == 0x75)
			{
				//删除文件
				DeleteUserFile(ref userName, ref requestMsg, ref responseMsg);
				networkStream.Write(responseMsg, 0, responseMsg.Length);
			}
			else if (requestMsg[0] == 0x76)
			{
				RenameUserFile(ref userName, ref requestMsg, ref responseMsg);
				networkStream.Write(responseMsg, 0, responseMsg.Length);
			}
			else
			{
				MessageBox.Show("无效请求");
			}
	
		}

		private void LoginRequest(ref string userName, ref bool hasLogin, ref byte[] requestMsg, ref byte[] responseMsg)
		{
			string userPass = string.Empty;
			string message = string.Empty;
			byte[] bUserPass = new byte[16];
			DataBaseManager dm = new DataBaseManager();
			Buffer.BlockCopy(requestMsg, 15, bUserPass, 0, 16);
			for (int i = 0; i < bUserPass.Length; i++)
			{
				userPass += bUserPass[i].ToString("x2");
			}
			int result = dm.LoginAuthentication(userName, userPass);
			if (result > 0)
			{
				//登录成功
				onlineUser.Add(userName);
				hasLogin = true;
				responseMsg[0] = 0x80;
			}
			else if (result == 0)
			{
				//密码错误
				responseMsg[0] = 0x81;
			}
			else
			{
				//用户不存在
				responseMsg[0] = 0x82;
			}
		}
		private bool CheckLogin(bool hasLogin, ref byte[] responseMsg)
		{
			if (!hasLogin)
			{
				responseMsg[0] = 0x83;
			}
			return hasLogin;
			
		}
		private void LogoutRequest(ref string userName, ref bool hasLogin, ref byte[] responseMsg)
		{
			onlineUser.Remove(userName);
			hasLogin = false;
			responseMsg[0] = 0x90;
		}
		private void GetListRequest(ref string userName, ref byte[] responseMsg)
		{
			DataBaseManager dm = new DataBaseManager();
			string result = dm.GetFileList(userName);
			responseMsg[0] = 0x84;
			byte[] tmp = Encoding.Default.GetBytes(result);
			Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, responseMsg, 1, 4);
			Buffer.BlockCopy(tmp, 0, responseMsg, 5, tmp.Length);
		}		
		private void RecvUploadFile(ref string userName, ref byte[] requestMsg, ref byte[] responseMsg, ref NetworkStream ns)
		{
			DataBaseManager dm = new DataBaseManager();
			int length = BitConverter.ToInt32(requestMsg, 39);
			long fileSize = BitConverter.ToInt64(requestMsg, 31);
			string user_fileName = Encoding.Default.GetString(requestMsg, 43, length);
			byte[] bmd5 = new byte[16];
			Buffer.BlockCopy(requestMsg, 15, bmd5, 0, 16);
			string ser_fileName = string.Empty;
			foreach (var i in bmd5)
				ser_fileName += i.ToString("x2");
			string ser_filePath = "./ServerFiles/" + ser_fileName;
			if (File.Exists(ser_filePath))  //文件重复
			{
				responseMsg[0] = 0x86;
				ns.Write(responseMsg, 0, responseMsg.Length);
				dm.MaintainUpFileTableAndFileTable(user_fileName, fileSize, userName, ser_fileName, ser_filePath);
				return;
			}
			responseMsg[0] = 0x60;
			ns.Write(responseMsg, 0, 1025);
			FileStream fs = new FileStream(ser_filePath, FileMode.Create, FileAccess.Write);
			byte[] fileData = new byte[1025];
			int readLength;
			while(true)
			{
				readLength = ns.Read(fileData, 0, 1025);
				fs.Write(fileData, 1, readLength - 1);
				if (fileData[0] == 0x62)
					break;
			}
			fs.Close();
			dm.MaintainUpFileTableAndFileTable(user_fileName, fileSize, userName, ser_fileName, ser_filePath);
			return;
		}
		private void SendDownloadFile(ref string userName, ref byte[] requestMsg, ref byte[] responseMsg, ref NetworkStream ns)
		{
			DataBaseManager dm = new DataBaseManager();
			int length = BitConverter.ToInt32(requestMsg, 39);
			string user_fileName = Encoding.Default.GetString(requestMsg, 43, length);
			string ser_filePath = dm.GetFilePath(userName, user_fileName);
			if (ser_filePath == "")
			{
				responseMsg[0] = 0x87;
				ns.Write(responseMsg, 0, responseMsg.Length);
				return;
			}
			responseMsg[0] = 0x85;
			byte[] tmp = Encoding.Default.GetBytes(Path.GetFileName(ser_filePath));
			Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, responseMsg, 1, 4);
			Buffer.BlockCopy(tmp, 0, responseMsg, 5, tmp.Length);
			ns.Write(responseMsg, 0, responseMsg.Length);
			ns.Read(requestMsg, 0, requestMsg.Length);
			byte[] fileData = new byte[1025];
			fileData[0] = 0x61;
			FileStream fs = new FileStream(ser_filePath, FileMode.Open, FileAccess.Read);
			long leftSize = fs.Length;
			int readLength;
			while((readLength = fs.Read(fileData, 1, 1024)) > 0)
			{
				leftSize -= readLength;
				if (leftSize <= 0)
					fileData[0] = 0x62;
				ns.Write(fileData, 0, 1 + readLength);
			}
			fs.Close();
		}
		private void DeleteUserFile(ref string userName, ref byte[] requestMsg, ref byte[] responseMsg)
		{
			DataBaseManager dm = new DataBaseManager();
			int length = BitConverter.ToInt32(requestMsg, 39);
			string user_fileName = Encoding.Default.GetString(requestMsg, 43, length);
			if (dm.DeleteUpFileTableItem(userName, user_fileName) > 0)
				responseMsg[0] = 0x90;
			else
				responseMsg[0] = 0x87;
		}
		private void RenameUserFile(ref string userName, ref byte[] requestMsg, ref byte[] responseMsg)
		{
			DataBaseManager dm = new DataBaseManager();
			int length = BitConverter.ToInt32(requestMsg, 31);
			int len = length;
			string fileName = Encoding.Default.GetString(requestMsg, 35, length);
			length = BitConverter.ToInt32(requestMsg, 35 + len);
			//MessageBox.Show("old name length: " + length.ToString());
			string oldFileName = Encoding.Default.GetString(requestMsg, 39 + len, length);
			if (dm.UpdateUpFileTable(userName, oldFileName, fileName) > 0)
				responseMsg[0] = 0x90;
			else
				responseMsg[0] = 0x87;
		}
	}
}
