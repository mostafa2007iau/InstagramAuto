using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ????????? ??
    /// English: Queue pagination
    /// </summary>
    public class PaginatedJobs
    {
        [JsonProperty("items")]
        public List<JobSummaryDto> Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }
}