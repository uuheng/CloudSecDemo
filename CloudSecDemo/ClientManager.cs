using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;

namespace CloudSecDemo
{
	public class ClientManager
	{
		public string workPath;
		public delegate void DelegateEventHander();
		public DelegateEventHander ReturnMsg;

		private Queue<WatchEvent> eventQueue;
		private string userName;
		private int port;
		private string ipString;
		List<string> upFileList;
		List<string> delFileList;
		string[] fileList;
		public ClientManager(string ip, int p)
		{
			port = p;
			ipString = ip;
			workPath = ConfigurationManager.AppSettings["TargetDir"].ToString();
			eventQueue = new Queue<WatchEvent>();
			System.Timers.Timer t = new System.Timers.Timer(2000);
			t.Elapsed += new System.Timers.ElapsedEventHandler(HandleQueue);
			t.AutoReset = true;
			t.Enabled = true;
			upFileList = new List<string>();
			delFileList = new List<string>();
		}

		public void AnalysesEvent(object sender, WatchEvent we)
		{
			if (we.fileEvent == 0)
				return;
			if (we.fileEvent != 3) //如果是删除操作，文件可能已经不存在
			{
				string fname = Path.GetFileName(we.filePath);
				string fextname = Path.GetExtension(we.filePath);
				if (fname.Length > 2 && fname.Substring(0, 2) == "~$" || fextname == ".tmp") //word临时文件
					return;
				if (File.GetAttributes(we.filePath) == FileAttributes.Hidden)
					return;
				FileInfo fi = new FileInfo(we.filePath);
				if (fi.Length == 0)  //空文件不做处理
					return;
			}
			eventQueue.Enqueue(we);
		}

		private void HandleQueue(object source, System.Timers.ElapsedEventArgs e)
		{
			if (eventQueue.Count <= 0)
				return;
			WatchEvent we;
			while(eventQueue.Count > 0)
			{
				//MessageBox.Show(eventQueue.Count.ToString());
				we = eventQueue.Dequeue();

				if (we.fileEvent == 1)
				{
					//MessageBox.Show("upload:" + we.filePath);
					UploadFileProcess(we.filePath);
					return;
				}
				if (we.fileEvent == 2)
				{
					//MessageBox.Show("change:" + we.filePath);
					UploadFileProcess(we.filePath);
					return;
				}
				if (we.fileEvent == 3)
				{
					//MessageBox.Show("delete:" + we.filePath);
					DeleteFileProcess(Path.GetFileName(we.filePath));
					return;
				}
				if (we.fileEvent == 4)
				{
					//MessageBox.Show("rename:" + we.filePath);
					RenameFileProcess(Path.GetFileName(we.filePath), Path.GetFileName(we.oldFilePath));
					return;
				}
			}
		}

		public void SyncProcess()
		{
			fileList = GetFileListProcess();
			//MessageBox.Show(workPath);
			Director(workPath);
			foreach (string file in upFileList)
			{
				UploadFileProcess(workPath + "/" + file);
			}
			int len = fileList.Length;
			for(int i = 0; i < len; i += 2)
			{
				if (fileList[i] != "")
				{
					//MessageBox.Show(file);
					DownloadFileProcess(fileList[i]);
				}	
			}
		}

		private void Director(string dir) //遍历工作目录下所有文件
		{
			DirectoryInfo d = new DirectoryInfo(dir);
			FileInfo[] files = d.GetFiles();//文件
			DirectoryInfo[] directs = d.GetDirectories();//文件夹
			foreach (FileInfo f in files)
			{
				string tmp = f.Name.Replace(workPath, "");
				int index;
				if ((index = Array.IndexOf(fileList, tmp)) < 0)  //fileList中不存在的需要上传
					upFileList.Add(f.Name);
				else if (index % 2 == 0)
				{
					FileInfo fi = new FileInfo(f.FullName);
					string localT = fi.LastWriteTime.ToString();
					//MessageBox.Show("cloud: " + fileList[index + 1] + "," + "local: " + localT);
					DateTime cloudTime = Convert.ToDateTime(fileList[index + 1]);
					DateTime localTime = Convert.ToDateTime(localT);
					int res = DateTime.Compare(cloudTime, fi.LastWriteTime);
					if (res > 0)
					{
						//MessageBox.Show("Need Download");
						File.Delete(f.FullName);
					}
					else
					{
						if (localT != fileList[index + 1])
						{
							//MessageBox.Show("Need Upload");
							upFileList.Add(f.Name);
						}
						fileList[index] = "";
					}
				}
			}
			//获取子文件夹内的文件列表，递归遍历  
			foreach (DirectoryInfo dd in directs)
			{
				Director(dd.FullName);
			}
		}

