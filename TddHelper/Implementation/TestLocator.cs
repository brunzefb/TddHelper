using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace DreamWorks.TddHelper.Implementation
{
	internal class TestLocator
	{
		private readonly List<ProjectItem> _projectItemList = new List<ProjectItem>();
		private readonly List<ProjectItem> _subItemList = new List<ProjectItem>();
		private readonly List<string> _fileList = new List<string>();
		public const string FullPathPropertyName = "FullPath";
		private const string CsprojExtension = ".csproj";
		private const string CsharpFileExtension = ".cs";
		private const string OpenFileCommand = "File.OpenFile";
		private const string TestDotCs = "test.cs";
		private const char Period = '.';
		private readonly DTE2 _dte;


		public TestLocator(DTE2 dte)
		{
			_dte = dte;
		}

		internal void OpenTestOrImplementation(object sender, EventArgs e)
		{
			
			if (_dte.ActiveWindow == null || _dte.ActiveDocument == null || _dte.ActiveWindow.Document == null)
				return;

			var fullName = _dte.ActiveWindow.Document.FullName;
			var isTest = fullName.ToLower().EndsWith(TestDotCs);
			var isCs = fullName.ToLower().EndsWith(CsharpFileExtension);

			if (!isCs)
				return;
			
			GetCSharpFilesFromSolution();

			var fileName = Path.GetFileName(fullName);
			string targetToActivate;
			if (!isTest)
				targetToActivate = FindPathToTestFile(fileName);
			else
				targetToActivate = FindPathImplementationFile(fileName);

			if (!_dte.get_IsOpenFile(Constants.vsViewKindTextView, targetToActivate))
				_dte.ExecuteCommand(OpenFileCommand, targetToActivate);
		}

		public void GetCSharpFilesFromSolution()
		{
			var solution = _dte.Solution;
			var solutionProjects = solution.Projects;

			if (solution == null || solutionProjects == null)
				return;

			_fileList.Clear();

			foreach (var p in solutionProjects)
			{
				var project = p as Project;
				if (project == null)
					continue;

				var props = project.Properties;

				if (!HasProperty(props, (FullPathPropertyName)))
					continue;

				if (!project.FileName.EndsWith(CsprojExtension))
					continue;

				_projectItemList.Clear();
				foreach (ProjectItem item in project.ProjectItems)
				{
					_subItemList.Clear();
					var mainItem = RecursiveGetProjectItem(item);
					_projectItemList.Add(mainItem);
					_projectItemList.AddRange(_subItemList);
				}
				foreach (var item in _projectItemList)
					GetFilesFromProjectItem(item, Path.GetDirectoryName(project.FileName));

			}
		}

		public List<string> ProjectFiles
		{
			get { return _fileList; }
		}

		private ProjectItem RecursiveGetProjectItem(ProjectItem item)
		{
			if (item.ProjectItems == null)
				return item;

			foreach (ProjectItem innerItem in item.ProjectItems)
			{
				_subItemList.Add(RecursiveGetProjectItem(innerItem));
			}
			return item;
		}

		private void GetFilesFromProjectItem(ProjectItem item, string directoryName)
		{
			if (item.FileCount == 0)
				return;
			if (item.FileCount == 1)
			{
				if (item.Name.ToLower().EndsWith(CsharpFileExtension))
					_fileList.Add(Path.Combine(directoryName, item.Name));
				return;
			}
			
			for (short i = 0; i < item.FileCount; i++)
				if (item.FileNames[i].ToLower().EndsWith(CsharpFileExtension))
					_fileList.Add(Path.Combine(directoryName, item.FileNames[i]));
		}

		private bool HasProperty(Properties properties, string propertyName)
		{
			if (properties != null)
			{
				foreach (Property item in properties)
				{
					if (item != null && item.Name == propertyName)
						return true;
				}
			}
			return false;
		}

		public string FindPathToTestFile(string csFile)
		{
			var idx = csFile.LastIndexOf(Period);
			if (idx == -1)
				return string.Empty;
			var testFileName = csFile.Substring(0, idx) + TestDotCs;

			foreach (var fullPathToFile in _fileList)
			{
				var fileName = Path.GetFileName(fullPathToFile);
				if (String.Equals(fileName, testFileName, StringComparison.OrdinalIgnoreCase))
					if (File.Exists(fullPathToFile))
						return fullPathToFile;
			}
			return string.Empty;
		}

		public string FindPathImplementationFile(string csFile)
		{
			var idx = csFile.LastIndexOf(TestDotCs, StringComparison.OrdinalIgnoreCase);
			if (idx == -1)
				return string.Empty;
			var implFile = csFile.Substring(0, idx) + CsharpFileExtension;

			foreach (var fullPathToFile in _fileList)
			{
				var fileName = Path.GetFileName(fullPathToFile);
				if (String.Equals(fileName, implFile, StringComparison.OrdinalIgnoreCase))
					if (File.Exists(fullPathToFile))
						return fullPathToFile;
			}
			return string.Empty;
		}
	}
}