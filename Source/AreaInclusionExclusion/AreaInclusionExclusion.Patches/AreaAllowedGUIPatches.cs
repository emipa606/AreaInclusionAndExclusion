using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AreaInclusionExclusion.Patches;

internal class AreaAllowedGUIPatches
{
    private static AreaAllowedEditMode editMode = AreaAllowedEditMode.None;

    private static DoAreaSelectorDelegate delegateDoAreaSelector = (DoAreaSelectorDelegate)AccessTools
        .Method(typeof(AreaAllowedGUI), "DoAreaSelector").CreateDelegate(typeof(DoAreaSelectorDelegate));

    public static IEnumerable<CodeInstruction> DoAllowedAreaSelectorsTranspiler(
        IEnumerable<CodeInstruction> codeInstructions, ILGenerator iLGenerator)
    {
        var list = codeInstructions.ToList();
        var num = 0;
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].opcode != OpCodes.Call || (MethodInfo)list[i].operand !=
                AccessTools.Method(typeof(AreaAllowedGUI), "DoAreaSelector"))
            {
                continue;
            }

            if (num == 0)
            {
                var index = i - 12;
                var label = iLGenerator.DefineLabel();
                var label2 = iLGenerator.DefineLabel();
                list[index].labels.Add(label);
                var list2 = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(AreaAllowedGUIPatches), "editMode")),
                    new CodeInstruction(OpCodes.Brtrue_S, label),
                    new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Event), nameof(Event.current))),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.rawType))),
                    new CodeInstruction(OpCodes.Brtrue_S, label),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Mouse), nameof(Mouse.IsOver))),
                    new CodeInstruction(OpCodes.Brfalse_S, label),
                    new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Event), nameof(Event.current))),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.button))),
                    new CodeInstruction(OpCodes.Brfalse_S, label2),
                    new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Event), nameof(Event.current))),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.button))),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Bne_Un_S, label),
                    new CodeInstruction(OpCodes.Ldc_I4_1)
                    {
                        labels = [label2]
                    },
                    new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(AreaAllowedGUI), "dragging"))
                };
                list.InsertRange(index, list2);
                i += list2.Count;
            }

            num++;
        }

        num = 0;
        for (var j = 0; j < list.Count; j++)
        {
            if (list[j].opcode != OpCodes.Ret)
            {
                continue;
            }

            if (num == 1)
            {
                var label3 = iLGenerator.DefineLabel();
                var label4 = iLGenerator.DefineLabel();
                list[j].labels.Add(label3);
                var list3 = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(AreaAllowedGUIPatches), nameof(editMode))),
                    new CodeInstruction(OpCodes.Brfalse_S, label3),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.current))),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.rawType))),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Bne_Un_S, label3),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.current))),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.button))),
                    new CodeInstruction(OpCodes.Brfalse_S, label4),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.current))),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.PropertyGetter(typeof(Event), nameof(Event.button))),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Bne_Un_S, label3),
                    new CodeInstruction(OpCodes.Ldc_I4_0)
                    {
                        labels = [label4]
                    },
                    new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(AreaAllowedGUI), "dragging")),
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Stsfld,
                        AccessTools.Field(typeof(AreaAllowedGUIPatches), nameof(editMode)))
                };
                list.InsertRange(j, list3);
                j += list3.Count;
            }

            num++;
        }

        return list;
    }

    public static bool DoAreaSelectorPrefix(Rect rect, Pawn p, Area area)
    {
        MouseoverSounds.DoRegion(rect);
        rect = rect.ContractedBy(1f);
        GUI.DrawTexture(rect, area == null ? BaseContent.GreyTex : area.ColorTexture);
        Text.Anchor = TextAnchor.MiddleLeft;
        var text = AreaUtility.AreaAllowedLabel_Area(area);
        var rect2 = rect;
        rect2.xMin += 3f;
        rect2.yMin += 2f;
        Widgets.Label(rect2, text);
        var draggingValue = (bool)FieldInfos.dragging.GetValue(null);
        var areaRestriction = p.playerSettings.AreaRestrictionInPawnCurrentMap;
        var areaExt = p.playerSettings.AreaRestrictionInPawnCurrentMap as AreaExt;
        var areaExtOperator = areaExt?.GetAreaOperator(area?.ID ?? -1) ?? AreaExtOperator.None;
        if (area == null)
        {
            if (areaRestriction == null || areaExt is { IsWholeExclusive: true })
            {
                GUI.color = Color.white;
                Widgets.DrawBox(rect, 2);
                GUI.color = Color.white;
            }
        }
        else if (areaExt != null)
        {
            switch (areaExtOperator)
            {
                case AreaExtOperator.Inclusion:
                    GUI.color = Color.white;
                    Widgets.DrawBox(rect, 2);
                    GUI.color = Color.white;
                    break;
                case AreaExtOperator.Exclusion:
                    GUI.color = Color.red;
                    Widgets.DrawBox(rect, 2);
                    GUI.color = Color.white;
                    break;
            }
        }
        else if (areaRestriction == area)
        {
            Widgets.DrawBox(rect, 2);
        }

        var mouseIsOver = Mouse.IsOver(rect);
        var buttonZero = Event.current.button == 0;
        var buttonOne = Event.current.button == 1;
        if (mouseIsOver)
        {
            area?.MarkForDraw();
        }

        if (editMode == AreaAllowedEditMode.None && mouseIsOver && draggingValue)
        {
            if (buttonZero)
            {
                if (area != null && (areaRestriction == area || areaExtOperator == AreaExtOperator.Inclusion))
                {
                    editMode = AreaAllowedEditMode.Remove;
                }
                else
                {
                    editMode = AreaAllowedEditMode.AddInclusion;
                }
            }
            else if (buttonOne)
            {
                if (areaExtOperator == AreaExtOperator.Exclusion)
                {
                    editMode = AreaAllowedEditMode.Remove;
                }
                else if (area != null)
                {
                    editMode = AreaAllowedEditMode.AddExclusion;
                }
            }
        }

        switch (editMode)
        {
            case AreaAllowedEditMode.AddInclusion when mouseIsOver && draggingValue:
            {
                if (areaRestriction != area)
                {
                    if (area == null)
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap = null;
                    }
                    else if (areaRestriction == null)
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap = area;
                    }
                    else if (areaExt != null)
                    {
                        if (!areaExt.Contains(area, AreaExtOperator.Inclusion))
                        {
                            p.playerSettings.AreaRestrictionInPawnCurrentMap =
                                areaExt.CloneWithOperationArea(AreaExtOperator.Inclusion, area);
                        }
                    }
                    else
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap =
                            new AreaExt(areaRestriction.Map, AreaExtOperator.Inclusion, areaRestriction)
                                .CloneWithOperationArea(AreaExtOperator.Inclusion, area);
                    }
                }

                break;
            }
            case AreaAllowedEditMode.AddInclusion:
            {
                if (!draggingValue)
                {
                    editMode = AreaAllowedEditMode.None;
                }

                break;
            }
            case AreaAllowedEditMode.AddExclusion when mouseIsOver && draggingValue:
            {
                if (areaRestriction != area)
                {
                    if (area == null)
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap = null;
                    }
                    else if (areaRestriction == null)
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap =
                            new AreaExt(area.Map, AreaExtOperator.Exclusion, area);
                    }
                    else if (areaExt != null)
                    {
                        if (!areaExt.Contains(area, AreaExtOperator.Exclusion))
                        {
                            p.playerSettings.AreaRestrictionInPawnCurrentMap =
                                areaExt.CloneWithOperationArea(AreaExtOperator.Exclusion, area);
                        }
                    }
                    else
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap =
                            new AreaExt(areaRestriction.Map, AreaExtOperator.Inclusion, areaRestriction)
                                .CloneWithOperationArea(AreaExtOperator.Exclusion, area);
                    }
                }

                break;
            }
            case AreaAllowedEditMode.AddExclusion:
            {
                if (!draggingValue)
                {
                    editMode = AreaAllowedEditMode.None;
                }

                break;
            }
            case AreaAllowedEditMode.Remove when mouseIsOver && draggingValue:
            {
                if (areaRestriction != null && area != null)
                {
                    if (areaRestriction == area)
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap = null;
                    }
                    else if (areaExt != null && areaExt.Contains(area))
                    {
                        p.playerSettings.AreaRestrictionInPawnCurrentMap =
                            areaExt.CloneWithOperationArea(AreaExtOperator.None, area);
                    }
                }

                break;
            }
            case AreaAllowedEditMode.Remove:
            {
                if (!draggingValue)
                {
                    editMode = AreaAllowedEditMode.None;
                }

                break;
            }
        }

        Text.Anchor = TextAnchor.UpperLeft;
        TooltipHandler.TipRegion(rect, text);
        return false;
    }

    internal enum AreaAllowedEditMode
    {
        None,
        AddInclusion,
        AddExclusion,
        Remove
    }

    internal static class FieldInfos
    {
        public static readonly FieldInfo dragging = AccessTools.Field(typeof(AreaAllowedGUI), "dragging");
    }

    private delegate void DoAreaSelectorDelegate(Rect rect, Pawn p, Area area);
}