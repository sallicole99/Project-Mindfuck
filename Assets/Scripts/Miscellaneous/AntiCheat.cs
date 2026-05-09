using UnityEngine;

public class AntiCheat : MonoBehaviour
{
    private void Start()
    {
        if (GameObject.Find("ExplorerBehaviour") != null)
        {
            Destroy(GameObject.Find("ExplorerBehaviour"));
        } 
    }
}