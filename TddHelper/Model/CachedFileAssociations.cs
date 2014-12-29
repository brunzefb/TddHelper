
using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.Model
{
	public class CachedFileAssociations
	{
		private string _solutionGuid;

		internal Dictionary<string, ImplementationToTest> Associations { get; set; }

		public CachedFileAssociations(string id)
		{
			_solutionGuid = id;
			Associations = new Dictionary<string, ImplementationToTest>();
			Messenger.Default.Register<OptionsClearFileAssociationsCache>(this, OnCacheCleared);
		}

		private void OnCacheCleared(OptionsClearFileAssociationsCache action)
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
			if (string.IsNullOrEmpty(TddSettings.Default.FileAssociations)) return;
			var instance =
				JsonConvert.DeserializeObject<CachedFileAssociations>(
					TddSettings.Default.FileAssociations);
			Associations = instance.Associations;
		}
		
		public void Save()
		{
			if (string.IsNullOrEmpty(_solutionGuid))
				return;
			TddSettings.Default.FileAssociations = JsonConvert.SerializeObject(this);
			TddSettings.Default.Save();
		}

		public void AddAssociation(string implementation, string test)
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

			if (!implementationToTest.ContainsKey(implementation))
				implementationToTest.Add(implementation.ToLowerInvariant(), test.ToLowerInvariant());
			else
				implementationToTest[implementation] = test.ToLowerInvariant();
		}

		public string ImplementationFromTest(string test)
		{
			string lowerTest = test.ToLowerInvariant();
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
				if (implementationToTest[implementationKey] == lowerTest)
				{
					if (File.Exists(implementationKey))
						return implementationKey;
					return string.Empty;
				}
			}
			return string.Empty;
		}

		public string TestFromImplementation(string implementation)
		{
			var lowerImplementation = implementation.ToLowerInvariant();
			if (string.IsNullOrEmpty(_solutionGuid))
				return string.Empty;
			ImplementationToTest implementationToTest;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTest = Associations[_solutionGuid];
			}
			else
				return string.Empty;

			if (implementationToTest.ContainsKey(lowerImplementation))
			{
				if (File.Exists(implementationToTest[lowerImplementation]))
					return implementationToTest[lowerImplementation];
				return string.Empty;
			}
			return string.Empty;
		}
	}
}