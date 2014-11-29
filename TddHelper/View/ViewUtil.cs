using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace DreamWorks.TddHelper.View
{
	public static class ViewUtil
	{
		public static void VsShowMessageBox(IVsUIShell uiShell, string message)
		{
			var clsid = Guid.Empty;
			int result;
			ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
				0,
				ref clsid,
				Resources.AppTitle,
				message,
				string.Empty,
				0,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
				OLEMSGICON.OLEMSGICON_INFO,
				0,
				out result));
		}
	}
}