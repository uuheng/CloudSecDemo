
namespace CloudSecDemo
{
	class DefindedCode
	{
		public const byte LOGIN = 0x70;
		public const byte LOGOUT = 0x71;
		public const byte GETLIST = 0x72;
		public const byte UPLOAD = 0x73;
		public const byte DOWNLOAD = 0x74;
		public const byte DELETE = 0x75;
		public const byte RENAME = 0x76;

		public const byte LOGSUCCESS = 0x80;
		public const byte PASSERROR = 0x81;
		public const byte USERMISS = 0x82;
		public const byte UNLOGIN = 0x83;
		public const byte FILELIST = 0x84;
		public const byte FILEDOWNLOAD = 0x85;
		public const byte FILEEXISTED = 0x86;
		public const byte DENIED = 0x87;
		public const byte AGREEUP = 0x88;

		public const byte READY = 0x60;
		public const byte SENDING = 0x61;
		public const byte END = 0x62;

		public const byte OK = 0x90;
		public const byte ERROR = 0x91;
		public const byte TOOLONG = 0x94;
	}
}
