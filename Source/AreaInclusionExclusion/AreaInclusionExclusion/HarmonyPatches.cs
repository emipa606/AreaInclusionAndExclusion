using System;
using System.Linq;
using AreaInclusionExclusion.Patches;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AreaInclusionExclusion;

[StaticConstructorOnStartup]
public class HarmonyPatches
{
    static HarmonyPatches()
    {
        var harmony = new Harmony("rimworld.gguake.areainclusionexclusion.main");
        harmony.Patch(AccessTools.Method(typeof(AreaAllowedGUI), nameof(AreaAllowedGUI.DoAllowedAreaSelectors)), null,
            null,
            new HarmonyMethod(typeof(AreaAllowedGUIPatches),
                nameof(AreaAllowedGUIPatches.DoAllowedAreaSelectorsTranspiler)));
        harmony.Patch(AccessTools.Method(typeof(AreaAllowedGUI), "DoAreaSelector"),
            new HarmonyMethod(typeof(AreaAllowedGUIPatches), nameof(AreaAllowedGUIPatches.DoAreaSelectorPrefix)));
        harmony.Patch(AccessTools.Method(typeof(AreaManager), "NotifyEveryoneAreaRemoved"),
            new HarmonyMethod(typeof(AreaManagerPatches), nameof(AreaManagerPatches.NotifyEveryoneAreaRemovedPrefix)));
        harmony.Patch(AccessTools.Method(typeof(AreaManager), nameof(AreaManager.ExposeData)), null,
            new HarmonyMethod(typeof(AreaManagerPatches), nameof(AreaManagerPatches.ExposeDataPostfix)));
        harmony.Patch(AccessTools.Method(typeof(AreaManager), nameof(AreaManager.AreaManagerUpdate)), null,
            new HarmonyMethod(typeof(AreaManagerPatches), nameof(AreaManagerPatches.AreaManagerUpdatePostfix)));
        harmony.Patch(AccessTools.Method(typeof(Area), nameof(Area.MarkForDraw)),
            new HarmonyMethod(typeof(AreaPatches), nameof(AreaPatches.MarkForDrawPrefix)));
        harmony.Patch(AccessTools.Method(typeof(Area), "MarkDirty"), null,
            new HarmonyMethod(typeof(AreaPatches), nameof(AreaPatches.MarkDirtyPostfix)));
        harmony.Patch(AccessTools.Method(typeof(Area), nameof(Area.Invert)), null,
            new HarmonyMethod(typeof(AreaPatches), nameof(AreaPatches.InvertPostfix)));
        harmony.Patch(
            typeof(Area).GetProperties().Single(x =>
                    x.GetIndexParameters().Length != 0 && x.GetIndexParameters()[0].ParameterType == typeof(int))
                .GetGetMethod(), null, null,
            new HarmonyMethod(typeof(AreaPatches), nameof(AreaPatches.ItemPropertyGetterTranspiler)));
        harmony.Patch(
            typeof(Area).GetProperties().Single(x =>
                    x.GetIndexParameters().Length != 0 && x.GetIndexParameters()[0].ParameterType == typeof(IntVec3))
                .GetGetMethod(), null, null,
            new HarmonyMethod(typeof(AreaPatches), nameof(AreaPatches.ItemPropertyGetterTranspiler)));
        harmony.Patch(
            AccessTools.Method(typeof(DebugLoadIDsSavingErrorsChecker),
                nameof(DebugLoadIDsSavingErrorsChecker.RegisterReferenced)),
            new HarmonyMethod(typeof(DebugLoadIDsSavingErrorsCheckerPatches),
                nameof(DebugLoadIDsSavingErrorsCheckerPatches.RegisterReferencedPrefix)));
        harmony.Patch(AccessTools.Method(typeof(LoadIDsWantedBank), nameof(LoadIDsWantedBank.RegisterLoadIDReadFromXml),
            [
                typeof(string),
                typeof(Type),
                typeof(string),
                typeof(IExposable)
            ]), null,
            new HarmonyMethod(typeof(LoadIDsWantedBankPatches),
                nameof(LoadIDsWantedBankPatches.RegisterLoadIDReadFromXmlPostfix)));
        harmony.Patch(
            AccessTools.PropertySetter(typeof(Pawn_PlayerSettings),
                nameof(Pawn_PlayerSettings.AreaRestrictionInPawnCurrentMap)),
            new HarmonyMethod(typeof(PlayerSettingsPatches),
                nameof(PlayerSettingsPatches.AreaRestrictionSetterPrefix)));
        harmony.Patch(AccessTools.Method(typeof(Pawn_PlayerSettings), nameof(Pawn_PlayerSettings.Notify_AreaRemoved)),
            null,
            new HarmonyMethod(typeof(PlayerSettingsPatches), nameof(PlayerSettingsPatches.NotifyAreaRemovedPostfix)));
        harmony.Patch(AccessTools.Method(typeof(MapComponentUtility), nameof(MapComponentUtility.MapRemoved)), null,
            new HarmonyMethod(typeof(GamePatches), nameof(GamePatches.MapRemovedPostfix)));
        Log.Message("[Area Inclusion&Exclusion] harmony patched");
    }
}