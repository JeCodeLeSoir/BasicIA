using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static void RemoveTree(Terrain terrain, int prototypeIndex)
    {
        List<TreeInstance> treeInstances = 
            new List<TreeInstance>(terrain.terrainData.treeInstances);

        if (prototypeIndex >= 0 && prototypeIndex < treeInstances.Count)
        {
            treeInstances.RemoveAt(prototypeIndex);
            terrain.terrainData.treeInstances = treeInstances.ToArray();
        }
    }

    public static Terrain GetTerrain(Vector3 pos)
    {
        Terrain[] terrains = Terrain.activeTerrains;

        foreach (Terrain terrain in terrains)
        {
            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;
            if (pos.x >= terrainPos.x && pos.x <= terrainPos.x + terrainSize.x &&
                pos.z >= terrainPos.z && pos.z <= terrainPos.z + terrainSize.z)
            {
                return terrain;
            }
        }

        return null;
    }

    public void Start()
    {
        Terrain[] terrains = Terrain.activeTerrains;
        for (int i = 0; i < terrains.Length; i++)
            terrains[i].terrainData = Clone(terrains[i].terrainData);
    }

    private TerrainData Clone(TerrainData src)
    {
        TerrainData dst = new TerrainData();

        // --- RESOLUTIONS ---
        dst.heightmapResolution = src.heightmapResolution;
        dst.size = src.size;
        dst.alphamapResolution = src.alphamapResolution;
        dst.baseMapResolution = src.baseMapResolution;
        dst.SetDetailResolution(src.detailResolution, src.detailResolutionPerPatch);

        // --- HEIGHTS ---
        dst.SetHeights(0, 0, src.GetHeights(0, 0, src.heightmapResolution, src.heightmapResolution));

        // --- SPLATMAPS (Textures) ---
        dst.terrainLayers = src.terrainLayers;
        dst.SetAlphamaps(0, 0, src.GetAlphamaps(0, 0, src.alphamapWidth, src.alphamapHeight));

        // --- TREES ---
        dst.treePrototypes = src.treePrototypes;
        dst.treeInstances = src.treeInstances;

        // --- DETAILS (GRASS) ---
        dst.detailPrototypes = src.detailPrototypes;

        for (int i = 0; i < src.detailPrototypes.Length; i++)
        {
            int[,] layer = src.GetDetailLayer(0, 0, src.detailWidth, src.detailHeight, i);
            dst.SetDetailLayer(0, 0, i, layer);
        }

        return dst;
    }

    public struct DataTree
    {
        public int prototypeIndex;
        public float distance;
    }

    public static DataTree[] GetTreeIndexs(TerrainData terrainData, 
        float d, Vector3 pos)
    {
        TreeInstance[] treeInstances = terrainData.treeInstances;

        List<DataTree> indexs = new List<DataTree>();

        for (int i = 0; i < treeInstances.Length; i++)
        {
            Vector3 treeWorldPos = Vector3.Scale(treeInstances[i].position, terrainData.size);

            var a = new Vector3(pos.x, 0, pos.z);
            var b = new Vector3(treeWorldPos.x, 0, treeWorldPos.z);

            float distance = Vector3.Distance(a, b);

            if (distance < d)
            {
                indexs.Add(new DataTree()
                {
                    prototypeIndex = i,
                    distance = distance
                });
            }
        }

        indexs.Sort((a, b) => 
        a.distance.CompareTo(b.distance));

        return indexs.ToArray();
    }
}