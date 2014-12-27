using System.IO;
using MvvmFx.MvvmLight;

namespace DreamWorks.TddHelper.ViewModel
{
	public class AddReferencesOptionsViewModel : ViewModelBase
	{
		private const string BrowseToSelect = "[Browse to Select Assembly]";
		private bool _useNuGet = true;
		private bool _useFileAssembly;
		private string _assemblyPath = BrowseToSelect;
		private string _packageId = "nUnit";
		private string _versionMajor = "2";
		private string _versionMinor = "6";
		private string _versionBuild = "3";
		private string _viewAssemblyPath = BrowseToSelect;

		public AddReferencesOptionsViewModel Clone(AddReferencesOptionsViewModel other)
		{
			_useNuGet = other.UseNuGet;
			_useFileAssembly = other.UseFileAssembly;
			_packageId = other.PackageId;
			_versionMajor = other.VersionMajor;
			_versionMinor = other.VersionMinor;
			_versionBuild = other.VersionBuild;
			_assemblyPath = other.AssemblyPath;
			_viewAssemblyPath = other.ViewAssemblyPath;
			return this;
		}

		public bool UseNuGet
		{
			get { return _useNuGet; }
			set
			{
				_useNuGet = value;
				RaisePropertyChanged(() => UseNuGet);
			}
		}

		public bool UseFileAssembly
		{
			get { return _useFileAssembly; }
			set
			{
				_useFileAssembly = value;
				RaisePropertyChanged(() => UseFileAssembly);
			}
		}

		public string PackageId
		{
			get { return _packageId; }
			set
			{
				_packageId = value;
				RaisePropertyChanged(() => PackageId);
			}
		}

		public string VersionMajor
		{
			get { return _versionMajor; }
			set
			{
				_versionMajor = value;
				RaisePropertyChanged(() => VersionMajor);
			}
		}

		public string VersionMinor
		{
			get { return _versionMinor; }
			set
			{
				_versionMinor = value;
				RaisePropertyChanged(() => VersionMinor);
			}
		}

		public string VersionBuild
		{
			get { return _versionBuild; }
			set
			{
				_versionBuild = value;
				RaisePropertyChanged(() => VersionBuild);
			}
		}

		public string AssemblyPath
		{
			get { return _assemblyPath; }
			set
			{
				_assemblyPath = value;
				if (!string.IsNullOrEmpty(value))
					_viewAssemblyPath = Path.GetFileName(value);
				else
					_viewAssemblyPath = BrowseToSelect;
				RaisePropertyChanged(() => ViewAssemblyPath);
			}
		}

		public string ViewAssemblyPath
		{
			get { return _viewAssemblyPath; }
		}

		public void UpdateUI()
		{
			RaisePropertyChanged(() => UseNuGet);
			RaisePropertyChanged(() => UseFileAssembly);
			RaisePropertyChanged(() => PackageId);
			RaisePropertyChanged(() => VersionMajor);
			RaisePropertyChanged(() => VersionMinor);
			RaisePropertyChanged(() => VersionBuild);
			RaisePropertyChanged(() => ViewAssemblyPath);
		}
	}
}