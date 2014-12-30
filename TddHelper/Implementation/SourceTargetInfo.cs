using System;
using System.IO;

namespace DreamWorks.TddHelper.Implementation
{
	public static class SourceTargetInfo
	{
		private const string CsharpFileExtension = ".cs";
		private const char Period = '.';
		private const string Space = " ";
		private static readonly log4net.ILog Logger = log4net.LogManager.
						GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


		public static string SourcePath { get; set; }
		public static string TargetPath { get; set; }

		public static string QuotedSourcePath
		{
			get
			{
				if (SourcePath.Contains(Space))
					return Enquote(SourcePath);
				return SourcePath;
			}
		}

		public static string QuotedTargetPath
		{
			get
			{
				if (TargetPath.Contains(Space))
					return Enquote(TargetPath);
				return TargetPath;
			}
		}

		private static string Enquote(string sourcePath)
		{
			return "\"" + sourcePath + "\"";
		}

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
			if (string.IsNullOrEmpty(sourceFileName))
			{
				Logger.Warn("SourceTargetInfo.GetTargetFileName() - sourceFileName is null");
				return string.Empty;
			}
			int index;
			if (IsSourcePathTest )
			{
				index = sourceFileName.LastIndexOf(StaticOptions.MainOptions.TestFileSuffix,
					StringComparison.OrdinalIgnoreCase);
				if (index == -1)
					return string.Empty;
				var fileName = sourceFileName.Substring(0, index) + CsharpFileExtension;
				Logger.InfoFormat("SourceTargetInfo.GetTargetFileName() returns: {0}", fileName);
				return fileName;
			}
			index = sourceFileName.LastIndexOf(Period);
			if (index == -1)
			{
				Logger.InfoFormat("SourceTargetInfo.GetTargetFileName cant find period, file={0}", sourceFileName);
				return string.Empty;
			}
			var targetFileName = sourceFileName.Substring(0, index) + 
				StaticOptions.MainOptions.TestFileSuffix;
			return targetFileName;
		}
	}
}