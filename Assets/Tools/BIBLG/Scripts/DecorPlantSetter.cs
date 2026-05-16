using UnityEngine;

namespace JohnsterSpaceTools
{
    public class DecorPlantSetter : MonoBehaviour
    {
        // Start is called before the first frame update
        public void Start()
        {
            if (plantTransform != null) //If the plant transform field is not empty
            {
                if (randomizePosition) //If the randomize position boolean is set to true
                {
                    //Randomize the position of the plant on the tile
                    plantTransform.position += new Vector3(Random.Range(-3.5f, 3.5f), 0f, Random.Range(-3.5f, 3.5f));
                }
            }
            else //If the plant transform field is empty
            {
                Destroy(gameObject); //Destroy the gameobject that this script is attached to
            }
        }

        public Transform plantTransform;
        public bool randomizePosition;
    }
}
