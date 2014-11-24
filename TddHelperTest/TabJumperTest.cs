using System.Collections.Generic;
using DreamWorks.TddHelper.Implementation;
using EnvDTE;
using EnvDTE80;
using Moq;
using NUnit.Framework;

namespace TddHelperTest
{
	[TestFixture]
	public class TabJumperTest
	{
		private TabJumper _t;
		private Mock<DTE2> _mockDte;
		private Mock<Windows> _mockWins;
		private Mock<Document> _mockDoc;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			_mockDte = new Mock<DTE2>();
			_mockWins = new Mock<Windows>();
			_mockWins.Setup(p => p.GetEnumerator()).Returns(WindowList);
			_mockDte.Setup(p => p.Windows).Returns(_mockWins.Object);
			_mockDoc = new Mock<Document>();
			_mockDte.Setup(p => p.ActiveDocument).Returns(_mockDoc.Object);
			
		}

		private IEnumerator<Window> WindowList()
		{
			yield return MockSingleWindow(0, 0);
			yield return MockSingleWindow(1, 0);
			yield return MockSingleWindow(0, 1);
			yield return MockSingleWindow(2, 1);
		}

		private Window MockSingleWindow(int left, int top)
		{
			var win = new Mock<Window>();
			win.Setup(p => p.Left).Returns(left);
			win.Setup(p => p.Top).Returns(top);
			win.Setup(p => p.Kind).Returns("Document");
			if (left == 2 && top == 1)
				win.Setup(p => p.Document).Returns(_mockDoc.Object);
			else
				win.Setup(p => p.Document).Returns((Document)null);
			return win.Object;
		}

		[SetUp]
		public void SetUp()
		{
			_t = new TabJumper(_mockDte.Object);
		}

		[Test]
		public void GetSortedTopLevelWindows()
		{
			var topLevel = _t.GetSortedTopLevelWindows();
			Assert.That(topLevel.Count, Is.EqualTo(3));
		}

		[Test]
		public void FindActiveWindowIndex()
		{
			_mockWins.Setup(p => p.GetEnumerator()).Returns(WindowList);
			
			var index = _t.FindActiveWindowIndex(_t.GetSortedTopLevelWindows());
			Assert.That(index, Is.EqualTo(2));
		}

		[Test]
		public void ExecuteJump()
		{

		}
	}
}