using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ?????? ????????
    /// English: Schedule model
    /// </summary>
    public class ScheduleDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("cron_expression")]
        public string CronExpression { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("actions")]
        public List<ScheduledActionDto> Actions { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ???????? ???
    /// English: Scheduled action model
    /// </summary>
    public class ScheduledActionDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("schedule_id")]
        public string ScheduleId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("target_id")]
        public string TargetId { get; set; }

        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; }

        [JsonProperty("next_run")]
        public DateTimeOffset? NextRun { get; set; }

        [JsonProperty("last_run")]
        public DateTimeOffset? LastRun { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Persian: ??? ?????? ?????????
    /// English: Automation plan model
    /// </summary>
    public class AutomationPlanDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("trigger")]
        public AutomationTriggerDto Trigger { get; set; }

        [JsonProperty("actions")]
        public List<AutomationActionDto> Actions { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ?????????
    /// English: Automation trigger model
    /// </summary>
    public class AutomationTriggerDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("conditions")]
        public Dictionary<string, object> Conditions { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ?????????
    /// English: Automation action model
    /// </summary>
    public class AutomationActionDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; }

        [JsonProperty("delay")]
        public TimeSpan? Delay { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }
    }
}