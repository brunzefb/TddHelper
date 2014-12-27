using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using SnkHelper;

namespace TddHelperTest.Cpp
{
	[TestFixture]
	public class HelperTest
	{
		[Test]
		public void FromSnk()
		{
			var di = new DirectoryInfo(AssemblyLoadDirectory);
			var key = Path.Combine(di.Parent.Parent.Parent.Parent.FullName, @"TddHelper\Key.snk");
			var output = Helper.PublicKeyFromSnkFile(key);
			Debug.WriteLine(output);
			Assert.That(output.Length, Is.EqualTo(320));
		}

		static public string AssemblyLoadDirectory
		{
			get
			{
				string codeBase = Assembly.GetCallingAssembly().CodeBase;
				var uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
	}
}