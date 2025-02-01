using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Buratino.Xtensions;

namespace Buratino.Xtensions
{
    public static class DataXtensions
    {
        public static string ClampLength(this string val, int count = 2, char ch = '0', bool before = true)
        {
            if (val.Length >= count)
            {
                if (val.Length > count)
                    val = val.Substring(0, count);
                return val;
            }
            string res = "";
            if (before)
                res = new string(ch, count - val.Length) + val;
            else
                res = val + new string(ch, count - val.Length);
            return res;
        }
        public static string NullOrEmpty(this object str, string def = null)
        {
            if (str == null) return def;
            if (str.ToString() == "") return def;
            return str.ToString();
        }
        public static bool IsNullOrEmpty(this object str)
        {
            return string.IsNullOrEmpty(str?.ToString());
        }
        public static string UriToStr(this string uri)
        {
            return HttpUtility.UrlDecode(uri);
        }
        public static string UriDataToStr(this string uri)
        {
            return HttpUtility.HtmlDecode(uri);
        }
        public static string StrToUri(this string uri)
        {
            return HttpUtility.UrlEncode(uri);
        }
        public static string[] FSpl(this string str, string of = "|", bool RemoveEmpties = true)
        {
            if (str == null)
                return new string[0];
            return str.Split(new string[] { of }, RemoveEmpties ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }
        public static TSource Clamp<TSource>(this TSource val, TSource min, TSource max) where TSource : IComparable
        {
            if (val.CompareTo(min) < 0)
                val = min;
            if (val.CompareTo(max) > 0)
                val = max;
            return val;
        }
        public static string _AlpthToQRId = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string IdtoQRId(this int id)
        {
            string res = "";
            while (id >= _AlpthToQRId.Length)
            {
                int ost = id % _AlpthToQRId.Length;
                id = id / _AlpthToQRId.Length;
                res = _AlpthToQRId[ost] + res;
            }
            if (id > 0)
            {
                res = _AlpthToQRId[id] + res;
            }
            return res;
        }
        public static int QRIdtoId(this string qrId)
        {
            qrId = qrId.ToUpper();
            int res = 0;
            for (int i = 0; i < qrId.Length; i++)
            {
                var ch = qrId[i];
                int pos = _AlpthToQRId.IndexOf(ch);
                if (pos >= 0)
                {
                    res += (int)Math.Pow(_AlpthToQRId.Length, qrId.Length - (i + 1)) * pos;
                }
                else
                {
                    throw new Exception("Неправильно обрабатывается QRCode");
                }
            }
            return res;
        }
        public static string DateTimeAgo(this DateTime date, bool onlyTimeAgo = false)
        {
            var period = DateTime.Now.Subtract(date);
            var timeAgo = period.PeriodToHuman();
            if (onlyTimeAgo)
            {
                return timeAgo;
            }
            else
            {
                return date.ToString() + "(" + timeAgo + ")";
            }
        }
        public static string PeriodToHuman(this TimeSpan date)
        {
            if (date.TotalMinutes < 1)
                return "только что";
            else if (date.TotalHours < 1)
                return $"{date.TotalMinutes.Round().AddTrueWord("минуту", "минуты", "минут")} назад";
            else if (date.TotalDays < 1)
                return $"{date.TotalHours.Round().AddTrueWord("час", "часа", "часов")} назад";
            else
                return $"{date.TotalDays.Round().AddTrueWord("день", "дня", "дней")} назад";
        }
        public static string PeriodLengthToHuman(this TimeSpan date)
        {
            return date.ToString();
        }
        public static int Round(this double number)
        {
            return (int)Math.Round(number, 0);
        }
        public static double Round(this double number, int count)
        {
            return Math.Round(number, count);
        }

        public static int Round(this decimal number)
        {
            return (int)Math.Round(number, 0);
        }

        public static decimal Round(this decimal number, int count)
        {
            return Math.Round(number, count);
        }

        static string alphabet = "0123456789QWERTYUIOPASDFGHJKLZXCVBNM";
        static Random ran = new Random();
        public static string GetRandom(this int length, bool DigitOnly = false)
        {
            string res = "";
            if (DigitOnly)
            {
                while (length > 0)
                {
                    res += alphabet[ran.Next(0, 10)];
                    length--;
                }
            }
            else
            {
                while (length > 0)
                {
                    res += alphabet[ran.Next(0, alphabet.Length)];
                    length--;
                }
            }
            return res;
        }
        public static byte[] ToBytes(this string text, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            if (text != null)
                return encoding.GetBytes(text);
            else
                return null;
        }
        public static string GetBase64Image(this byte[] data)
        {
            return @"data:image/jpeg;base64," + Convert.ToBase64String(data);
        }

        /// <summary>
        /// Возвращает массив из 2 эллементов. Первый всегда заполнен, второй может быть пустой строкой
        /// </summary>
        /// <param name="str"></param>
        /// <param name="spliter"></param>
        /// <param name="firstMatch"></param>
        /// <returns></returns>
        public static string[] TrueSplit(this string str, string spliter = ":", bool firstMatch = true)
        {
            if (str == null)
                return null;
            string[] ot = new string[2];

            int n;
            if (firstMatch)
                n = str.IndexOf(spliter);
            else
                n = str.LastIndexOf(spliter);

            if (n >= 0)
            {
                ot[0] = str.Substring(0, n).Trim();
                ot[1] = str.Substring(n + spliter.Length).Trim();
            }
            else
            {
                ot[0] = str.Trim();
                ot[1] = null;
            }
            return ot;
        }
        public static double Interpolate(this double a, double b, double k = 0.5)
        {
            return (double)(a + (b - a) * k);
        }
        public static byte Interpolate(this byte a, byte b, double k = 0.5)
        {
            return (byte)(a + (b - a) * k);
        }
        public static Color Interpolate(this Color a, Color b, double k = 0.5)
        {
            return Color.FromArgb(a.R.Interpolate(b.R, k), a.G.Interpolate(b.G, k), a.B.Interpolate(b.B, k));
        }
        public static bool Between<TSource>(this TSource val, TSource min, TSource max) where TSource : IComparable
        {
            return val.CompareTo(min) >= 0 && val.CompareTo(max) <= 0;
        }
        public static bool Between_LTE_GTE<TSource>(this TSource val, TSource min, TSource max) where TSource : IComparable
        {
            if (min != null && max != null)
                return val.Between(min, max);
            else if (min != null)
                return val.CompareTo(min) >= 0;
            else if (max != null)
                return val.CompareTo(max) <= 0;
            else
                return true;
        }
        public static bool Between<T>(this int val, IEnumerable<T> array)
        {
            var max = array.Count();
            return val.Between(0, max);
        }
        public static TimeSpan TimeLeft(this DateTime oldTimeStamp)
        {
            return DateTime.Now.Subtract(oldTimeStamp);
        }
        public static string Add(this string str, string addStr, bool isBefore = false)
        {
            if (isBefore)
            {
                return addStr + str;
            }
            else
            {
                return str + addStr;
            }
        }
        public static string Join(this IEnumerable<string> arr, string sep)
        {
            return string.Join(sep, arr);
        }
        public static string Join<T>(this IEnumerable<T> arr, string sep)
        {
            return string.Join(sep, arr);
        }
        public static Exception NormalizePhoneNumber(ref string phone)
        {
            if (phone == null)
                return new Exception("Значение phone не задано");
            if (phone == "")
                return new Exception("Значение phone пустое");
            phone = phone.DeleteChars(" ", Convert.ToChar(160).ToString(), "-", "+", "(", ")");
            if (phone.Length == 10 && phone.StartsWith("9"))//prfct
            {
                phone = "+7" + phone;
            }
            else if (phone.Length == 12 && phone.StartsWith("+7"))//prfct
            {
                phone = phone;
            }
            else if (phone.Length == 11)
            {
                if (phone.StartsWith("8"))
                {
                    phone = "+7" + phone.Substring(1);
                }
                else if (phone.StartsWith("7"))
                {
                    phone = "+7" + phone.Substring(1);
                }
                else
                {
                    return new Exception($"Значение phone не поддается анализу {phone}");
                }
            }
            else
            {
                return new Exception($"Значение phone не поддается анализу {phone}");
            }
            return null;
        }
        public static string DeleteChars(this string str, params char[] chars)
        {
            return str.DeleteChars(chars.Select(x => x.ToString()).ToArray());
        }
        public static string DeleteChars(this string str, params string[] chars)
        {
            if (str == null)
                return null;
            for (int i = 0; i < chars.Length; i++)
            {
                str = str.Replace(chars[i], "");
            }
            return str;
        }
        public static int IndexOfArr(this int[][] arr2, int[] arr1)
        {
            for (int i = 0; i < arr2.Length; i++)
            {
                if (arr2[i] == arr1)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="count">Количество знаков перед запятой, которые необходимо убрать</param>
        /// <returns></returns>
        public static int GetMajor(this int ceil, int count = 3)
        {
            if (count == 0)
                return ceil;
            string ceilStr = ceil.ToString();
            if (ceilStr.Length <= count)
                return 0;
            return int.Parse(ceilStr.Remove(ceilStr.Length - count));
        }
        public static int GetMajor(this double number, int count = 3)
        {
            int ceil = (int)Math.Floor(number);
            return ceil.GetMajor(count);
        }
        public static Dictionary<string, string> AddToDic(this Dictionary<string, string> dic, string key, string val)
        {
            if (dic == null)
                dic = new Dictionary<string, string>();
            dic.Add(key, val);
            return dic;
        }
        public static Dictionary<string, string> AddToDic(this string key, string val)
        {
            var dic = new Dictionary<string, string>();
            dic.Add(key, val);
            return dic;
        }
        public static string AddTrueWord(this int A, params string[] words)
        {
            return A + " " + A.TrueWord(words);
        }
        public static string TrueWord(this int A, params string[] words)
        {
            //час часа часов
            if (A >= 11 & A <= 19)
                return words[2];
            else if (A.ToString().EndsWith("1"))
                return words[0];
            else if (int.Parse(A.ToString().Last() + "").Between(2, 4))
                return words[1];
            else return words[2];
        }
        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> obj, Func<T, TKey> predicate)
        {
            return obj.GroupBy(predicate).Select(x => x.First());
        }

        [DllImport("kernel32.dll")]
        private static extern void QueryPerformanceCounter(ref long ticks);
        public static void NanoSecColebrate()
        {
            long startTicks = 0L;
            QueryPerformanceCounter(ref startTicks);
            _TicksPerNanoSecond = (long)Math.Round(startTicks / Environment.TickCount / 1000.0, 0);//6844
            _TicksPerNanoSecond = Math.Abs(_TicksPerNanoSecond);
            if (_TicksPerNanoSecond != 10)
            {
                Console.WriteLine("Неожиданные значения при калибровке таймера: " + _TicksPerNanoSecond);
            }
        }
        public static long _TicksPerNanoSecond = 1;
        static long[] _TimeMesureK = new long[6] { 1, 1000, 1000000, 1000000 * 60, 1000000L * 60 * 60, 1000000L * 60 * 60 * 24 };
        public enum TimeMesure
        {
            Micro = 0,
            Milli = 1,
            Sec = 2,
            Minute = 3,
            Hour = 4,
            Day = 5
        }
        public static long NanoSec(long startTime = 0, TimeMesure timeMesure = TimeMesure.Milli)
        {
            long startTicks = 0L;
            QueryPerformanceCounter(ref startTicks);
            var res = startTicks / _TicksPerNanoSecond;
            if (startTime == 0)
                return res;
            else
            {
                res = (long)((res - startTime) * 1.0 / _TimeMesureK[(int)timeMesure]);
                return res;
            }
        }


        /// <summary>
        /// Является ли свойство одинаковым у всех элементов массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="data"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static bool IsSameProp<T, TOut>(this IEnumerable<T> data, Func<T, TOut> func) where TOut : IComparable
        {
            var count = data.GetPropKinds(func).Count();
            return count == 1;
        }


        /// <summary>
        /// Является ли свойство уникальным у всех элементов массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="data"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static bool IsUniqueProp<T, TOut>(this IEnumerable<T> data, Func<T, TOut> func) where TOut : IComparable
        {
            var count = data.GetPropKinds(func).Count();
            return count == data.Count();
        }


        /// <summary>
        /// Возвращает множество значений свойств у элемента массива
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="data"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<TOut> GetPropKinds<T, TOut>(this IEnumerable<T> data, Func<T, TOut> func) where TOut : IComparable
        {
            var res = data.Select(func).Distinct();
            return res;
        }
        public static bool EqualsWithAny<T>(this T data, params T[] vars) where T : IComparable
        {
            return vars.Any(x => x.CompareTo(data) == 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="countWeekends">true - подсчет выходных</param>
        /// <param name="addingWeekends">true - прибавление, иначе - вычитание</param>
        /// <returns></returns>
        public static double Subtract(this DateTime startDate, DateTime endDate, bool countWeekends, bool addingWeekends)
        {
            endDate = endDate.AddHours(23 - endDate.Hour);
            double countOfWeekends = 0;
            int velocity = endDate.CompareTo(startDate);
            for (DateTime index = startDate; index.Date != endDate.Date; index = index.AddDays(velocity))
            {
                if (countWeekends)
                {
                    if (index.DayOfWeek != DayOfWeek.Sunday && index.DayOfWeek != DayOfWeek.Saturday)
                    {

                    }
                    else
                    {
                        countOfWeekends++;
                    }
                }
            }
            var res = endDate.Subtract(startDate).TotalDays;
            if (addingWeekends)
                res += countOfWeekends;
            else
                res -= countOfWeekends;
            return res;
        }
        public static string TimeToHuman(this double days)
        {
            var res = days > 1 ? ((int)Math.Floor(days)).AddTrueWord("день", "дня", "дней") : ((int)(days * 24)).AddTrueWord("час", "часа", "часов");
            return res;
        }
        public static string TimeToShortHuman(this double days)
        {
            var res = days > 1 ? (int)Math.Floor(days) + "д" : (int)(days * 24) + "Ч";
            return res;
        }
        public static int GetImportant(this double days)
        {
            if (days > 7)
                return 0;
            else if (days > 3)
                return 1;
            else if (days > 1)
                return 2;
            else
                return 3;
        }
        public static string GetYMColor(this int important)
        {
            switch (important)
            {
                case 0:
                    return "night";
                case 1:
                    return "violet";
                case 2:
                    return "darkOrange";
                case 3:
                    return "red";
                default:
                    return "red";
            }
        }
        public static string GetBootColor(this int important)
        {
            switch (important)
            {
                case 0:
                    return "secondary";
                case 1:
                    return "info";
                case 2:
                    return "warning";
                case 3:
                    return "danger";
                default:
                    return "danger";
            }
        }
        public static string GetHTMLColor(this int important)
        {
            switch (important)
            {
                case 0:
                    return "gray";
                case 1:
                    return "purple";
                case 2:
                    return "orange";
                case 3:
                    return "red";
                default:
                    return "red";
            }
        }
        public static string LoadPrefab(string name)
        {
            string prefab;
            StreamReader streamReader = new StreamReader(Path.Combine("files", name));
            prefab = streamReader.ReadToEnd();
            streamReader.Close();
            return prefab;
        }
        public static string GetFolderId(this string fullLink)
        {
            if (fullLink.Length > 0)
            {
                fullLink = fullLink.Substring(fullLink.LastIndexOf('/') + 1);
            }
            return fullLink;
        }

        public static string Serialize(this object inputData)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            };

            var res = JsonSerializer.Serialize(inputData, options);
            return res;
        }
        public static T Deserialize<T>(this string inputData)
        {
            var options = new JsonSerializerOptions
            {
                //Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            };
            try
            {
                var res = JsonSerializer.Deserialize<T>(inputData, options);
                return res;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static string MoneyToHuman(this decimal money, int decimalPlaces = 2)
        {
            money = Math.Round(money, decimalPlaces);
            string[] parts = money.ToString().Split(',');
            string res = parts[0];
            if (parts.Length > 0)
            {
                for (int i = res.Length - 4; i >= 0; i -= 3)
                {
                    res = res.Insert(i + 1, " ");
                }
            }
            else
            {
                return money.ToString();
            }
            if (parts.Length == 2)
                res += "," + parts[1].Substring(0, decimalPlaces);
            return res;
        }

        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType)
            {
                try
                {
                    return Activator.CreateInstance(t);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }

        public static Size Scale(this Size size, double k)
        {
            return new Size((int)(size.Width * k), (int)(size.Height * k));
        }

        /// <summary>
        /// Проверяет размер. Если он меньше, пропорционально увеличивает.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="width">if 0, then not clamping</param>
        /// <param name="height">if 0, then not clamping</param>
        /// <returns></returns>
        public static Size Clamp(this Size size, int width = 0, int height = 0)
        {
            var res = size;
            if (width > 0)
            {
                if (res.Width < width)
                {
                    res = res.Scale(width * 1.0 / res.Width);
                }
            }
            if (height > 0)
            {
                if (res.Width < width)
                {
                    res = res.Scale(height * 1.0 / res.Height);
                }
            }
            return res;
        }

        public static string ConvertToMimeType(this string ext)
        {
            ext = ext.ToLower();
            switch (ext)
            {
                case ".jpeg":
                    return "image/jpeg";
                case ".jpg":
                    return "image/jpeg";
                case ".avi":
                    return "video/avi";
                case ".mp4":
                    return "video/mp4";
                default:
                    return "content/binary";
            }
        }
        public static string UnUnicode(this string str)
        {
            if (str == null)
                return str;
            return str
                .Replace("/u002B", "+")
                .Replace("/u00A0", " ")
                .Replace("/u0026", "&");
        }

        /// <summary>
        /// Возвращает 1 из 4 типов перечения. Не принимает непересекающиеся периоды.
        /// </summary>
        /// <param name="s">Сделка</param>
        /// <param name="e">Сделка</param>
        /// <param name="s1">Период</param>
        /// <param name="e1">Период</param>
        /// <returns>0-outer(100%); 1-inner(E-S); 2-lefter(E-s1); 3-righter(e1-S)</returns>
        public static int GetTypeOfSaleXcross(DateTime s, DateTime e, DateTime s1, DateTime e1)
        {
            if (s <= s1 && e >= e1)
                return 0;
            else if (s >= s1 && e <= e1)
                return 1;
            else if (e >= s1 && s <= s1)
                return 2;
            else if (s <= e1 && e >= e1)
                return 3;
            throw new Exception("Эта ваще чо такое");
        }

        /// <summary>
        /// Возвращает период пересечения в секундах
        /// </summary>
        /// <param name="s">Сделка</param>
        /// <param name="e">Сделка</param>
        /// <param name="s1">Период</param>
        /// <param name="e1">Период</param>
        /// <returns></returns>
        public static double GetPeriodOfSaleXcross(DateTime s, DateTime e, DateTime s1, DateTime e1)
        {
            var type = GetTypeOfSaleXcross(s, e, s1, e1);
            switch (type)
            {
                case 0:
                    return e1.Subtract(s1).TotalSeconds;
                case 1:
                    return e.Subtract(s).TotalSeconds;
                case 2:
                    return e.Subtract(s1).TotalSeconds;
                case 3:
                    return e1.Subtract(s).TotalSeconds;
                default:
                    break;
            }
            throw new Exception("Эта ваще чо такое");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">Сделка</param>
        /// <param name="e">Сделка</param>
        /// <param name="s1">Период</param>
        /// <param name="e1">Период</param>
        /// <returns></returns>
        public static bool IsXcrossing(DateTime s, DateTime e, DateTime s1, DateTime e1)
        {
            return s < e1 && e > s1;
        }

        public static double Abs(this double number)
        {
            return Math.Abs(number);
        }
        public static decimal Abs(this decimal number)
        {
            return Math.Abs(number);
        }
        public static int Abs(this int number)
        {
            return Math.Abs(number);
        }

        public static bool In<T>(this T subject, params T[] array)
        {
            return array.Contains(subject);
        }

        public static bool NotIn<T>(this T subject, params T[] array)
        {
            return !subject.In(array);
        }

        public static double UnixTicks(this DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }
        public static double SafeConvertToDouble(this string doubleStr)
        {
            doubleStr = doubleStr.NullOrEmpty("0");
            doubleStr = doubleStr.Replace(".", ",");

            string whiteList = "0123456789,";
            var trash = doubleStr.Except(whiteList);
            doubleStr = doubleStr.DeleteChars(trash.ToArray());

            doubleStr = doubleStr.Trim(',');
            if (double.TryParse(doubleStr, out double res))
                return res;
            return 0;
        }
        public static string GetWhatsAppUrl(string truePhone)
        {
            return truePhone == null ? "#" : "https://api.whatsapp.com/send?phone=" + truePhone;
        }
        public static string GetTGUrl(string trueUsername)
        {
            return trueUsername == null ? "#" : "https://t.me/" + trueUsername;
        }
        public static string ConvertToTranslit(this string source)
        {
            source = source.ToLower();
            string result = "";
            foreach (var item in source)
            {
                result += item switch
                {
                    'а' => "a",
                    'б' => "b",
                    'в' => "v",
                    'г' => "g",
                    'д' => "d",
                    'е' => "e",
                    'ё' => "e",
                    'ж' => "zh",
                    'з' => "z",
                    'и' => "i",
                    'й' => "i",
                    'к' => "k",
                    'л' => "l",
                    'м' => "m",
                    'н' => "n",
                    'о' => "o",
                    'п' => "p",
                    'р' => "r",
                    'с' => "s",
                    'т' => "t",
                    'у' => "u",
                    'ф' => "f",
                    'х' => "kh",
                    'ц' => "ts",
                    'ч' => "ch",
                    'ш' => "sh",
                    'щ' => "shch",
                    'ъ' => "",
                    'ы' => "y",
                    'ь' => "",
                    'э' => "e",
                    'ю' => "iu",
                    'я' => "ia",
                    _ => ""
                };
            }
            return result;
        }

        public static string ToInputDate(this DateTime dateTime)
        {
            return string.Format("{0:yyyy/MM/dd}", dateTime).Replace(".", "-");
        }
        public static double AverageSafe<T>(this IEnumerable<T> arr, Func<T, double> func, double emptyRes = 0)
        {
            if (arr.Count() == 0)
            {
                return emptyRes;
            }
            return arr.Average(func);
        }

        public static T GetOrAdd<T>(this Dictionary<string, T> dic, string key)
        {
            if (dic.ContainsKey(key))
                return dic[key];
            else
            {
                T value = default;
                dic.Add(key, value);
                return value;
            }
        }

        public static DateTime GetMonthStart(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }
    }
}