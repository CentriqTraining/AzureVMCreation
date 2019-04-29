using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureManagement
{
    public static class Utilities
    {
        public static string DelimitedWith<T>(this IEnumerable<T> collectionToConvert, Func<T, string> expressionToReturnForEach, char delimiter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in collectionToConvert)
            {
                sb.Append(expressionToReturnForEach(t));
                sb.Append(delimiter);
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}
