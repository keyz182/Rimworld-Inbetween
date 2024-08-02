using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Inbetween;

static class ModLog
{
    [Conditional("DEBUG")]
    public static void Debug(string x)
    {
        Verse.Log.Message(x);
    }

    public static void Log(string msg, [CallerMemberName] string caller = "unknown")
    {
        Verse.Log.Message($"<color=#1c6beb>[Inbetween] - {caller}</color> {msg ?? "<null>"}");
    }

    public static void Warn(string msg, [CallerMemberName] string caller = "unknown")
    {
        Verse.Log.Warning($"<color=#1c6beb>[Inbetween] - {caller}</color> {msg ?? "<null>"}");
    }

    public static void Error(string msg, Exception e = null, [CallerMemberName] string caller = "unknown")
    {
        Verse.Log.Error($"<color=#1c6beb>[Inbetween] - {caller}</color> {msg ?? "<null>"}");
        if (e != null)
            Verse.Log.Error(e.ToString());
    }
}
