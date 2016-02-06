using System;
using System.Collections.Generic;
using System.Linq;

namespace Steam.Query.Tests
{
    internal static class TestExtensions
    {

        private static readonly Random Rng = new Random();

        public static List<T> Shuffle<T>(this List<T> list)
        {
            var newList = list.ToList();

            for (var i = 0; i < newList.Count; i++)
            {
                var j = Rng.Next(newList.Count);

                var e = newList[i];
                newList[i] = newList[j];
                newList[j] = e;
            }

            return newList;
        }

    }
}
