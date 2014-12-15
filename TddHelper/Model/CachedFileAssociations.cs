// Copyright AB SCIEX 2014. All rights reserved.

using System.Collections.Generic;
using DreamWorks.TddHelper.View;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.Model
{
	internal class ImplementationToTest : Dictionary<string, string>
	{
	}

	internal class CachedFileAssociations
	{
		private string _solutionGuid;

		public Dictionary<string, ImplementationToTest> Associations { get; set; }

		public CachedFileAssociations(string id)
		{
			_solutionGuid = id;
			Associations = new Dictionary<string, ImplementationToTest>();
			Messenger.Default.Register<OptionsClearCache>(this, OnCacheCleared);
		}

		private void OnCacheCleared(OptionsClearCache action)
		{
			ClearCache();
		}

		public void ClearCache()
		{
			TddSettings.Default.FileAssociations = string.Empty;
			TddSettings.Default.Save();
			Associations.Clear();
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
				implementationToTest.Add(implementation, test);
			else
				implementationToTest[implementation] = test;
		}

		public string ImplementationFromTest(string test)
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
				if (implementationToTest[implementationKey] == test)
					return implementationKey;
			}
			return string.Empty;
		}

		public string TestFromImplementation(string implementation)
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