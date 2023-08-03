using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TerraMours.Chat.Ava.Models.Class {
    public class ApiResponse<T> {
        [JsonPropertyName("code")]
        public int StatusCode { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("data")]
        public T? Data { get; set; }
        [JsonPropertyName("errors")]
        public IDictionary<string, IList<string>> Errors { get; set; }
    }
    public class LoginRes {

        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
