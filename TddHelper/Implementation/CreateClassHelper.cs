
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DreamWorks.TddHelper.Resources;
using DreamWorks.TddHelper.View;
using EnvDTE;
using EnvDTE80;

namespace DreamWorks.TddHelper.Implementation
{
	public static class CreateClassHelper
	{
		private const string CSharpLanguageName = "CSharp";
		private const string ClassItemTemplateName = "Class.zip";

		private static ProjectItem LastProjectItem;
	
		public static bool TryToCreateNewTargetClass()
		{
			if (!StaticOptions.MainOptions.AutoCreateTestFile)
			{
				const int noDontCreateFile = 7; // winuser.h - IDNO
				var result = ViewUtil.VsShowMessageBox(Access.Shell, Strings.ConfirmFileCreation);
				if (result == noDontCreateFile)
					return false;
			}
			var targetProjectPath = GetAssociatedTargetProjectPath();
			if (string.IsNullOrEmpty(targetProjectPath))
				return false;

			var solution = Access.Dte.Solution as Solution2;
			if (solution == null)
				return false;
			var templatePath = solution.GetProjectItemTemplate(ClassItemTemplateName, CSharpLanguageName);
			var targetProject = ProjectFromPath(targetProjectPath);
			if (targetProject == null)
				return false;

			return CreateTargetClassInTargetProject(targetProject, templatePath);
		}
		private static Project ProjectFromPath(string path)
		{
			foreach (Project project in Access.Dte.Solution.Projects)
			{
				try
				{
					if (!string.IsNullOrEmpty(project.FullName) &&
						string.Equals(project.FullName, path, StringComparison.CurrentCultureIgnoreCase))
						return project;
				}
				catch
				{
				}
			}
			return null;
		}
		private static string PathForProject(string project)
		{
			foreach (Project proj in Access.Dte.Solution.Projects)
			{
				if (proj.Name.Contains(project))
					return proj.FullName;
			}
			return null;
		}

		private static bool CreateTargetClassInTargetProject(Project targetProject,
			string classTemplatePath)
		{
			var sourceProjectPath = Access.ProjectModel.ProjectPathFromFilePath(SourceTargetInfo.SourcePath);
			Project sourceProject = ProjectFromPath(sourceProjectPath);

			if (!StaticOptions.MainOptions.MirrorProjectFolders)
			{
				targetProject.ProjectItems.AddFromTemplate(classTemplatePath, SourceTargetInfo.TargetFileName);
				SetTargetPathWithAddedItem(targetProject.ProjectItems);
				return true;
			}

			var relative = RelativePathHelper.GetRelativePath(Path.GetDirectoryName(sourceProject.FullName),
				Path.GetDirectoryName(SourceTargetInfo.SourcePath));
			var trimmed = relative.TrimStart(new[] { '.', Path.DirectorySeparatorChar });

			// If we just have a single item without dir separator chars
			// then there is no folder structure to create
			if (string.IsNullOrEmpty(trimmed) ||
				!trimmed.Contains(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
			{
				targetProject.ProjectItems.AddFromTemplate(classTemplatePath, SourceTargetInfo.TargetFileName);
				SetTargetPathWithAddedItem(targetProject.ProjectItems);
				return true;
			}

			// get a stack
			var folderArray = trimmed.Split(new[] { Path.DirectorySeparatorChar });
			var folderStack = StackFromArray(folderArray);

			// add folders recursively if they don't exist
			AddOrGetProjectFolderItem(targetProject.ProjectItems, folderStack);
			LastProjectItem.ProjectItems.AddFromTemplate(classTemplatePath, SourceTargetInfo.TargetFileName);

			SetTargetPathWithAddedItem(LastProjectItem.ProjectItems);
			return true;
		}

		private static void AddOrGetProjectFolderItem(ProjectItems projectItems, Stack<string> folderStack)
		{
			while (folderStack.Count > 0)
			{
				var currentFolder = folderStack.Pop();

				if (ContainsItem(currentFolder, projectItems))
				{
					LastProjectItem = projectItems.Item(currentFolder);
					AddOrGetProjectFolderItem(LastProjectItem.ProjectItems, folderStack); // recurse
				}
				else
				{
					LastProjectItem = projectItems.AddFolder(currentFolder);
					AddOrGetProjectFolderItem(LastProjectItem.ProjectItems, folderStack); // recurse
				}
			}
		}

		private static bool ContainsItem(string itemName, ProjectItems items)
		{
			return items.Cast<ProjectItem>().Any(item => item.Name == itemName);
		}
		private static Stack<string> StackFromArray(string[] folderArray)
		{
			var folderStack = new Stack<string>();
			// stack is LIFO, so we must add in reverse order
			// we omit the first folder passed in due to string processing.
			for (var i = folderArray.Length - 1; i > 0; i--)
				folderStack.Push(folderArray[i]);
			return folderStack;
		}

		private static void SetTargetPathWithAddedItem(ProjectItems items)
		{
			// AddFromTemplate returns null, according to MSDN :-(
			// since we just added the item, it must be there and
			// at the correct level.
			foreach (ProjectItem item in items)
			{
				if (item.Name != SourceTargetInfo.TargetFileName)
					continue;
				if (item.Document == null)
					continue;
				SourceTargetInfo.TargetPath = item.Document.FullName;
				return;
			}
			Debug.Assert(false);
		}

		private static string GetAssociatedTargetProjectPath()
		{
			string targetProjectPath;
			string sourceProjectPath = Access.ProjectModel.ProjectPathFromFilePath(SourceTargetInfo.SourcePath);

			if (string.IsNullOrEmpty(sourceProjectPath))
				return null;

			// check the cache first
			if (SourceTargetInfo.IsSourcePathTest)
				targetProjectPath =
					Access.ProjectModel.ImplementationProjectFromTestProject(sourceProjectPath);
			else
				targetProjectPath =
					Access.ProjectModel.TestProjectFromImplementationProject(sourceProjectPath);

			if (!string.IsNullOrEmpty(targetProjectPath))
				return targetProjectPath;

			// ask user in which project they want to create missing test/implementation
			var associateTestProjectDialog = new AssociateTestProject(sourceProjectPath);
			ViewUtil.SetModalDialogOwner(associateTestProjectDialog);

			var dlgResult = associateTestProjectDialog.ShowDialog();
			if (!dlgResult.HasValue || dlgResult != true)
				return null;
			if (!associateTestProjectDialog.ViewModel.RequestCreateProject)
				return associateTestProjectDialog.SelectedProject;

			var projectName = associateTestProjectDialog.ViewModel.NewProjectName;
			if (!CreateProjectHelper.CreateProject(projectName))
				return null;
			return PathForProject(projectName);
		}
	}
}