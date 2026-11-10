using System;
using System.Collections.Generic;

namespace MobileParadise.Games.Solitaire.Logic
{
    public enum DrawResult
    {
        None = 0,       // nothing to draw (stock and waste are empty)
        Drew = 1,       // card(s) moved from the stock to the waste
        Recycled = 2    // the stock was empty: the waste went back to the stock
    }

    /// <summary>
    /// The rules of Klondike Solitaire, with no Unity code.
    /// 7 columns (tableau), a stock and a waste, and 4 foundations built Ace to King by suit.
    /// Columns are built downward in alternating colors; a King (or a run starting with a King) goes on an empty column.
    /// Every successful action can be undone.
    /// </summary>
    public class KlondikeGame
    {
        private const int Columns = 7;

        private readonly List<int>[] _tableau = new List<int>[Columns];
        private readonly List<int>[] _foundation = new List<int>[CardInfo.SuitCount];
        private readonly int[] _hidden = new int[Columns];
        private readonly Stack<KlondikeState> _undo = new Stack<KlondikeState>();
        private readonly int _drawCount;
        private List<int> _stock = new List<int>();
        private List<int> _waste = new List<int>();

        public int Moves { get; private set; }
        public int DrawCount { get { return _drawCount; } }
        public bool CanUndo { get { return _undo.Count > 0; } }

        public int StockCount { get { return _stock.Count; } }
        public int WasteCount { get { return _waste.Count; } }

        /// <summary>The card on top of the stock (the one drawn next).</summary>
        public int StockTop { get { return _stock[_stock.Count - 1]; } }

