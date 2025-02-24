using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IOt_TEST
{
    public class ThingsBoardClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private string _token;

        public ThingsBoardClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _client = new HttpClient();
        }

        public async Task AuthenticateAsync(string username, string password)
        {
            try
            {
                var authData = new { username, password };
                var json = JsonConvert.SerializeObject(authData);

                var response = await _client.PostAsync(
                    $"{_baseUrl}/api/auth/login",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TokenResponse>(content);

                _token = result.Token;
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication failed: {ex.Message}");
            }
        }

        public async Task<double> GetLatestTelemetryValueAsync(string deviceId, string key)
        {
            try
            {
                var endTs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var startTs = endTs - 7000; // последние 7 секунд 

                var response = await _client.GetAsync(
                    $"{_baseUrl}/api/plugins/telemetry/DEVICE/{deviceId}/values/timeseries" +
                    $"?keys={key}&startTs={startTs}&endTs={endTs}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var telemetry = JsonConvert.DeserializeObject<Dictionary<string, List<TelemetryData>>>(content);

                if (telemetry != null && telemetry.ContainsKey(key) && telemetry[key].Count > 0)
                {
                    return double.Parse(telemetry[key][0].Value);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get telemetry: {ex.Message}");
            }
        }
    }

    public class TokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class TelemetryData
    {
        [JsonProperty("ts")]
        public long Ts { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}