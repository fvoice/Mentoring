using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace FileSystemVisitor.Tests
{
	/// <summary>
	/// Unit tests for <see cref="FileSystemVisitor"/>
	/// </summary>
	[TestClass]
	public class FileSystemVisitorTests
	{
		private IUnityContainer _container;

		/// <summary>
		/// Test initialization
		/// </summary>
		[TestInitialize]
		public void Setup()
		{
			_container = new UnityContainer();
			_container.RegisterType<IFileSystemVisitor, FileSystemVisitor>();
		}

		/// <summary>
		/// Checks that <see cref="FileSystemVisitor"/> is failed on non initialized state
		/// </summary>
		[TestMethod]
		public void FileSystemVisitorShouldBeFailedOnNonInitializedState()
		{
			var visitor = _container.Resolve<IFileSystemVisitor>();
			//var e = Environment;

			Assert.ThrowsException<Exception>(() => visitor.ToList());
		}

		/// <summary>
		/// Checks that <see cref="FileSystemVisitor"/> is failed on non existing directory configuration
		/// </summary>
		[TestMethod]
		public void FileSystemVisitorShouldBeFailedOnNonExistingDirectory()
		{
			var visitor = _container.Resolve<IFileSystemVisitor>();

			Assert.ThrowsException<Exception>(() => visitor.Initialize(@"QWERTY:\fictional directory\and one more\"));
		}
	}
}
