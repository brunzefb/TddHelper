
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

		internal Dictionary<string, ImplementationToTest> Associations { get; set; }

		public CachedProjectAssociations(string id)
		{
			_solutionGuid = id;
			Associations = new Dictionary<string, ImplementationToTest>();
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
				var implementationToTest = Associations[_solutionGuid];
				implementationToTest.Clear();
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
			var implLower = implementationProject.ToLowerInvariant();
			var testLower = testProject.ToLowerInvariant();
			if (string.IsNullOrEmpty(_solutionGuid))
				return;
			ImplementationToTest implementationToTest;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTest = Associations[_solutionGuid];
			}
			else
			{
				implementationToTest = new ImplementationToTest();
				Associations.Add(_solutionGuid, implementationToTest);
			}

			if (!implementationToTest.ContainsKey(implLower))
				implementationToTest.Add(implLower, testLower);
			else
				implementationToTest[implLower] = testLower;
		}

		public string ImplementationProjectFromTestProject(string testProject)
		{
			var testProjectLower = testProject.ToLowerInvariant();
			if (string.IsNullOrEmpty(_solutionGuid))
				return string.Empty;
			ImplementationToTest implementationToTest;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTest = Associations[_solutionGuid];
			}
			else
				return string.Empty;

			foreach (var implementationKey in implementationToTest.Keys)
			{
				if (implementationToTest[implementationKey] == testProjectLower)
				{
					if (File.Exists(implementationKey) && 
						Access.ProjectModel.ProjectPathsList.Contains(implementationKey))
						return implementationKey;
				}
			}
			return string.Empty;
		}

		public string TestProjectFromImplementationProject(string implementation)
		{
			var implProjLower = implementation.ToLowerInvariant();
			if (string.IsNullOrEmpty(_solutionGuid))
				return string.Empty;
			ImplementationToTest implementationToTest;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTest = Associations[_solutionGuid];
			}
			else
				return string.Empty;

			if (implementationToTest.ContainsKey(implProjLower))
			{
				if (File.Exists(implementationToTest[implProjLower]) &&
					Access.ProjectModel.ProjectPathsList.Contains(implementationToTest[implProjLower]))
					return implementationToTest[implProjLower];
				return string.Empty;
			}
			return string.Empty;
		}
	}
}