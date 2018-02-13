using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace FileSystemVisitor
{
	public interface IFileSystemVisitor
	{
		event EventHandler<EventArgs> OnStart;

		IEnumerable<FileSystemInfoBase> Enumerate(string startPath);
	}
}
