using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace FileSystemVisitor
{
	public class FileSystemVisitor : IFileSystemVisitor
	{
		private readonly IFileSystem _fileSystem;

		public event EventHandler Start;
		public event EventHandler Finish;
		public event EventHandler FileFound;
		public event EventHandler DirectoryFound;
		public event EventHandler<FilteredFileSystemInfoFound> FilteredFileFound;
		public event EventHandler<FilteredFileSystemInfoFound> FilteredDirectoryFound;
		public FilterPredicate Filter { get; set; }

		public FileSystemVisitor(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public IEnumerable<FileSystemInfoBase> Enumerate(string startPath)
		{
			Start?.Invoke(this, EventArgs.Empty);

			CheckPath(startPath);
			var result = _fileSystem.DirectoryInfo.FromDirectoryName(startPath).EnumerateFileSystemInfos("*", SearchOption.AllDirectories);
			foreach (var systemInfoBase in result)
			{
				ProcessFoundEntry(systemInfoBase);

				if (Filter != null && !Filter(systemInfoBase))
				{
					continue;
				}

				var args = new FilteredFileSystemInfoFound();//not obvious - should filtered events be raised in case of empty filter?

				ProcessFilteredEntry(systemInfoBase, args);

				if (args.IsProcessTerminated)
				{
					break;
				}

				if (!args.IsExcluded)
				{
					yield return systemInfoBase;
				}
			}

			Finish?.Invoke(this, EventArgs.Empty);
		}

		private void ProcessFilteredEntry(FileSystemInfoBase systemInfoBase, FilteredFileSystemInfoFound args)
		{
			var isDirectory = systemInfoBase.Attributes.HasFlag(FileAttributes.Directory);
			if (isDirectory)
			{
				FilteredDirectoryFound?.Invoke(systemInfoBase, args);
			}
			else
			{
				FilteredFileFound?.Invoke(systemInfoBase, args);
			}
		}

		private void ProcessFoundEntry(FileSystemInfoBase systemInfoBase)
		{
			var isDirectory = systemInfoBase.Attributes.HasFlag(FileAttributes.Directory);
			if (isDirectory)
			{
				DirectoryFound?.Invoke(systemInfoBase, EventArgs.Empty);
			}
			else
			{
				FileFound?.Invoke(systemInfoBase, EventArgs.Empty);
			}
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
