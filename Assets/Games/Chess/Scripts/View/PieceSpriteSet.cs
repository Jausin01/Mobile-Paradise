using MobileParadise.Games.Chess.Logic;
using UnityEngine;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// The 12 piece sprites. Swap the art by swapping this asset's sprites.
    /// Array order matches PieceType 1..6: Pawn, Knight, Bishop, Rook, Queen, King.
    /// </summary>
    [CreateAssetMenu(fileName = "PieceSpriteSet", menuName = "Mobile Paradise/Chess/Piece Sprite Set")]
    public class PieceSpriteSet : ScriptableObject
    {
        [SerializeField] private Sprite[] _white = new Sprite[6];
        [SerializeField] private Sprite[] _black = new Sprite[6];

        public Sprite Get(PieceType type, PieceColor color)
        {
            if (type == PieceType.None) return null;

            Sprite[] set = color == PieceColor.White ? _white : _black;
            return set[(int)type - 1];
        }
    }
}
