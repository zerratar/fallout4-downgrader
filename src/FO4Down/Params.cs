using System.ComponentModel;

namespace Fallout4Downgrader
{
    public static class Params
    {
        private static string[] startupArguments;

        public static int IndexOf(string param)
        {
            for (var x = 0; x < startupArguments.Length; ++x)
            {
                if (startupArguments[x].Equals(param, StringComparison.OrdinalIgnoreCase))
                    return x;
            }

            return -1;
        }

        public static bool HasParameter(string param)
        {
            return IndexOf(param) > -1;
        }

        public static T Get<T>(string param, T defaultValue = default)
        {
            var index = IndexOf(param);

            if (index == -1 || index == (startupArguments.Length - 1))
                return defaultValue;

            var strParam = startupArguments[index + 1];

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                return (T)converter.ConvertFromString(strParam);
            }

            return default;
        }

        public static List<T> GetList<T>(string param)
        {
            var list = new List<T>();
            var index = IndexOf(param);

            if (index == -1 || index == (startupArguments.Length - 1))
                return list;

            index++;

            while (index < startupArguments.Length)
            {
                var strParam = startupArguments[index];

                if (strParam[0] == '-') break;

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    list.Add((T)converter.ConvertFromString(strParam));
                }

                index++;
            }

            return list;
        }

        internal static void Init(string[] args)
        {
            startupArguments = args;
        }
    }
}
