using System.Security.Cryptography;
using DreamWorks.TddHelper.ViewModel;
using NUnit.Framework;


namespace TddHelperTest
{
	[TestFixture]
	public class OptionsViewModelTest
	{
		OptionsViewModel _t;
		private bool _propertyChangedCalled;

		[SetUp]
		public void SetUp()
		{
			_t = new OptionsViewModel();
		}

		[Test]
		public void Properties()
		{
			_t.UnitTestLeft = true;
			Assert.That(_t.UnitTestLeft, Is.True);

			_t.UnitTestRight = true;
			Assert.IsTrue(_t.UnitTestRight);

			_t.NoSplit = true;
			Assert.IsTrue(_t.NoSplit);

			_t.TestFileSuffix = "abcd";
			Assert.That(_t.TestFileSuffix, Is.EqualTo("abcd"));

			_t.ProjectSuffix = "ee";
			Assert.That(_t.ProjectSuffix, Is.EqualTo("ee"));

			_t.AutoCreateTestProject = true;
			Assert.IsTrue(_t.AutoCreateTestProject);

			_t.AutoCreateTestFile = true;
			Assert.IsTrue(_t.AutoCreateTestFile);

			_t.MirrorProjectFolders = true;
			Assert.IsTrue(_t.MirrorProjectFolders);

			_t.CreateReference = true;
			Assert.IsTrue(_t.CreateReference);

			_t.MakeFriendAssembly = true;
			Assert.IsTrue(_t.MakeFriendAssembly);

			_t.Clean = true;
			Assert.IsTrue(_t.Clean);
			
		}

		[Test]
		public void UpdateUI()
		{
			_t.PropertyChanged += _t_PropertyChanged;
			_t.UpdateUI();
			Assert.That(_propertyChangedCalled, Is.True);
		}

		void _t_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			_propertyChangedCalled = true;
		}
	}
}