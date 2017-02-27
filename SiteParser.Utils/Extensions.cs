using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SiteParser.Utils
{
    public static class Extensions
    {
        public static bool IsAscii(this string value)
        {
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            { }
        }

        public static void Clear<T>(this ConcurrentBag<T> bag)
        {
            T item;
            while (!bag.IsEmpty)
            {
                bag.TryTake(out item);
            }
        }

        public static List<T> TryTakeAll<T>(this ConcurrentBag<T> bag)
        {
            List<T> result = new List<T>();

            while (!bag.IsEmpty)
            {
                T item;
                bag.TryTake(out item);

                if (item != null)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static void AddRange<T>(this ConcurrentBag<T> bag, IEnumerable<T> list)
        {
            foreach (T item in list)
            {
                bag.Add(item);
            }
        }        
    }
}