        public bool IsWon
        {
            get
            {
                for (int suit = 0; suit < CardInfo.SuitCount; suit++)
                {
                    if (_foundation[suit].Count < CardInfo.King) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// A new shuffled game.
        /// </summary>
        public KlondikeGame(int drawCount, Random random)
            : this(drawCount, ShuffledDeck(random))
        {
        }

        /// <summary>
        /// A new game dealt from the given deck (52 cards): 1 card on the first column, 2 on the second... the rest is the stock.
        /// </summary>
        public KlondikeGame(int drawCount, int[] deck)
        {
            _drawCount = drawCount;
            CreatePiles();

            int next = 0;
            for (int column = 0; column < Columns; column++)
            {
                for (int i = 0; i <= column; i++)
                {
                    _tableau[column].Add(deck[next++]);
                }
                _hidden[column] = column;   // only the last card of each column starts face up
            }
            while (next < deck.Length)
            {
                _stock.Add(deck[next++]);
            }
        }

        public static int[] ShuffledDeck(Random random)
        {
            int[] deck = new int[CardInfo.CardCount];
            for (int i = 0; i < deck.Length; i++)
            {
                deck[i] = i;
            }
            for (int i = deck.Length - 1; i > 0; i--)   // Fisher-Yates shuffle
            {
                int j = random.Next(i + 1);
                int temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
            return deck;
        }

        /// <summary>
        /// Rebuilds a saved game (the undo history starts empty).
        /// </summary>
        public KlondikeGame(KlondikeState state)
        {
            _drawCount = state.DrawCount;
            CreatePiles();
            Restore(state);
        }

        // ---- Questions for the screen ----------------------------------------------

        public int TableauCount(int column)
        {
            return _tableau[column].Count;
        }

        public int TableauCard(int column, int index)
        {
            return _tableau[column][index];
        }

        public bool IsFaceUp(int column, int index)
        {
            return index >= _hidden[column];
        }

        public int WasteCard(int index)
        {
            return _waste[index];
        }

        public int FoundationCount(int suit)
        {
            return _foundation[suit].Count;
        }

        public int FoundationTop(int suit)
        {
            List<int> pile = _foundation[suit];
            return pile.Count == 0 ? -1 : pile[pile.Count - 1];
        }

        // ---- Actions -----------------------------------------------------------------

        /// <summary>
        /// Turns over cards from the stock (one or three). With an empty stock, the waste goes back to it.
        /// </summary>
        public DrawResult Draw()
        {
            if (_stock.Count == 0 && _waste.Count == 0) return DrawResult.None;

            _undo.Push(Capture());
            Moves++;

            if (_stock.Count > 0)
            {
                int count = Math.Min(_drawCount, _stock.Count);
                for (int i = 0; i < count; i++)
                {
                    int last = _stock.Count - 1;
                    _waste.Add(_stock[last]);
                    _stock.RemoveAt(last);
                }
                return DrawResult.Drew;
            }

            while (_waste.Count > 0)   // turn the waste over: its top card ends up at the bottom of the stock
            {
                int last = _waste.Count - 1;
                _stock.Add(_waste[last]);
                _waste.RemoveAt(last);
            }
            return DrawResult.Recycled;
        }

        /// <summary>
        /// Can the card (or run of cards, for a column) at 'from' / 'cardIndex' be put on 'to'?
        /// cardIndex is only used for columns: the first card of the run. For a foundation, 'to' can be any foundation:
        /// the card always goes to the pile of its own suit.
        /// </summary>
        public bool CanMove(PileRef from, int cardIndex, PileRef to)
        {
            List<int> run = GetRun(from, cardIndex);
            if (run == null) return false;

            if (to.Kind == PileKind.Foundation)
            {
                return run.Count == 1 && CanGoToFoundation(run[0]);
            }

            if (to.Kind != PileKind.Tableau) return false;
            if (from.Kind == PileKind.Tableau && from.Index == to.Index) return false;

            List<int> target = _tableau[to.Index];
            if (target.Count == 0) return CardInfo.Rank(run[0]) == CardInfo.King;

            int top = target[target.Count - 1];
            return CardInfo.IsRed(top) != CardInfo.IsRed(run[0]) && CardInfo.Rank(top) == CardInfo.Rank(run[0]) + 1;
        }

        public bool Move(PileRef from, int cardIndex, PileRef to)
        {
            if (!CanMove(from, cardIndex, to)) return false;

            List<int> run = GetRun(from, cardIndex);
            _undo.Push(Capture());
            RemoveRun(from, run.Count);

            if (to.Kind == PileKind.Foundation)
            {
                _foundation[CardInfo.Suit(run[0])].Add(run[0]);
            }
            else
            {
                _tableau[to.Index].AddRange(run);
            }

            Moves++;
            return true;
        }

        /// <summary>
        /// Sends the card to its foundation if it fits there.
        /// </summary>
        public bool MoveToFoundation(PileRef from, int cardIndex)
        {
            return Move(from, cardIndex, new PileRef(PileKind.Foundation, 0));
        }

        public bool Undo()
        {
            if (_undo.Count == 0) return false;

            Restore(_undo.Pop());
            return true;
        }

        public KlondikeState Capture()
        {
            KlondikeState state = new KlondikeState();
            state.DrawCount = _drawCount;
            state.Moves = Moves;
            state.Hidden = (int[])_hidden.Clone();
            state.Stock = Copy(_stock);
            state.Waste = Copy(_waste);
            state.Tableau = new CardPile[Columns];
            for (int i = 0; i < Columns; i++)
            {
                state.Tableau[i] = Copy(_tableau[i]);
            }
            state.Foundation = new CardPile[CardInfo.SuitCount];
            for (int i = 0; i < CardInfo.SuitCount; i++)
            {
                state.Foundation[i] = Copy(_foundation[i]);
            }
            return state;
        }

        // ---- Helpers -------------------------------------------------------------------

        private void CreatePiles()
        {
            for (int i = 0; i < Columns; i++)
            {
                _tableau[i] = new List<int>();
            }
            for (int i = 0; i < CardInfo.SuitCount; i++)
            {
                _foundation[i] = new List<int>();
            }
        }

        private void Restore(KlondikeState state)
        {
            Moves = state.Moves;
            _stock = new List<int>(state.Stock.Cards);
            _waste = new List<int>(state.Waste.Cards);
            for (int i = 0; i < Columns; i++)
            {
                _tableau[i] = new List<int>(state.Tableau[i].Cards);
                _hidden[i] = state.Hidden[i];
            }
            for (int i = 0; i < CardInfo.SuitCount; i++)
            {
                _foundation[i] = new List<int>(state.Foundation[i].Cards);
            }
        }

        private static CardPile Copy(List<int> cards)
        {
            CardPile pile = new CardPile();
            pile.Cards = cards.ToArray();
            return pile;
        }

        private bool CanGoToFoundation(int card)
        {
            return CardInfo.Rank(card) == _foundation[CardInfo.Suit(card)].Count + 1;
        }

        /// <summary>
        /// The cards that would be picked up, or null if they cannot be picked up.
        /// </summary>
        private List<int> GetRun(PileRef from, int cardIndex)
        {
            List<int> run = new List<int>();

            switch (from.Kind)
            {
                case PileKind.Waste:
                    if (_waste.Count == 0) return null;
                    run.Add(_waste[_waste.Count - 1]);
                    return run;

                case PileKind.Foundation:
                    int top = FoundationTop(from.Index);
                    if (top < 0) return null;
                    run.Add(top);
                    return run;

                case PileKind.Tableau:
                    List<int> column = _tableau[from.Index];
                    if (cardIndex < _hidden[from.Index] || cardIndex >= column.Count) return null;
                    for (int i = cardIndex; i < column.Count; i++)
                    {
                        run.Add(column[i]);
                    }
                    return run;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Takes the picked-up cards off their pile, and turns over the new top card of a column.
        /// </summary>
        private void RemoveRun(PileRef from, int count)
        {
            List<int> pile;
            switch (from.Kind)
            {
                case PileKind.Waste: pile = _waste; break;
                case PileKind.Foundation: pile = _foundation[from.Index]; break;
                default: pile = _tableau[from.Index]; break;
            }

            pile.RemoveRange(pile.Count - count, count);

            if (from.Kind == PileKind.Tableau && pile.Count > 0 && _hidden[from.Index] >= pile.Count)
            {
                _hidden[from.Index] = pile.Count - 1;
            }
        }
    }
}
