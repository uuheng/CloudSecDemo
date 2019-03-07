using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CloudSecDemo
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Select sel = new Select();
			sel.ShowDialog();
			if (sel.DialogResult == DialogResult.OK) //Start ServerForm
			{
				Application.Run(new Server());
			}
			else if(sel.DialogResult == DialogResult.Yes) //Start ClientForm
			{
				Login login = new Login();
				login.ShowDialog();
				if (login.DialogResult == DialogResult.OK)
				{
					ClientManager clientManager = login.ClientManager;
					Application.Run(new Client(clientManager));
				}
			}
			
		}
	}
}
