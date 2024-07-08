using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PokerCalculator
{
    class OmahaEval : PEval
    {
        //Given hero and partial (or not) board cards, draw villain hand and the rest of the board
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ProcessCardSet(ulong cardSet)
        {
            return PEval.GeneralProcessCardSet(cardSet);
        }

        //Given hero and partial (or not) board cards, draw villain hand and the rest of the board
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ProcessCardSet(ulong pocketcards, ulong board)
        {
            return PEval.GeneralProcessCardSet(pocketcards | board);
        }

        public static int ProcessCardSet(ulong [] pocketcards, ulong board)
        {
            int finalResult = 0;
            int tempResult;

            for (int i = 0; i < pocketcards.Length - 1; i++) {
                for (int j = i + 1; j < pocketcards.Length; j++) {
                    tempResult = ProcessCardSet(pocketcards[i] | pocketcards[j] | board);
                    if (tempResult > finalResult) { 
                        finalResult = tempResult;
                    }
                }
            }
            return finalResult;
        }

        public static void StripCardSet(ulong cardSet, out ulong[] stripedCards)
        {
            int i = 0;
            ulong temp;
            stripedCards = new ulong[4];

            for (; i < 4; i++)
            {
                temp = cleanLSB(cardSet);
                stripedCards[i] = cardSet ^ temp;
                cardSet = temp;
            }
        }
    }
}
