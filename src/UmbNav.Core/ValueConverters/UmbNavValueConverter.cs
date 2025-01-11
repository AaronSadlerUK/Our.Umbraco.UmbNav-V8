using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using UmbNav.Core.Interfaces;
using UmbNav.Core.Models;
using UmbNav.Core.PropertyEditors;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace UmbNav.Core.ValueConverters
{
#if NET8_0_OR_GREATER
#nullable enable

    using Umbraco.Cms.Core.DeliveryApi;
    using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
    using Umbraco.Cms.Core.PublishedCache;

    public class CustomUmbNavValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
        private readonly ILogger _logger;
        private readonly IUmbNavMenuBuilderService _umbNavMenuBuilderService;

        public CustomUmbNavValueConverter(
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IApiContentRouteBuilder apiContentRouteBuilder,
            ILogger logger,
            IUmbNavMenuBuilderService umbNavMenuBuilderService)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _apiContentRouteBuilder = apiContentRouteBuilder;
            _logger = logger;
            _umbNavMenuBuilderService = umbNavMenuBuilderService;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.Equals("AaronSadler.UmbNav");

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(IEnumerable<UmbNavItem>);

        public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Elements;

        public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(IEnumerable<UmbNavItem>);

        public override object? ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object? inter,
            bool preview)
        {
            if (inter is null)
            {
                _logger.Warning("No intermediate value found for property {PropertyAlias}.", propertyType.Alias);
                return Enumerable.Empty<UmbNavItem>();
            }

            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<UmbNavItem>>(inter.ToString() ?? string.Empty);
                if (items == null)
                {
                    _logger.Warning("Failed to deserialize UmbNav items for property {PropertyAlias}.", propertyType.Alias);
                    return Enumerable.Empty<UmbNavItem>();
                }

                // Build the menu using the UmbNavMenuBuilderService
                var configuration = propertyType.DataType.ConfigurationAs<UmbNavConfiguration>();
                if (configuration != null)
                {
                    return _umbNavMenuBuilderService.BuildMenu(
                        items,
                        0,
                        configuration.RemoveNaviHideItems,
                        configuration.HideNoopener,
                        configuration.HideNoreferrer,
                        configuration.HideIncludeChildren,
                        configuration.AllowMenuItemDescriptions);
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error converting UmbNav intermediate value for property {PropertyAlias}.", propertyType.Alias);
                return Enumerable.Empty<UmbNavItem>();
            }
        }

        public object? ConvertIntermediateToDeliveryApiObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object? inter,
            bool preview,
            bool expanding)
        {
            if (inter is null)
            {
                _logger.Warning("No intermediate value found for Delivery API conversion on property {PropertyAlias}.", propertyType.Alias);
                return null;
            }

            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<UmbNavItem>>(inter.ToString() ?? string.Empty);
                if (items == null)
                {
                    _logger.Warning("Failed to deserialize UmbNav items for Delivery API on property {PropertyAlias}.", propertyType.Alias);
                    return null;
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error converting UmbNav intermediate value for Delivery API on property {PropertyAlias}.", propertyType.Alias);
                return null;
            }
        }
    }
    #else
    public class UmbNavValueConverter : PropertyValueConverterBase
    {


        private readonly IUmbNavMenuBuilderService _umbNavMenuBuilderService;
        private readonly ILogger _logger;

        private bool _removeNaviHideItems;
        private bool _removeNoopener;
        private bool _removeNoreferrer;
        private bool _removeIncludeChildNodes;
        private bool _allowMenuItemDescriptions;

        public UmbNavValueConverter(ILogger logger, IUmbNavMenuBuilderService umbNavMenuBuilderService)
        {
            _logger = logger;
            _umbNavMenuBuilderService = umbNavMenuBuilderService;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(UmbNavConstants.PropertyEditorAlias);
        }

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<UmbNavItem>);

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (inter == null)
            {
                return Enumerable.Empty<UmbNavItem>();
            }

            var configuration = propertyType.DataType.ConfigurationAs<UmbNavConfiguration>();

            if (configuration != null)
            {
                _removeNaviHideItems = configuration.RemoveNaviHideItems;
                _removeNoopener = configuration.HideNoopener;
                _removeNoreferrer = configuration.HideNoreferrer;
                _removeIncludeChildNodes = configuration.HideIncludeChildren;
                _allowMenuItemDescriptions = configuration.AllowMenuItemDescriptions;
            }

            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<UmbNavItem>>(inter.ToString());

                return _umbNavMenuBuilderService.BuildMenu(items, 0, _removeNaviHideItems, _removeNoopener, _removeNoreferrer, _removeIncludeChildNodes, _allowMenuItemDescriptions);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to convert UmbNav {ex}", ex);
            }

            return Enumerable.Empty<UmbNavItem>();
        }
    }
    #endif
}