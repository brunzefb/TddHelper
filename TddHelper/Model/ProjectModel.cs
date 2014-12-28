using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace DreamWorks.TddHelper.Model
{
	internal class ProjectModel
	{
		private readonly DTE2 _dte;
		private readonly CachedFileAssociations _cachedFileAssociations;
		private readonly CachedProjectAssociations _cachedProjectAssociations;
		private readonly List<string> _projectPathsList = new List<string>();
		private readonly List<ProjectItem> _projectItemList = new List<ProjectItem>();
		private readonly List<ProjectItem> _subItemList = new List<ProjectItem>();

		private readonly Dictionary<string, string> _fileToProjectDictionary =
			new Dictionary<string, string>();

		public const string FullPathPropertyName = "FullPath";
		private const string CsprojExtension = ".csproj";
		private const string CsharpFileExtension = ".cs";
		private static readonly ManualResetEvent BusyEvent = new ManualResetEvent(false);


		public ProjectModel(DTE2 dte)
		{
			_dte = dte;

			_cachedFileAssociations = new CachedFileAssociations(string.Empty);
			_cachedProjectAssociations = new CachedProjectAssociations(string.Empty);
			_cachedFileAssociations.Load();
			_cachedProjectAssociations.Load();

			SubscribeToSolutionEvents();
			var task = Task.Run(new Action(GetCSharpFilesFromSolution));
		}

		private void SubscribeToSolutionEvents()
		{
			_dte.Events.SolutionEvents.ProjectAdded += SolutionEvents_ProjectAdded;
			_dte.Events.SolutionEvents.ProjectRemoved += SolutionEvents_ProjectRemoved;
			_dte.Events.SolutionEvents.ProjectRenamed += SolutionEvents_ProjectRenamed;
			_dte.Events.SolutionItemsEvents.ItemAdded += SolutionItemsEvents_ItemAdded;
			_dte.Events.SolutionItemsEvents.ItemRemoved += SolutionItemsEvents_ItemRemoved;
			_dte.Events.SolutionItemsEvents.ItemRenamed += SolutionItemsEvents_ItemRenamed;
			_dte.Events.SolutionEvents.Opened += SolutionEvents_Opened;
			_dte.Events.SolutionEvents.AfterClosing += SolutionEvents_AfterClosing;
		}

		public void SolutionEvents_AfterClosing()
		{
			StaticOptions.IsSolutionLoaded = false;
			_fileToProjectDictionary.Clear();
			_projectPathsList.Clear();
			_projectItemList.Clear();
			_subItemList.Clear();
		}

		public void SolutionEvents_Opened()
		{
			StaticOptions.IsSolutionLoaded = true;
			if (_dte.Solution != null)
			{
				_cachedFileAssociations.UpdateSolutionId(_dte.Solution.ExtenderCATID);
				_cachedProjectAssociations.UpdateSolutionId(_dte.Solution.ExtenderCATID);
			}
		}

		public void SolutionItemsEvents_ItemRenamed(ProjectItem projectItem, string oldName)
		{
		}

		public void SolutionItemsEvents_ItemRemoved(ProjectItem projectItem)
		{
		}

		public void SolutionItemsEvents_ItemAdded(ProjectItem projectItem)
		{
		}

		public void SolutionEvents_ProjectRenamed(Project project, string oldName)
		{
		}

		public void SolutionEvents_ProjectRemoved(Project project)
		{
		}

		public void SolutionEvents_ProjectAdded(Project project)
		{
		}

		public void GetCSharpFilesFromSolution()
		{
			BusyEvent.Reset();
			try
			{
				var solution = _dte.Solution;

				if (solution == null || solution.Projects == null)
					return;

				var solutionProjects = solution.Projects;
				RelativePathHelper.BasePath = Path.GetDirectoryName(solution.FullName);

				
				_fileToProjectDictionary.Clear();
				
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
						GetFilesFromProjectItem(item, project);
				}

				GetProjectPathsList();
			}
			finally
			{
				BusyEvent.Set();
			}
			
		}

		private void GetProjectPathsList()
		{
			var solution = _dte.Solution;

			if (solution == null || solution.Projects == null)
				return;

			var solutionProjects = solution.Projects;
			
			_projectPathsList.Clear();
			foreach (Project project in solutionProjects)
			{
				if (!project.FileName.EndsWith(CsprojExtension))
					continue;
				_projectPathsList.Add(project.FileName);
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