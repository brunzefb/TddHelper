using DreamWorks.TddHelper.Model;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;

namespace DreamWorks.TddHelper.Implementation
{
	public static class Access
	{
		public static DTE2 Dte { get; set; }
		public static IProjectModel ProjectModel { get; set; }
		public static IVsUIShell Shell { get; set; }
		public static IVsPackageInstaller PackageInstaller { get; set; }
	}
}