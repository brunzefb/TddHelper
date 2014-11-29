// Copyright 2014 Friedrich Brunzema [brunzefb AT gmail DOT com]
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using DreamWorks.TddHelper.Implementation;
using DreamWorks.TddHelper.View;
using EnvDTE;
using EnvDTE80;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace DreamWorks.TddHelper
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideOptionPageAttribute(typeof(OptionsPageCustom), "Tdd Helper", "TddHelper", 100, 102, true, new [] { "Change Tdd Helper options" })]
	[Guid(GuidList.guidTddHelperPkgString)]
	public sealed class TddHelperPackage : Package
	{
		
		
		private readonly TabJumper _tabJumper;
		private readonly TestLocator _solutionHelper;

		public TddHelperPackage()
		{
			var dte = (DTE2)GetService(typeof(DTE));
			_tabJumper = new TabJumper(dte);
			_solutionHelper = new TestLocator(dte);
		}

		protected override void Initialize()
		{
			base.Initialize();

			var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (null == menuCommandService)
				return;

			var cmdIdJumpRight = new CommandID(GuidList.guidTddHelperCmdSet, (int)PkgCmdIDList.cmdIdJumpRight);
			var menuItemJumpRight= new MenuCommand(_tabJumper.JumpRight, cmdIdJumpRight);
			menuCommandService.AddCommand(menuItemJumpRight);

			var cmdIdJumpLeft = new CommandID(GuidList.guidTddHelperCmdSet, (int)PkgCmdIDList.cmdIdJumpLeft);
			var menuItemJumpLeft = new MenuCommand(_tabJumper.JumpLeft, cmdIdJumpLeft);
			menuCommandService.AddCommand(menuItemJumpLeft);

			var cmdIdLocateTest = new CommandID(GuidList.guidTddHelperCmdSet, (int)PkgCmdIDList.cmdIdLocateTest);
			var menuItemLocateTest = new MenuCommand(_solutionHelper.OpenTestOrImplementation, cmdIdLocateTest);
			menuCommandService.AddCommand(menuItemLocateTest);
			
		}
		
	}

}