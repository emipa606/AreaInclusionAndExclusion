using System.Collections.Generic;
using Verse;

namespace AreaInclusionExclusion;

public class AreaExtEventManager
{
    private const int areaManagerRefCheckTimerDelay = 300;
    private static readonly List<WeakReference<AreaExt>> allAreaExts = [];

    private static int areaManagerRefCheckTimer;

    public static void Register(AreaExt areaExt)
    {
        allAreaExts.Add(new WeakReference<AreaExt>(areaExt));
    }

    public static void CheckAndRemoveDeadRef()
    {
        allAreaExts.RemoveAll(x => !x.IsAlive);
    }

    public static void OnAreaEdited(Area area)
    {
        CheckAndRemoveDeadRef();
        foreach (var allAreaExt in allAreaExts)
        {
            allAreaExt.Target.OnAreaEdited(area);
        }
    }

    public static void OnAreaRemoved(Area area)
    {
        CheckAndRemoveDeadRef();
        foreach (var allAreaExt in allAreaExts)
        {
            allAreaExt.Target.OnAreaRemoved(area);
        }
    }

    public static void OnMapRemoved(Map map)
    {
        foreach (var item in Find.WorldPawns.AllPawnsAliveOrDead)
        {
            if (item.playerSettings?.AreaRestrictionInPawnCurrentMap is AreaExt areaExt &&
                map.uniqueID == areaExt.MapID)
            {
                item.playerSettings.AreaRestrictionInPawnCurrentMap = null;
            }
        }
    }

    public static void OnAreaManagerUpdate()
    {
        if (areaManagerRefCheckTimer++ % 300 == 0)
        {
            areaManagerRefCheckTimer = 0;
            CheckAndRemoveDeadRef();
        }

        foreach (var allAreaExt in allAreaExts)
        {
            allAreaExt.Target.OnAreaUpdate();
        }
    }
}