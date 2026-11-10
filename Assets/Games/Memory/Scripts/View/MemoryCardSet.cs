using UnityEngine;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// A theme: the pictures that can appear on the cards (e.g. Fruits, Animals).
    /// Built from the PNGs in Art/<Theme> by "Mobile Paradise > Memory > 1. Build Card Sets".
    /// </summary>
    [CreateAssetMenu(fileName = "CardSet", menuName = "Mobile Paradise/Memory/Card Set")]
    public class MemoryCardSet : ScriptableObject
    {
        [SerializeField] private Sprite[] _faces;

        public int Count
        {
            get { return _faces.Length; }
        }

        public Sprite GetFace(int index)
        {
            return _faces[index];
        }
    }
}
