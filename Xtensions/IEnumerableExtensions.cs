namespace Buratino.Xtensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> ActionAll<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
            return source;
        }

        public static IEnumerable<KeyValuePair<TIn, TOut>> FuncAll<TIn, TOut>(this IEnumerable<TIn> source, Func<TIn, TOut> func)
        {
            var keyValuePairs = new List<KeyValuePair<TIn, TOut>>();
            foreach (var item in source)
            {
                keyValuePairs.Add(new KeyValuePair<TIn, TOut>(item, func(item)));
            }
            return keyValuePairs;
        }

        //todo - переименовать
        public static string Join<TIn>(this IEnumerable<TIn> source, string separator = "\r\n", Func<TIn, string> selector = null)
        {
            if (selector == null)
                selector = (x) => x.ToString();

            return string.Join(separator, source.Select(selector));
        }

        public static bool In<T>(this T subject, IEnumerable<T> array)
        {
            return array.Contains(subject);
        }

        public static bool In<TIn1, TIn2>(this TIn1 obj, IEnumerable<TIn2> list, Func<TIn1, TIn2, bool> func)
        {
            return list.Any(x => func(obj, x));
        }

        public static IEnumerable<IEnumerable<T>> DevideBy<T>(this IEnumerable<T> source, int amount)
        {
            return source.Select((x, i) => (i, x))
                .GroupBy(x => x.i / amount)
                .Select(x => x.Select(y => y.x));
        }
    }
}
