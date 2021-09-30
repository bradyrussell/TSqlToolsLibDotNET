using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSqlToolsLib
{
    public class CaseChangingCharStream : ICharStream
    {
        private ICharStream stream;
        private bool upper;

        /// <summary>
        /// Constructs a new CaseChangingCharStream wrapping the given <paramref name="stream"/> forcing
        /// all characters to upper case or lower case.
        /// </summary>
        /// <param name="stream">The stream to wrap.</param>
        /// <param name="upper">If true force each symbol to upper case, otherwise force to lower.</param>
        public CaseChangingCharStream(ICharStream stream, bool upper)
        {
            this.stream = stream;
            this.upper = upper;
        }

        public int Index
        {
            get
            {
                return stream.Index;
            }
        }

        public int Size
        {
            get
            {
                return stream.Size;
            }
        }

        public string SourceName
        {
            get
            {
                return stream.SourceName;
            }
        }

        public void Consume()
        {
            stream.Consume();
        }

        public string GetText(Interval interval)
        {
            return stream.GetText(interval);
        }

        public int LA(int i)
        {
            int c = stream.LA(i);

            if (c <= 0)
            {
                return c;
            }

            char o = (char)c;

            if (upper)
            {
                return (int)char.ToUpperInvariant(o);
            }

            return (int)char.ToLowerInvariant(o);
        }

        public int Mark()
        {
            return stream.Mark();
        }

        public void Release(int marker)
        {
            stream.Release(marker);
        }

        public void Seek(int index)
        {
            stream.Seek(index);
        }
    }
}
