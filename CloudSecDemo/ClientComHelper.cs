using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace CloudSecDemo
{
	class ClientComHelper : Communication
	{
		IPAddress targetIP;
		FileCrypto fc;
		int targetPort;
		string workPath;

		public ClientComHelper(string ipStr, int port, string workPath) : base()
		{
			this.workPath = workPath;
			fc = new FileCrypto("./tmp/", workPath, "key");
			tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
			targetIP = IPAddress.Parse(ipStr);
			targetPort = port;
			tcpClient.Connect(targetIP, targetPort);
			nstream = tcpClient.GetStream();
		}

		~ClientComHelper()
		{
			tcpClient.Close();
		}

		public void MakeRequestPacket(byte code, string userName, byte[] hashCode, long fileSize, string fileName, string newName)
		{
			//计算msgLength

			message[0] = code;
			
			byte[] bUserName = Encoding.Default.GetBytes(userName);
			byte[] bUserNameLength = BitConverter.GetBytes(bUserName.Length);
			Buffer.BlockCopy(bUserNameLength, 0, message, 1, 4);
			msgLength = 5;
			Buffer.BlockCopy(bUserName, 0, message, msgLength, bUserName.Length);
			msgLength += bUserName.Length;
			if (hashCode != null)
			{
				Buffer.BlockCopy(hashCode, 0, message, msgLength, 16);
				msgLength += 16;
			}
			if (fileSize == 0 && fileName == null)
				return;
			if (hashCode == null)
				msgLength += 16;
			Buffer.BlockCopy(BitConverter.GetBytes(fileSize), 0, message, msgLength, 8);
			msgLength += 8;
			if (code == DefindedCode.UPLOAD)
			{
				FileInfo fi = new FileInfo(workPath + "/" +fileName);
				string changeTime = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
				//MessageBox.Show("client:" + changeTime);
				byte[] bChangeTime = Encoding.Default.GetBytes(changeTime);
				Buffer.BlockCopy(bChangeTime, 0, message, msgLength, bChangeTime.Length);
				//MessageBox.Show(bChangeTime.Length.ToString());
			}
			msgLength += 19;
			byte[] bFileName = Encoding.Default.GetBytes(fileName);
			byte[] bFileNameLength = BitConverter.GetBytes(bFileName.Length);
			Buffer.BlockCopy(bFileNameLength, 0, message, msgLength, 4);
			msgLength += 4;
			Buffer.BlockCopy(bFileName, 0, message, msgLength, bFileName.Length);
			msgLength += bFileName.Length;
			if (newName != null)
			{
				byte[] bNewName = Encoding.Default.GetBytes(newName);
				byte[] bNewNameLength = BitConverter.GetBytes(bNewName.Length);
				Buffer.BlockCopy(bNewNameLength, 0, message, msgLength, 4);
				msgLength += 4;
				Buffer.BlockCopy(bNewName, 0, message, msgLength, bNewName.Length);
				msgLength += bNewName.Length;
			}
			
		}
		public string RecvFileList()
		{
			byte[] recvData = new byte[DATA_LENGTH];
			string fileList = string.Empty;
			int readLength;
			while(true)
			{
				readLength = nstream.Read(recvData, 0, DATA_LENGTH);
				fileList += Encoding.Default.GetString(recvData, 1, readLength - 1);
				if (recvData[0] == 0x62)
					break;
			}
			return fileList;
		}
		public override void SendFile(string sendPath)
		{
			string enPath = fc.FileEncrypt(sendPath);
			base.SendFile(enPath);
			File.Delete(enPath);
		}
		public override void RecvFile(string storePath)
		{
			string enPath = fc.encryptedFileDir + Path.GetFileName(storePath);
			base.RecvFile(enPath);
			fc.decryptedFileDir = Path.GetDirectoryName(storePath) + "/";
			fc.FileDecrypt(enPath);
			File.Delete(enPath);
		}
	}
}
