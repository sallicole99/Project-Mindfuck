using UnityEngine;
using System.Collections.Generic;

public class EntranceScript : MonoBehaviour
{
    private void Start() => gc = FindObjectOfType<GameControllerScript>();

    public void Disable() => gameObjects.ForEach(obj => obj.SetActive(false));

    public void Enable()
    {
        gameObjects.ForEach(obj => obj.SetActive(true));
        if (gc.finaleMode)
        {
            wall.material = map;
        }
    }

    [Header("Entrance Materials and Wall")]
    [SerializeField] private Material map;
    [SerializeField] private MeshRenderer wall;

    [Header("Game Objects List")]
    [SerializeField] private List<GameObject> gameObjects = new List<GameObject>();
    
    private GameControllerScript gc;
}