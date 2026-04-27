using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ConnectSphere.Tests.Helpers
{
    public class ApiClient
    {
        private readonly HttpClient _client;
        private string? _token;

        // Gateway URL
        private const string BaseUrl = "http://localhost:8080";

        public ApiClient()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        // Set JWT token for authorized requests
        public void SetToken(string token)
        {
            _token = token;
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // POST request
        public async Task<HttpResponseMessage> PostAsync(
            string endpoint, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");
            return await _client.PostAsync(endpoint, content);
        }

        // GET request
        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            return await _client.GetAsync(endpoint);
        }

        // PUT request
        public async Task<HttpResponseMessage> PutAsync(
            string endpoint, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");
            return await _client.PutAsync(endpoint, content);
        }

        // DELETE request
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            return await _client.DeleteAsync(endpoint);
        }

        // Deserialize response
        public async Task<T?> DeserializeResponse<T>(
            HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
