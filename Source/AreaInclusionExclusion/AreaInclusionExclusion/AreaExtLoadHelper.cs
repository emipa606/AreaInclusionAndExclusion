using System.Collections.Generic;
using Verse;

namespace AreaInclusionExclusion;

public class AreaExtLoadHelper
{
    private static readonly Dictionary<int, List<WeakReference<AreaExt>>> cache =
        new Dictionary<int, List<WeakReference<AreaExt>>>();

    public static AreaExt OnLoadingVars(string id)
    {
        var areaExtID = new AreaExtID(id);
        if (!cache.ContainsKey(areaExtID.MapID))
        {
            cache[areaExtID.MapID] = [];
        }

        var weakReference = cache[areaExtID.MapID].Find(x => x.IsAlive && x.Target.GetUniqueLoadID() == id);
        AreaExt areaExt;
        if (weakReference != null)
        {
            areaExt = weakReference.Target;
        }
        else
        {
            areaExt = new AreaExt(areaExtID);
            cache[areaExtID.MapID].Add(new WeakReference<AreaExt>(areaExt));
        }

        return areaExt;
    }

    public static void OnResolveCrossRef(int mapID)
    {
        if (cache.TryGetValue(mapID, out var value))
        {
            foreach (var item in value)
            {
                if (item.IsAlive)
                {
                    item.Target.Init();
                }
            }
        }

        cache.Remove(mapID);
    }
}