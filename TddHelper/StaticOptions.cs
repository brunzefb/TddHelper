
using DreamWorks.TddHelper.ViewModel;

namespace DreamWorks.TddHelper
{
	public static class StaticOptions
	{
		public static OptionsViewModel MainOptions {get;set;}
		public static AddReferencesOptionsViewModel ReferencesOptions { get; set; }
		public static bool IsSolutionLoaded { get; set; }
	}
}
