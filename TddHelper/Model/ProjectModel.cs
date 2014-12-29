using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DreamWorks.TddHelper.Utility;
using EnvDTE;
using EnvDTE80;

namespace DreamWorks.TddHelper.Model
{
	public class ProjectModel : IProjectModel
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

		public ProjectModel(DTE2 dte)
		{
			_dte = dte;
			_cachedFileAssociations = new CachedFileAssociations(string.Empty);
			_cachedProjectAssociations = new CachedProjectAssociations(string.Empty);
			_cachedFileAssociations.Load();
			_cachedProjectAssociations.Load();
			
		}

		public List<string> ProjectPathsList 
		{
			get
			{
				return _projectPathsList;
			}
		}

		public void UpdateSolutionId()
		{
			_cachedFileAssociations.UpdateSolutionId(_dte.Solution.ExtenderCATID);
			_cachedProjectAssociations.UpdateSolutionId(_dte.Solution.ExtenderCATID);
		}

		public List<string> CsharpFilesInProject
		{
			get { return _fileToProjectDictionary.Keys.ToList(); }
		}

		public void AddFileAssociationToCache(string implementation, string test)
		{
			_cachedFileAssociations.AddAssociation(implementation, test);
			_cachedFileAssociations.Save();
		}

		public void AddProjectAssociationToCache(string implementation, string test)
		{
			_cachedProjectAssociations.AddAssociation(implementation, test);
			_cachedProjectAssociations.Save();
		}

		public string ProjectPathFromFilePath(string path )
		{
			if (!string.IsNullOrEmpty(path) &&
				_fileToProjectDictionary.ContainsKey(path))
			{
				return _fileToProjectDictionary[path];
			}
			return string.Empty;
		}

		public void Clean()
		{
			_fileToProjectDictionary.Clear();
			_projectPathsList.Clear();
			_projectItemList.Clear();
			_subItemList.Clear();
		}

		public void GetCSharpFilesFromSolution()
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
				// ReSharper disable once UseIndexedProperty
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

		public string ImplementationProjectFromTestProject(string sourceProjectPath)
		{
			return _cachedProjectAssociations.ImplementationProjectFromTestProject(sourceProjectPath);
		}

		public string TestProjectFromImplementationProject(string sourceProjectPath)
		{
			return _cachedProjectAssociations.TestProjectFromImplementationProject(sourceProjectPath);
		}

		public string FindTargetFileInCache(string targetFileName)
		{
			string foundFile;
			var isTest =
				targetFileName.ToLower().EndsWith(StaticOptions.MainOptions.TestFileSuffix.ToLower());
			if (isTest)
				foundFile = _cachedFileAssociations.ImplementationFromTest(targetFileName);
			else
				foundFile = _cachedFileAssociations.TestFromImplementation(targetFileName);
			return foundFile;
		}
	}
}