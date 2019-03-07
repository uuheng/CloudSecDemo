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
		private string serStorePath = "./ServerFiles/";
		string connectionString;
		const int resLength = 1025;
		const int reqLength = 256;
		public HashSet<string> onlineUser;
		public delegate void ReturnMsgDelegate(string val);
		public ReturnMsgDelegate ReturnMsg;

		public ServiceManager(int p, string con)  //以端口号为参数的构造函数
		{
			onlineUser = new HashSet<string>();
			Port = p;
			connectionString = con;
		}

		public void InitProcess()
		{
			DataBaseManager dbm = new DataBaseManager(connectionString);
			dbm.InitProcess();
			DirectoryInfo dir = new DirectoryInfo(serStorePath);
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
			ReturnMsg?.Invoke("云端初始化完成");
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

		private void RecvClient(object obj)
		{
			string userName = string.Empty;
			byte[] bHashCode = new byte[16];
			long fileSize = 0;
			string fileName = string.Empty;
			string newName = string.Empty;
			string uploadTime = string.Empty;

			TcpClient client = obj as TcpClient;
			ServerComHelper serverComHelper = new ServerComHelper(client);
			byte[] request = serverComHelper.RecvMsg();
			ReadRequest(request, ref userName, ref bHashCode, ref fileSize, ref uploadTime, ref fileName, ref newName);
			if (!CheckLogin(userName) && request[0] != DefindedCode.LOGIN)
			{
				serverComHelper.MakeResponsePacket(DefindedCode.UNLOGIN); //没有登录
				serverComHelper.SendMsg();
				return;
			}

			byte res;
			string serFileName = serStorePath;
			switch (request[0])
			{
				case DefindedCode.LOGIN:
					res = LoginRequest(userName, bHashCode);
					serverComHelper.MakeResponsePacket(res);
					serverComHelper.SendMsg();
					break;
				case DefindedCode.LOGOUT:
					res = LogoutRequest(userName);
					serverComHelper.MakeResponsePacket(res);
					serverComHelper.SendMsg();
					break;
				case DefindedCode.GETLIST:
					serverComHelper.MakeResponsePacket(DefindedCode.FILELIST);
					serverComHelper.SendMsg();
					serverComHelper.RecvMsg();
					serverComHelper.SendFileList(GetListRequest(userName));
					ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + userName + "拉取文件列表");
					break;
				case DefindedCode.UPLOAD:
					res = UploadRequest(userName, fileName, fileSize, bHashCode, uploadTime);
					serverComHelper.MakeResponsePacket(res);
					serverComHelper.SendMsg();
					if (res == DefindedCode.AGREEUP)
					{
						ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":接收来自" + userName + "的文件");
						for (int i = 0; i < bHashCode.Length; i++)
						{
							serFileName += bHashCode[i].ToString("x2");
						}
						serverComHelper.RecvFile(serFileName);
					}
					else if (res == DefindedCode.FILEEXISTED)
						ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":" + userName + "上传文件已做去重处理");
					ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":" + userName + "上传*" + fileName + "*成功");
					break;
				case DefindedCode.DOWNLOAD:
					ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":" + userName + "请求下载*" + fileName + "*");
					res = DownloadRequest(userName, fileName, ref serFileName);
					serverComHelper.MakeResponsePacket(res);
					serverComHelper.SendMsg();
					if (res == DefindedCode.FILEDOWNLOAD)
					{
						serverComHelper.RecvMsg();
						serverComHelper.SendFile(serFileName);
						ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":" + userName + "下载完成");
					}
					break;
				case DefindedCode.DELETE:
					res = DeleteRequest(userName, fileName);
					serverComHelper.MakeResponsePacket(res);
					serverComHelper.SendMsg();
					ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":" + userName + "删除");
					break;
				case DefindedCode.RENAME:
					res = RenameRequest(userName, fileName, newName);
					serverComHelper.MakeResponsePacket(res);
					serverComHelper.SendMsg();
					ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":" + userName + "重命名");
					break;
				default:
					break;
			}		
	
		}

		private void ReadRequest(byte[] request, ref string userName, ref byte[] bHashCode, ref long fileSize, ref string uploadTime, ref string fileName, ref string newName)
		{
			int readOffset = 5;
			int requestLength = request.Length;
			int userNameLength = BitConverter.ToInt32(request, 1);
			userName = Encoding.Default.GetString(request, readOffset, userNameLength);
			readOffset += userNameLength;
			if (readOffset >= requestLength)
				return;
			Buffer.BlockCopy(request, readOffset, bHashCode, 0, 16);
			readOffset += 16;
			if (readOffset >= requestLength)
				return;
			fileSize = BitConverter.ToInt64(request, readOffset);
			readOffset += 8;
			if (readOffset >= requestLength)
				return;
			uploadTime = Encoding.Default.GetString(request, readOffset, 19);
			readOffset += 19;
			if (readOffset >= requestLength)
				return;
			int fileNameLength = BitConverter.ToInt32(request, readOffset);
			readOffset += 4;
			fileName = Encoding.Default.GetString(request, readOffset, fileNameLength);
			readOffset += fileNameLength;
			if (readOffset >= requestLength)
				return;
			int newNameLength = BitConverter.ToInt32(request, readOffset);
			readOffset += 4;
			newName = Encoding.Default.GetString(request, readOffset, newNameLength);
		}

		private byte LoginRequest(string userName, byte[] bUserPass)
		{
			string userPass = string.Empty;
			DataBaseManager dm = new DataBaseManager(connectionString);
			for (int i = 0; i < bUserPass.Length; i++)
			{
				userPass += bUserPass[i].ToString("x2");
			}
			int result = dm.LoginAuthentication(userName, userPass);
			if (result > 0)  //登录成功
			{
				onlineUser.Add(userName);
				ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ": " + userName + "登录");
				return DefindedCode.LOGSUCCESS;
			}
			else if (result == 0)  //密码错误
				return DefindedCode.PASSERROR;
			else  //用户不存在
				return DefindedCode.USERMISS;
		}

		private bool CheckLogin(string userName)
		{
			return onlineUser.Contains(userName);		
		}

		private byte LogoutRequest(string userName)
		{
			onlineUser.Remove(userName);
			ReturnMsg?.Invoke(DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ":" + userName + " 用户注销");
			return DefindedCode.OK;
		}

		private string GetListRequest(string userName)
		{
			DataBaseManager dm = new DataBaseManager(connectionString);
			string result = dm.GetFileList(userName);
			return result;
		}		

		private byte UploadRequest(string userName, string userFile, long fileSize, byte[] bHashCode, string uploadTime)
		{
			DataBaseManager dm = new DataBaseManager(connectionString);
			string hash = string.Empty;
			for (int i = 0; i < bHashCode.Length; i++)
			{
				hash += bHashCode[i].ToString("x2");
			}
			int status = dm.InsertFile(userFile, fileSize, userName, hash, hash, uploadTime);
			if (status == 1)
				return DefindedCode.AGREEUP;
			return DefindedCode.FILEEXISTED;
		}
		private byte DownloadRequest(string userName, string userFile, ref string serFile)
		{
			DataBaseManager dm = new DataBaseManager(connectionString);
			string hash = dm.GetFilePath(userName, userFile);
			if (hash == "")
				return DefindedCode.ERROR;
			serFile += hash;
			return DefindedCode.FILEDOWNLOAD;
		}
		private byte DeleteRequest(string userName, string userFile)
		{
			DataBaseManager dm = new DataBaseManager(connectionString);
			if (dm.RemoveFile(userName, userFile) > 0)
				return DefindedCode.OK;
			return DefindedCode.DENIED;
		}
		private byte RenameRequest(string userName, string fileName, string newName)
		{
			DataBaseManager dm = new DataBaseManager(connectionString);
			if (dm.RenameFile(userName, fileName, newName) > 0)
				return DefindedCode.OK;
			return DefindedCode.DENIED;
		}
	}
}
