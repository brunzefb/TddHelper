using System;
using System.Globalization;
using Microsoft.Win32;

// from: http://pascoal.net/2011/04/getting-visual-studio-installation-directory/, 
// with small fixes and support for VS2013 by Friedrich Brunzema

namespace DreamWorks.TddHelper.Utility
{
	public enum VisualStudioVersion  {Vs2013=120, Vs2010 = 100, Vs2008 = 90 , Vs2005 = 80, VsNet2003 = 71, VsNet2002 = 70, Other = 0};

	public static class VisualStudioHelper
	{
		internal static string GetVisualStudioInstallationDir(VisualStudioVersion version)
		{
			string registryKeyString = String.Format(@"SOFTWARE\Microsoft\VisualStudio\{0}",
				GetVersionNumber(version));

			using (
				RegistryKey vsLocalMachineKey = Registry.LocalMachine.OpenSubKey(registryKeyString))
			{
				if (vsLocalMachineKey == null)
					return string.Empty;
				return vsLocalMachineKey.GetValue("InstallDir") as string;
			}
		}

		private static string GetVersionNumber(VisualStudioVersion version)
		{
			if (version == VisualStudioVersion.Other) throw new Exception("Not supported version");

			int intVersionNumber = (int)version / 10;
			return string.Format("{0:0.0}", (float)intVersionNumber);
		}
	}
}
