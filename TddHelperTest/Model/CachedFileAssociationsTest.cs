using DreamWorks.TddHelper.Model;
using NUnit.Framework;

namespace TddHelperTest.Model
{
	[TestFixture]
	public class CachedFileAssociationsTest
	{
		CachedFileAssociations _t;

		[SetUp]
		public void SetUp()
		{
			_t = new CachedFileAssociations("123");
			
		}

		[Test]
		public void AddingEtc()
		{
			_t.AddAssociation("a.cs", "aTest.cs");
			_t.AddAssociation("b.cs", "bTest.cs");
			AssertAssociationsOk();
		}

		private void AssertAssociationsOk()
		{
			Assert.That(_t.ImplementationFromTest("foo"), Is.Empty);
			Assert.That(_t.ImplementationFromTest("aTest.cs"),
				Is.EqualTo("a.cs"));
			Assert.That(_t.TestFromImplementation("fff"), Is.Empty);
			Assert.That(_t.TestFromImplementation("a.cs"),
				Is.EqualTo("aTest.cs"));
		}

		[Test]
		public void Persistence()
		{
			_t.ClearCache();
			AddingEtc();
			_t.Save();
			_t =  new CachedFileAssociations("123");
			_t.Load();
			AssertAssociationsOk();
		}
	}
}