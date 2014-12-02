// Copyright AB SCIEX 2014. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using DreamWorks.TddHelper.View;
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

		private const string TestDotCs = "test.cs";
		private const char Period = '.';
		private const string Document = "Document";

		private readonly DTE2 _dte;
		private const string OpenFileCommand = "File.OpenFile";
		private const string NewVerticalTabGroupCommand = "Window.NewVerticalTabGroup";
		private const string WindowMoveToNextTabGroupCommand = "Window.MoveToNextTabGroup";
		private const string FileSaveAll = "File.SaveAll";
		private const string WindowCloseAllDocuments = "Window.CloseAllDocuments";
		private string _unitTestPath;
		private string _implementationPath;

		public TestLocator(DTE2 dte)
		{
			_dte = dte;
		}

		internal void OpenTestOrImplementation(object sender, EventArgs e)
		{
			if (_dte.ActiveWindow == null || _dte.ActiveDocument == null ||
			    _dte.ActiveWindow.Document == null)
				return;

			var sourcePath = _dte.ActiveWindow.Document.FullName;
			var isSourcePathTest =
				sourcePath.ToLower().EndsWith(StaticOptions.TddHelper.TestFileSuffix.ToLower());
			var isCs = sourcePath.ToLower().EndsWith(CsharpFileExtension);

			if (!isCs)
				return;

			GetCSharpFilesFromSolution();

			var fileName = Path.GetFileName(sourcePath);
			_implementationPath = string.Empty;
			_unitTestPath = string.Empty;
			if (!isSourcePathTest)
				_unitTestPath = FindPathToTestFile(fileName);
			else
				_implementationPath = FindPathImplementationFile(fileName);

			Load();
		}

		private void Load()
		{
			// sanity check
			var topLevel = GetTopLevelWindows();
			if (topLevel.Count == 0 || !File.Exists(_unitTestPath) ||
			    !File.Exists(_implementationPath))
				return;

			// need to activate left most window because FileOpen loads in current tab well
			var leftMostWindow = topLevel[0];
			leftMostWindow.Document.Activate();

			// save and close both source and target -- this avoids a lot of problem scenarios
			// such as floating windows
			var unitTestDocument = GetDocumentForPath(_unitTestPath);
			var implementationDocument = GetDocumentForPath(_implementationPath);

			UnloadDocuments(unitTestDocument, implementationDocument);

			if (!StaticOptions.TddHelper.NoSplit)
				LoadDocumentsIntoOneTabWell();
			else
				LoadAndPlaceImplementationAndTest();
		}

		private void LoadAndPlaceImplementationAndTest()
		{
			if (StaticOptions.TddHelper.UnitTestLeft)
			{
				_dte.ExecuteCommand(OpenFileCommand, _unitTestPath);
				_dte.ExecuteCommand(OpenFileCommand, _implementationPath);
			}
			else
			{
				_dte.ExecuteCommand(OpenFileCommand, _implementationPath);
				_dte.ExecuteCommand(OpenFileCommand, _unitTestPath);
			}

			if (ViewUtil.IsMoreThanOneTabWellShown())
				_dte.ExecuteCommand(WindowMoveToNextTabGroupCommand);
			else
				_dte.ExecuteCommand(NewVerticalTabGroupCommand);

			var unitTestDocument = GetDocumentForPath(_unitTestPath);
			unitTestDocument.Activate();
		}

		private void LoadDocumentsIntoOneTabWell()
		{
			_dte.ExecuteCommand(OpenFileCommand, _implementationPath);
			_dte.ExecuteCommand(OpenFileCommand, _unitTestPath);
		}

		private void UnloadDocuments(Document unitTestDocument, Document implementationDocument)
		{
			if (!StaticOptions.TddHelper.Clean)
			{
				SaveAndCloseIfOpen(unitTestDocument);
				SaveAndCloseIfOpen(implementationDocument);
			}
			else
			{
				_dte.ExecuteCommand(FileSaveAll);
				_dte.ExecuteCommand(WindowCloseAllDocuments);
			}
		}

		private static void SaveAndCloseIfOpen(Document document)
		{
			if (document != null)
			{
				document.Save();
				document.Close();
			}
		}


		private Document GetDocumentForPath(string targetPath)
		{
			foreach (Document d in _dte.Documents)
			{
				if (string.Equals(d.FullName, targetPath, StringComparison.CurrentCultureIgnoreCase))
					return d;
			}
			return null;
		}

		internal List<Window> GetTopLevelWindows()
		{
			// Documents with a "left" or "top" value > 0 are the focused ones in each group, 
			// so we only need to collect those
			var topLevelWindows = new List<Window>();
			foreach (Window window in _dte.Windows)
			{
				if (window.Kind == Document && (window.Left == 0 || window.Top == 0))
					topLevelWindows.Add(window);
			}

			return topLevelWindows;
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