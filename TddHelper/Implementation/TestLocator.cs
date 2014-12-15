
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using DreamWorks.TddHelper.Model;
using DreamWorks.TddHelper.View;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Window = System.Windows.Window;

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
		private readonly IVsUIShell _shell;
		
		private readonly CachedFileAssociations _cachedFileAssociations;
		
		public TestLocator(DTE2 dte, IVsUIShell shell)
		{
			_dte = dte;
			_shell = shell;
			_cachedFileAssociations = new CachedFileAssociations(string.Empty);
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
			{
				_unitTestPath = FindPathToTestFile(fileName);
				_implementationPath = sourcePath;
			}
			else
			{
				_unitTestPath = sourcePath;
				_implementationPath = FindPathImplementationFile(fileName);
			}

			Load();
		}

		private void Load()
		{
			if (!File.Exists(_unitTestPath) || !File.Exists(_implementationPath))
				return;

			SaveAndUnloadDocuments();

			if (StaticOptions.TddHelper.NoSplit)
				LoadDocumentsIntoOneTabWell();
			else
				LoadAndPlaceImplementationAndTest();
		}

		private void LoadAndPlaceImplementationAndTest()
		{
			ActivateFirstDocument();
			try
			{
				if (StaticOptions.TddHelper.UnitTestLeft)
				{
					_dte.ExecuteCommand(OpenFileCommand, _unitTestPath);
					_dte.ExecuteCommand(OpenFileCommand, _implementationPath);
					var doc = GetDocumentForPath(_implementationPath);
					if (doc != null)
						doc.Activate();
				}
				else
				{
					_dte.ExecuteCommand(OpenFileCommand, _implementationPath);
					_dte.ExecuteCommand(OpenFileCommand, _unitTestPath);
					var doc = GetDocumentForPath(_unitTestPath);
					if (doc != null)
						doc.Activate();
				}

				if (ViewUtil.IsMoreThanOneTabWellShown())
				{
					_dte.ExecuteCommand(WindowMoveToNextTabGroupCommand);
				}
				else
				{
					_dte.ExecuteCommand(NewVerticalTabGroupCommand);
				}
			}
			catch (COMException e)
			{
				Debug.WriteLine(e.Message);
				Debug.WriteLine(e.StackTrace);
			}
		}

		private void LoadDocumentsIntoOneTabWell()
		{
			_dte.ExecuteCommand(OpenFileCommand, _implementationPath);
			_dte.ExecuteCommand(OpenFileCommand, _unitTestPath);
		}

		private void SaveAndUnloadDocuments()
		{
			var unitTestDocument = GetDocumentForPath(_unitTestPath);
			var implementationDocument = GetDocumentForPath(_implementationPath);

			if (StaticOptions.TddHelper.Clean)
			{
				_dte.ExecuteCommand(FileSaveAll);
				_dte.ExecuteCommand(WindowCloseAllDocuments);
			}
			else
			{
				SaveAndCloseIfOpen(unitTestDocument);
				SaveAndCloseIfOpen(implementationDocument);
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

		internal bool ActivateFirstDocument()
		{
			foreach (EnvDTE.Window window in _dte.Windows)
			{
				// document in the first tab well has Left==32
				if (window.Kind == Document && window.Left == 32)
				{
					window.Activate();
					window.Document.Activate();
					return true;
				}
			}
			return false;
		}

		public void GetCSharpFilesFromSolution()
		{
			var solution = _dte.Solution;
			
			_cachedFileAssociations.UpdateSolutionId(solution.ExtenderCATID;);
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
					GetFilesFromProjectItem(item);
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

		private void GetFilesFromProjectItem(ProjectItem item)
		{
			if (item.FileCount == 0)
				return;
			if (item.FileCount == 1)
			{
				var filePath = item.get_FileNames(0);
				if (filePath.ToLower().EndsWith(CsharpFileExtension))
					_fileList.Add(filePath);
				return;
			}

			for (short i = 0; i < item.FileCount; i++)
			{
				if (item.FileNames[i].ToLower().EndsWith(CsharpFileExtension) &&
				    item.Document != null)
				{
					_fileList.Add(Path.Combine(item.Document.Path, item.Name));
				}
			}
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

			return Find(testFileName);
		}

		public string FindPathImplementationFile(string csFile)
		{
			var idx = csFile.LastIndexOf(TestDotCs, StringComparison.OrdinalIgnoreCase);
			if (idx == -1)
				return string.Empty;
			var implFile = csFile.Substring(0, idx) + CsharpFileExtension;

			return Find(implFile);
		}

		private string Find(string searchedFile)
		{
			var candidateList = new List<string>();
			foreach (var fullPathToFile in _fileList)
			{
				var fileName = Path.GetFileName(fullPathToFile);
				if (String.Equals(fileName, searchedFile, StringComparison.OrdinalIgnoreCase))
				{
					if (File.Exists(fullPathToFile))
						candidateList.Add(fullPathToFile);
				}
			}

			if (candidateList.Count == 0)
				return string.Empty;
			if (candidateList.Count == 1)
				return candidateList[0];
			if (candidateList.Count > 1)
				return ResolveConflicts(searchedFile, candidateList);
			return string.Empty;
		}

		private string ResolveConflicts(string searchedFile, IEnumerable<string> candidateList)
		{
			var targetFile = FindTargetFileInCache(searchedFile);

			if (!string.IsNullOrEmpty(targetFile))
				return targetFile;

			var resolveFileConflictDialog = new ResolveFileConflictDialog(candidateList);
			SetModalDialogOwner(resolveFileConflictDialog);

			var dlgResult = resolveFileConflictDialog.ShowDialog();
			if (dlgResult.HasValue && dlgResult == true)
				return resolveFileConflictDialog.ViewModel.SelectedFile.Path;
			return string.Empty;
		}

		private string FindTargetFileInCache(string searchedFile)
		{
			string targetFile;
			
			var isTest =
				searchedFile.ToLower().EndsWith(StaticOptions.TddHelper.TestFileSuffix.ToLower());
			if (isTest)
				targetFile = _cachedFileAssociations.ImplementationFromTest(searchedFile);
			else
				targetFile = _cachedFileAssociations.TestFromImplementation(searchedFile);
			return targetFile;
		}

		private void SetModalDialogOwner(Window targetWindow)
		{
			IntPtr hWnd;
			_shell.GetDialogOwnerHwnd(out hWnd);
			// ReSharper disable once PossibleNullReferenceException
			var parent = HwndSource.FromHwnd(hWnd).RootVisual;
			targetWindow.Owner = (Window)parent;
		}
	}
}