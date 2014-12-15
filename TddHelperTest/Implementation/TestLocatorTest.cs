using System.Collections.Generic;
using System.IO;
using DreamWorks.TddHelper.Implementation;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Moq;
using NUnit.Framework;
using Constants = EnvDTE.Constants;

namespace TddHelperTest.Implementation
{
	[TestFixture]
	public class TestLocatorTest
	{
		private Mock<DTE2> _mockDte;
		private Mock<Projects> _mockProjects;
		private Mock<Solution> _mockSolution;
		private Mock<Properties> _mockProps;
		private Mock<Property> _mockProp;
		private Mock<Window> _mockWin;
		private Mock<Document> _mockDoc;
		private TestLocator _testLocator;
		private Mock<IVsUIShell> _mockUiShell;
		
		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			_mockDte = new Mock<DTE2>();
			_mockProjects = new Mock<Projects>();
			_mockSolution = new Mock<Solution>();
			_mockProps = new Mock<Properties>();
			_mockProp = new Mock<Property>();
			_mockWin = new Mock<Window>();
			_mockDoc = new Mock<Document>();
			_mockUiShell = new Mock<IVsUIShell>();
			_mockDte.Setup(dte => dte.Solution).Returns(() => _mockSolution.Object);
			_mockSolution.Setup(sol => sol.Projects).Returns(() => _mockProjects.Object);
			_mockProjects.Setup(projects => projects.GetEnumerator()).Returns(ProjectList);
			_mockProps.Setup(prop => prop.GetEnumerator()).Returns(PropertyList);
		}

