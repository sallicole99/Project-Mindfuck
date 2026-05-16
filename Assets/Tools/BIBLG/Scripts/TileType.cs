using UnityEngine;

namespace JohnsterSpaceTools
{
    public class TileType : MonoBehaviour
    {
        public enum TileVariant
        {
            Corner, End, Full, Open, Single, Straight
        }
        public TileVariant tileType;
        public bool destroyScript = true;
    }
}
