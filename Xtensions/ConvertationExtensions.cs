namespace Buratino.Xtensions
{
    public static class ConvertationExtensions
    {
        public static int AsInt(this object source)
        {
            if (source == null)
                return 0;
            if (int.TryParse(source.ToString(), out int res))
                return res;
            return 0;
        }

        public static long AsLong(this object source)
        {
            if (source == null)
                return 0;
            if (long.TryParse(source.ToString(), out long res))
                return res;
            return 0;
        }

        public static DateTime? AsDateTime(this object source)
        {
            if (source == null)
                return null;
            if (DateTime.TryParse(source.ToString(), out DateTime res))
                return res;
            return null;
        }
    }
}
