using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace PSVR2Gamepad.Update
{
    public static class UpdateChecker
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/BlueberryWolf/PSVR2-Gamepad/releases/latest";
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private record GitHubRelease(string? tag_name);

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            var assemblyVersion = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PSVR2-Gamepad", assemblyVersion));
            return client;
        }

        public static async Task<(bool IsUpdateAvailable, string? LatestVersionTag)> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (currentVersion == null) return (false, null);

                var latestRelease = await _httpClient.GetFromJsonAsync<GitHubRelease>(GitHubApiUrl, cancellationToken);

                if (string.IsNullOrEmpty(latestRelease?.tag_name))
                {
                    return (false, null);
                }

                var tagName = latestRelease.tag_name.AsSpan().TrimStart('v');
                if (Version.TryParse(tagName, out var latestVersion))
                {
                    if (latestVersion > currentVersion)
                    {
                        return (true, latestRelease.tag_name);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check failed (network): {ex.Message}");
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check failed (parsing): {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                // This is expected if the token is cancelled, so we can ignore it.
            }

            return (false, null);
        }
    }
}