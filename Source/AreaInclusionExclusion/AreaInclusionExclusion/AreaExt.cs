using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace AreaInclusionExclusion;

public class AreaExt : Area
{
    private readonly AreaExtID areaExtID;

    private Color cachedColor = Color.black;

    private string cachedLabel = "";

    private AreaExtCellDrawer drawer;

    private bool initialized;

    public AreaExt(AreaExtID areaExtID)
    {
        this.areaExtID = areaExtID;
        Init();
        AreaExtEventManager.Register(this);
    }

    public AreaExt(Map map, AreaExtOperator op, Area area)
    {
        var areas = new List<KeyValuePair<int, AreaExtOperator>>
        {
            new KeyValuePair<int, AreaExtOperator>(area.ID, op)
        };
        areaExtID = new AreaExtID(map.uniqueID, areas);
        Init();
        AreaExtEventManager.Register(this);
    }

    public override string Label => cachedLabel;

    public override Color Color => cachedColor;

    public override int ListPriority => int.MaxValue;

    public int MapID => areaExtID.MapID;

    public bool Empty => areaExtID.Areas.Count == 0;

    public bool IsOneInclusion
    {
        get
        {
            if (areaExtID.Areas.Count == 1)
            {
                return areaExtID.Areas[0].Value == AreaExtOperator.Inclusion;
            }

            return false;
        }
    }

    public bool IsWholeExclusive
    {
        get
        {
            if (areaExtID.Areas.Count >= 1)
            {
                return areaExtID.Areas[0].Value == AreaExtOperator.Exclusion;
            }

            return false;
        }
    }

    public List<KeyValuePair<Area, AreaExtOperator>> InnerAreas
    {
        get
        {
            var list = new List<KeyValuePair<Area, AreaExtOperator>>();
            foreach (var areaInfo in areaExtID.Areas)
            {
                var areaID = areaInfo.Key;
                var key = areaManager.AllAreas.Find(x => x.ID == areaID);
                list.Add(new KeyValuePair<Area, AreaExtOperator>(key, areaInfo.Value));
            }

            return list;
        }
    }

    public bool Contains(Area area)
    {
        return areaExtID.Areas.Any(x => x.Key == area.ID);
    }

    public bool Contains(Area area, AreaExtOperator op)
    {
        return areaExtID.Areas.Any(x => x.Key == area?.ID && x.Value == op);
    }

    public override string GetUniqueLoadID()
    {
        return areaExtID.ToString();
    }

    public void Init()
    {
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            return;
        }

        areaManager = Find.Maps.Find(x => x.uniqueID == areaExtID.MapID).areaManager;
        FieldInfos.innerGrid.SetValue(this, new BoolGrid(areaManager.map));
        drawer = new AreaExtCellDrawer(this);
        Update();
    }

    public AreaExt CloneWithOperationArea(AreaExtOperator op, Area area)
    {
        var list = new List<KeyValuePair<int, AreaExtOperator>>(areaExtID.Areas);
        list.RemoveAll(x => x.Key == area.ID);
        _ = areaExtID;
        switch (op)
        {
            case AreaExtOperator.Inclusion:
                list.Insert(0, new KeyValuePair<int, AreaExtOperator>(area.ID, AreaExtOperator.Inclusion));
                break;
            case AreaExtOperator.Exclusion:
                list.Add(new KeyValuePair<int, AreaExtOperator>(area.ID, AreaExtOperator.Exclusion));
                break;
            default:
                if (list.Count == 0)
                {
                    return null;
                }

                break;
        }

        return new AreaExt(new AreaExtID(areaManager.map.uniqueID, list));
    }

    public AreaExtOperator GetAreaOperator(int areaID)
    {
        return areaExtID.Areas.Find(x => x.Key == areaID).Value;
    }

    public void CheckAndUpdate()
    {
        if (!initialized)
        {
            Update();
        }
    }

    public static BitArray GetAreaBitArray(Area area)
    {
        return new BitArray((bool[])FieldInfos.boolGridArr.GetValue(FieldInfos.innerGrid.GetValue(area)));
    }

    public void Update()
    {
        var map = Find.Maps.Find(x => x.uniqueID == areaExtID.MapID);
        if (map == null)
        {
            return;
        }

        areaManager = map.areaManager;
        var innerAreas = InnerAreas;
        var obj = (BoolGrid)FieldInfos.innerGrid.GetValue(this);
        var array = (bool[])FieldInfos.boolGridArr.GetValue(obj);
        var bitArray = new BitArray(array);
        if (innerAreas.Count > 0 && innerAreas.Any(x => x.Value == AreaExtOperator.Inclusion))
        {
            bitArray.SetAll(false);
        }
        else
        {
            bitArray.SetAll(true);
        }

        var stringBuilder = new StringBuilder();
        for (var i = 0; i < innerAreas.Count; i++)
        {
            var keyValuePair = innerAreas[i];
            var key = keyValuePair.Key;
            var value = keyValuePair.Value;
            var areaBitArray = GetAreaBitArray(key);
            switch (value)
            {
                case AreaExtOperator.Inclusion:
                    bitArray = bitArray.Or(areaBitArray);
                    if (i > 0)
                    {
                        stringBuilder.Append("+");
                    }

                    break;
                case AreaExtOperator.Exclusion:
                    bitArray = bitArray.And(areaBitArray.Not());
                    stringBuilder.Append("-");
                    break;
            }

            stringBuilder.Append(key.Label);
        }

        bitArray.CopyTo(array, 0);
        FieldInfos.boolGridArr.SetValue(obj, array);
        FieldInfos.boolGridTrueCount.SetValue(obj, array.Count(x => x));
        cachedLabel = stringBuilder.ToString();
        cachedColor = innerAreas.Count == 1 ? innerAreas[0].Key.Color : Color.black;

        initialized = true;
        drawer.dirty = true;
    }

    public void OnAreaEdited(Area area)
    {
        if (areaExtID.Areas.Any(x => x.Key == area.ID))
        {
            initialized = false;
        }
    }

    public void OnAreaRemoved(Area area)
    {
        if (!areaExtID.Areas.Any(x => x.Key == area.ID))
        {
            return;
        }

        initialized = false;
        areaExtID.OnAreaRemoved(area);
    }

    public void OnAreaUpdate()
    {
        if (!initialized)
        {
            Update();
        }

        drawer.Update();
    }

    public new void MarkForDraw()
    {
        if (MapID == Find.CurrentMap.uniqueID)
        {
            drawer.MarkForDraw();
        }
    }

    internal static class FieldInfos
    {
        public static readonly FieldInfo innerGrid = AccessTools.Field(typeof(Area), "innerGrid");

        public static readonly FieldInfo boolGridArr = AccessTools.Field(typeof(BoolGrid), "arr");

        public static readonly FieldInfo boolGridTrueCount = AccessTools.Field(typeof(BoolGrid), "trueCountInt");
    }
}