		public byte LoginProcess(string userName, string userPass)
		{
			this.userName = userName;
			ClientComHelper clientComHelper = new ClientComHelper(ipString, port, workPath);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] tmp = Encoding.Default.GetBytes(userPass);
			byte[] passMD5 = md5.ComputeHash(tmp);
			clientComHelper.MakeRequestPacket(DefindedCode.LOGIN, userName, passMD5, 0, null, null);
			clientComHelper.SendMsg();
			byte [] response = clientComHelper.RecvMsg();
			return response[0];
		}

		public byte LogoutProcess()
		{
			ClientComHelper clientComHelper = new ClientComHelper(ipString, port, workPath);
			clientComHelper.MakeRequestPacket(DefindedCode.LOGOUT, userName, null, 0, null, null);
			clientComHelper.SendMsg();
			byte[] response = clientComHelper.RecvMsg();
			clientComHelper = null;
			return response[0];
		}

		public string[] GetFileListProcess()
		{
			ClientComHelper clientComHelper = new ClientComHelper(ipString, port, workPath);
			clientComHelper.MakeRequestPacket(DefindedCode.GETLIST, userName, null, 0, null, null);
			clientComHelper.SendMsg();
			byte[] response = clientComHelper.RecvMsg();
			if (response[0] == DefindedCode.FILELIST)
			{
				clientComHelper.MakeRequestPacket(DefindedCode.READY, userName, null, 0, null, null);
				clientComHelper.SendMsg();
				string fileListString = clientComHelper.RecvFileList();
				string[] fileList = Regex.Split(fileListString, "\r\n");
				return fileList;
			}
			MessageBox.Show("需要登录");
			return null;	
		}

		public byte UploadFileProcess(string filePath)
		{
			//MessageBox.Show("UP:" + filePath);
			ClientComHelper clientComHelper = new ClientComHelper(ipString, port, workPath);
			MD5 md5 = new MD5CryptoServiceProvider();
			long fileSize;
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			fileSize = fs.Length;
			byte[] fileMd5 = md5.ComputeHash(fs);
			fs.Close();
			clientComHelper.MakeRequestPacket(DefindedCode.UPLOAD, userName, fileMd5, fileSize, Path.GetFileName(filePath), null);
			clientComHelper.SendMsg();
			byte[] response = clientComHelper.RecvMsg();
			if (response[0] == DefindedCode.AGREEUP)
				clientComHelper.SendFile(filePath);
			ReturnMsg?.Invoke();
			return response[0];
		}

		public byte DownloadFileProcess(string fileName)
		{
			//MessageBox.Show("DOWN:" + fileName);
			string downloadPath = workPath + "/";
			ClientComHelper clientComHelper = new ClientComHelper(ipString, port, workPath);
			clientComHelper.MakeRequestPacket(DefindedCode.DOWNLOAD, userName, null, 0, fileName, null);
			clientComHelper.SendMsg();
			byte[] response = clientComHelper.RecvMsg();
			if (response[0] == DefindedCode.FILEDOWNLOAD)
			{
				clientComHelper.SendMsg();
				clientComHelper.RecvFile(downloadPath + fileName);
			}
			return response[0];
		}

		public byte DeleteFileProcess(string fileName)
		{
			//MessageBox.Show("DEL:" + fileName);
			ClientComHelper clientComHelper = new ClientComHelper(ipString, port, workPath);
			clientComHelper.MakeRequestPacket(DefindedCode.DELETE, userName, null, 0, fileName, null);
			clientComHelper.SendMsg();
			byte[] response = clientComHelper.RecvMsg();
			ReturnMsg?.Invoke();
			return response[0];
		}

