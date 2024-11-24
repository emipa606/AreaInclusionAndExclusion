using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AreaInclusionExclusion.Patches;

internal class PlayerSettingsPatches
{
    public static bool AreaRestrictionSetterPrefix(ref Area value)
    {
        if (value is not AreaExt areaExt)
        {
            return true;
        }

        if (!Find.Maps.Any(x => x.uniqueID == areaExt.MapID))
        {
            value = null;
        }

        if (areaExt.InnerAreas.Count == 0)
        {
            value = null;
        }

        if (areaExt.IsOneInclusion)
        {
            value = areaExt.InnerAreas[0].Key;
        }

        return true;
    }

    public static void NotifyAreaRemovedPostfix(Pawn_PlayerSettings __instance, Area area)
    {
        if (FieldInfos.areaAllowedInt.GetValue(__instance) is AreaExt { Empty: not false })
        {
            FieldInfos.areaAllowedInt.SetValue(__instance, null);
        }
    }

    internal static class FieldInfos
    {
        public static readonly FieldInfo areaAllowedInt =
            AccessTools.Field(typeof(Pawn_PlayerSettings), "areaAllowedInt");
    }
}