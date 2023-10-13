
using System;
using System.Collections.Generic;
using Cms.Integrations.Magento.Content.Category;
using Newtonsoft.Json;

namespace Cms.Integrations.Magento.Client.Responses;

public class CategoriesResponse
{
    [JsonProperty("items")]
    public List<CategoryExternal> Items { get; set; }

    [JsonProperty("search_criteria")]
    public SearchCriteria SearchCriteria { get; set; }

    [JsonProperty("total_count")]
    public int TotalCount { get; set; }
}

public class SearchCriteria
{
    [JsonProperty("filter_groups")]
    public List<object> FilterGroups { get; set; }

    [JsonProperty("page_size")]
    public int PageSize { get; set; }
}