		public byte RenameFileProcess(string fileName, string oldName)
		{
			//MessageBox.Show("RENAME:" + oldName + " to " + fileName);
			ClientComHelper clientComHelper = new ClientComHelper(ipString, port, workPath);
			clientComHelper.MakeRequestPacket(DefindedCode.RENAME, userName, null, 0, oldName, fileName);
			clientComHelper.SendMsg();
			byte[] response = clientComHelper.RecvMsg();
			ReturnMsg?.Invoke();
			return response[0];
		}
		//public int UploadFileProcess(string filePath)
		//{
		//	byte[] requestMsg = new byte[256];
		//	if (!File.Exists(filePath))
		//		return 1;   
		//	FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		//	if (fs.Length == 0)
		//	{
		//		fs.Close();
		//		return 2; //文件为空，上传取消
		//	}	
		//	MD5 md5 = new MD5CryptoServiceProvider();
		//	byte[] fileMd5 = md5.ComputeHash(fs);
		//	fs.Close();
		//	string fileName = Path.GetFileName(filePath);
		//	string enFileName = string.Empty;
		//	foreach (var i in fileMd5)
		//		enFileName += i.ToString("x2");
		//	string enFilePath = "./tmp/" + enFileName;
		//	FileCrypto fc = new FileCrypto(enFilePath, filePath, "admin");
		//	fc.FileEncrypt();
		//	FileStream enFs = new FileStream(enFilePath, FileMode.Open, FileAccess.Read);
		//	long fsize = enFs.Length;
		//	enFs.Close();
		//	requestMsg[0] = 0x73;
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(userName);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
		//		Buffer.BlockCopy(fileMd5, 0, requestMsg, 15, 16);
		//	}
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(fileName);
		//		Buffer.BlockCopy(BitConverter.GetBytes(fsize), 0, requestMsg, 31, 8);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 39, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 43, tmp.Length);
		//	}
		//	//byte res = SendRequestMsgForUp(requestMsg, enFilePath);
		//	File.Delete(enFilePath);
		//	//if (res == 0x60)
		//	//	//上传成功
		//	//	return 1;
		//	//if (res == 0x83)
		//	//	//需要登录
		//	//	return 0;
		//	return -1;  //未知错误
			
		//}

		//public int DownloadFileProcess(string fileName)
		//{
		//	byte[] requestMsg = new byte[256];
		//	requestMsg[0] = 0x74;
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(userName);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
		//	}
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(fileName);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 39, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 43, tmp.Length);
		//	}
		//	string deFilePath = "./DownloadFiles/" + fileName;
		//	//byte res = SendRequestMsgForDown(requestMsg, deFilePath);
		//	//if (res == 0x62) //下载完成
		//	//	return 1;
		//	//if (res == 0x83) //需要登录
		//	//	return 0;
		//	return 0;
		//}

		//public int DeleteFileProcess(string fileName)
		//{
		//	byte[] requestMsg = new byte[256];
		//	requestMsg[0] = 0x75;
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(userName);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
		//	}
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(fileName);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 39, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 43, tmp.Length);
		//	}
		//	//byte res = SendRequestMsg(requestMsg);
		//	//if (res == 0x90)
		//	//	return 1;
		//	//if (res == 0x83)
		//	//	return 0;
		//	return -1;

		//}

		//public int RenameFileProcess(string fileName, string oldFileName)
		//{
		//	byte[] requestMsg = new byte[256];
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(userName);
		//		requestMsg[0] = 0x76;
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
		//	}
		//	int len;
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(fileName);
		//		len = tmp.Length;
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 31, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 35, tmp.Length);
		//	}
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(oldFileName);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 35 + len, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 39 + len, tmp.Length);
		//	}
		//	//byte res = SendRequestMsg(requestMsg);
		//	//if (res == 0x90)
		//	//	return 1; //操作成功
		//	//if (res == 0x83)
		//	//	return 0; //需要登录
		//	return -1; // 未知错误
		//}

		//public int CreateEmptyFileProcess(string filePath)
		//{
		//	byte[] requestMsg = new byte[256];
		//	string fileName = Path.GetFileName(filePath);
		//	requestMsg[0] = 0x73;
		//	{
		//		byte[] tmp = Encoding.Default.GetBytes(fileName);
		//		Buffer.BlockCopy(BitConverter.GetBytes((long)0), 0, requestMsg, 31, 8);
		//		Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 39, 4);
		//		Buffer.BlockCopy(tmp, 0, requestMsg, 43, tmp.Length);
		//	}
		//	byte res = SendRequestMsg(requestMsg);
		//	if (res == 0x90)
		//		return 1;
		//	if (res == 0x83)
		//		return 0;
		//	return -1;
		//}
	}
}
