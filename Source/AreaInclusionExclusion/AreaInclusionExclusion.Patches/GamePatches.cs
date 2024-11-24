using Verse;

namespace AreaInclusionExclusion.Patches;

public static class GamePatches
{
    public static void MapRemovedPostfix(Map map)
    {
        AreaExtEventManager.OnMapRemoved(map);
    }
}