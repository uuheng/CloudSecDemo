using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace CloudSecDemo
{
	class ServerComHelper : Communication
	{
		public ServerComHelper(TcpClient client) : base()
		{
			tcpClient = client;
			nstream = tcpClient.GetStream();
		}

		public void MakeResponsePacket(byte code) 
		{
			message[0] = code;
			msgLength = 1;
		}
		public void SendFileList(string fileList)
		{
			byte[] bFileList = Encoding.Default.GetBytes(fileList);
			byte[] sendData = new byte[DATA_LENGTH];
			int bFileListLength = bFileList.Length;
			if (bFileListLength == 0)
			{
				sendData[0] = 0x62;
				nstream.Write(sendData, 0, 1);
				return;
			}
			int offset = 0;
			while(bFileListLength - offset > DATA_LENGTH - 1)
			{
				if (bFileListLength - offset == DATA_LENGTH - 1)
					sendData[0] = 0x62;
				else
					sendData[0] = 0x61;
				Buffer.BlockCopy(bFileList, offset, sendData, 1, DATA_LENGTH - 1);
				nstream.Write(sendData, 0, DATA_LENGTH);
				offset += DATA_LENGTH - 1;
			}
			if (offset < bFileListLength)
			{
				sendData[0] = 0x62;
				Buffer.BlockCopy(bFileList, offset, sendData, 1, bFileListLength - offset);
				nstream.Write(sendData, 0, 1 + bFileListLength - offset);
			}
		}
		
	}
}
