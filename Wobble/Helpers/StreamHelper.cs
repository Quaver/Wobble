using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wobble.Helpers
{
    public static class StreamHelper
    {
        /// <summary>
        ///     Converts a stream to a byte[]
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (!s.CanRead)
                throw new ArgumentException("Stream cannot be read");

            if (s is MemoryStream ms)
                return ms.ToArray();

            var pos = s.CanSeek ? s.Position : 0L;
            if (pos != 0L)
                s.Seek(0, SeekOrigin.Begin);

            var result = new byte[s.Length];
            s.Read(result, 0, result.Length);
            if (s.CanSeek)
                s.Seek(pos, SeekOrigin.Begin);
            return result;
        }
    }
}
