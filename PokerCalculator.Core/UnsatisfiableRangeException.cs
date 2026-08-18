using System;

namespace PokerCalculator
{
    // Thrown by PEval.RandomHandRange when it can't find a villain card assignment that
    // avoids collisions within the caller-supplied attempt budget - either no valid deal
    // exists at all given hero/board/ranges, or a valid one is too rare to be worth
    // chasing. Without this cap the retry loop spins forever on a worker thread. The API
    // maps this to a 400 response (see Program.cs).
    public class UnsatisfiableRangeException : Exception
    {
        public UnsatisfiableRangeException(string message) : base(message)
        {
        }
    }
}
