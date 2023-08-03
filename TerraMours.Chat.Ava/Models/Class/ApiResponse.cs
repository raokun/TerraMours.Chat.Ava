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

    public class PagedRes<T> {
        public IEnumerable<T> Items { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PagedRes(IEnumerable<T> items, int total, int page, int pageSize) {
            Items = items;
            Total = total;
            Page = page;
            PageSize = pageSize;
        }
    }
    public class ChatConversationRes {
        /// <summary>
        /// 主键
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// 会话名称
        /// </summary>
        public string? ConversationName { get; set; }
    }

    public class LoginRes {

        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
