// Copyright AB SCIEX 2014. All rights reserved.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace DreamWorks.TddHelper.Model
{
	internal class ImplementationToTest : Dictionary<string, string>
	{
	}

	internal class CachedFileAssociations
	{
		private readonly string _solutionGuid;

		public Dictionary<string, ImplementationToTest> Associations { get; set; }

		public CachedFileAssociations(string id)
		{
			_solutionGuid = id;
			Associations = new Dictionary<string, ImplementationToTest>();
			if (!string.IsNullOrEmpty(TddSettings.Default.FileAssociations))
			{
				var instance =
					JsonConvert.DeserializeObject<CachedFileAssociations>(
						TddSettings.Default.FileAssociations);
				Associations = instance.Associations;
			}
		}

		public void Save()
		{
			TddSettings.Default.FileAssociations = JsonConvert.SerializeObject(this);
			TddSettings.Default.Save();
		}

		public void ClearCache()
		{
			TddSettings.Default.FileAssociations = string.Empty;
			TddSettings.Default.Save();
			Associations.Clear();

		}

		public void AddAssociation(string implementation, string test)
		{
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

			if (!implementationToTest.ContainsKey(_solutionGuid))
				implementationToTest.Add(implementation, test);
			else
				implementationToTest[implementation] = test;
		}

		public string ImplementationFromTest(string test)
		{
			ImplementationToTest implementationToTest;
			if (Associations.ContainsKey(_solutionGuid))
			{
				implementationToTest = Associations[_solutionGuid];
			}
			else
				return string.Empty;

			foreach (var implementationKey in implementationToTest.Keys)
			{
				if (implementationToTest[implementationKey].ToLower() == test)
					return implementationKey;
			}
			return string.Empty;
		}

		public string TestFromImplementation(string implementation)
		{
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