using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????? ???????
    /// English: Targeting filter model
    /// </summary>
    public class TargetingFilterDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("criteria")]
        public TargetingCriteriaDto Criteria { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???????? ???????
    /// English: Targeting criteria model
    /// </summary>
    public class TargetingCriteriaDto
    {
        [JsonProperty("follower_count")]
        public RangeFilter FollowerCount { get; set; }

        [JsonProperty("following_count")]
        public RangeFilter FollowingCount { get; set; }

        [JsonProperty("media_count")]
        public RangeFilter MediaCount { get; set; }

        [JsonProperty("engagement_rate")]
        public RangeFilter EngagementRate { get; set; }

        [JsonProperty("keywords")]
        public List<string> Keywords { get; set; }

        [JsonProperty("languages")]
        public List<string> Languages { get; set; }

        [JsonProperty("locations")]
        public List<string> Locations { get; set; }

        [JsonProperty("exclude_keywords")]
        public List<string> ExcludeKeywords { get; set; }

        [JsonProperty("include_private")]
        public bool IncludePrivate { get; set; }

        [JsonProperty("include_verified")]
        public bool IncludeVerified { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ??????
    /// English: Range filter model
    /// </summary>
    public class RangeFilter
    {
        [JsonProperty("min")]
        public int? Min { get; set; }

        [JsonProperty("max")]
        public int? Max { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ???????
    /// English: Targeting result model
    /// </summary>
    public class TargetingResultDto
    {
        [JsonProperty("filter_id")]
        public string FilterId { get; set; }

        [JsonProperty("matches")]
        public bool Matches { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("matched_criteria")]
        public List<string> MatchedCriteria { get; set; }

        [JsonProperty("failed_criteria")]
        public List<string> FailedCriteria { get; set; }
    }
}