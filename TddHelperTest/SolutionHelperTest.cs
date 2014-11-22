
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

			var pi4 = new Mock<ProjectItem>();
			pi3.Setup(projItem => projItem.ProjectItems).Returns((ProjectItems)null);
			pi3.Setup(projItem => projItem.FileCount).Returns(2);
			// ReSharper disable UseIndexedProperty
			pi3.Setup(projItem => projItem.get_FileNames(0)).Returns(Path.GetTempFileName() +".cs");
			pi3.Setup(projItem => projItem.get_FileNames(1)).Returns(Path.GetTempFileName() +".cs");
			// ReSharper restore UseIndexedProperty
			projectItemQueue.Enqueue(pi4.Object);
			return projectItemQueue;
		}

		
		private IEnumerator<Project> ProjectList()
		{
			var projectMocksQueue = GetProjectQueue();
			while (projectMocksQueue.Count > 0)
			{
				yield return projectMocksQueue.Dequeue();
				var list = ProjectItemList(); // second foreach ProjectItem needs data
			}

		}

		private IEnumerator<Property> PropertyList()
		{
			_mockProp.Setup(p => p.Name).Returns(SolutionHelper.FullPathPropertyName);
			yield return _mockProp.Object;
		}

		[Test]
		public void Test()
		{
			var helper = new SolutionHelper(_mockDte.Object);
			helper.GetCSharpFilesFromSolution();
		}
	}
}