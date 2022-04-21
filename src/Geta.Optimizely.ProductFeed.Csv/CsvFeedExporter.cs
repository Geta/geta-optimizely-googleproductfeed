// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Csv
{
    public class CsvFeedExporter<TEntity> : AbstractFeedContentExporter<TEntity>
    {
        private readonly CsvFeedDescriptor<TEntity> _descriptor;
        private readonly CsvWriter _writer;

        public CsvFeedExporter(CsvFeedDescriptor<TEntity> descriptor)
        {
            _descriptor = descriptor;
            _writer = new CsvWriter(new StreamWriter(Buffer), new CsvConfiguration(CultureInfo.InvariantCulture));
        }

        public override void BeginExport(CancellationToken cancellationToken)
        {
            _writer.WriteHeader(_descriptor.CsvEntityType);
            _writer.NextRecord();
        }

        public override object ConvertEntry(TEntity entity, CancellationToken cancellationToken)
        {
            return Converter.Convert(entity);
        }

        public override byte[] SerializeEntry(object value, CancellationToken cancellationToken)
        {
            _writer.WriteRecord(value);
            _writer.NextRecord();

            return Array.Empty<byte>();
        }

        public override ICollection<FeedEntity> FinishExport(CancellationToken cancellationToken)
        {
            _writer.Flush();

            return base.FinishExport(cancellationToken);
        }
    }
}