		[SetUp]
		public void SetUp()
		{
			_testLocator = new TestLocator(_mockDte.Object, _mockUiShell.Object);
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

		private IEnumerator<Project> NullProjectList()
		{
			yield return null;
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
			pi7.Setup(projItem => projItem.FileCount).Returns(3);
			// ReSharper disable UseIndexedProperty
			pi7.Setup(projItem => projItem.get_FileNames(0)).Returns(Path.GetTempFileName() + "Test.cs");
			pi7.Setup(projItem => projItem.get_FileNames(1)).Returns(Path.GetTempFileName() + ".cs");
			pi7.Setup(projItem => projItem.get_FileNames(2)).Returns(@"c:\temp\file1Test.cs");
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
			_mockProp.Setup(p => p.Name).Returns(TestLocator.FullPathPropertyName);
			yield return _mockProp.Object;
		}

		[Test]
		public void EnumerateFiles()
		{
			
			_testLocator.GetCSharpFilesFromSolution();
			Assert.That(_testLocator.ProjectFiles.Count, Is.EqualTo(12));
		}

		[Test]
		public void EnumerateFiles_Null_Objects()
		{
			_mockSolution.Setup(sol => sol.Projects).Returns(() => (Projects)null);
			_testLocator.GetCSharpFilesFromSolution();
			Assert.That(_testLocator.ProjectFiles.Count, Is.EqualTo(0));

			_mockSolution.Setup(sol => sol.Projects).Returns(() => _mockProjects.Object);
			_mockProjects.Setup(projects => projects.GetEnumerator()).Returns(NullProjectList());
			_testLocator.GetCSharpFilesFromSolution();
			Assert.That(_testLocator.ProjectFiles.Count, Is.EqualTo(0));
			_mockProjects.Setup(projects => projects.GetEnumerator()).Returns(ProjectList);

			_mockProps.Setup(prop => prop.GetEnumerator()).Returns(PropertyListNull);
			_testLocator.GetCSharpFilesFromSolution();
			Assert.That(_testLocator.ProjectFiles.Count, Is.EqualTo(0));
			_mockProps.Setup(prop => prop.GetEnumerator()).Returns(PropertyList);

			_mockProjects.Setup(projects => projects.GetEnumerator()).Returns(ProjectVbList);
			_testLocator.GetCSharpFilesFromSolution();
			Assert.That(_testLocator.ProjectFiles.Count, Is.EqualTo(0));
			_mockProjects.Setup(projects => projects.GetEnumerator()).Returns(ProjectList);

		}

		private IEnumerator<Project> ProjectVbList()
		{
			var projectMocksQueue = GetVbProjectQueue();
			while (projectMocksQueue.Count > 0)
				yield return projectMocksQueue.Dequeue();

		}
		private Queue<Project> GetVbProjectQueue()
		{
			var projectMocksQueue = new Queue<Project>();
			var p1 = new Mock<Project>();
			p1.Setup(proj => proj.FileName).Returns(@"c:\\temp\foo.vbproj");
			p1.Setup(proj => proj.Properties).Returns(_mockProps.Object);
			projectMocksQueue.Enqueue(p1.Object);
			return projectMocksQueue;
		}

		private IEnumerator<Property> PropertyListNull()
		{
			yield return null;
		}

		[Test]
		public void FindPathToImpl()
		{
			const string testFileName = @"c:\temp\file1.cs";
			if (File.Exists(testFileName))
				File.Delete(testFileName);
			
			_testLocator.GetCSharpFilesFromSolution();
			Assert.That(_testLocator.FindPathImplementationFile("foobar"), Is.Empty);
			var testFile = _testLocator.FindPathImplementationFile("file1Test.cs");
			Assert.That(testFile, Is.Empty); // file does not exist
			File.Create(testFileName);
			testFile = _testLocator.FindPathImplementationFile("file1Test.cs");
			Assert.That(testFile, Is.SamePath(testFileName));
		}

		[Test]
		public void FindPathToTest()
		{
			_testLocator.GetCSharpFilesFromSolution();
			Assert.That(_testLocator.FindPathToTestFile("foobar"), Is.Empty);
			var list = _testLocator.ProjectFiles;
			var fullPathToTest = list[3];
			var index = fullPathToTest.LastIndexOf("Test", System.StringComparison.Ordinal);
			var fullPathToImpl = fullPathToTest.Substring(0, index) + ".cs";
			var impl = _testLocator.FindPathToTestFile(Path.GetFileName(fullPathToImpl));
			Assert.That(impl, Is.Empty); // file does not exist
			File.Create(fullPathToTest);
			var foundFile = _testLocator.FindPathToTestFile(Path.GetFileName(fullPathToImpl));
			Assert.That(fullPathToTest, Is.SamePath(foundFile));
		}

		[Test]
		public void OpenTestOrImplementation_Happy_Implementation()
		{
			_mockDoc.Setup(a => a.FullName).Returns(@"c:\temp\file1Test.cs");
			_mockWin.Setup(a => a.Document).Returns(_mockDoc.Object);
			_mockDte.Setup(a => a.ActiveWindow).Returns(_mockWin.Object);
			_mockDte.Setup(a => a.ActiveDocument).Returns(_mockDoc.Object);
			
			_mockDte.Setup(a => a.get_IsOpenFile(Constants.vsViewKindTextView, It.IsAny<string>())).Returns(false);
			_mockDte.Setup(a => a.ExecuteCommand(It.IsAny<string>(), It.IsAny<string>()))
				.Callback((string a, string b)=>ExecuteCallback(a,b));

			_testLocator.OpenTestOrImplementation(null, null);
		}

		
		[Test]
		public void OpenTestOrImplementation_Happy_TestFile()
		{
			_testLocator.GetCSharpFilesFromSolution();
			File.Create(@"c:\temp\file1Test.cs");
			_mockDoc.Setup(a => a.FullName).Returns(@"c:\temp\file1.cs");
			_mockWin.Setup(a => a.Document).Returns(_mockDoc.Object);
			_mockDte.Setup(a => a.ActiveWindow).Returns(_mockWin.Object);
			_mockDte.Setup(a => a.ActiveDocument).Returns(_mockDoc.Object);

			_mockDte.Setup(a => a.get_IsOpenFile(Constants.vsViewKindTextView, It.IsAny<string>())).Returns(false);
			_mockDte.Setup(a => a.ExecuteCommand(It.IsAny<string>(), It.IsAny<string>()))
				.Callback((string a, string b) => ExecuteCallback2(a, b));

			_testLocator.OpenTestOrImplementation(null, null);
		}

		[Test]
		public void OpenTestOrImplementation_NonHappy()
		{
			_mockDoc.Setup(a => a.FullName).Returns(@"c:\temp\file1Test.cs");
			_mockWin.Setup(a => a.Document).Returns(_mockDoc.Object);
			_mockDte.Setup(a => a.ActiveWindow).Returns((Window)null);
			_testLocator.OpenTestOrImplementation(null, null);

			_mockDoc.Setup(a => a.FullName).Returns(@"c:\temp\NonCSFile.txt");
			_mockWin.Setup(a => a.Document).Returns(_mockDoc.Object);
			_mockDte.Setup(a => a.ActiveWindow).Returns(_mockWin.Object);
			_mockDte.Setup(a => a.ActiveDocument).Returns(_mockDoc.Object);
			_testLocator.OpenTestOrImplementation(null, null);

		}

		public void ExecuteCallback(string command, string targetToActivate)
		{
			Assert.That(targetToActivate, Is.EqualTo(@"c:\temp\file1.cs"));
		}

		public void ExecuteCallback2(string command, string targetToActivate)
		{
			Assert.That(targetToActivate, Is.EqualTo(@"c:\temp\file1Test.cs"));
		}
	}
}