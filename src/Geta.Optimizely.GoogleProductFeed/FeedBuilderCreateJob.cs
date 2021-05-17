﻿// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Geta.Optimizely.GoogleProductFeed
{
    [ScheduledPlugIn(
        GUID = "F270B8BF-7B25-4254-B4D3-4FE843B2D559",
        DisplayName = "Google ProductFeed - Create feed",
        Description = "Creates and stores Google product feed in database")]
    public class FeedBuilderCreateJob : ScheduledJobBase
    {
        private readonly IFeedHelper _feedHelper;

        public FeedBuilderCreateJob(IFeedHelper feedHelper)
        {
            _feedHelper = feedHelper;
        }

        public override string Execute()
        {
            bool result = _feedHelper.GenerateAndSaveData();

            return result
                ? "Job successfully executed. Feed created and saved to the database."
                : "Job failed - FeedBuilder.Build() returned null.";
        }
    }
}