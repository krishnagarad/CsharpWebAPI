using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventCalendar.Application.Contracts.CivicPlus.Models
{
    public class EventResponseDto
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("items")]
        public List<EventsDto> Items { get; set; } = new();
    }
}
