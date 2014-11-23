
using System.Collections.Generic;
using System.IO;
using DreamWorks.TddHelper;
using EnvDTE;
using EnvDTE80;
using Moq;
using NUnit.Framework;

namespace TddHelperTest
{
	[TestFixture]
	public class SolutionHelperTest
	{
		private Mock<DTE2> _mockDte;
		private Mock<Projects> _mockProjects;
		private Mock<Solution> _mockSolution;
		private Mock<Properties> _mockProps;
		private Mock<Property> _mockProp;
		
		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			_mockDte = new Mock<DTE2>();
			_mockProjects = new Mock<Projects>();
			_mockSolution = new Mock<Solution>();
			_mockProps = new Mock<Properties>();
			_mockProp = new Mock<Property>();

			_mockDte.Setup(dte => dte.Solution).Returns(() => _mockSolution.Object);
			_mockSolution.Setup(sol => sol.Projects).Returns(() => _mockProjects.Object);
			_mockProjects.Setup(projects => projects.GetEnumerator()).Returns(ProjectList);
			_mockProps.Setup(prop => prop.GetEnumerator()).Returns(PropertyList);
		}

		private Queue<Project> GetProjectQueue()
		{
			var projectMocksQueue = new Queue<Project>();
			var p1 = new Mock<Project>();
			p1.Setup(proj => proj.FileName).Returns(@"c:\\temp\foo.csproj");
			p1.Setup(proj => proj.Properties).Returns(_mockProps.Object);
			var mockProjectItems = new Mock<ProjectItems>();
			mockProjectItems.Setup(p => p.GetEnumerator()).Returns(ProjectItemList);
			p1.Setup(proj => proj.ProjectItems).Returns(mockProjectItems.Object);
			projectMocksQueue.Enqueue(p1.Object);
			var p2 = new Mock<Project>();
			p2.Setup(proj => proj.FileName).Returns(@"c:\\temp\bar.csproj");
			p2.Setup(proj => proj.Properties).Returns(_mockProps.Object);
			p2.Setup(proj => proj.ProjectItems).Returns(mockProjectItems.Object);
			projectMocksQueue.Enqueue(p2.Object);
			return projectMocksQueue;
		}

		private IEnumerator<ProjectItem> ProjectItemList()
		{
			var projectItemQueue = GetProjectItemQueue();
			while (projectItemQueue.Count > 0)
				yield return projectItemQueue.Dequeue();
		}

		private Queue<ProjectItem> GetProjectItemQueue()
		{
			var projectItemQueue = new Queue<ProjectItem>();
			var pi1 = new Mock<ProjectItem>();
			pi1.Setup(projItem => projItem.ProjectItems).Returns((ProjectItems)null);
			pi1.Setup(projItem => projItem.FileCount).Returns(0);
			projectItemQueue.Enqueue(pi1.Object);
			
			var pi2 = new Mock<ProjectItem>();
			pi2.Setup(projItem => projItem.ProjectItems).Returns((ProjectItems)null);
			pi2.Setup(projItem => projItem.FileCount).Returns(1);
			pi2.Setup(projItem => projItem.Name).Returns("file1.cs");
			projectItemQueue.Enqueue(pi2.Object);

			var pi3 = new Mock<ProjectItem>();
			pi3.Setup(projItem => projItem.ProjectItems).Returns((ProjectItems)null);
			pi3.Setup(projItem => projItem.FileCount).Returns(1);
			pi3.Setup(projItem => projItem.Name).Returns("file1.cs");
			projectItemQueue.Enqueue(pi3.Object);

			// test recursion
			var pi4 = new Mock<ProjectItem>();
			pi4.Setup(projItem => projItem.ProjectItems).Returns(GetSubProjectItems().Object);
			pi4.Setup(projItem => projItem.FileCount).Returns(0);
			// ReSharper restore UseIndexedProperty
			projectItemQueue.Enqueue(pi4.Object);

			var pi7 = new Mock<ProjectItem>();
			pi7.Setup(projItem => projItem.ProjectItems).Returns((ProjectItems)null);
			pi7.Setup(projItem => projItem.FileCount).Returns(2);
			// ReSharper disable UseIndexedProperty
			pi7.Setup(projItem => projItem.get_FileNames(0)).Returns(Path.GetTempFileName() + "Test.cs");
			pi7.Setup(projItem => projItem.get_FileNames(1)).Returns(Path.GetTempFileName() + ".cs");
			// ReSharper restore UseIndexedProperty
			projectItemQueue.Enqueue(pi7.Object);

			return projectItemQueue;
		}

		private Mock<ProjectItems> GetSubProjectItems()
		{
			var pi6 = new Mock<ProjectItems>();
			pi6.Setup(projItem => projItem.GetEnumerator()).Returns(SpecialProjectList);
			return pi6;
		}

		private IEnumerator<ProjectItem> SpecialProjectList()
		{
			var pi3 = new Mock<ProjectItem>();
			pi3.Setup(projItem => projItem.ProjectItems).Returns((ProjectItems)null);
			pi3.Setup(projItem => projItem.FileCount).Returns(1);
			pi3.Setup(projItem => projItem.Name).Returns("file1.cs");
			yield return pi3.Object;
		}

		private IEnumerator<Project> ProjectList()
		{
			var projectMocksQueue = GetProjectQueue();
			while (projectMocksQueue.Count > 0)
				yield return projectMocksQueue.Dequeue();

		}

		private IEnumerator<Property> PropertyList()
		{
			_mockProp.Setup(p => p.Name).Returns(SolutionHelper.FullPathPropertyName);
			yield return _mockProp.Object;
		}

		[Test]
		public void EnumerateFiles()
		{
			var helper = new SolutionHelper(_mockDte.Object);
			helper.GetCSharpFilesFromSolution();
			Assert.That(helper.ProjectFiles.Count, Is.EqualTo(10));
		}

		[Test]
		public void FindPathToImpl()
		{
			const string testFileName = @"c:\temp\file1.cs";
			if (File.Exists(testFileName))
				File.Delete(testFileName);
			var helper = new SolutionHelper(_mockDte.Object);
			helper.GetCSharpFilesFromSolution();
			Assert.That(helper.FindPathImplementationFile("foobar"), Is.Empty);
			var testFile = helper.FindPathImplementationFile("file1Test.cs");
			Assert.That(testFile, Is.Empty); // file does not exist
			File.Create(testFileName);
			testFile = helper.FindPathImplementationFile("file1Test.cs");
			Assert.That(testFile, Is.SamePath(testFileName));
		}

		[Test]
		public void FindPathToTest()
		{
			var helper = new SolutionHelper(_mockDte.Object);
			helper.GetCSharpFilesFromSolution();
			Assert.That(helper.FindPathToTestFile("foobar"), Is.Empty);
			var list = helper.ProjectFiles;
			var fullPathToTest = list[3];
			var index = fullPathToTest.LastIndexOf("Test", System.StringComparison.Ordinal);
			var fullPathToImpl = fullPathToTest.Substring(0, index) + ".cs";
			var impl = helper.FindPathToTestFile(Path.GetFileName(fullPathToImpl));
			Assert.That(impl, Is.Empty); // file does not exist
			File.Create(fullPathToTest);
			var foundFile = helper.FindPathToTestFile(Path.GetFileName(fullPathToImpl));
			Assert.That(fullPathToTest, Is.SamePath(foundFile));
		}
	}
}