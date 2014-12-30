using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DreamWorks.TddHelper.Model
{
	public class ImplementationToTestMapper
	{
		public ImplementationToTestMapper()
		{
			Dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public Dictionary<string, string> Dictionary { get; set; }

		public void Add(string implementation, string test)
		{
			Dictionary.Add(implementation, test);
		}

		public string this[string index]
		{
			get { return Dictionary[index]; }
			set
			{
				if (!string.IsNullOrEmpty(value))
					Dictionary[index] = value;
			}
		}

		
		[JsonIgnore]
		public List<string> Keys
		{
			get { return Dictionary.Keys.ToList(); }	
		}

		public bool ContainsKey(string index)
		{
			return Dictionary.ContainsKey(index);
		}
		public void Clear()
		{
			Dictionary.Clear();
		}
	}
}