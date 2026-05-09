using UnityEngine;
using System.Collections.Generic;

public class AllLighting : MonoBehaviour
{
    private void Awake()
    {
        Instance = this;
        AllLightings = new List<Lighting>(FindObjectsOfType<Lighting>());
    }

    private void OnDestroy() => AllLightings.Clear();
    
    public static AllLighting Instance;
    private List<Lighting> AllLightings = new List<Lighting>();
    public List<Lighting> Lights
    {
        get
		{
            return AllLightings;
		}
    }
}
