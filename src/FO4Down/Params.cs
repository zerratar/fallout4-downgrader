using System.ComponentModel;

namespace Fallout4Downgrader
{
    public class Params
    {
        private string[] arguments;

        public int IndexOf(string param)
        {
            for (var x = 0; x < arguments.Length; ++x)
            {
                if (arguments[x].Equals(param, StringComparison.OrdinalIgnoreCase))
                    return x;
            }

            return -1;
        }

        public bool Contains(string param)
        {
            return IndexOf(param) > -1;
        }

        public T Get<T>(string param, T defaultValue = default)
        {
            var index = IndexOf(param);

            if (index == -1 || index == (arguments.Length - 1))
                return defaultValue;

            var strParam = arguments[index + 1];

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                return (T)converter.ConvertFromString(strParam);
            }

            return default;
        }

        public List<T> GetList<T>(string param)
        {
            var list = new List<T>();
            var index = IndexOf(param);

            if (index == -1 || index == (arguments.Length - 1))
                return list;

            index++;

            while (index < arguments.Length)
            {
                var strParam = arguments[index];

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

        internal static Params FromArgs(string[] args)
        {
            return new Params { arguments = args };
        }
    }
}
