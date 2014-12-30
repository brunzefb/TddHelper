
using System;
using System.Collections.Generic;
using System.IO;
using DreamWorks.TddHelper.Implementation;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.Model
{
	public class CachedProjectAssociations
	{
		private string _solutionGuid;

		public Dictionary<string, ImplementationToTestMapper> Associations { get; set; }

		public CachedProjectAssociations(string id)
		{
			_solutionGuid = id;
			Associations = new Dictionary<string, ImplementationToTestMapper>(StringComparer.OrdinalIgnoreCase);
			Messenger.Default.Register<OptionsClearProjectAssociationsCache>(this, OnCacheCleared);
		}

		private void OnCacheCleared(OptionsClearProjectAssociationsCache action)
		{
			ClearCache();
		}

		public void ClearCache()
		{
			if (Associations.ContainsKey(_solutionGuid))
			{
				var toTestMapper = Associations[_solutionGuid];
				toTestMapper.Clear();
			}
			Save();
		}

		public void UpdateSolutionId(string id)
		{
			_solutionGuid = id;
		}

		public void Load()
		{
			if (string.IsNullOrEmpty(_solutionGuid))
				return;
			if (string.IsNullOrEmpty(TddSettings.Default.ProjectAssociations)) return;
			var instance =
				JsonConvert.DeserializeObject<CachedFileAssociations>(
					TddSettings.Default.ProjectAssociations);
			Associations = instance.Associations;
		}
		
		public void Save()
		{
			if (string.IsNullOrEmpty(_solutionGuid))
				return;
			TddSettings.Default.FileAssociations = JsonConvert.SerializeObject(this);
			TddSettings.Default.Save();
		}

		public void AddAssociation(string implementationProject, string testProject)
		{
			if (string.IsNullOrEmpty(_solutionGuid))
				return;
			ImplementationToTestMapper implementationToTestMapper;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTestMapper = Associations[_solutionGuid];
			}
			else
			{
				implementationToTestMapper = new ImplementationToTestMapper();
				Associations.Add(_solutionGuid, implementationToTestMapper);
			}

			if (!implementationToTestMapper.Dictionary.ContainsKey(implementationProject))
				implementationToTestMapper.Add(implementationProject, testProject);
			else
				implementationToTestMapper[implementationProject] = testProject;
		}

		public string ImplementationProjectFromTestProject(string testProject)
		{
			if (string.IsNullOrEmpty(_solutionGuid))
				return string.Empty;
			ImplementationToTestMapper implementationToTestMapper;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTestMapper = Associations[_solutionGuid];
			}
			else
				return string.Empty;

			foreach (var implementationProjectFile in implementationToTestMapper.Keys)
			{
				if (string.Equals(implementationToTestMapper[implementationProjectFile],
					testProject, StringComparison.OrdinalIgnoreCase))
				{
					if (File.Exists(implementationProjectFile) &&
						ProjectPathListsContains(implementationProjectFile))
					{
						return implementationProjectFile;
					}
				}
			}
			return string.Empty;
		}

		private bool ProjectPathListsContains(string key)
		{
			foreach (var p in Access.ProjectModel.ProjectPathsList)
				if (string.Equals(p, key, StringComparison.OrdinalIgnoreCase))
					return true;
			return false;
		}

		public string TestProjectFromImplementationProject(string implementation)
		{
			if (string.IsNullOrEmpty(_solutionGuid))
				return string.Empty;
			ImplementationToTestMapper implementationToTestMapper;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTestMapper = Associations[_solutionGuid];
			}
			else
				return string.Empty;

			if (implementationToTestMapper.ContainsKey(implementation))
			{
				var testProject = implementationToTestMapper[implementation];
				if (File.Exists(testProject) &&
					Access.ProjectModel.ProjectPathsList.Contains(testProject))
					return testProject;
				return string.Empty;
			}
			return string.Empty;
		}
	}
}