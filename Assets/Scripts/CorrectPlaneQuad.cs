using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif

public class CorrectPlaneQuad : MonoBehaviour
{
    private void Start()
    {
        Mesh[] availableMeshes = Resources.FindObjectsOfTypeAll<Mesh>();
        Mesh quad = null;
        foreach(Mesh mesh in availableMeshes)
        {
            if (mesh.name != "Quad")
            {
                continue;
            }
            quad = mesh;
            break;
        }
        foreach(MeshFilter mf in FindObjectsOfType<MeshFilter>())
        {
            if (quad == null)
            {
                Debug.LogWarning("There is no Quad mesh in this project!"); //Probably will never happen since the Quad is a default Unity asset, but whatever.
                break;
            }
            string objectName = mf.gameObject.name;
            if (Array.IndexOf(exclusions, objectName) > -1)
            {
                continue;
            }
            MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
            #if UNITY_EDITOR
            string mfn = "";
            if (mf.sharedMesh != null)
            {
                mfn = mf.sharedMesh.name;
            }
            else
            {
                Debug.LogWarning("Mesh Filter with NULL Mesh found, please make sure your Mesh Filters all have Meshes!" + " Mesh Filter was attached to Game Object " + objectName);
                continue;
            }
            #else
            string mfn = "";
            if (mf.mesh != null)
            {
                mfn = mf.mesh.name;
            }
            else
            {
                Debug.LogWarning("Mesh Filter with NULL Mesh found, please make sure your Mesh Filters all have Meshes!" + " Mesh Filter was attached to Game Object " + objectName);
                continue;
            }
            #endif
            if (mr == null || !(mfn.Contains("Plane")))
            {
                continue;
            }
            #if UNITY_EDITOR
            Undo.RecordObject(mf, "Change Mesh Filter to Quad");
            mf.sharedMesh = quad;
            #else
            mf.mesh = quad;
            #endif
            MeshCollider mc = mf.gameObject.GetComponent<MeshCollider>();
            #if UNITY_EDITOR
            Undo.RecordObject(mc, "Change Mesh Collider to Quad");
            #endif
            if (mc != null)
            {
                mc.sharedMesh = quad;
            }
            Transform mt = mf.transform;
            #if UNITY_EDITOR
            Undo.RecordObject(mt, "Change Transform Scale/Rotation");
            #endif
            Vector3 scale = mt.localScale;
            scale.x *= 10f;
            if (Mathf.Sign(scale.y) == -1f)
            {
                scale.y *= -10f;
                scale.z *= -10f;
            }
            else
            {
                scale.y *= 10f;
                scale.z *= 10f;
            }
            scale.x = Mathf.Round(scale.x);
            scale.y = Mathf.Round(scale.y);
            scale.z = Mathf.Round(scale.z);
            mt.localScale = scale;
            mt.Rotate(rotationCorrection);
        }
    }

    //ADD EXCLUDED GAME OBJECT NAMES HERE!
    //THESE ARE CASE SENSITIVE AND MUST BE THE ENTIRE NAME!
    private static readonly string[] exclusions = new string[]{"Game Object Exclusion"};

    //Correction vector for Plane to Quad, don't mess with this unless you know what you're doing.
    private static readonly Vector3 rotationCorrection = new Vector3(90f, 180f, 0f);
}