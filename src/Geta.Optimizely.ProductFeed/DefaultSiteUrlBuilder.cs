// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Linq;
using EPiServer.Web;

namespace Geta.Optimizely.ProductFeed;

public class DefaultSiteUrlBuilder : ISiteUrlBuilder
{
    private readonly string _siteUrl;

    public DefaultSiteUrlBuilder(ISiteDefinitionRepository siteDefinitionRepository)
    {
        _siteUrl = siteDefinitionRepository.List().FirstOrDefault()?.SiteUrl.ToString();
    }

    public string BuildUrl()
    {
        return _siteUrl;
    }
}
