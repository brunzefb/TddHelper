﻿
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DreamWorks.TddHelper.Implementation;
using DreamWorks.TddHelper.Resources;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using Microsoft.VisualStudio.Shell.Interop;

namespace DreamWorks.TddHelper.View
{
	public static class ViewUtil
	{
		private const string PartTabPanel = "PART_TabPanel";
		private static int _tabPanelCount;

		public static int VsShowMessageBox(IVsUIShell uiShell, string message)
		{
			var clsid = Guid.Empty;
			int result;
			ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
				0,
				ref clsid,
				Strings.AppTitle,
				message,
				string.Empty,
				0,
				OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
				OLEMSGICON.OLEMSGICON_INFO,
				0,
				out result));
			return result;
		}

		public static bool IsMoreThanOneTabWellShown()
		{
			FrameworkElement element;
			_tabPanelCount = 0;
			TryFindDocTabPanelElement(Application.Current.MainWindow, out element);
			return _tabPanelCount > 1;
		}

		public static void SetModalDialogOwner(Window targetWindow)
		{
			IntPtr hWnd;
			Access.Shell.GetDialogOwnerHwnd(out hWnd);
			// ReSharper disable once PossibleNullReferenceException
			var parent = HwndSource.FromHwnd(hWnd).RootVisual;
			targetWindow.Owner = (Window)parent;
		}

		private static bool TryFindDocTabPanelElement(FrameworkElement parent, out FrameworkElement foundChild)
		{
			foundChild = null;

			if (parent == null)
			{
				return false;
			}

			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				foundChild = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
				if (foundChild != null && foundChild.Name == PartTabPanel)
				{
					var docTabPanel = foundChild as DocumentTabPanel;
					if (docTabPanel != null)
					{
						var tabCount = docTabPanel.Children.Count;
						Debug.WriteLine("Got a docTabPanel with nTabs=" + tabCount);
						_tabPanelCount++;
						return false; // continue search for more
					}
				}
				if (TryFindDocTabPanelElement(foundChild, out foundChild))
				{
					return true;
				}
			}
			return false;
		}
	}
}