using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Fare
{
    public static class RandomGenerator
    {
        private static Random RandomInstance = new Random();
        
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {

            var index = 0;
            foreach (var item in source) {
                if (predicate.Invoke(item)) {
                    return index;
                }
                index++;
            }

            return -1;
        }
        
        public static T RandomItemWithProbability<T>(this IList<T> items, Func<T,int> weightFunc) {
            var sum = items.Sum(weightFunc);
            var cumulative = 0;
            var borders = items.Select(x => cumulative += weightFunc(x));
            var index = RandomInstance.Next(sum);
            return items[borders.IndexOf(x => x > index)];
        }

    }
}
