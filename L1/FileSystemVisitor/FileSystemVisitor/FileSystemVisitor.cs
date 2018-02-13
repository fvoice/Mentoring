using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace FileSystemVisitor
{
	public class FileSystemVisitor : IFileSystemVisitor
	{
		private readonly IFileSystem _fileSystem;

		public FileSystemVisitor(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public event EventHandler<EventArgs> OnStart;

		public IEnumerable<FileSystemInfoBase> Enumerate(string startPath)
		{
			CheckPath(startPath);
			return _fileSystem.DirectoryInfo.FromDirectoryName(startPath).EnumerateFileSystemInfos("*", SearchOption.AllDirectories);
		}

		private void CheckPath(string startPath)
		{
			if (string.IsNullOrEmpty(startPath))
			{
				throw new ArgumentException("_startPath mustn't be empty");
			}
			if (!_fileSystem.Directory.Exists(startPath))
			{
				throw new ArgumentException("_startPath must exist");
			}
		}
	}
}
