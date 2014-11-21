// Guids.cs
// MUST match guids.h
using System;

namespace DreamWorks.TddHelper
{
    static class GuidList
    {
        public const string guidTddHelperPkgString = "804fe725-8637-4682-97b5-07ce08876c6b";
        public const string guidTddHelperCmdSetString = "69e57938-8d6f-4c63-9c48-1edcf5b5ebc9";
        public static readonly Guid guidTddHelperCmdSet = new Guid(guidTddHelperCmdSetString);
    };
}