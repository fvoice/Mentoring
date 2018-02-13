using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using NUnit.Framework;
using Unity;

namespace FileSystemVisitor.Tests
{
	public class FileSystemVisitorTests
	{
		private IUnityContainer _container;

		private readonly IList<string> _testDirectories = new List<string>
		{
			@"Root\DirA",
			@"Root\DirB",
			@"Root\DirC",
			@"Root\DirA\SubDirA",
			@"Root\DirA\SubDirA2",
			@"Root\DirB\SubDirB",
			@"Root\DirC\SubDirC",
		};

		private readonly IList<string> _testFiles = new List<string>
		{
			@"Root\FileA",
			@"Root\FileA2",
			@"Root\FileB",
			@"Root\FileC",
			@"Root\DirA\SubFileA",
			@"Root\DirB\SubFileB",
			@"Root\DirC\SubFileC",
		};

		[SetUp]
		public void Setup()
		{
			_container = new UnityContainer();
			_container.RegisterType<IFileSystemVisitor, FileSystemVisitor>();

			var fs = new MockFileSystem();

			foreach (var testDirectory in _testDirectories)
			{
				fs.AddDirectory(testDirectory);
			}

			foreach (var testFile in _testFiles)
			{
				fs.AddFile(testFile, new MockFileData(string.Empty));
			}

			_container.RegisterInstance(typeof(IFileSystem), fs);
		}

		[Test]
		public void FileSystemVisitorShouldBeFailedOnNullOrEmptyPath()
		{
			var visitor = _container.Resolve<IFileSystemVisitor>();
			
			Assert.Throws<ArgumentException>(() => visitor.Enumerate(null));
			Assert.Throws<ArgumentException>(() => visitor.Enumerate(string.Empty));
		}

		[Test]
		public void FileSystemVisitorShouldBeFailedOnNonExistingDirectory()
		{
			var visitor = _container.Resolve<IFileSystemVisitor>();

			Assert.Throws<ArgumentException>(() => visitor.Enumerate(@"QWERTY:\fictional directory\and one more\"));
		}

		[Test]
		public void FileSystemVisitorShouldEnumerateAllFileSystemEntires()
		{
			//Arrange
			var visitor = _container.Resolve<IFileSystemVisitor>();
			var fs = _container.Resolve<IFileSystem>();

			IList<string> foundEntries = new List<string>();

			List<string> expectedEntries = _testDirectories.ToList();
			expectedEntries.AddRange(_testFiles);

			//Act
			foreach (FileSystemInfoBase entry in visitor.Enumerate("Root"))
			{
				foundEntries.Add(entry.FullName.Replace(fs.Directory.GetCurrentDirectory(), string.Empty)); //cut beggining of a path
			}

			//Assert
			var diff = foundEntries.Except(expectedEntries);
			Assert.AreEqual(0, diff.Count());
		}
	}
}
