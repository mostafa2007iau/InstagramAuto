using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ???? API
    /// English: API response model
    /// </summary>
    public class Response
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ?? ???????? ?????
    /// English: Generic API response model
    /// </summary>
    public class Response<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ????????? ???
    /// English: Paginated output model
    /// </summary>
    public class PaginatedOutput<T>
    {
        [JsonProperty("items")]
        public List<T> Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???????? ?????????
    /// English: Pagination metadata model
    /// </summary>
    public class PaginationMeta
    {
        [JsonProperty("next_cursor")]
        public string NextCursor { get; set; }

        [JsonProperty("prev_cursor")]
        public string PrevCursor { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("count_returned")]
        public int CountReturned { get; set; }

        [JsonProperty("page_size")]
        public int PageSize { get; set; }
    }
}