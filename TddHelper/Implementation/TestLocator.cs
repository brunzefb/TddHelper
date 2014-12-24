using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

		private readonly Dictionary<string, string> _fileToProjectDictionary =
			new Dictionary<string, string>();

		public const string FullPathPropertyName = "FullPath";
		private const string CsprojExtension = ".csproj";
		private const string CsharpFileExtension = ".cs";
		private const char Period = '.';
		private const string Document = "Document";
		private readonly DTE2 _dte;
		private const string OpenFileCommand = "File.OpenFile";
		private const string NewVerticalTabGroupCommand = "Window.NewVerticalTabGroup";
		private const string WindowMoveToNextTabGroupCommand = "Window.MoveToNextTabGroup";
		private const string FileSaveAll = "File.SaveAll";
		private const string WindowCloseAllDocuments = "Window.CloseAllDocuments";
		private const string RelativePathToClassTemplate = @"ItemTemplates\CSharp\Code\1033\Class\Class.vstemplate";
		private string _sourcePath;
		private string _targetPath;
		private string _targetFileName;
		private bool _isSourcePathTest;

		private readonly IVsUIShell _shell;
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

			_sourcePath = _dte.ActiveWindow.Document.FullName;
			if (!_sourcePath.ToLower().EndsWith(CsharpFileExtension))
				return;

			_isSourcePathTest =
				_sourcePath.ToLowerInvariant()
					.EndsWith(StaticOptions.TddHelper.TestFileSuffix.ToLowerInvariant());

			GetCSharpFilesFromSolution();

			_targetFileName = GetTargetFileName();
			_targetPath = FindExistingFileWithConflictResolution();
			if (_targetPath == null) // dialog cancelled
				return;
			if (_targetPath == string.Empty) // not found
				if (!TryToCreateNewTargetClass())
					return;
			Load();
		}

		private string GetTargetFileName()
		{
			if (!StaticOptions.TddHelper.TestFileSuffix.Contains(CsharpFileExtension))
				return string.Empty;

			var sourceFileName = Path.GetFileName(_sourcePath);
			Debug.Assert(sourceFileName != null);
			int index;
			if (_isSourcePathTest)
			{
				index = sourceFileName.LastIndexOf(StaticOptions.TddHelper.TestFileSuffix,
					StringComparison.OrdinalIgnoreCase);
				if (index == -1)
					return string.Empty;
				return sourceFileName.Substring(0, index) + CsharpFileExtension;
			}
			index = sourceFileName.LastIndexOf(Period);
			if (index == -1)
				return string.Empty;
			return sourceFileName.Substring(0, index) + StaticOptions.TddHelper.TestFileSuffix;
		}

		private void Load()
		{
			if (string.IsNullOrEmpty(_targetPath))
				return;

			SaveAndUnloadDocuments();

			if (StaticOptions.TddHelper.NoSplit)
				LoadDocumentsIntoOneTabWell();
			else
				LoadAndPlaceImplementationAndTest();
		}

		private bool TryToCreateNewTargetClass()
		{
			var targetProjectPath = GetAssociatedTargetProjectPath();
			if (string.IsNullOrEmpty(targetProjectPath))
				return false;

			var visualStudioIdeFolder =
				VisualStudioHelper.GetVisualStudioInstallationDir(VisualStudioVersion.Vs2013);
			if (string.IsNullOrEmpty(visualStudioIdeFolder))
				return false;

			// this is (at time of writing 11/2014) pretty poorly documented - 
			// you must pass a .vstemplate file to the ProjectItems.AddFromTemplate API.
			var classTemplate = Path.Combine(visualStudioIdeFolder, RelativePathToClassTemplate);
			if (!File.Exists(classTemplate))
				return false;

			var targetProject = ProjectFromPath(targetProjectPath);
			if (targetProject == null)
				return false;

			return CreateTargetClassInTargetProject(targetProject, classTemplate);
		}


		private bool CreateTargetClassInTargetProject(Project targetProject,
			string classTemplate)
		{
			var sourceProjectPath = GetSourceProjectPath();
			Project sourceProject = ProjectFromPath(sourceProjectPath);

			if (!StaticOptions.TddHelper.MirrorProjectFolders)
			{
				targetProject.ProjectItems.AddFromTemplate(classTemplate, _targetFileName);
				return true;
			}

			var relative = RelativePathHelper.GetRelativePath(sourceProject.FullName, Path.GetDirectoryName(_sourcePath));
			var trimmed = relative.TrimStart(new[] { '.', Path.DirectorySeparatorChar });

			if (string.IsNullOrEmpty(trimmed) || !trimmed.Contains(Path.DirectorySeparatorChar.ToString()))
			{
				targetProject.ProjectItems.AddFromTemplate(classTemplate, _targetFileName);
				return true;
			}

			var folderArray = trimmed.Split(new[] { Path.DirectorySeparatorChar });
			var folderStack = StackFromArray(folderArray);

			AddOrGetProjectFolderItem(targetProject.ProjectItems, folderStack);
			LastProjectItem.ProjectItems.AddFromTemplate(classTemplate, _targetFileName);
			return true;
		}

		private static Stack<string> StackFromArray(string[] folderArray)
		{
			var folderStack = new Stack<string>();
			// stack is LIFO, so we must add in reverse order
			for (var i = folderArray.Length - 1; i > 0; i--)
				folderStack.Push(folderArray[i]);
			return folderStack;
		}

		private static ProjectItem LastProjectItem;
		private static void AddOrGetProjectFolderItem(ProjectItems projectItems, Stack<string> folderStack )
		{
			while (folderStack.Count > 0)
			{
				var currentFolder = folderStack.Pop();

				if (ContainsItem(currentFolder, projectItems))
				{
					LastProjectItem = projectItems.Item(currentFolder);
					AddOrGetProjectFolderItem(LastProjectItem.ProjectItems, folderStack);
				}
				else
				{
					LastProjectItem = projectItems.AddFolder(currentFolder);
					AddOrGetProjectFolderItem(LastProjectItem.ProjectItems, folderStack);
				}
			}
		}

		private static bool ContainsItem(string itemName, ProjectItems items)
		{
			return items.Cast<ProjectItem>().Any(item => item.Name == itemName);
		}

		private Project ProjectFromPath(string path)
		{
			foreach (Project project in _dte.Solution.Projects)
			{
				if (string.Equals(project.FullName, path, StringComparison.CurrentCultureIgnoreCase))
					return project;
			}
			return null;
		}


		private string GetAssociatedTargetProjectPath()
		{
			string targetProjectPath;
			string sourceProjectPath = GetSourceProjectPath();

			if (string.IsNullOrEmpty(sourceProjectPath))
				return null;
	
			// check the cache first
			if (_isSourcePathTest)
				targetProjectPath =
					_cachedProjectAssociations.ImplementationProjectFromTestProject(sourceProjectPath);
			else
				targetProjectPath =
					_cachedProjectAssociations.TestProjectFromImplementationProject(sourceProjectPath);

			if (!string.IsNullOrEmpty(targetProjectPath))
				return targetProjectPath;

			// ask user in which project they want to create missing test/implementation
			var associateTestProjectDialog = new AssociateTestProject(_projectPathsList,
				sourceProjectPath,
				_cachedProjectAssociations, _isSourcePathTest);
			SetModalDialogOwner(associateTestProjectDialog);

			var dlgResult = associateTestProjectDialog.ShowDialog();
			if (dlgResult.HasValue && dlgResult == true)
			{
				targetProjectPath = associateTestProjectDialog.SelectedProject;
			}
			return targetProjectPath;
		}

		private string GetSourceProjectPath()
		{
			if (!string.IsNullOrEmpty(_sourcePath) &&
			    _fileToProjectDictionary.ContainsKey(_sourcePath))
				return _fileToProjectDictionary[_sourcePath];
			return string.Empty;
		}

		private void LoadAndPlaceImplementationAndTest()
		{
			// Make sure the active window is in the first tab well
			// as we will load our douments there
			ActivateFirstDocument();

			try
			{
				if (StaticOptions.TddHelper.UnitTestLeft ^ _isSourcePathTest)
				{
					_dte.ExecuteCommand(OpenFileCommand, _targetPath);
					_dte.ExecuteCommand(OpenFileCommand, _sourcePath);
				}
				else
				{
					_dte.ExecuteCommand(OpenFileCommand, _sourcePath);
					_dte.ExecuteCommand(OpenFileCommand, _targetPath);
				}

				//var doc = GetDocumentForPath(_targetPath);
				//if (doc != null)
				//	doc.Activate();

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
			_dte.ExecuteCommand(OpenFileCommand, _sourcePath);
			_dte.ExecuteCommand(OpenFileCommand, _targetPath);
		}

		private void SaveAndUnloadDocuments()
		{
			var sourceDocument = GetDocumentForPath(_sourcePath);
			var targetDocument = GetDocumentForPath(_targetPath);

			if (StaticOptions.TddHelper.Clean)
			{
				_dte.ExecuteCommand(FileSaveAll);
				_dte.ExecuteCommand(WindowCloseAllDocuments);
			}
			else
			{
				SaveAndCloseIfOpen(sourceDocument);
				SaveAndCloseIfOpen(targetDocument);
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
			foreach (Document document in _dte.Documents)
			{
				if (string.Equals(document.FullName, targetPath,
					StringComparison.CurrentCultureIgnoreCase))
					return document;
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


		public Dictionary<string, string> FilesToProjectDictionary
		{
			get { return _fileToProjectDictionary; }
		}


		private string FindExistingFileWithConflictResolution()
		{
			var candidateList = new List<string>();
			foreach (var fullPathToFile in _fileToProjectDictionary.Keys)
			{
				var fileName = Path.GetFileName(fullPathToFile);
				if (String.Equals(fileName, _targetFileName, StringComparison.OrdinalIgnoreCase))
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
				return ResolveConflicts(candidateList);
			return string.Empty;
		}

		private string ResolveConflicts( IEnumerable<string> candidateList)
		{
			var targetFile = FindTargetFileInCache();

			if (!string.IsNullOrEmpty(targetFile))
				return targetFile;

			var resolveFileConflictDialog = new ResolveFileConflictDialog(candidateList);
			SetModalDialogOwner(resolveFileConflictDialog);

			var dlgResult = resolveFileConflictDialog.ShowDialog();
			if (!dlgResult.HasValue || dlgResult != true)
				return null;

			var selectedFilePath = resolveFileConflictDialog.ViewModel.SelectedFile.Path;
			if (_isSourcePathTest)
				_cachedFileAssociations.AddAssociation(selectedFilePath, _targetFileName);
			else
				_cachedFileAssociations.AddAssociation(_targetFileName, selectedFilePath);
			_cachedFileAssociations.Save();

			return selectedFilePath;
		}

		private string FindTargetFileInCache()
		{
			string targetFile;

			var isTest =
				_targetFileName.ToLower().EndsWith(StaticOptions.TddHelper.TestFileSuffix.ToLower());
			if (isTest)
				targetFile = _cachedFileAssociations.ImplementationFromTest(_targetFileName);
			else
				targetFile = _cachedFileAssociations.TestFromImplementation(_targetFileName);
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
				return;
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
	}
}