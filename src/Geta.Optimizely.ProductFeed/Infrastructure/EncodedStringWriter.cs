﻿using System.IO;
using System.Text;

namespace Geta.Optimizely.ProductFeed.Infrastructure
{
    public sealed class EncodedStringWriter : StringWriter
    {
        public EncodedStringWriter(Encoding encoding)
        {
            Encoding = encoding;
        }

        public override Encoding Encoding { get; }
    }
}