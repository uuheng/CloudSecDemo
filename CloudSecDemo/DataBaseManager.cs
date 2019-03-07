using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CloudSecDemo
{
	class DataBaseManager
	{
		string connectionString;
		string queryString;

		public DataBaseManager(string con)
		{
			connectionString = con;
		}

		private void CreateCommand(string queryString)
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand(queryString, connection);
				command.Connection.Open();
				command.ExecuteNonQuery();
			}
		}

		private void CreateUser(string userName, string passwd)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] tmp = Encoding.Default.GetBytes(passwd);
			byte[] passMD5 = md5.ComputeHash(tmp);
			string passString = string.Empty;
			foreach (var i in passMD5)
				passString += i.ToString("x2");
			string queryString = "USE Starry;" +
				"INSERT INTO UserTable VALUES (" +
				"'" + userName + "'," +
				"'" + passString + "'," +
				"NULL" +
				");";
			CreateCommand(queryString);
		}

		public void InitProcess()
		{
			queryString = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", "Starry");
			//MessageBox.Show(connectionString);
			int count = 0;
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand(queryString, connection);
				command.Connection.Open();
				count = Convert.ToInt32(command.ExecuteScalar());
				command.ExecuteNonQuery();
			}

			if (count != 0)
			{
				//删表
				queryString = "USE Starry; DROP TABLE UserTable; DROP TABLE UpFileTable;  DROP TABLE FileTable;";
				CreateCommand(queryString);
			}
			else
			{
				//建数据库 Starry
				queryString = "CREATE DATABASE Starry";
				CreateCommand(queryString);
			}
			//建表 UserTable
			queryString = "USE Starry;" +
				"CREATE TABLE UserTable (" +
				"USER_ID INT IDENTITY(1,1) PRIMARY KEY," +          //用户ID 自增IDENTITY(1,1)
				"USER_NAME NVARCHAR(50) NOT NULL," +                //用户名    NVARCHAR  unicode
				"PASSWORD NVARCHAR(50)," +                          //密码
				"NONE NCHAR(10)" +                                  //预留字段
				");";
			CreateCommand(queryString);

			//建表 FileTable
			queryString = "USE Starry;" +
				"CREATE TABLE FileTable (" +
				"FILE_ID INT IDENTITY(1,1) PRIMARY KEY," +          //文件ID
				"HASH NVARCHAR(50) NOT NULL," +                     //Hash值 MD5(F)
				"FILE_SIZE BIGINT NOT NULL," +                       //文件大小
				"PHYSICAL_ADD NVARCHAR(MAX) NOT NULL," +            //物理地址
				"COUNT INT NOT NULL" +                                  //拥有数量
				");";
			CreateCommand(queryString);

			//建表 UpFileTable
			queryString = "USE Starry;" +
				"CREATE TABLE UpFileTable (" +
				"FILE_ID INT," +                                    //文件标识
				"USER_ID  INT NOT NULL," +                          //用户标识    
				"FILE_NAME NVARCHAR(50) NOT NULL," +                //用户上传文件名
				"USER_NAME NVARCHAR(50) NOT NULL," +                //用户登录名
				"UPLOAD_TIME DATETIME NOT NULL," +                  // 用户上传文件时间
				"NONE NCHAR(10)" +                                  //预留字段
				");";
			CreateCommand(queryString);
			CreateUser("admin", "123456");
		}
		private int ExecuteScalar(string queryString)
		{
			int count = 0;
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand(queryString, connection);
				command.Connection.Open();
				count = Convert.ToInt32(command.ExecuteScalar());
			}
			return count;
		}


		public int LoginAuthentication(string userName, string passwd)
		{
			int result = 0;
			//MessageBox.Show(userName);
			queryString = "USE Starry;" +
				"SELECT * FROM UserTable " +
				"Where USER_NAME='" + userName + "';";
			result = ExecuteScalar(queryString);
			if (result == 0)
			{
				//MessageBox.Show("Failed");
				return -1;
			}

			queryString = "USE Starry;" +
				"SELECT * FROM UserTable " +
				"Where USER_NAME='" + userName + "'" +
			   "AND PASSWORD='" + passwd + "';";
			//MessageBox.Show(userName + " :" + passwd);
			return ExecuteScalar(queryString);
		}

		public string GetFileList(string userName)
		{
			queryString = "USE Starry;" +
				"SELECT * FROM UpFileTable " +
				"Where USER_NAME='" + userName + "';";

			string fileList = string.Empty;

			using (SqlConnection connection =
					   new SqlConnection(connectionString))
			{
				SqlCommand command =
					new SqlCommand(queryString, connection);
				connection.Open();
				SqlDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					fileList += (string)reader[2] + "\r\n" + ((DateTime)reader[4]).ToString() + "\r\n";
				}
				reader.Close();
			}
			return fileList;
		}
		public int InsertFile(string userFile, long fileSize, string userName, string hash, string physicalAdd, string uploadTime)
		{
			//MessageBox.Show(uploadTime);
			int status = 0; //文件重复
			int fileID;
			//int fileID = InsertFileTable(hash, fileSize, physicalAdd, ref repetition);
			queryString = "USE Starry;" +
				"SELECT * FROM FileTable " +
				"Where HASH='" + hash + "';";
			fileID = ExecuteScalar(queryString);
			
			if (fileID == 0)
			{
				queryString = "USE Starry;" +
				"INSERT INTO FileTable VALUES (" +
				"'" + hash + "'," +
				fileSize + "," +
				"'" + physicalAdd + "'," +
				"0" +
				");";
				ExecuteScalar(queryString);
				queryString = "USE Starry;" +
					"SELECT * FROM FileTable " +
					"WHERE HASH='" + hash + "';";
				fileID = ExecuteScalar(queryString);
				status = 1;  //云端不存在，需要上传
			}
			
			//判断同一用户重名文件
			queryString = "USE Starry;" +
				"SELECT * FROM UpFileTable " +
				"WHERE USER_NAME='" + userName + "' " +
				"AND FILE_NAME='" + userFile + "';";
			int res = ExecuteScalar(queryString);
			if (res != 0)
			{
				queryString = "USE Starry;" +
					"UPDATE UpFileTable " +
					"SET FILE_ID='" + fileID + "', " +
					"UPLOAD_TIME='" + uploadTime + "' " +
					"Where FILE_NAME='" + userFile + "' " +
					"AND USER_NAME='" + userName + "';";
				ExecuteScalar(queryString);
			}
			else
			{
				//获取用户ID
				queryString = "USE Starry;" +
					"SELECT * FROM UserTable " +
					"Where USER_NAME='" + userName + "';";
				int userID = ExecuteScalar(queryString);
				queryString = string.Format("USE Starry; " +
					"INSERT INTO UpFileTable VALUES ({0}, {1}, '{2}', '{3}', '{4}', NULL);",
					fileID.ToString(), userID.ToString(), userFile, userName, uploadTime);
				ExecuteScalar(queryString);
			}
			return status;
		}
		public string GetFilePath(string userName, string fileName)
		{
			queryString = "USE Starry;" +
				"SELECT * FROM UpFileTable " +
				"Where USER_NAME='" + userName + "' " +
			   "AND FILE_NAME='" + fileName + "';";
			int fileID = ExecuteScalar(queryString);
			if (fileID <= 0)
				return "";
			queryString = "USE Starry;" +
				"SELECT * FROM FileTable " +
				"Where FILE_ID='" + fileID + "';";
			string physicalAdd;
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand(queryString, connection);
				connection.Open();
				SqlDataReader reader = command.ExecuteReader();
				reader.Read();
				physicalAdd = (string)reader[3];
				reader.Close();
			}
			return physicalAdd;
		}
		public int RemoveFile(string userName, string fileName)
		{
			queryString = "USE Starry;" +
				"DELETE  FROM UpFileTable " +
				"Where USER_NAME='" + userName + "' " +
				"AND FILE_NAME='" + fileName + "';";
			return ExecuteScalar(queryString);
		}
		public int RenameFile(string userName, string oldFileName, string fileName)
		{
			queryString = "USE Starry;" +
				"UPDATE UpFileTable " +
				"SET FILE_NAME='" + fileName + "' " +
				"Where USER_NAME='" + userName + "' " +
				"AND FILE_NAME='" + oldFileName + "';";
			return ExecuteScalar(queryString);
		}
		public List<string> GetCloudFiles()
		{
			List<string> cloudFiles = new List<string>();
			queryString = "USE Starry;" +
				"SELECT * FROM FileTable;";
			using (SqlConnection connection =
					new SqlConnection(connectionString))
			{
				SqlCommand command =
					new SqlCommand(queryString, connection);
				connection.Open();
				SqlDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					cloudFiles.Add((string)reader[1] + "       " + ((long)reader[2]).ToString() + "B");
				}
				reader.Close();
			}
			return cloudFiles;
		}
	}
}
