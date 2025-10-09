using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ????? ?????
    /// English: Job summary model
    /// </summary>
    public class JobSummaryDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset Created_at { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset Updated_at { get; set; }

        [JsonProperty("account_id")]
        public string Account_id { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    /// <summary>
    /// Persian: ?????? ?????
    /// English: Job detail model
    /// </summary>
    public class JobDetailDto : JobSummaryDto
    {
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("result")]
        public object Result { get; set; }

        [JsonProperty("retry_count")]
        public int Retry_count { get; set; }

        [JsonProperty("max_retries")]
        public int Max_retries { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????????? ?????
    /// English: Paginated jobs model
    /// </summary>
    public class PaginatedJobsDto
    {
        [JsonProperty("items")]
        public JobSummaryDto[] Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }
}