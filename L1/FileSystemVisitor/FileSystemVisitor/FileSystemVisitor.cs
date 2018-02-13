using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FileSystemVisitor
{
	public class FileSystemVisitor : IFileSystemVisitor
	{
		private string _directory;

		public event EventHandler<EventArgs> OnStart;

		public void Initialize(string directory)
		{
			_directory = directory;
			EnsureWellConfigured();
		}

		public IEnumerator<FileSystemInfo> GetEnumerator()
		{
			EnsureWellConfigured();
			return null;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void EnsureWellConfigured()
		{
			if (string.IsNullOrEmpty(_directory))
			{
				throw new Exception("Directory mustn't be empty");
			}
			if (!Directory.Exists(_directory))
			{
				throw new Exception("Directory must exist");
			}
		}
	}
}
