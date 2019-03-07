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

namespace CloudSecDemo
{
	class ClientManager
	{
		public List<string> fileList;
		private Queue<WatchEvent> eventQueue;
		private string userName;
		private IPAddress iPAddress;
		private int port;
		public ClientManager(string IPaddr, int p, string un)
		{
			iPAddress = IPAddress.Parse(IPaddr);
			port = p;
			userName = un;
			fileList = new List<string>();
			eventQueue = new Queue<WatchEvent>();
			System.Timers.Timer t = new System.Timers.Timer(2000);
			t.Elapsed += new System.Timers.ElapsedEventHandler(HandleQueue);
			t.AutoReset = true;
			t.Enabled = true;
		}

		public void AnalysesEvent(object sender, WatchEvent we)
		{
			if (we.fileEvent != 3)
			{
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
				if (we.fileEvent == 0)
				{
					//MessageBox.Show("Nothing change");
					return;
				}

				if (we.fileEvent == 1)
				{
					//MessageBox.Show("new" + we.filePath);
					UploadFileProcess(we.filePath);
					return;
				}
				if (we.fileEvent == 2)
				{
					//MessageBox.Show("change" + we.filePath);
					UploadFileProcess(we.filePath);
					return;
				}
				if (we.fileEvent == 3)
				{
					//MessageBox.Show("delete" + we.filePath);
					DeleteFileProcess(Path.GetFileName(we.filePath));
					return;
				}
				if (we.fileEvent == 4)
				{
					//MessageBox.Show("rename" + we.filePath);
					RenameFileProcess(Path.GetFileName(we.filePath), Path.GetFileName(we.oldFilePath));
					return;
				}
			}
		}

		private byte SendRequestMsg(byte[] requestMsg)
		{
			TcpClient tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
			try
			{	
				tcpClient.Connect(iPAddress, port);
			}
			catch
			{
				MessageBox.Show("网络连接错误或云端没有启动");
				return 0x92;
			}
			NetworkStream networkStream = tcpClient.GetStream();
			networkStream.Write(requestMsg, 0, requestMsg.Length);
			byte[] responseMsg = new byte[1025];
			networkStream.Read(responseMsg, 0, 1025);
			if (responseMsg[0] == 0x84)
			{
				//处理文件列表
				fileList.Clear();
				int segLength = BitConverter.ToInt32(responseMsg, 1);
				string listInfo = Encoding.Default.GetString(responseMsg, 5, segLength);
				string[] arr = Regex.Split(listInfo, "\r\n");
				foreach (var item in arr)
				{
					//MessageBox.Show(item);
					if (item.Trim() != "")
						fileList.Add(item);
				}
			}
			tcpClient.Close();
			return responseMsg[0];
		}

		private byte SendRequestMsgForUp(byte[] requestMsg, string enFilePath)
		{
			TcpClient tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
			tcpClient.Connect(iPAddress, port);
			NetworkStream networkStream = tcpClient.GetStream();
			networkStream.Write(requestMsg, 0, requestMsg.Length);
			byte[] responseMsg = new byte[1025];
			networkStream.Read(responseMsg, 0, responseMsg.Length);
			if (responseMsg[0] == 0x60)
			{
				//允许上传
				byte[] fileData = new byte[1025];
				using (FileStream fs = new FileStream(enFilePath, FileMode.Open, FileAccess.Read))
				{
					long leftSize = fs.Length;
					int readLength;
					fileData[0] = 0x61;
					while ((readLength = fs.Read(fileData, 1, 1024)) > 0)
					{
						leftSize -= readLength;
						if (leftSize <= 0)
							fileData[0] = 0x62;
						networkStream.Write(fileData, 0, 1 + readLength);
					}
				}
			}
			tcpClient.Close();
			return responseMsg[0];
		}

		private byte SendRequestMsgForDown(byte[] requestMsg, string deFilePath)
		{
			TcpClient tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
			tcpClient.Connect(iPAddress, port);
			NetworkStream networkStream = tcpClient.GetStream();
			networkStream.Write(requestMsg, 0, requestMsg.Length);
			byte[] fileData = new byte[1025];
			networkStream.Read(fileData, 0, fileData.Length);
			if (fileData[0] == 0x85) //允许下载，已经收到文件头
			{
				int fileNameLength = BitConverter.ToInt32(fileData, 1);
				string fileName = Encoding.Default.GetString(fileData, 5, fileNameLength);   //获取文件的哈希名
				string enFilePath = "./tmp/" + fileName;
				//以上解析文件头
				requestMsg[0] = 0x60;
				networkStream.Write(requestMsg, 0, requestMsg.Length);
				using (FileStream fs = new FileStream(enFilePath, FileMode.Create, FileAccess.Write))
				{
					int readLength;
					while (true)
					{
						readLength = networkStream.Read(fileData, 0, 1025);
						fs.Write(fileData, 1, readLength - 1);
						if (fileData[0] == 0x62)
							break;
					}
					fs.Close();
				}	
				FileCrypto fc = new FileCrypto(enFilePath, deFilePath, "admin");
				fc.FileDecrypt();
			}
			tcpClient.Close();
			return fileData[0];
		}

