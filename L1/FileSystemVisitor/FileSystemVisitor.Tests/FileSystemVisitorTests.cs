using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

		private IList<string> TestEntires
		{
			get
			{
				List<string> entries = _testDirectories.ToList();
				entries.AddRange(_testFiles);
				return entries;
			}
		}

		private IFileSystem FileSystem => _container.Resolve<IFileSystem>();
		private string CurrentDirectory => FileSystem.Directory.GetCurrentDirectory();

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
		[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
		public void FileSystemVisitorShouldBeFailedOnNullOrEmptyPath()
		{
			var visitor = _container.Resolve<IFileSystemVisitor>();
			
			Assert.Throws<ArgumentException>(() => visitor.Enumerate(null).ToList(), "null value of startPath hasn't raised ArgumentException");
			Assert.Throws<ArgumentException>(() => visitor.Enumerate(string.Empty).ToList(), "string.Empty value of startPath hasn't raised ArgumentException");
			Assert.Throws<ArgumentException>(() => visitor.Enumerate(@"QWERTY:\fictional directory\and one more\").ToList(), "Fictional value of startPath hasn't raised ArgumentException");
		}

		[Test]
		public void FileSystemVisitorShouldEnumerateAllFileSystemEntires()
		{
			//Arrange
			var visitor = _container.Resolve<IFileSystemVisitor>();
			
			//Act
			IList<string> foundEntries = visitor.Enumerate("Root").Select(x => x.FullName.Replace(CurrentDirectory, string.Empty)).ToList();

			//Assert
			var diff = foundEntries.Except(TestEntires);
			Assert.AreEqual(0, diff.Count(), "Found and Expected entiry collections are different");
		}

		[Test]
		public void FileSystemVisitorShouldFilterFileSystemEntires()
		{
			//Arrange
			var visitor = _container.Resolve<IFileSystemVisitor>();
			string filterValue = "A";
			visitor.Filter = x => x.Name.Contains(filterValue);

			var expectedFileredEntries = TestEntires.Where(x => x.Contains(filterValue));

			//Act
			IList<string> foundEntries = visitor.Enumerate("Root").Select(x => x.FullName.Replace(CurrentDirectory, string.Empty)).ToList();

			//Assert
			var diff = foundEntries.Except(expectedFileredEntries);
			Assert.AreEqual(0, diff.Count(), "Found and Expected entiry collections are different");
		}

		[Test]
		public void FileSystemVisitorShouldRaiseEvents()
		{
			//Arrange
			var visitor = _container.Resolve<IFileSystemVisitor>();
			string filterValue = "A";
			visitor.Filter = x => x.Name.Contains(filterValue);

			int expectedRaisedEvents = 2;
			var raisedEvents = new List<string>();

			visitor.Start += (sender, args) => raisedEvents.Add(nameof(visitor.Start));
			visitor.Finish += (sender, args) => raisedEvents.Add(nameof(visitor.Finish));
			visitor.FileFound += (sender, args) => raisedEvents.Add(nameof(visitor.FileFound));
			visitor.DirectoryFound += (sender, args) => raisedEvents.Add(nameof(visitor.DirectoryFound));
			visitor.FilteredFileFound += (sender, args) => raisedEvents.Add(nameof(visitor.FilteredFileFound));
			visitor.FilteredDirectoryFound += (sender, args) => raisedEvents.Add(nameof(visitor.FilteredDirectoryFound));

			var dirFoundEventsAmount = TestEntires.Count(x => !x.Contains("File"));
			var fileFoundEventsAmount = TestEntires.Count(x => x.Contains("File"));

			var expectedFileredEntries = TestEntires.Where(x => x.Contains(filterValue)).ToList();

			var filteredDirFoundEventsAmount = expectedFileredEntries.Count(x => !x.Contains("File"));
			var filteredFileFoundEventsAmount = expectedFileredEntries.Count(x => x.Contains("File"));

			expectedRaisedEvents = expectedRaisedEvents + dirFoundEventsAmount + fileFoundEventsAmount + filteredDirFoundEventsAmount + filteredFileFoundEventsAmount;

			//Act
			// ReSharper disable once UnusedVariable
			IList<string> foundEntries = visitor.Enumerate("Root").Select(x => x.FullName.Replace(CurrentDirectory, string.Empty)).ToList();

			//Assert
			Assert.AreEqual(expectedRaisedEvents, raisedEvents.Count, "Wrong count of raised events");
			Assert.AreEqual(nameof(visitor.Start), raisedEvents[0], "OnStart event hasn't been raised or has been raised in wrong order");
			Assert.AreEqual(nameof(visitor.Finish), raisedEvents[raisedEvents.Count - 1], "OnFinish event hasn't been raised or has been raised in wrong order");

			Assert.AreEqual(dirFoundEventsAmount, raisedEvents.Count(x => x == nameof(visitor.DirectoryFound)), "DirectoryFound events amount is not equal to expected amount");
			Assert.AreEqual(fileFoundEventsAmount, raisedEvents.Count(x => x == nameof(visitor.FileFound)), "FileFound events amount is not equal to expected amount");

			Assert.AreEqual(filteredDirFoundEventsAmount, raisedEvents.Count(x => x == nameof(visitor.FilteredDirectoryFound)), "FileFoFilteredDirectoryFoundund events amount is not equal to expected amount");
			Assert.AreEqual(filteredFileFoundEventsAmount, raisedEvents.Count(x => x == nameof(visitor.FilteredFileFound)), "FilteredFileFound events amount is not equal to expected amount");
		}

		[Test]
		public void FileSystemVisitorShouldExcludeFilteredFileSystemEntires()
		{
			//Arrange
			var visitor = _container.Resolve<IFileSystemVisitor>();
			string filterValue = "A";
			string excludeValue = "SubFileA";
			visitor.Filter = x => x.Name.Contains(filterValue);
			visitor.FilteredFileFound += (sender, found) =>
			{
				if (sender is FileSystemInfoBase fileSysemInfo && fileSysemInfo.Name.Contains(excludeValue))
				{
					found.IsExcluded = true;
				}
			};

			var expectedFileredEntries = TestEntires.Where(x => x.Contains(filterValue) && !x.Contains(excludeValue));

			//Act
			IList<string> foundEntries = visitor.Enumerate("Root").Select(x => x.FullName.Replace(CurrentDirectory, string.Empty)).ToList();

			//Assert
			var diff = foundEntries.Except(expectedFileredEntries);
			Assert.AreEqual(0, diff.Count(), "Found and Expected entiry collections are different");
		}

		[Test]
		public void FileSystemVisitorShouldTerminateEnumerationOnCondition()
		{
			//Arrange
			var visitor = _container.Resolve<IFileSystemVisitor>();
			visitor.FilteredDirectoryFound += (sender, found) => found.IsProcessTerminated = true;
			visitor.FilteredFileFound += (sender, found) => found.IsProcessTerminated = true;

			//Act
			IList<string> foundEntries = visitor.Enumerate("Root").Select(x => x.FullName.Replace(CurrentDirectory, string.Empty)).ToList();

			//Assert
			Assert.AreEqual(0, foundEntries.Count, "Found and Expected entiry collections are different");
		}
	}
}
