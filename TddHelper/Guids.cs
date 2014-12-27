// Guids.cs
// MUST match guids.h
using System;

namespace DreamWorks.TddHelper
{
    static class GuidList
    {
        public const string guidTddHelperPkgString = "804fe725-8637-4682-97b5-07ce08876c6b";
        public const string guidTddHelperCmdSetString = "69e57938-8d6f-4c63-9c48-1edcf5b5ebc9";
	    public const string guidTddCustomOptionsPage = "DD3A66D9-4788-475D-995F-5CC8202C2498";
		public const string guidAddReferencesOptionsPage = "CBB3E3E5-5B85-4DFA-AE8A-67F6EA4A3F0A";
		
        public static readonly Guid guidTddHelperCmdSet = new Guid(guidTddHelperCmdSetString);
    };
}