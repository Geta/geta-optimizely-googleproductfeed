﻿// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.GoogleProductFeed.Models;

namespace Geta.Optimizely.GoogleProductFeed.Repositories
{
    public interface IFeedRepository
    {
        void RemoveOldVersions(int numberOfGeneratedFeeds);

        FeedData GetLatestFeedData(string siteHost);

        void Save(FeedData feedData);
    }
}