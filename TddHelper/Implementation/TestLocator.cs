
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using DreamWorks.TddHelper.Model;
using DreamWorks.TddHelper.Utility;
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
		private readonly Dictionary<string, string> _fileToProjectDictionary = new Dictionary<string, string>();
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
		private const string RelativePathToClassTemplate = @"ItemTemplates\CSharp\Code\1033\Class\Class.vstemplate";
		private string _unitTestPath;
		private string _implementationPath;
		private readonly IVsUIShell _shell;
		private bool _isSourcePathTest;
		private readonly CachedFileAssociations _cachedFileAssociations;
		private readonly CachedProjectAssociations _cachedProjectAssociations;
		private readonly List<string> _projectPathsList = new List<string>();
		
		public TestLocator(DTE2 dte, IVsUIShell shell)
		{
			_dte = dte;
			_shell = shell;
			_cachedFileAssociations = new CachedFileAssociations(string.Empty);
			_cachedProjectAssociations = new CachedProjectAssociations(string.Empty);
			_cachedFileAssociations.Load();
			_cachedProjectAssociations.Load();
		}

		internal void OpenTestOrImplementation(object sender, EventArgs e)
		{
			if (_dte.ActiveWindow == null || _dte.ActiveDocument == null ||
			    _dte.ActiveWindow.Document == null)
				return;

			var sourcePath = _dte.ActiveWindow.Document.FullName;
			_isSourcePathTest = sourcePath.ToLower().EndsWith(StaticOptions.TddHelper.TestFileSuffix.ToLower());
			var isCs = sourcePath.ToLower().EndsWith(CsharpFileExtension);

			if (!isCs)
				return;

			GetCSharpFilesFromSolution();

			var fileName = Path.GetFileName(sourcePath);
			_implementationPath = string.Empty;
			_unitTestPath = string.Empty;
			if (!_isSourcePathTest)
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
			if(!TryCreateTargetPath())
				return;
			
			SaveAndUnloadDocuments();

			if (StaticOptions.TddHelper.NoSplit)
				LoadDocumentsIntoOneTabWell();
			else
				LoadAndPlaceImplementationAndTest();
		}

		private bool TryCreateTargetPath()
		{
			var projectForTargetPath = GetProjectForTargetPath();
			if (string.IsNullOrEmpty(projectForTargetPath))
				return false;

			var visualStudioIdeFolder = VisualStudioHelper.GetVisualStudioInstallationDir(VisualStudioVersion.Vs2013);
			if (string.IsNullOrEmpty(visualStudioIdeFolder))
				return false;

			var classTemplate = Path.Combine(visualStudioIdeFolder,RelativePathToClassTemplate);
			if (!File.Exists(classTemplate))
				return false;

			var project = ProjectFromPath(projectForTargetPath);
			if (project == null)
				return false;
			
		
			var folderItem = project.ProjectItems.AddFolder("MyNewNewFolder3");
			var newItem = folderItem.ProjectItems.AddFromTemplate(classTemplate, "MyNewClass3.cs");
			return false;
		}

		Project ProjectFromPath(string path)
		{
			foreach (Project project in _dte.Solution.Projects)
			{

				if (string.Equals(project.FullName, path, StringComparison.CurrentCultureIgnoreCase))
					return project;
			}
			return null;
		}

		private string GetProjectForTargetPath()
		{
			// one of these two will not be there
			string unitTestProject = string.Empty;
			string implementationProject = string.Empty;

			if (_fileToProjectDictionary.ContainsKey(_unitTestPath))
				unitTestProject = _fileToProjectDictionary[_unitTestPath];
			if (_fileToProjectDictionary.ContainsKey(_implementationPath))
				implementationProject = _fileToProjectDictionary[_implementationPath];

			if (_isSourcePathTest && !string.IsNullOrEmpty(unitTestProject))
			{
				return GetAssociatedTargetProject(unitTestProject);
			}
			if (!_isSourcePathTest && !string.IsNullOrEmpty(implementationProject))
			{
				return GetAssociatedTargetProject(implementationProject);
			}
			Debug.Assert(false, "Should not happen");
			return string.Empty;
		}

		private string GetAssociatedTargetProject(string sourceProject)
		{
			string targetProject;

			// check the cache first
			if (_isSourcePathTest)
				targetProject = _cachedProjectAssociations.ImplementationProjectFromTestProject(sourceProject);
			else
				targetProject = _cachedProjectAssociations.TestProjectFromImplementationProject(sourceProject);

			if (!string.IsNullOrEmpty(targetProject)) 
				return targetProject;

			// ask user in which project they want to create missing test/implementation
			var associateTestProjectDialog = new AssociateTestProject(_projectPathsList, sourceProject, 
				_cachedProjectAssociations, _isSourcePathTest);
			SetModalDialogOwner(associateTestProjectDialog);

			var dlgResult = associateTestProjectDialog.ShowDialog();
			if (dlgResult.HasValue && dlgResult == true)
			{
				targetProject = associateTestProjectDialog.SelectedProject;
			}
			return targetProject;
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
			
			if (solution == null || solution.Projects == null)
				return;

			var solutionProjects = solution.Projects;
			RelativePathHelper.BasePath = Path.GetDirectoryName(solution.FullName);

			_cachedFileAssociations.UpdateSolutionId(solution.ExtenderCATID);
			_cachedProjectAssociations.UpdateSolutionId(solution.ExtenderCATID);
			_fileToProjectDictionary.Clear();
			
			_projectPathsList.Clear();

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

				_projectPathsList.Add(project.FullName);

				_projectItemList.Clear();
				foreach (ProjectItem item in project.ProjectItems)
				{
					_subItemList.Clear();
					var mainItem = RecursiveGetProjectItem(item);
					_projectItemList.Add(mainItem);
					_projectItemList.AddRange(_subItemList);
				}
				foreach (var item in _projectItemList)
					GetFilesFromProjectItem(item, project);
			}
		}

		public Dictionary<string,string> FilesToProjectDictionary
		{
			get { return _fileToProjectDictionary; }
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

		private void GetFilesFromProjectItem(ProjectItem item, Project project)
		{
			if (item.FileCount == 0)
				return;
			if (item.FileCount == 1)
			{
				var filePath = item.get_FileNames(0);
				if (filePath.ToLower().EndsWith(CsharpFileExtension))
					_fileToProjectDictionary.Add(filePath, project.FullName);
				
			}

			//should not happen more than one file per item
			Debug.Assert(item.FileCount <= 1);

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
			foreach (var fullPathToFile in _fileToProjectDictionary.Keys)
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
			if (!dlgResult.HasValue || dlgResult != true) 
				return string.Empty;

			var selectedFilePath = resolveFileConflictDialog.ViewModel.SelectedFile.Path;
			if (_isSourcePathTest)
				_cachedFileAssociations.AddAssociation(selectedFilePath, searchedFile);
			else
				_cachedFileAssociations.AddAssociation(searchedFile, selectedFilePath);
			_cachedFileAssociations.Save();
				
			return selectedFilePath;
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