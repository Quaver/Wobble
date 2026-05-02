using System;
using System.Collections;
using System.Collections.Generic;

namespace Wobble.Helpers
{
    public static class ListHelper
    {
        public static int LastTrue<T>(this IReadOnlyList<T> list, Func<T, bool> f, int lo = 0, int hi = -1)
        {
            if (hi == -1)
                hi = list.Count - 1;

            // if none of the values in the range work, return lo - 1
            lo--;
            while (lo < hi)
            {
                // find the middle of the current range (rounding up)
                var mid = lo + (hi - lo + 1) / 2;
                if (f(list[mid]))
                {
                    // if mid works, then all numbers smaller than mid also work
                    lo = mid;
                }
                else
                {
                    // if mid does not work, greater values would not work either
                    hi = mid - 1;
                }
            }

            return lo;
        }

        public class Iota : IReadOnlyList<int>
        {
            public Iota(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start { get; }
            public int End { get; }

            public IEnumerator<int> GetEnumerator()
            {
                for (var i = Start; i <= End; i++)
                {
                    yield return i;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => End - Start + 1;

            public int this[int index] => Start + index;
        }
    }
}