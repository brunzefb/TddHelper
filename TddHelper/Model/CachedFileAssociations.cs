
using System;
using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.Model
{
	public class CachedFileAssociations
	{
		private string _solutionGuid;

		public Dictionary<string, ImplementationToTestMapper> Associations { get; set; }

		public CachedFileAssociations(string id)
		{
			_solutionGuid = id;
			Associations = new Dictionary<string, ImplementationToTestMapper>(StringComparer.OrdinalIgnoreCase);
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

			if (!implementationToTestMapper.ContainsKey(implementation))
				implementationToTestMapper.Add(implementation, test);
			else
				implementationToTestMapper[implementation] = test;
		}

		public string ImplementationFromTest(string test)
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

			foreach (var implementationKey in implementationToTestMapper.Keys)
			{
				var testFileInCache = implementationToTestMapper[implementationKey];
				if (string.Equals(testFileInCache, test, StringComparison.OrdinalIgnoreCase))
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
				var testFile = implementationToTestMapper[implementation];
				if (File.Exists(testFile))
					return testFile;
				return string.Empty;
			}
			return string.Empty;
		}
	}
}