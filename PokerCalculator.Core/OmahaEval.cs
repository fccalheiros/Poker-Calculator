// Omaha hand evaluation: unlike Hold'em, the best hand must use exactly two pocket
// cards and three board cards, so every such combination has to be tried. Villain
// hands have 4 hole cards instead of 2 (RandomHand), but range-based hand drawing
// (RandomHandRange) is game-agnostic and lives in PEval.
using System;

namespace PokerCalculator
{
    public class OmahaEval : PEval
    {
        // Evaluate the best Omaha hand using exactly two pocket cards and three board cards.
        public static int ProcessCardSet(ulong pocketcards, ulong boardcards)
        {
            StripCardSet(pocketcards, out ulong[] pocket);
            StripCardSet(boardcards, out ulong[] board);
            return ProcessCardSet(pocket, board);
        }

        public static int ProcessCardSet(ulong[] pocketcards, ulong boardcards)
        {
            StripCardSet(boardcards, out ulong[] board);
            return ProcessCardSet(pocketcards, board);
        }


        public static int ProcessCardSet(ulong[] pocketcards, ulong[] board)
        {
            int finalResult = 0;

            for (int h1 = 0; h1 < pocketcards.Length - 1; h1++)
            {
                for (int h2 = h1 + 1; h2 < pocketcards.Length; h2++)
                {
                    ulong holeCards = pocketcards[h1] | pocketcards[h2];

                    for (int b1 = 0; b1 < board.Length - 2; b1++)
                    {
                        for (int b2 = b1 + 1; b2 < board.Length - 1; b2++)
                        {
                            for (int b3 = b2 + 1; b3 < board.Length; b3++)
                            {
                                int tempResult = PEval.GeneralProcessCardSet(holeCards | board[b1] | board[b2] | board[b3]);

                                if (tempResult > finalResult)
                                {
                                    finalResult = tempResult;
                                }
                            }
                        }
                    }
                }
            }

            return finalResult;
        }

        // Draws a board and a random, unrestricted 4-card villain hand.
        public static void RandomHand(ulong heroCards, ulong currentBoard, int boardCardsLeft, out ulong boardCards, out ulong villainCards, Random R) =>
            PEval.RandomHand(heroCards, currentBoard, boardCardsLeft, 4, out boardCards, out villainCards, R);

        public static MatchupResult SimulateMatchup(ulong heroHand, ulong currentBoard, int boardCardsLeft, Random random)
        {
            RandomHand(heroHand, currentBoard, boardCardsLeft, out ulong board, out ulong villainHand, random);
            int heroResult = ProcessCardSet(heroHand, board);
            int villainResult = ProcessCardSet(villainHand, board);

            if (heroResult > villainResult)
                return MatchupResult.Win;

            if (heroResult < villainResult)
                return MatchupResult.Loss;

            return MatchupResult.Tie;
        }

        public static void StripCardSet(ulong cardSet, out ulong[] stripedCards)
        {
            ulong[] tempCards = new ulong[52];
            int count = 0;

            while (cardSet != 0)
            {
                ulong temp = cleanLSB(cardSet);
                tempCards[count++] = cardSet ^ temp;
                cardSet = temp;
            }

            stripedCards = new ulong[count];

            for (int i = 0; i < count; i++)
            {
                stripedCards[i] = tempCards[i];
            }
        }
    }
}
