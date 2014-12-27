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
			var expected = "0024000004800000940000000602000000240000525341310004000001000100d345d8e137f1ae840a1594f2d53c8f6e47ee87d31621a8363c68af5afb3033512efdb1f12b2f2d133a56dd64fc59a1eafd12bba22e0c92054f848b446948da59c9ccc8d3c1ed6d9840b9f6d6c440bcd3eacb3c68474139c0f68a01b98bbcbd0b61de4d6db8b1a140539c3028bf99f03a8d8364501605bcd11c95395352e786d8";
			Assert.That(output, Is.EqualTo(expected));
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