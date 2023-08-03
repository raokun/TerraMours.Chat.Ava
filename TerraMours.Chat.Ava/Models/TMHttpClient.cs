using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TerraMours.Chat.Ava.Models.Class;
using static System.Net.WebRequestMethods;

namespace TerraMours.Chat.Ava.Models {
    public  class TMHttpClient {
        public HttpClient httpClient;

        public TMHttpClient() {
            httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(VMLocator.AppToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", VMLocator.AppToken);
            }
        }
        public  async Task<ApiResponse<TResponse>> PostAsync<TResponse>(string url, object dto) {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json");
            var requestUrl = AppSettings.Instance.BaseUrl + url;
            var response = await httpClient.PostAsync(requestUrl, jsonContent);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) {
                var responseData = JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseContent, new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                });
                return responseData;
            }
            else {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent);
                return new ApiResponse<TResponse> {
                    StatusCode = (int)response.StatusCode,
                    Message = null,
                    Errors = errorResponse.Errors
                };
            }
        }


        public  async Task<TResponse> GetAsync<TResponse>(string url, object dto) {
            var queryString = string.Join("&",
                dto.GetType().GetProperties()
                    .Select(property => $"{property.Name}={HttpUtility.UrlEncode(property.GetValue(dto)?.ToString())}"));

            var requestUrl = $"{AppSettings.Instance.BaseUrl}{url}?{queryString}";

            var response = await httpClient.GetAsync(requestUrl);

            return await JsonSerializer.DeserializeAsync<TResponse>(await response.Content.ReadAsStreamAsync());
        }
    }
}
