using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace AreaInclusionExclusion.Patches;

internal class AreaPatches
{
    public static bool MarkForDrawPrefix(Area __instance)
    {
        if (__instance is not AreaExt ext)
        {
            return true;
        }

        ext.MarkForDraw();
        return false;
    }

    public static void MarkDirtyPostfix(Area __instance)
    {
        AreaExtEventManager.OnAreaEdited(__instance);
    }

    public static void InvertPostfix(Area __instance)
    {
        AreaExtEventManager.OnAreaEdited(__instance);
    }

    public static IEnumerable<CodeInstruction> ItemPropertyGetterTranspiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        var labelOriginal = il.DefineLabel();
        var instList = instructions.ToList();
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Isinst, typeof(AreaExt));
        yield return new CodeInstruction(OpCodes.Brfalse_S, labelOriginal);
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Call,
            AccessTools.Method(typeof(AreaExt), nameof(AreaExt.CheckAndUpdate)));
        var i = 0;
        while (i < instList.Count)
        {
            if (i == 0)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0)
                {
                    labels = [labelOriginal]
                };
            }
            else
            {
                yield return instList[i];
            }

            var num = i + 1;
            i = num;
        }
    }
}