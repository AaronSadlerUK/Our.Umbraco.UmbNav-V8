using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UmbNav.Core.Enums;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace UmbNav.Core.Models
{
    public class UmbNavItem
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("udi")]
        public GuidUdi Udi { get; set; }

        [JsonProperty("key")]
        public Guid Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("menuitemdescription")]
        public string Description { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("noopener")]
        public string Noopener { get; set; }

        [JsonProperty("noreferrer")]
        public string Noreferrer { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }

        [JsonProperty("children")]
        public IEnumerable<UmbNavItem> Children { get; set; }

        [JsonIgnore]
        public UmbNavItem Parent { get; set; }

        [JsonIgnore]
        public IPublishedContent Content { get; set; }

        [JsonIgnore]
        public UmbNavItemType ItemType { get; set; }

        [JsonIgnore]
        public int Level { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }

        [JsonProperty("collapsed")]
        internal bool Collapsed { get; set; }

        [JsonProperty("hideLoggedIn")]
        internal bool HideLoggedIn { get; set; }

        [JsonProperty("hideLoggedOut")]
        internal bool HideLoggedOut { get; set; }

        [JsonProperty("includeChildNodes")]
        internal bool IncludeChildNodes { get; set; }

        [JsonProperty("customClasses")]
        public string CustomClasses { get; set; }

        [JsonProperty("image")]
        internal ImageItem[] ImageArray { get; set; }

        [JsonIgnore]
        public IPublishedContent Image { get; set; }

        [JsonProperty("itemType")] internal string MenuItemType { get; set; } = "link";

        [JsonIgnore]
        [Obsolete("If you need to check ancestors, use the IsActive() extension instead")]
        public bool IsActive { get; set; }

        [JsonProperty("displayAsLabel")]
        public bool DisplayAsLabel { get; set; }
    }
}
