using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using Verse;

namespace AreaInclusionExclusion.Patches;

internal class LoadIDsWantedBankPatches
{
    private static Regex mapIDDecoder = new Regex("\\@\\!\\[Map_(\\d*)\\]", RegexOptions.Compiled);

    private static readonly FieldInfo fieldLoadedObjectDirectory =
        AccessTools.Field(typeof(CrossRefHandler), "loadedObjectDirectory");

    private static readonly FieldInfo fieldAllObjectsByLoadID =
        AccessTools.Field(typeof(LoadedObjectDirectory), "allObjectsByLoadID");

    public static void RegisterLoadIDReadFromXmlPostfix(LoadIDsWantedBank __instance, string targetLoadID,
        Type targetType, string pathRelToParent, IExposable parent)
    {
        if (targetType != typeof(Area) && !targetType.IsInstanceOfType(typeof(Area)) ||
            !targetLoadID.StartsWith("@!"))
        {
            return;
        }

        var dictionary =
            fieldAllObjectsByLoadID.GetValue(fieldLoadedObjectDirectory.GetValue(Scribe.loader.crossRefs)) as
                Dictionary<string, ILoadReferenceable>;
        var value = AreaExtLoadHelper.OnLoadingVars(targetLoadID);
        dictionary?.TryAdd(targetLoadID, value);
    }
}