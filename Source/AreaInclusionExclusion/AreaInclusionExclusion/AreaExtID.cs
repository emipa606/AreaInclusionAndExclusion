using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Verse;

namespace AreaInclusionExclusion;

public class AreaExtID
{
    private static readonly Regex decoder =
        new Regex("(\\@\\!\\[Map_(\\d*)\\])?(([\\+\\-])(\\d+))", RegexOptions.Compiled);

    public AreaExtID(int mapID, IEnumerable<KeyValuePair<int, AreaExtOperator>> areas)
    {
        MapID = mapID;
        Areas = [..areas];
    }

    public AreaExtID(string id)
    {
        var matchCollection = decoder.Matches(id);
        Areas = new List<KeyValuePair<int, AreaExtOperator>>(matchCollection.Count);
        for (var i = 0; i < matchCollection.Count; i++)
        {
            if (i == 0)
            {
                MapID = int.Parse(matchCollection[i].Groups[2].Value);
            }

            Areas.Add(new KeyValuePair<int, AreaExtOperator>(int.Parse(matchCollection[i].Groups[5].Value),
                matchCollection[i].Groups[4].Value == "+" ? AreaExtOperator.Inclusion : AreaExtOperator.Exclusion));
        }
    }

    public int MapID { get; } = -1;


    public List<KeyValuePair<int, AreaExtOperator>> Areas { get; }

    public void OnAreaRemoved(Area area)
    {
        if (MapID == area.Map.uniqueID && Areas.Any(x => x.Key == area.ID))
        {
            Areas.RemoveAll(x => x.Key == area.ID);
        }
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("@![Map_{0}]", MapID);
        foreach (var areaInfo in Areas)
        {
            if (areaInfo.Value == AreaExtOperator.None)
            {
                Log.Warning("Invalid Area operator detected: " + areaInfo.Value);
                continue;
            }

            stringBuilder.Append(areaInfo.Value == AreaExtOperator.Inclusion ? "+" : "-");
            stringBuilder.Append(areaInfo.Key.ToString());
        }

        return stringBuilder.ToString();
    }
}