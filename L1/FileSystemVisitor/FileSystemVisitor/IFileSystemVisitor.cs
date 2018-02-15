using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace FileSystemVisitor
{
	public delegate bool FilterPredicate(FileSystemInfoBase fileSystemInfo);

	public class FilteredFileSystemInfoFound
	{
		public bool IsProcessTerminated { get; set; }
		public bool IsExcluded { get; set; }
	}

	public interface IFileSystemVisitor
	{
		event EventHandler Start;
		event EventHandler Finish;

		event EventHandler FileFound;
		event EventHandler DirectoryFound;

		event EventHandler<FilteredFileSystemInfoFound> FilteredFileFound;
		event EventHandler<FilteredFileSystemInfoFound> FilteredDirectoryFound;

		FilterPredicate Filter { get; set; }

		IEnumerable<FileSystemInfoBase> Enumerate(string startPath);
	}
}
