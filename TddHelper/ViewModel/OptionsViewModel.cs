using MvvmFx.MvvmLight;

namespace DreamWorks.TddHelper.ViewModel
{
	internal class OptionsViewModel : ViewModelBase
	{
		private string _testFileSuffix;
		private string _projectSuffix;
		private bool _autoCreateTestProject;
		private bool _autoCreateTestFile;
		private bool _mirrorProjectFolders;
		private bool _createReference;
		private bool _makeFriendAssembly;
		private bool _unitTestLeft;
		private bool _unitTestRight;
		private bool _noSplitWindow;
		private bool _clean;

		public bool UnitTestLeft
		{
			get { return _unitTestLeft; }
			set
			{
				_unitTestLeft = value;
				RaisePropertyChanged(() => UnitTestLeft);
			}
		}

		public bool UnitTestRight
		{
			get { return _unitTestRight; }
			set
			{
				_unitTestRight = value;
				RaisePropertyChanged(() => UnitTestRight);
			}
		}

		public bool NoSplit
		{
			get { return _noSplitWindow; }
			set
			{
				_noSplitWindow = value;
				RaisePropertyChanged(() => NoSplit);
			}
		}

		public string TestFileSuffix
		{
			get { return _testFileSuffix; }
			set
			{
				_testFileSuffix = value;
				RaisePropertyChanged(() => TestFileSuffix);
			}
		}

		public string ProjectSuffix
		{
			get { return _projectSuffix; }
			set
			{
				_projectSuffix = value;
				RaisePropertyChanged(() => ProjectSuffix);
			}
		}

		public bool AutoCreateTestProject
		{
			get { return _autoCreateTestProject; }
			set
			{
				_autoCreateTestProject = value;
				RaisePropertyChanged(() => AutoCreateTestProject);
			}
		}

		public bool AutoCreateTestFile
		{
			get { return _autoCreateTestFile; }
			set
			{
				_autoCreateTestFile = value;
				RaisePropertyChanged(() => AutoCreateTestFile);
			}
		}

		public bool MirrorProjectFolders
		{
			get { return _mirrorProjectFolders; }
			set
			{
				_mirrorProjectFolders = value;
				RaisePropertyChanged(() => MirrorProjectFolders);
			}
		}

		public bool CreateReference
		{
			get { return _createReference; }
			set
			{
				_createReference = value;
				RaisePropertyChanged(() => CreateReference);
			}
		}

		public bool MakeFriendAssembly
		{
			get { return _makeFriendAssembly; }
			set
			{
				_makeFriendAssembly = value;
				RaisePropertyChanged(() => MakeFriendAssembly);
			}
		}

		public bool Clean
		{
			get { return _clean; }
			set
			{
				_clean = value;
				RaisePropertyChanged(() => Clean);
			}
		}
	}
}