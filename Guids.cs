// Guids.cs
// MUST match guids.h
using System;

namespace FriedrichBrunzema.TddHelper
{
    static class GuidList
    {
        public const string guidTddHelperPkgString = "6ca49b7a-1b45-4d4d-8b8b-16cbc261fa55";
        public const string guidTddHelperCmdSetString = "3ce766a1-adba-4aa6-b069-37fd48db8cf3";

        public static readonly Guid guidTddHelperCmdSet = new Guid(guidTddHelperCmdSetString);
    };
}