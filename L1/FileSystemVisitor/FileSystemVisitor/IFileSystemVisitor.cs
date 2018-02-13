using System;
using System.Collections.Generic;
using System.IO;

namespace FileSystemVisitor
{
	public interface IFileSystemVisitor : IEnumerable<FileSystemInfo>
	{
		event EventHandler<EventArgs> OnStart;

		void Initialize(string directory);
	}
}
