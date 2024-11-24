using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AreaInclusionExclusion;

public class AreaExtCellDrawer(AreaExt areaExt)
{
    private const float opacity = 0.33f;

    private readonly List<Mesh> meshes = [];
    public readonly AreaExt parentAreaExt = areaExt;

    public bool dirty = true;

    private Material material;

    private bool shouldDraw;

    public void RegenerateMesh()
    {
        foreach (var mesh2 in meshes)
        {
            mesh2.Clear();
        }

        meshes.Clear();
        var y = AltitudeLayer.WorldClipper.AltitudeFor();
        var num = 0;
        var list = new List<Vector3>();
        var list2 = new List<Color>();
        var list3 = new List<int>();
        var mesh = new Mesh();
        var size = parentAreaExt.Map.Size;
        var innerAreas = parentAreaExt.InnerAreas;
        for (var i = 0; i < innerAreas.Count; i++)
        {
            var key = innerAreas[i].Key;
            if (innerAreas[i].Value != AreaExtOperator.Inclusion)
            {
                continue;
            }

            var bitArray = new BitArray(size.x * size.z);
            bitArray.SetAll(false);
            for (var j = i + 1; j < innerAreas.Count; j++)
            {
                if (innerAreas[j].Value == AreaExtOperator.Exclusion)
                {
                    bitArray = bitArray.Or(AreaExt.GetAreaBitArray(innerAreas[j].Key));
                }
            }

            bitArray = bitArray.Not();

            var cellRect = new CellRect(0, 0, size.x, size.z);
            for (var k = cellRect.minX; k <= cellRect.maxX; k++)
            {
                for (var l = cellRect.minZ; l <= cellRect.maxZ; l++)
                {
                    var index = CellIndicesUtility.CellToIndex(k, l, size.x);
                    if (!key[index] || !bitArray[index])
                    {
                        continue;
                    }

                    list.Add(new Vector3(k, y, l));
                    list.Add(new Vector3(k, y, l + 1));
                    list.Add(new Vector3(k + 1, y, l + 1));
                    list.Add(new Vector3(k + 1, y, l));
                    list2.Add(key.Color);
                    list2.Add(key.Color);
                    list2.Add(key.Color);
                    list2.Add(key.Color);
                    var count = list.Count;
                    list3.Add(count - 4);
                    list3.Add(count - 3);
                    list3.Add(count - 2);
                    list3.Add(count - 4);
                    list3.Add(count - 2);
                    list3.Add(count - 1);
                    num++;
                    if (num < 16383)
                    {
                        continue;
                    }

                    mesh.SetVertices(list);
                    mesh.SetColors(list2);
                    mesh.SetTriangles(list3, 0);
                    list.Clear();
                    list2.Clear();
                    list3.Clear();
                    meshes.Add(mesh);
                    mesh = new Mesh();
                    num = 0;
                }
            }
        }

        if (list.Count > 0)
        {
            mesh.SetVertices(list);
            mesh.SetColors(list2);
            mesh.SetTriangles(list3, 0);
            meshes.Add(mesh);
        }

        if (material == null)
        {
            material = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 1f, 1f, 0.33f), true);
            material.renderQueue = 3600;
        }

        dirty = false;
    }

    public void MarkForDraw()
    {
        shouldDraw = true;
    }

    public void Update()
    {
        if (shouldDraw)
        {
            if (dirty)
            {
                RegenerateMesh();
            }

            foreach (var mesh in meshes)
            {
                Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
            }
        }

        shouldDraw = false;
    }
}