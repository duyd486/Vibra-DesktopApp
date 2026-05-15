using Duende.IdentityModel.OidcClient;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Services;

namespace Vibra_DesktopApp.Singleton
{
    class ApiManager
    {
        public static ApiManager? Instance { get; private set; }

        private const string baseUrl = "http://spotify_clone_api.test/api/";

        private readonly HttpClient client = new();

        private User? currentUser;

        public static ApiManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ApiManager();
            }

            return Instance;
        }

        public string GetOrigin()
        {
            return new Uri(baseUrl, UriKind.Absolute).GetLeftPart(UriPartial.Authority);
        }

        public string? ToAbsoluteUrl(string? urlOrPath)
        {
            if (string.IsNullOrWhiteSpace(urlOrPath))
                return null;

            urlOrPath = urlOrPath.Trim();

            if (Uri.TryCreate(urlOrPath, UriKind.Absolute, out _))
                return urlOrPath;

            var origin = GetOrigin();

            if (urlOrPath.StartsWith("/", StringComparison.Ordinal))
                return origin + urlOrPath;

            return origin + "/" + urlOrPath;
        }

        #region Login & SignUp

        public async Task<bool> LoginAsync(string email, string password)
        {
            var payload = new { email = email, password = password };
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(baseUrl + "login", content);

            string result = await response.Content.ReadAsStringAsync();

            ResponseBase<User>? res = JsonSerializer.Deserialize<ResponseBase<User>>(result);

            if (res?.code == 200)
            {
                client.DefaultRequestHeaders.Authorization = null;
                MessageBox.Show("Đăng nhập thành công" + res.data?.name);
                currentUser = res?.data;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + currentUser?.token);
                SessionManager.SaveUser(currentUser!);
                return true;
            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại");
                return false;
            }
        }

        public async Task<bool> LoginWithGoogleAsync()
        {
            try
            {
                string[] scopes =
                {
                    "openid",
                    "email",
                    "profile"
                };

                var secrets = new ClientSecrets
                {
                    // Cua tao
                    ClientId = "231981628500-9iltqh5g94m3hbddt86amourhvlbd3p0.apps.googleusercontent.com",
                    ClientSecret = "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"
                };

                var dataStore = new FileDataStore("GoogleLogin");

                await dataStore.ClearAsync();

                var credential =
                    await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        secrets,
                        scopes,
                        "user",
                        CancellationToken.None,
                        dataStore
                    );

                if (credential.Token.IsExpired(credential.Flow.Clock))
                {
                    await credential.RefreshTokenAsync(CancellationToken.None);
                }

                string accessToken = credential.Token.AccessToken;

                using HttpClient googleClient = new();

                googleClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var googleResponse =
                    await googleClient.GetAsync(
                        "https://www.googleapis.com/oauth2/v2/userinfo");

                string googleJson =
                    await googleResponse.Content.ReadAsStringAsync();

                //MessageBox.Show(googleJson);

                using JsonDocument doc =
                    JsonDocument.Parse(googleJson);

                string email =
                    doc.RootElement.GetProperty("email").GetString()!;

                //MessageBox.Show(email);

                string deviceToken = Environment.MachineName;

                HttpResponseMessage response =
                    await client.GetAsync(
                        baseUrl +
                        $"firebase/auth?email={Uri.EscapeDataString(email)}&device_token={Uri.EscapeDataString(deviceToken)}");

                string result =
                    await response.Content.ReadAsStringAsync();

                //MessageBox.Show(result);

                ResponseBase<User>? res =
                    JsonSerializer.Deserialize<ResponseBase<User>>(result);

                if (res?.code == 200)
                {
                    currentUser = res.data;

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            currentUser?.token);

                    SessionManager.SaveUser(currentUser!);

                    //MessageBox.Show("Đăng nhập Google thành công");

                    return true;
                }

                MessageBox.Show("Đăng nhập thất bại");

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<bool> SignUpAsync(string email, string password)
        {
            var payload = new { email = email, password = password };
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(baseUrl + "signup", content);

            string result = await response.Content.ReadAsStringAsync();

            //MessageBox.Show(result);

            ResponseBase<User>? res = JsonSerializer.Deserialize<ResponseBase<User>>(result);

            if (res?.code == 200)
            {
                MessageBox.Show("Chúng tôi vừa gửi tới email của bạn một mã xác thực, vui lòng kiểm tra email để xác thực tài khoản hiện tại!");
                return true;
            }
            else
            {
                MessageBox.Show("Đăng ký thất bại");
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = null;
                currentUser = null;
                SessionManager.Clear();
                return true;
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message); return false;
            }
        }

        #endregion

        public async Task<T> HttpGetAsync<T>(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(baseUrl + url).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                ResponseBase<T>? res = JsonSerializer.Deserialize<ResponseBase<T>>(result);

                return res!.data;
            }
            catch (Exception ex)
            {
                if (Application.Current?.Dispatcher?.CheckAccess() == true)
                {
                    MessageBox.Show(ex.Message);
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(() => MessageBox.Show(ex.Message));
                }
                return default!;
            }
        }

        public async Task HttpGetNoDataAsync(string url)
        {
            HttpResponseMessage response = await client.GetAsync(baseUrl + url);
            response.EnsureSuccessStatusCode();
        }

        public async Task HttpPostNoDataAsync(string url, object? payload = null)
        {
            var json = JsonSerializer.Serialize(payload ?? new { });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl + url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<T> HttpPostFormAsync<T>(string url, MultipartFormDataContent form)
        {
            try
            {
                var response = await client.PostAsync(baseUrl + url, form).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var res = JsonSerializer.Deserialize<ResponseBase<T>>(result);
                return res!.data;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
                return default!;
            }
        }

        public void SetCurrentUser(User? user)
        {
            if (user is null)
                return;

            var token = currentUser?.token;
            currentUser = user;


            if (string.IsNullOrWhiteSpace(currentUser.token) && !string.IsNullOrWhiteSpace(token))
                currentUser.token = token;

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    currentUser?.token);
        }

        public User? GetCurrentUser()
        {
            return currentUser;
        }
    }

    public class ResponseBase<T>
    {
        public int? code { get; set; }
        public T data { get; set; } = default!;
        public string? message { get; set; }
    }
}
