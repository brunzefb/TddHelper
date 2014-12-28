using System;
using System.Diagnostics;
using System.IO;

namespace DreamWorks.TddHelper.Implementation
{
	public static class SourceTargetInfo
	{
		private const string CsharpFileExtension = ".cs";
		private const char Period = '.';

		public static string SourcePath { get; set; }
		public static string TargetPath { get; set; }

		public static void Clear()
		{
			SourcePath = string.Empty;
			TargetPath = string.Empty;
		}

		public static bool IsSourcePathCsFile
		{
			get { return SourcePath.ToLower().EndsWith(CsharpFileExtension); }
		}

		public static bool IsSourcePathTest
		{
			get
			{
				if (string.IsNullOrEmpty(SourcePath))
					return false;
				return SourcePath.ToLowerInvariant()
					.EndsWith(StaticOptions.MainOptions.TestFileSuffix.ToLowerInvariant());
			}
		}

		public static string TargetFileName
		{
			get { return GetTargetFileName(); }
		}

		private static string GetTargetFileName()
		{
			if (!StaticOptions.MainOptions.TestFileSuffix.Contains(CsharpFileExtension))
				return string.Empty;

			var sourceFileName = Path.GetFileName(SourcePath);
			Debug.Assert(sourceFileName != null);
			int index;
			if (IsSourcePathTest)
			{
				index = sourceFileName.LastIndexOf(StaticOptions.MainOptions.TestFileSuffix,
					StringComparison.OrdinalIgnoreCase);
				if (index == -1)
					return string.Empty;
				return sourceFileName.Substring(0, index) + CsharpFileExtension;
			}
			index = sourceFileName.LastIndexOf(Period);
			if (index == -1)
				return string.Empty;
			return sourceFileName.Substring(0, index) + StaticOptions.MainOptions.TestFileSuffix;
		}
	}
}