		public int LoginProcess(string userPass)
		{
			byte[] requestMsg = new byte[256];
			using (MD5 md5 = new MD5CryptoServiceProvider())
			{
				byte[] tmp = Encoding.Default.GetBytes(userPass);
				byte[] passMD5 = md5.ComputeHash(tmp);
				requestMsg[0] = 0x70;
				Buffer.BlockCopy(passMD5, 0, requestMsg, 15, 16);
			}
			byte[] tmp2 = Encoding.Default.GetBytes(userName);
			byte[] tmp1 = BitConverter.GetBytes(tmp2.Length);
			
			Buffer.BlockCopy(tmp1, 0, requestMsg, 1, tmp1.Length);
			Buffer.BlockCopy(tmp2, 0, requestMsg, 5, tmp2.Length);
			byte res = SendRequestMsg(requestMsg);
			if (res == 0x80)
			{
				//登录成功
				return 1;
			}
			if (res == 0x81)
			{
				//密码错误
				return 0;
			}
			if (res == 0x82)
			{
				//用户不存在
				return -1;
			}
			return -2; //未知错误
		}

		public int GetFileListProcess()
		{
			byte[] requestMsg = new byte[256];
			requestMsg[0] = 0x72;
			byte[] tmp = Encoding.Default.GetBytes(userName);
			Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
			byte res = SendRequestMsg(requestMsg);
			if (res == 0x84)
				return 1; //成功
			if (res == 0x83)
				return 0; //用户需要登录
			return -1;
		}

		public int UploadFileProcess(string filePath)
		{
			byte[] requestMsg = new byte[256];
			if (!File.Exists(filePath))
				return 1;   
			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			if (fs.Length == 0)
			{
				fs.Close();
				return 2; //文件为空，上传取消
			}	
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] fileMd5 = md5.ComputeHash(fs);
			fs.Close();
			string fileName = Path.GetFileName(filePath);
			string enFileName = string.Empty;
			foreach (var i in fileMd5)
				enFileName += i.ToString("x2");
			string enFilePath = "./tmp/" + enFileName;
			FileCrypto fc = new FileCrypto(enFilePath, filePath, "admin");
			fc.FileEncrypt();
			FileStream enFs = new FileStream(enFilePath, FileMode.Open, FileAccess.Read);
			long fsize = enFs.Length;
			enFs.Close();
			requestMsg[0] = 0x73;
			{
				byte[] tmp = Encoding.Default.GetBytes(userName);
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
				Buffer.BlockCopy(fileMd5, 0, requestMsg, 15, 16);
			}
			{
				byte[] tmp = Encoding.Default.GetBytes(fileName);
				Buffer.BlockCopy(BitConverter.GetBytes(fsize), 0, requestMsg, 31, 8);
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 39, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 43, tmp.Length);
			}
			byte res = SendRequestMsgForUp(requestMsg, enFilePath);
			File.Delete(enFilePath);
			if (res == 0x60)
				//上传成功
				return 1;
			if (res == 0x83)
				//需要登录
				return 0;
			return -1;  //未知错误
			
		}

		public int DownloadFileProcess(string fileName)
		{
			byte[] requestMsg = new byte[256];
			requestMsg[0] = 0x74;
			{
				byte[] tmp = Encoding.Default.GetBytes(userName);
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
			}
			{
				byte[] tmp = Encoding.Default.GetBytes(fileName);
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 39, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 43, tmp.Length);
			}
			string deFilePath = "./DownloadFiles/" + fileName;
			byte res = SendRequestMsgForDown(requestMsg, deFilePath);
			if (res == 0x62) //下载完成
				return 1;
			if (res == 0x83) //需要登录
				return 0;
			return res;
		}

		public int DeleteFileProcess(string fileName)
		{
			byte[] requestMsg = new byte[256];
			requestMsg[0] = 0x75;
			{
				byte[] tmp = Encoding.Default.GetBytes(userName);
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
			}
			{
				byte[] tmp = Encoding.Default.GetBytes(fileName);
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 39, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 43, tmp.Length);
			}
			byte res = SendRequestMsg(requestMsg);
			if (res == 0x90)
				return 1;
			if (res == 0x83)
				return 0;
			return -1;

		}

		public int RenameFileProcess(string fileName, string oldFileName)
		{
			byte[] requestMsg = new byte[256];
			{
				byte[] tmp = Encoding.Default.GetBytes(userName);
				requestMsg[0] = 0x76;
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 1, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 5, tmp.Length);
			}
			int len;
			{
				byte[] tmp = Encoding.Default.GetBytes(fileName);
				len = tmp.Length;
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 31, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 35, tmp.Length);
			}
			{
				byte[] tmp = Encoding.Default.GetBytes(oldFileName);
				Buffer.BlockCopy(BitConverter.GetBytes(tmp.Length), 0, requestMsg, 35 + len, 4);
				Buffer.BlockCopy(tmp, 0, requestMsg, 39 + len, tmp.Length);
			}
			byte res = SendRequestMsg(requestMsg);
			if (res == 0x90)
				return 1; //操作成功
			if (res == 0x83)
				return 0; //需要登录
			return -1; // 未知错误
		}

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
