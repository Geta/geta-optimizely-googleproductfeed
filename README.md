# Geta Optimizely Product Feed

## Status

![](https://tc.geta.no/app/rest/builds/buildType:(id:GetaPackages_OptimizelyGoogleProductFeed_00ci),branch:master/statusIcon)
[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=Geta_geta-optimizely-productfeed)](https://sonarcloud.io/summary/new_code?id=Geta_geta-optimizely-productfeed)
[![Platform](https://img.shields.io/badge/Platform-.NET%205-blue.svg?style=flat)](https://docs.microsoft.com/en-us/dotnet/)
[![Platform](https://img.shields.io/badge/Optimizely-%2012-orange.svg?style=flat)](http://world.episerver.com/cms/)
[![Platform](https://img.shields.io/badge/Optimizely%20Commerce-14-orange.svg?style=flat)](http://world.episerver.com/commerce/)

Credits: [How to make a Google Shopping Feed with C# and serve it through the Web API](http://blog.codenamed.nl/2015/05/14/creating-a-google-shopping-feed-with-c/).

This will create a Google Product Feed based on the [Atom specification](https://support.google.com/merchants/answer/160593?hl=en). For information on what is required and what the different attributes/properties mean, please see the [Product data specification](https://support.google.com/merchants/answer/188494).

## Installation

```
> dotnet add package Geta.Optimizely.ProductFeed
> dotnet add package Geta.Optimizely.ProductFeed.Google
```

## Configuration

For the `ProductFeed` to work, you have to call `AddProductFeed()` and `AddGoogleProductFeed()` extension methods in `Startup.ConfigureServices` method. In an action parameter of this method, you can provide a DB connection string.

```csharp
services
    .AddProductFeed(x =>
    {
        x.ConnectionString = _configuration.GetConnectionString("EPiServerDB");
    })
    .AddGoogleProductFeed(descriptor =>
    {
        descriptor.SetMapper<FeedEntityMapper>();
    });
```

## Feed Entity Mapping

`FeedEntityMapper` is mapping implementation to provide data for your Google entity mapping.

```csharp
public class FeedEntityMapper : IProductFeedEntityMapper
{
    public Feed GenerateFeedEntity(FeedDescriptor feedDescriptor)
    {
        // logic to generate feed entity (root) goes here
    }

    public Entry GenerateEntry(CatalogContentBase catalogContent)
    {
        // logic to generate each entry in the feed goes here
    }
}
```

Alternatively, you can configure a connection string in the `appsettings.config` file. A configuration from the `appsettings.json` will override configuration configured in Startup. Below is an `appsettings.json` configuration example.

```json
"Geta": {
    "ProductFeed": {
        "ConnectionString": "Data Source=..."
    }
}
```

Default URL for the google product feed is mounted on: `/googleproductfeed`. But you can configure to whatever suits your project best:

```csharp
services
    .AddProductFeed()
    .AddGoogleProductFeed(descriptor =>
    {
        descriptor.FileName = "/my-own-folder/my-feed";
        descriptor.SetMapper<FeedEntityMapper>();
    });
```

## FeedBuilder

You need to implement the abstract class `FeedBuilder` and the method `Build`. This will provide the feed data. `Build` method returns a list of feeds, this is required so that `FeedBuilder` can produce feeds for both multi-site and single-site projects. Example bellow can be extended to support multi-site projects.

### Default FeedBuilder

You can inherit from default base feed builder class (`DefaultFeedBuilderBase`) which will help you get started.
It contains `CatalogEntry` enumeration code and sample error handling. You will need to implement following methods:

```csharp
protected abstract Feed GenerateFeedEntity();

protected abstract Entry GenerateEntry(CatalogContentBase catalogContent);
```

For example:

```csharp
public class MyFeedBuilder : DefaultFeedBuilderBase
{
    private readonly IPricingService _pricingService;
    private readonly Uri _siteUri;

    public MyFeedBuilder(
        IContentLoader contentLoader,
        ReferenceConverter referenceConverter,
        IPricingService pricingService,
        ISiteDefinitionRepository siteDefinitionRepository,
        IContentLanguageAccessor languageAccessor) : base(contentLoader, referenceConverter, languageAccessor)
    {
        _pricingService = pricingService;
        _siteUri = siteDefinitionRepository.List().FirstOrDefault()?.Hosts.GetPrimaryHostDefinition().Url;
    }

    protected override Feed GenerateFeedEntity()
    {
        return new Feed
        {
            Updated = DateTime.UtcNow,
            Title = "My products",
            Link = _siteUri.ToString()
        };
    }

    protected override Entry GenerateEntry(CatalogContentBase catalogContent)
    {
        return ...;
    }

    private HostDefinition GetPrimaryHostDefinition(IList<HostDefinition> hosts)
    {
        if (hosts == null)
        {
            throw new ArgumentNullException(nameof(hosts));
        }

        return hosts.FirstOrDefault(h => h.Type == HostDefinitionType.Primary && !h.IsWildcardHost())
               ?? hosts.FirstOrDefault(h => !h.IsWildcardHost());
    }
}
```

### Implement Your Own Builder

If you need more flexible solution to build Google Product Feed - you can implement whole builder yourself.
Below is given sample feed builder (based on Quicksilver demo project). Please use it as a starting point and adjust things that you need to customize.
Also, keep in mind that for example error handling is not implemented in this sample (which means if variation generation fails - job will be aborted and feed will not be generated at all).

```csharp
public class MyFeedBuilder : FeedBuilder
{
    private readonly IContentLoader _contentLoader;
    private readonly ReferenceConverter _referenceConverter;
    private readonly IPricingService _pricingService;
    private readonly ILogger _logger;
    private readonly IContentLanguageAccessor _languageAccessor;
    private readonly Uri _siteUri;

    public MyFeedBuilder(
        IContentLoader contentLoader,
        ReferenceConverter referenceConverter,
        IPricingService pricingService,
        ISiteDefinitionRepository siteDefinitionRepository,
        IContentLanguageAccessor languageAccessor)
    {
        _contentLoader = contentLoader;
        _referenceConverter = referenceConverter;
        _pricingService = pricingService;
        _logger = LogManager.GetLogger(typeof(MyFeedBuilder));
        _languageAccessor = languageAccessor;
        _siteUri = GetPrimaryHostDefinition(siteDefinitionRepository.List().FirstOrDefault()?.Hosts)?.Url;
    }

    public override List<Feed> Build()
    {
        List<Feed> generatedFeeds = new List<Feed>();
        Feed feed = new Feed
        {
            Updated = DateTime.UtcNow,
            Title = "My products",
            Link = _siteUri.ToString()
        };

        IEnumerable<ContentReference> catalogReferences = _contentLoader.GetDescendents(_referenceConverter.GetRootLink());
        IEnumerable<CatalogContentBase> items = _contentLoader.GetItems(catalogReferences, CreateDefaultLoadOption()).OfType<CatalogContentBase>();

        List<Entry> entries = new List<Entry>();
        foreach (CatalogContentBase catalogContent in items)
        {
            FashionVariant variationContent = catalogContent as FashionVariant;

            try
            {
                if (variationContent == null)
                    continue;

                FashionProduct product = _contentLoader.Get<CatalogContentBase>(variationContent.GetParentProducts().FirstOrDefault()) as FashionProduct;
                string variantCode = variationContent.Code;
                IPriceValue defaultPrice = _pricingService.GetPrice(variantCode);

                Entry entry = new Entry
                {
                    Id = variationContent.Code,
                    Title = variationContent.DisplayName,
                    Description = product?.Description.ToHtmlString(),
                    Link = variationContent.GetUrl(),
                    Condition = "new",
                    Availability = "in stock",
                    Brand = product?.Brand,
                    MPN = "",
                    GTIN = "...",
                    GoogleProductCategory = "",
                    Shipping = new List<Shipping>
                    {
                        new Shipping
                        {
                            Price = "Free",
                            Country = "US",
                            Service = "Standard"
                        }
                    }
                };

                string image = variationContent.GetDefaultAsset<IContentImage>();

                if (!string.IsNullOrEmpty(image))
                {
                    entry.ImageLink = Uri.TryCreate(_siteUri, image, out Uri imageUri) ? imageUri.ToString() : image;
                }

                if (defaultPrice != null)
                {
                    IPriceValue discountPrice = _pricingService.GetDiscountPrice(variantCode);

                    entry.Price = defaultPrice.UnitPrice.ToString();
                    entry.SalePrice = discountPrice.ToString();
                    entry.SalePriceEffectiveDate = $"{DateTime.UtcNow:yyyy-MM-ddThh:mm:ss}/{DateTime.UtcNow.AddDays(7):yyyy-MM-ddThh:mm:ss}";
                }

                entries.Add(entry);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to generate feed item for catalog entry ({catalogContent.ContentGuid})", ex);
            }
        }

        feed.Entries = entries;
        generatedFeeds.Add(feed);

        return generatedFeeds;
    }

    private HostDefinition GetPrimaryHostDefinition(IList<HostDefinition> hosts)
    {
        if (hosts == null)
        {
            throw new ArgumentNullException(nameof(hosts));
        }

        return hosts.FirstOrDefault(h => h.Type == HostDefinitionType.Primary && !h.IsWildcardHost())
                ?? hosts.FirstOrDefault(h => !h.IsWildcardHost());
    }

    private LoaderOptions CreateDefaultLoadOption()
    {
        LoaderOptions loaderOptions = new LoaderOptions
        {
            LanguageLoaderOption.FallbackWithMaster(_languageAccessor.Language)
        };

        return loaderOptions;
    }
}
```

## Feed Generation

Populating the feed is handled through a scheduled job and the result is serialized and stored in the database. See job `Google ProductFeed - Create feed` in the Admin mode.

## Troubleshooting

If your request to `/googleproductfeed` (or any other path that you configured for the feed) returns 404 with message `No feed generated`, make sure you run the job to populate the feed.

## Local development setup

See description in [shared repository](https://github.com/Geta/package-shared/blob/master/README.md#local-development-set-up) regarding how to setup local development environment.

### Docker hostnames

Instead of using the static IP addresses the following hostnames can be used out-of-the-box.

http://googleproductfeed.getalocaltest.me
http://manager-googleproductfeed.getalocaltest.me


## Package maintainer

https://github.com/valdisiljuconoks

## Changelog

[Changelog](CHANGELOG.md)
