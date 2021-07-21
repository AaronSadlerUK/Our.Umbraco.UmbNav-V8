﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UmbNavV8.Core.Enums;
using UmbNavV8.Core.Models;
using UmbNavV8.Core.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;

namespace UmbNavV8.Core.ValueConverters
{
    public class UmbNavV8ValueConverter : PropertyValueConverterBase
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly ILogger _logger;

        private bool _removeNaviHideItems;
        private bool _removeNoopener;
        private bool _removeNoreferrer;

        public UmbNavV8ValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, ILogger logger)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _logger = logger;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditorAlias);
        }

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<UmbNavItem>);

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            if (inter == null)
            {
                return Enumerable.Empty<UmbNavItem>();
            }

            var configuration = propertyType.DataType.ConfigurationAs<UmbNavV8Configuration>();

            if (configuration != null)
            {
                _removeNaviHideItems = configuration.RemoveNaviHideItems;
                _removeNoopener = configuration.HideNoopener;
                _removeNoreferrer = configuration.HideNoreferrer;
            }

            try
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<UmbNavItem>>(inter.ToString());

                return BuildMenu(items);
            }
            catch (Exception ex)
            {
                _logger.Error<UmbNavV8ValueConverter>("Failed to convert UmbNav {ex}", ex);
            }

            return Enumerable.Empty<UmbNavItem>();
        }

        private IEnumerable<UmbNavItem> BuildMenu(IEnumerable<UmbNavItem> items, int level = 0)
        {
            var isLoggedIn = Current.UmbracoHelper.MemberIsLoggedOn();
            items = items.ToList();

            foreach (var item in items)
            {
                item.Level = level;

                if (item.Id > 0)
                {
                    IPublishedContent umbracoContent;
                    string currentCulture;

                    if (item.Udi != null)
                    {
                        currentCulture = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(item.Udi)?.GetCultureFromDomains();
                        umbracoContent = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(item.Udi);
                    }
                    else
                    {
                        currentCulture = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(item.Id)?.GetCultureFromDomains();
                        umbracoContent = _publishedSnapshotAccessor.PublishedSnapshot.Content.GetById(item.Id);
                    }

                    if (umbracoContent != null)
                    {
                        item.ItemType = UmbNavItemType.Content;

                        if (_removeNaviHideItems && !umbracoContent.IsVisible())
                        {
                            continue;
                        }

                        if (item.HideLoggedIn && isLoggedIn)
                        {
                            continue;
                        }

                        if (item.HideLoggedOut && !isLoggedIn)
                        {
                            continue;
                        }

                        if (_removeNoopener)
                        {
                            item.Noopener = null;
                        }

                        if (_removeNoreferrer)
                        {
                            item.Noreferrer = null;
                        }

                        if (string.IsNullOrWhiteSpace(item.Title))
                        {
                            item.Title = umbracoContent.Name(currentCulture);
                        }
                    }
                }

                if (item.Children.Any())
                {
                    var childLevel = item.Level + 1;

                    BuildMenu(item.Children, childLevel);
                }
            }

            items = items.Where(x => x.ItemType == UmbNavItemType.Link);

            return items;
        }
    }
}