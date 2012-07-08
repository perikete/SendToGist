// Guids.cs
// MUST match guids.h

using System;

namespace SendToGist
{
    static class GuidList
    {
        public const string guidSendToGistPkgString = "9940ba2e-7057-4825-b225-e5cb22dca324";
        public const string guidSendToGistCmdSetString = "cdd9d24f-7624-4e08-a722-928885d94af0";
        public const string guidToolWindowPersistanceString = "49c1488e-1826-4dae-bdad-67fb4deece9c";

        public static readonly Guid guidSendToGistCmdSet = new Guid(guidSendToGistCmdSetString);
    };
}