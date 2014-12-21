
using System.Collections.Generic;

using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.Model
{
	
	public class CachedProjectAssociations
	{
		private string _solutionGuid;

		public Dictionary<string, ImplementationToTest> Associations { get; set; }

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

			if (!implementationToTest.ContainsKey(implementationProject))
				implementationToTest.Add(implementationProject, testProject);
			else
				implementationToTest[implementationProject] = testProject;
		}

		public string ImplementationProjectFromTestProject(string testProject)
		{
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
				if (implementationToTest[implementationKey] == testProject)
					return implementationKey;
			}
			return string.Empty;
		}

		public string TestProjectFromImplementationProject(string implementation)
		{
			if (string.IsNullOrEmpty(_solutionGuid))
				return string.Empty;
			ImplementationToTest implementationToTest;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTest = Associations[_solutionGuid];
			}
			else
				return string.Empty;

			if (implementationToTest.ContainsKey(implementation))
				return implementationToTest[implementation];
			return string.Empty;
		}
	}
}