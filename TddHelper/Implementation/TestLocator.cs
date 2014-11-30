// Copyright AB SCIEX 2014. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		private const string Document = "Document";
		private const int Left = 0;
		private const int Right = 1;
		private const int OnlyOne = 1;
		
		private Window _tempWindow;
		private readonly DTE2 _dte;
		private const string NewVerticalTabGroupCommand = "Window.NewVerticalTabGroup";
		private const string WindowMoveToPreviousTabGroup = "Window.MoveToPreviousTabGroup";
		private const string WindowMoveToNextTabGroupCommand = "Window.MoveToNextTabGroup";
		private const string NewFileCommand = "File.NewFile";
		private const string DummyTextDocument = "Text.txt";

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
			var isSourcePathTest = sourcePath.ToLower().EndsWith(StaticOptions.TddHelper.TestFileSuffix);
			var isCs = sourcePath.ToLower().EndsWith(CsharpFileExtension);

			if (!isCs)
				return;

			GetCSharpFilesFromSolution();

			var fileName = Path.GetFileName(sourcePath);
			string targetToActivate;
			if (!isSourcePathTest)
				targetToActivate = FindPathToTestFile(fileName);
			else
				targetToActivate = FindPathImplementationFile(fileName);

			LoadOther(sourcePath, targetToActivate, isSourcePathTest);
		}

		private void LoadOther(string sourcePath, string targetPath, bool sourcePathIsTest)
		{
			var targetDocument = GetDocumentForPath(targetPath);
			if (null == targetDocument)
			{
				_dte.ExecuteCommand(OpenFileCommand, targetPath);
			}
			else
				targetDocument.Activate();

			if (!StaticOptions.TddHelper.NoSplit)
			{
				var sortedTopLevelWindows = GetSortedTopLevelWindows();
				if (sortedTopLevelWindows.Count() == OnlyOne)
					_dte.ExecuteCommand(NewVerticalTabGroupCommand);
				sortedTopLevelWindows.Clear();

				ArrangeWindows(sourcePath, targetPath, sourcePathIsTest);
			}
			var sourceDocument = GetDocumentForPath(sourcePath);
			if (sourceDocument != null)
				sourceDocument.Activate();
		}

		private void ArrangeWindows(string sourcePath, string targetPath, bool sourcePathIsTest)
		{
			int indexOfTarget = FindActiveWindowIndex(); // we loaded or activated target
			if (!sourcePathIsTest) 
			{
				// target is test
				var indexOfTest = indexOfTarget;
				WindowMoveHelper(indexOfTest, Right, false); 
				
				// source is the implementation file
				var implementationDocument = GetDocumentForPath(sourcePath);
				if (implementationDocument != null)
					implementationDocument.Activate();
				var indexOfImplementation = FindActiveWindowIndex();
				WindowMoveHelper(indexOfImplementation, Left, true);
			}
			else
			{
				// target is implementation
				var indexOfImplementation = indexOfTarget;
				WindowMoveHelper(indexOfImplementation, Left, true);

				// source is the test file
				var implementationDocument = GetDocumentForPath(targetPath);
				if (implementationDocument != null)
					implementationDocument.Activate();
				var indexOfTest = FindActiveWindowIndex();
				WindowMoveHelper(indexOfTest, Right, false); 

			}
			if (_tempWindow != null)
				_tempWindow.Close();
		}


		private void WindowMoveHelper(int windowIndexToCheck, int positionToCheckForMove, bool moveToNext)
		{
			var isUnitTestLeft = StaticOptions.TddHelper.UnitTestLeft;
			string windowMoveCommand = moveToNext? WindowMoveToNextTabGroupCommand : WindowMoveToPreviousTabGroup;
			
			if (isUnitTestLeft && windowIndexToCheck == positionToCheckForMove)
			{
				var topLevelWindow = WindowAtIndex(windowIndexToCheck);
				if (topLevelWindow == null)
					return;
				if (topLevelWindow.Collection.Count == OnlyOne)
				{
					_dte.ExecuteCommand(NewFileCommand, DummyTextDocument);
					_tempWindow = _dte.ActiveWindow;
				}
				_dte.ExecuteCommand(windowMoveCommand);
			}
		}

		private Window WindowAtIndex(int index)
		{
			int currentIndex = 0;

			var sortedTopLevelWindows = GetSortedTopLevelWindows();
			try
			{
				foreach (Window window in sortedTopLevelWindows)
				{
					if (currentIndex == index)
						return window;
					currentIndex++;
				}
			}
			finally
			{
				sortedTopLevelWindows.Clear();
			}
			return null;
		}


		private Document GetDocumentForPath(string targetPath)
		{
			var sortedTopLevelWindows = GetSortedTopLevelWindows();
			try
			{
				foreach (var topLevelWindow in sortedTopLevelWindows)
				{
					foreach (Window lowerLevelWindow in topLevelWindow.Collection)
					{
						if (lowerLevelWindow.Document != null &&
						    !string.IsNullOrEmpty(lowerLevelWindow.Document.Path))
						{
							var fullPath = Path.Combine(lowerLevelWindow.Document.Path, lowerLevelWindow.Caption);
							if (string.Equals(fullPath, targetPath, StringComparison.CurrentCultureIgnoreCase))
								return lowerLevelWindow.Document;
						}
					}
				}
			}
			finally
			{
				sortedTopLevelWindows.Clear();
			}
			return null;
		}

		internal int FindActiveWindowIndex()
		{
			var sortedTopLevelWindows = GetSortedTopLevelWindows();
			try
			{
				for (var i = 0; i < sortedTopLevelWindows.Count; ++i)
				{
					if (sortedTopLevelWindows[i].Document != _dte.ActiveDocument)
						continue;
					return i;
				}
			}
			finally
			{
				sortedTopLevelWindows.Clear();
			}
			return 0;
		}

		internal List<Window> GetSortedTopLevelWindows()
		{
			// Documents with a "left" or "top" value > 0 are the focused ones in each group, 
			// so we only need to collect those
			var topLevelWindows = new List<Window>();
			foreach (Window window in _dte.Windows)
			{
				if (window.Kind == Document && (window.Left == 0 || window.Top == 0))
					topLevelWindows.Add(window);
			}
			topLevelWindows.Sort((a, b) => a.Left < b.Left ? -1 : 1);
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