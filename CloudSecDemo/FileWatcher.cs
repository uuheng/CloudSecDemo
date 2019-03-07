using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace CloudSecDemo
{
	public class WatchEvent
	{
		public string filePath;
		public string oldFilePath;
		public int fileEvent;   //新建1  修改2  删除3  重命名4  不做操作0
	}
	class FileWatcher
	{
		FileSystemWatcher watcher;
		private Dictionary<string, DateTime> dateTimeDictionary = new Dictionary<string, DateTime>();
		public delegate void DelegateEventHander(object sender, WatchEvent we);
		public DelegateEventHander SendEvent;
		public FileWatcher(string path, string filter)
		{
			watcher = new FileSystemWatcher();
			watcher.Path = path;
			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
			watcher.Filter = filter;
			watcher.Changed += new FileSystemEventHandler(OnProcess);
			watcher.Created += new FileSystemEventHandler(OnProcess);
			watcher.Deleted += new FileSystemEventHandler(OnProcess);
			watcher.Renamed += new RenamedEventHandler(OnRenamed);
		}
		public void Start()
		{
			watcher.EnableRaisingEvents = true;
		}
		private bool CheckPath(string path)
		{
			if (File.Exists(path))
				return true;
			return false;
		}
		private void OnProcess(object sender, FileSystemEventArgs e)
		{
			WatchEvent we = new WatchEvent();
			if (e.ChangeType == WatcherChangeTypes.Deleted)
			{
				we.fileEvent = 3;
				we.filePath = e.FullPath;
			}
			else if (!CheckPath(e.FullPath))
			{
				we.fileEvent = 0;
			}
			else if (e.ChangeType == WatcherChangeTypes.Created)
			{
				we.filePath = e.FullPath;
				we.fileEvent = 1;
			}
			else if (e.ChangeType == WatcherChangeTypes.Changed)
			{
				//if (!dateTimeDictionary.ContainsKey(e.FullPath) || (dateTimeDictionary.ContainsKey(e.FullPath) && File.GetLastWriteTime(e.FullPath).Ticks - dateTimeDictionary[e.FullPath].Ticks > 1e7))
				//{
				//	we.filePath = e.FullPath;
				//	we.fileEvent = 2;
				//	dateTimeDictionary[e.FullPath] = File.GetLastWriteTime(e.FullPath);
				//}
				we.filePath = e.FullPath;
				we.fileEvent = 2;
			}
			else
				we.fileEvent = 0;
			SendEvent?.Invoke(this, we);
		}

		private void OnRenamed(object sender, RenamedEventArgs e)
		{
			WatchEvent we = new WatchEvent();
			we.fileEvent = 4;
			we.filePath = e.FullPath;
			we.oldFilePath = e.OldFullPath;
			SendEvent?.Invoke(this, we);
		}
	}
}
