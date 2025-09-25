using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventCalendar.Application.Contracts.CivicPlus.Models
{
    public class EventsDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("startDate")]
        public string StartDate { get; set; }
        [JsonPropertyName("endDate")]
        public string EndDate { get; set; }
    }
}
