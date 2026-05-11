using System;
using System.Collections;
#if UNITY_EDITOR 
 using UnityEditor;
 #endif 
using UnityEngine;

namespace TheBCMHT{
public class HallBuilder : EditorWindow
{
    TileType tileType = TileType.Full;
    int tileIndex;
    int direction;
    Vector3 v3;
    int length = 1;
    Material floor;
    Material wall;
    Material ceiling;
    Transform parent;
    bool containsCeiling = true;
    bool containsFloor = true;
    bool busy;

    [MenuItem("Tools/CM Tools/Hall Builder")]
    public static void ShowWindow()
    {
        GetWindow(typeof(HallBuilder));
    }

    private void OnGUI()
    {
        if (floor == null && TheBCMHT_Helper.getSampleFloor(false) != null)
        {
            floor = TheBCMHT_Helper.getSampleFloor(false);
        }
        if (wall == null && TheBCMHT_Helper.getSampleWall(false) != null)
        {
            wall = TheBCMHT_Helper.getSampleWall(false);
        }
        if (ceiling == null && TheBCMHT_Helper.getSampleCeiling(false) != null)
        {
            ceiling = TheBCMHT_Helper.getSampleCeiling(false);
        }
        tileType = (TileType)EditorGUILayout.EnumPopup("Tile type:", tileType);
        if (tileType == TileType.Single)
        {
            tileIndex = EditorGUILayout.IntField("Tile index:", tileIndex);
			if(tileIndex > 3 || tileIndex < 0)
			{
				tileIndex = 0;
			}
        }
        if ((tileType == TileType.Straight) || (tileType == TileType.Corner) || (tileType == TileType.End))
        {
			direction = EditorGUILayout.IntField("Direction:", direction);
        }
        v3 = EditorGUILayout.Vector3Field("Position:", v3);
        if ((tileType == TileType.Single && direction == tileIndex) || (tileType == TileType.Full) || (tileType == TileType.Open))
        {
            direction = 0;
        }
        if ((direction > 3 && ((tileType == TileType.Single) || (tileType == TileType.End) || (tileType == TileType.Corner))) || (direction > 1 && tileType == TileType.Straight) || direction < 0)
        {
            direction = 0;
        }
        length = Mathf.Clamp(EditorGUILayout.IntField("Length:", length), 1, TheBCMHT_Helper.getMaxValueFromTileType(tileType));
        floor = (Material)EditorGUILayout.ObjectField("Floor material:", floor, typeof(Material), false);
        wall = (Material)EditorGUILayout.ObjectField("Wall material:", wall, typeof(Material), false);
        ceiling = (Material)EditorGUILayout.ObjectField("Ceiling material:", ceiling, typeof(Material), false);
        containsCeiling = EditorGUILayout.Toggle("Contains ceiling:", containsCeiling);
        containsFloor = EditorGUILayout.Toggle("Contains floor:", containsFloor);
        parent = (Transform)EditorGUILayout.ObjectField("Parent:", parent, typeof(Transform), true);
        if (!busy && GUILayout.Button("Build hall"))
        {
            BuildTiles();
        }
    }

    private void BuildTiles()
    {
        busy = true;
        for (int i = 0; i < length; i++)
        {
            string lowerStr = tileType.ToString().ToLower();
            Hashtable tilesPath = new Hashtable
            {
                ["open"] = "Tiles/Open",
                ["single"] = "Tiles/Single_",
                ["straight"] = "Tiles/Straight_",
                ["corner"] = "Tiles/Corner_",
                ["end"] = "Tiles/End_",
                ["full"] = "Tiles/Full"
            };
            string realTilePath = (string)tilesPath[lowerStr];
            if (tileType != TileType.Open && tileType != TileType.Full)
            {
                realTilePath = realTilePath + (tileType == TileType.Single ? tileIndex : direction);
            }
            GameObject _tile = Instantiate(Resources.Load<GameObject>(realTilePath), v3 + TheBCMHT_Helper.getVector3FromDir(direction) * 10f * i, Quaternion.identity, parent);
            ApplyMaterials(_tile);
            if (!containsCeiling)
            {
                foreach (Transform transform in _tile.transform.GetComponentsInChildren<Transform>())
                {
                    if (transform.gameObject.name.Contains("Ceiling"))
                    {
                        DestroyImmediate(transform.gameObject);
                    }
                }
            }
            if (!containsFloor)
            {
                foreach (Transform transform in _tile.transform.GetComponentsInChildren<Transform>())
                {
                    if (transform.gameObject.name.Contains("Floor"))
                    {
                        DestroyImmediate(transform.gameObject);
                    }
                }
            }
        }
        busy = false;
    }

    private void ApplyMaterials(GameObject tile)
    {
        foreach (MeshRenderer meshRenderer in tile.GetComponentsInChildren<MeshRenderer>())
        {
            if (meshRenderer.gameObject.name.Contains("Floor"))
            {
                meshRenderer.sharedMaterial = this.floor;
            }
            else if (meshRenderer.gameObject.name.Contains("Wall"))
            {
                meshRenderer.sharedMaterial = this.wall;
            }
            else if (meshRenderer.gameObject.name.Contains("Ceiling"))
            {
                meshRenderer.sharedMaterial = this.ceiling;
            }
        }
    }
}
}