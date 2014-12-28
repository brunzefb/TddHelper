
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using DreamWorks.TddHelper.Implementation;
using DreamWorks.TddHelper.Model;
using DreamWorks.TddHelper.View;
using DreamWorks.TddHelper.ViewModel;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using NuGet.VisualStudio;


namespace DreamWorks.TddHelper
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideOptionPage(typeof(OptionsPageCustom), "Tdd Helper", "TddHelper", 100, 102, true,
		new[] {"Change Tdd Helper options"})]
	[ProvideOptionPage(typeof(AddReferenceOptions), "Tdd Helper", "TestAssemblyReference", 100, 113, true,
		new[] { "Change Tdd Helper reference options" })]
	[Guid(GuidList.guidTddHelperPkgString)]
	public sealed class TddHelperPackage : Package
	{
		private TabJumper _tabJumper;
		private TestLocator _solutionHelper;
		private ProjectModel _projectModel;

		public TddHelperPackage()
		{
		}

		protected override void Initialize()
		{
			base.Initialize();

			var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
			var packageInstaller = componentModel.GetService<IVsPackageInstaller>();
			var dte = (DTE2)GetService(typeof(DTE));
			var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
			LoadOptions();
			_projectModel = new ProjectModel(dte);
			_tabJumper = new TabJumper(dte);
			_solutionHelper = new TestLocator(dte, uiShell, packageInstaller, _projectModel);
			
			var menuCommandService =
				GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (null == menuCommandService)
				return;

			var cmdIdJumpRight = new CommandID(GuidList.guidTddHelperCmdSet,
				(int)PkgCmdIDList.cmdIdJumpRight);
			var menuItemJumpRight = new MenuCommand(_tabJumper.JumpRight, cmdIdJumpRight);
			menuCommandService.AddCommand(menuItemJumpRight);

			var cmdIdJumpLeft = new CommandID(GuidList.guidTddHelperCmdSet,
				(int)PkgCmdIDList.cmdIdJumpLeft);
			var menuItemJumpLeft = new MenuCommand(_tabJumper.JumpLeft, cmdIdJumpLeft);
			menuCommandService.AddCommand(menuItemJumpLeft);

			var cmdIdLocateTest = new CommandID(GuidList.guidTddHelperCmdSet,
				(int)PkgCmdIDList.cmdIdLocateTest);
			var menuItemLocateTest = new MenuCommand(_solutionHelper.OpenTestOrImplementation,
				cmdIdLocateTest);
			menuCommandService.AddCommand(menuItemLocateTest);
		}
		
		private static void LoadOptions()
		{
			var options = new OptionsViewModel();
			if (!string.IsNullOrEmpty(TddSettings.Default.Settings))
			{
				options = JsonConvert.DeserializeObject<OptionsViewModel>(TddSettings.Default.Settings);
			}
			StaticOptions.MainOptions = options;

			var addOptions = new AddReferencesOptionsViewModel();
			if (!string.IsNullOrEmpty(TddSettings.Default.AddAssemblySettings))
			{
				addOptions = JsonConvert.DeserializeObject<AddReferencesOptionsViewModel>(TddSettings.Default.AddAssemblySettings);
			}
			StaticOptions.ReferencesOptions = addOptions;
		}
	}
}