using UnityEngine;
using static TerrainManager;

public class PlayerInteract : MonoBehaviour {

    public GameObject[] treeColliderBox;

    public void UpdateTreeColliderBox(Vector3 pos)
    {
        void freeObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        for (int i = 0; i < treeColliderBox.Length; i++)
        {
            freeObject(treeColliderBox[i]);
        }

        GameObject getObject()
        {
            for (int i = 0; i < treeColliderBox.Length; i++)
            {
                if (!treeColliderBox[i].activeSelf)
                {
                    return treeColliderBox[i];
                }
            }
            return null;
        }
    
        Terrain terrain = GetTerrain(pos);
        DataTree[] protoTypeIndex;

        protoTypeIndex = GetTreeIndexs(terrain.terrainData, 
            5f, pos - terrain.transform.position);
    
        for (int i = 0; i < protoTypeIndex.Length; i++)
        {
            DataTree dataTree = protoTypeIndex[i];
            GameObject obj = getObject();
    
            if (obj != null)
            {
                obj.SetActive(true);
                
                Vector3 treePos = terrain.terrainData. GetTreeInstance(dataTree.prototypeIndex).position;
            
                treePos = Vector3.Scale(treePos,
                    terrain.terrainData.size) +
                    terrain.transform.position;

                obj.transform.position = treePos;
            }
        }
    }
}