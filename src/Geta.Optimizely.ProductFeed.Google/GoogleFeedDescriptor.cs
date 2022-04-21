// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed.Google
{
    public class GoogleFeedDescriptor<TEntity> : FeedDescriptor
    {
        public GoogleFeedDescriptor() : base("google", "/googleproductfeed", "application/xml") { }

        public GoogleFeedDescriptor<TEntity> SetConverter<TConverter>() where TConverter : IProductFeedConverter<TEntity>
        {
            Converter = typeof(TConverter);
            return this;
        }
    }
}
