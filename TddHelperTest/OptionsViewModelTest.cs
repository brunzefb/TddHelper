using DreamWorks.TddHelper.ViewModel;
using NUnit.Framework;


namespace TddHelperTest
{
	[TestFixture]
	public class OptionsViewModelTest
	{
		OptionsViewModel _optionsViewModel;
		private bool _propertyChangedCalled;

		[SetUp]
		public void SetUp()
		{
			_optionsViewModel = new OptionsViewModel();
		}

		[Test]
		public void Properties()
		{
			_optionsViewModel.UnitTestLeft = true;
			Assert.That(_optionsViewModel.UnitTestLeft, Is.True);

			_optionsViewModel.UnitTestRight = true;
			Assert.IsTrue(_optionsViewModel.UnitTestRight);

			_optionsViewModel.NoSplit = true;
			Assert.IsTrue(_optionsViewModel.NoSplit);

			_optionsViewModel.TestFileSuffix = "abcd";
			Assert.That(_optionsViewModel.TestFileSuffix, Is.EqualTo("abcd"));

			_optionsViewModel.ProjectSuffix = "ee";
			Assert.That(_optionsViewModel.ProjectSuffix, Is.EqualTo("ee"));

			_optionsViewModel.AutoCreateTestProject = true;
			Assert.IsTrue(_optionsViewModel.AutoCreateTestProject);

			_optionsViewModel.AutoCreateTestFile = true;
			Assert.IsTrue(_optionsViewModel.AutoCreateTestFile);

			_optionsViewModel.MirrorProjectFolders = true;
			Assert.IsTrue(_optionsViewModel.MirrorProjectFolders);

			_optionsViewModel.CreateReference = true;
			Assert.IsTrue(_optionsViewModel.CreateReference);

			_optionsViewModel.MakeFriendAssembly = true;
			Assert.IsTrue(_optionsViewModel.MakeFriendAssembly);

			_optionsViewModel.Clean = true;
			Assert.IsTrue(_optionsViewModel.Clean);
			
		}

		[Test]
		public void UpdateUI()
		{
			_optionsViewModel.PropertyChanged += optionsViewModelPropertyChanged;
			_optionsViewModel.UpdateUI();
			Assert.That(_propertyChangedCalled, Is.True);
		}

		void optionsViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			_propertyChangedCalled = true;
		}
	}
}