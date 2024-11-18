// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Repositories;

public interface IFeedRepository
{
    FeedEntity GetLatestFeed(Uri siteUri);

    void Save(ICollection<FeedEntity> feedData);

    Task SaveAsync(ICollection<FeedEntity> feedData, CancellationToken cancellationToken);

    FeedDescriptor FindDescriptorByUri(Uri siteUri);
}
