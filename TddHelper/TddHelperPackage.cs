// Copyright AB SCIEX 2014. All rights reserved.

using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using DreamWorks.TddHelper.Implementation;
using DreamWorks.TddHelper.View;
using DreamWorks.TddHelper.ViewModel;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideOptionPage(typeof(OptionsPageCustom), "Tdd Helper", "TddHelper", 100, 102, true,
		new[] {"Change Tdd Helper options"})]
	[Guid(GuidList.guidTddHelperPkgString)]
	public sealed class TddHelperPackage : Package
	{
		private TabJumper _tabJumper;
		private TestLocator _solutionHelper;
		

		public TddHelperPackage()
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			var dte = (DTE2)GetService(typeof(DTE));
			LoadOptions();
			_tabJumper = new TabJumper(dte);
			_solutionHelper = new TestLocator(dte);
			
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
			StaticOptions.TddHelper = options;
		}
	}
}