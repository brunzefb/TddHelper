using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using DreamWorks.TddHelper.Implementation;
using DreamWorks.TddHelper.Model;
using DreamWorks.TddHelper.Utility;
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
		new[] {"Change Tdd Helper reference options"})]
	[Guid(GuidList.guidTddHelperPkgString)]
	public sealed class TddHelperPackage : Package
	{
		private TabJumper _tabJumper;
		private TestLocator _solutionHelper;
		private ProjectModel _projectModel;
		private static readonly log4net.ILog Logger = log4net.LogManager.
			GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public TddHelperPackage()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += currentDomain_UnhandledException;
		}

		protected override void Initialize()
		{
			base.Initialize();
			SetupLogging();
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

		private void SetupLogging()
		{
			const string defaultLogConfigTemplate =
				@"
  <log4net>
    <appender name='LogFileAppender' type='log4net.Appender.RollingFileAppender'>
      <file value='{REPLACE}' />
      <appendToFile value='true' />
      <rollingStyle value='Size' />
      <maxSizeRollBackups value='2' />
      <maximumFileSize value='20MB' />
      <datePattern value='yyyy-MM-dd.HH-mm' />
      <staticLogFileName value='true' />
      <immediateFlush value='true' />
      <lockingModel type='log4net.Appender.FileAppender+MinimalLock' />
      <layout type='log4net.Layout.PatternLayout'>
        <conversionPattern value='%date{ISO8601}&#9;%-4thread&#9;%level&#9;%message%newline' />
      </layout>
    </appender>
    <appender name='ConsoleAppender' type='log4net.Appender.ConsoleAppender'>
      <target value='Console.Error' />
      <layout type='log4net.Layout.PatternLayout'>
        <conversionPattern value='%date{ISO8601}&#9;%-4thread&#9;%level&#9;%message%newline' />
      </layout>
    </appender>
    <root>
      <level value='ALL' />
      <appender-ref ref='ConsoleAppender' />
      <appender-ref ref='LogFileAppender' />
    </root>
  </log4net>
";
			var commonApps = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			var folder = Path.Combine(commonApps, @"TddHelper");
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			string pathToLogfile = Path.Combine(folder, "TddHelper.log");
			var logConfig = defaultLogConfigTemplate.Replace("{REPLACE}", pathToLogfile);
			var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(logConfig));
			log4net.Config.XmlConfigurator.Configure(stream);
			Logger.Info("TddStarted logging at " + DateTime.Now);
		}

		private void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			if (ex == null)
				return;
			ExceptionLogHelper.LogException(ex);
		}
	}
}