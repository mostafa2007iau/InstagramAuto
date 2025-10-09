using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ??????? ?????
    /// English: User profile model
    /// </summary>
    public class UserProfileDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("biography")]
        public string Biography { get; set; }

        [JsonProperty("profile_pic_url")]
        public string ProfilePicUrl { get; set; }

        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }

        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }

        [JsonProperty("follower_count")]
        public int FollowerCount { get; set; }

        [JsonProperty("following_count")]
        public int FollowingCount { get; set; }

        [JsonProperty("media_count")]
        public int MediaCount { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ?? ?????
    /// English: User relationship model
    /// </summary>
    public class UserRelationship
    {
        [JsonProperty("following")]
        public bool Following { get; set; }

        [JsonProperty("followed_by")]
        public bool FollowedBy { get; set; }

        [JsonProperty("blocking")]
        public bool Blocking { get; set; }

        [JsonProperty("muting")]
        public bool Muting { get; set; }

        [JsonProperty("is_restricted")]
        public bool IsRestricted { get; set; }
    }

    /// <summary>
    /// Persian: ???? ?????? ????
    /// English: Follow action response
    /// </summary>
    public class FollowResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}