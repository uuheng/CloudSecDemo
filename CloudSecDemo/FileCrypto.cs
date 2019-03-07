using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Configuration;

namespace CloudSecDemo
{
	class FileCrypto
	{
		public string encryptedFileDir;
		public string decryptedFileDir;
		string key;
		public FileCrypto(string e, string d, string k)
		{
			encryptedFileDir = e;
			decryptedFileDir = d;
			string salt = ConfigurationManager.AppSettings["Salt"].ToString();
			MD5 md5 = MD5.Create();
			byte[] tmp = md5.ComputeHash(Encoding.Default.GetBytes(k + salt));
			key = string.Empty;
			foreach (var i in tmp)
				key += i.ToString("x2");
		}
		private byte[] AESEncrypt(byte[] plainText)
		{
			if (plainText == null || plainText.Length <= 0)
				return null;
			if (string.IsNullOrEmpty(key))
			{
				MessageBox.Show("Key is empty");
				return null;
			}
			byte[] bKey = Encoding.Default.GetBytes(key);
			if (bKey.Length % 32 != 0)
			{
				MessageBox.Show("Key must be divided 32");
				return null;
			}

			byte[] encrypted;
			using (Aes aes = Aes.Create())
			{
				aes.Key = bKey;
				//aes.KeySize = bKey.Length * 8;
				aes.Padding = PaddingMode.PKCS7;
				aes.Mode = CipherMode.ECB;
				ICryptoTransform encryptor = aes.CreateEncryptor();
				encrypted = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
			}
			return encrypted;
		}

		private byte[] AESDecrypt(byte[] cipherText)
		{
			if (cipherText == null || cipherText.Length <= 0)
				return null;
			if (string.IsNullOrEmpty(key))
			{
				MessageBox.Show("Key is empty");
				return null;
			}
			byte[] bKey = Encoding.Default.GetBytes(key);
			if (bKey.Length % 32 != 0)
			{
				MessageBox.Show("Key must be divided 32");
				return null;
			}
			byte[] decrypted;
			using (Aes aes = Aes.Create())
			{
				aes.Key = bKey;
				//aes.KeySize = bKey.Length;
				aes.Padding = PaddingMode.PKCS7;
				aes.Mode = CipherMode.ECB;
				ICryptoTransform decryptor = aes.CreateDecryptor();
				decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
			}
			return decrypted;
		}
		public string FileEncrypt(string rawFile)
		{
			//string ciphertextPath = "D:/杂项/加密后/" + DateTime.Now.ToString();
			string enFileFullPath = encryptedFileDir + Path.GetFileName(rawFile);
			byte[] inbuffer = new byte[1024];
			int readCount;
			using (FileStream freader = new FileStream(rawFile, FileMode.Open))
			{
				using (FileStream fwriter = new FileStream(enFileFullPath, FileMode.Create))
				{
					while ((readCount = freader.Read(inbuffer, 0, inbuffer.Length)) > 0)
					{
						if (readCount != inbuffer.Length)
						{
							//MessageBox.Show(readCount);
							byte[] tmp = new byte[readCount];
							Buffer.BlockCopy(inbuffer, 0, tmp, 0, readCount);
							byte[] enbyte = AESEncrypt(tmp);
							fwriter.Write(enbyte, 0, enbyte.Length);
							//MessageBox.Show(enbyte.Length);
						}
						else
						{
							//MessageBox.Show(inbuffer.Length);
							byte[] enbyte = AESEncrypt(inbuffer);
							fwriter.Write(enbyte, 0, enbyte.Length);
							//MessageBox.Show(enbyte.Length);
						}
					}
					fwriter.Close();
				}
				freader.Close();
			}
			return enFileFullPath;
		}

		public void FileDecrypt(string enFile)
		{
			string deFileFullPath = decryptedFileDir + Path.GetFileName(enFile);
			int readCount;
			byte[] deBuffer = new byte[1040];
			using (FileStream freader = new FileStream(enFile, FileMode.Open))
			{
				using (FileStream fwriter = new FileStream(deFileFullPath, FileMode.Create))
				{
					while ((readCount = freader.Read(deBuffer, 0, deBuffer.Length)) > 0)
					{
						if (readCount != deBuffer.Length)
						{
							byte[] tmp = new byte[readCount];
							Buffer.BlockCopy(deBuffer, 0, tmp, 0, readCount);
							byte[] debyte = AESDecrypt(tmp);
							fwriter.Write(debyte, 0, debyte.Length);
						}
						else
						{
							byte[] debyte = AESDecrypt(deBuffer);
							fwriter.Write(debyte, 0, debyte.Length);
						}
					}
				}
			}
		}
	}
}
