using System;
using System.Collections.Generic;
using MobileParadise.Core.Save;
using MobileParadise.Games.Domino.Logic;

namespace MobileParadise.Games.Domino.View
{
    /// <summary>
    /// The match in progress: the scores and, if a round is being played, that round. Saved after every move.
    /// Load() returns null when there is nothing usable to continue.
    /// </summary>
    [Serializable]
    public class DominoSaveData
    {
        private const string Key = "domino_save";

        public bool HasGame;
        public int PlayerScore;
        public int ComputerScore;

        public bool HasRound;
        public int[] ChainIds;
        public int[] ChainLefts;
        public int[] ChainRights;
        public int[] PlayerHand;
        public int[] ComputerHand;
        public int[] Stock;
        public int Turn;      // 0 player, 1 computer
        public int Passes;

        public static DominoSaveData Load()
        {
            DominoSaveData data = SaveSystem.Load<DominoSaveData>(Key);
            return data.HasGame && data.IsValid() ? data : null;
        }

        /// <summary>
        /// Saves the scores and the round (a round that is already over is saved as scores only).
        /// </summary>
        public static void Save(DominoRound round, int playerScore, int computerScore)
        {
            DominoSaveData data = new DominoSaveData();
            data.HasGame = true;
            data.PlayerScore = playerScore;
            data.ComputerScore = computerScore;

            if (!round.IsOver)
            {
                data.HasRound = true;
                IList<PlacedTile> chain = round.Chain;
                data.ChainIds = new int[chain.Count];
                data.ChainLefts = new int[chain.Count];
                data.ChainRights = new int[chain.Count];
                for (int i = 0; i < chain.Count; i++)
                {
                    data.ChainIds[i] = chain[i].Id;
                    data.ChainLefts[i] = chain[i].Left;
                    data.ChainRights[i] = chain[i].Right;
                }
                data.PlayerHand = Copy(round.Hand(DominoSide.Player));
                data.ComputerHand = Copy(round.Hand(DominoSide.Computer));
                data.Stock = Copy(round.Stock);
                data.Turn = (int)round.Turn;
                data.Passes = round.Passes;
            }
            SaveSystem.Save(Key, data);
        }

        public static void Clear()
        {
            SaveSystem.Save(Key, new DominoSaveData());
        }

        public DominoRound ToRound()
        {
            List<PlacedTile> chain = new List<PlacedTile>();
            for (int i = 0; i < ChainIds.Length; i++)
            {
                chain.Add(new PlacedTile(ChainIds[i], ChainLefts[i], ChainRights[i]));
            }
            return new DominoRound(chain, PlayerHand, ComputerHand, Stock, (DominoSide)Turn, Passes);
        }

        // ---- Validation -------------------------------------------------------

        private bool IsValid()
        {
            if (PlayerScore < 0 || ComputerScore < 0) return false;
            return !HasRound || IsRoundValid();
        }

        // All 28 tiles must appear exactly once, and the chain must link up.
        private bool IsRoundValid()
        {
            if (ChainIds == null || ChainLefts == null || ChainRights == null) return false;
            if (PlayerHand == null || ComputerHand == null || Stock == null) return false;
            if (ChainIds.Length == 0 || ChainLefts.Length != ChainIds.Length || ChainRights.Length != ChainIds.Length) return false;
            if (PlayerHand.Length == 0 || ComputerHand.Length == 0) return false;
            if (Turn < 0 || Turn > 1 || Passes < 0 || Passes > 1) return false;

            bool[] seen = new bool[DominoTiles.Count];
            int total = 0;
            if (!MarkAll(ChainIds, seen, ref total) || !MarkAll(PlayerHand, seen, ref total)
                || !MarkAll(ComputerHand, seen, ref total) || !MarkAll(Stock, seen, ref total))
            {
                return false;
            }
            if (total != DominoTiles.Count) return false;

            for (int i = 0; i < ChainIds.Length; i++)
            {
                int id = ChainIds[i];
                bool straight = ChainLefts[i] == DominoTiles.Low(id) && ChainRights[i] == DominoTiles.High(id);
                bool flipped = ChainLefts[i] == DominoTiles.High(id) && ChainRights[i] == DominoTiles.Low(id);
                if (!straight && !flipped) return false;
                if (i > 0 && ChainRights[i - 1] != ChainLefts[i]) return false;
            }
            return true;
        }

        private static bool MarkAll(int[] ids, bool[] seen, ref int total)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                if (!DominoTiles.IsValidId(ids[i]) || seen[ids[i]]) return false;
                seen[ids[i]] = true;
                total++;
            }
            return true;
        }

        private static int[] Copy(IList<int> list)
        {
            int[] copy = new int[list.Count];
            list.CopyTo(copy, 0);
            return copy;
        }
    }
}
