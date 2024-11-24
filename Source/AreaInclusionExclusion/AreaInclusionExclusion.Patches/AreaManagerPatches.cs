using Verse;

namespace AreaInclusionExclusion.Patches;

internal class AreaManagerPatches
{
    public static bool NotifyEveryoneAreaRemovedPrefix(Area area)
    {
        AreaExtEventManager.OnAreaRemoved(area);
        return true;
    }

    public static void ExposeDataPostfix(AreaManager __instance)
    {
        if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
        {
            AreaExtLoadHelper.OnResolveCrossRef(__instance.map.uniqueID);
        }
    }

    public static void AreaManagerUpdatePostfix(AreaManager __instance)
    {
        AreaExtEventManager.OnAreaManagerUpdate();
    }
}