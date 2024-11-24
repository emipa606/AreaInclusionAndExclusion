using Verse;

namespace AreaInclusionExclusion.Patches;

internal class DebugLoadIDsSavingErrorsCheckerPatches
{
    public static bool RegisterReferencedPrefix(ILoadReferenceable obj)
    {
        return Scribe.mode != LoadSaveMode.Saving || obj is not AreaExt;
    }
}