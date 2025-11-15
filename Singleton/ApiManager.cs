using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Vibra_DesktopApp.Singleton
{
    class ApiManager
    {
        public static ApiManager? Instance { get; private set; }

        private const string baseUrl = "http://spotify_clone_api.test/api/";


        private UserData? currentUser;

        public static ApiManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ApiManager();
            }
            return Instance;
        }


        public async Task LoginAsync(string email, string password, Action OpenApp)
        {  
            using HttpClient client = new HttpClient();

            var payload = new { email = email, password = password };
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(baseUrl + "login", content);


            string result = await response.Content.ReadAsStringAsync();
            LoginResponse? login = JsonSerializer.Deserialize<LoginResponse>(result);
            if (login?.code == 200)
            {
                MessageBox.Show("Đăng nhập thành công" + login.data.name);
                currentUser = login?.data;
                OpenApp();

            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại");
            }
        }

        public async Task SignUpAsync(string email, string password, Action CloseSignUp)
        {
            using HttpClient client = new HttpClient();

            var payload = new { email = email, password = password };
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(baseUrl + "signup", content);


            string result = await response.Content.ReadAsStringAsync();

            MessageBox.Show(result);

            LoginResponse? login = JsonSerializer.Deserialize<LoginResponse>(result);


            if (login?.code == 200)
            {
                MessageBox.Show("Chúng tôi vừa gửi tới email của bạn một mã xác thực, vui lòng kiểm tra email để xác thực tài khoản hiện tại!");
            }
            else
            {
                MessageBox.Show("Đăng ký thất bại");
            }

            CloseSignUp();
        }

        public UserData? GetCurrentUser()
        {
            return currentUser;
        }
    }
    public class UserData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public DateTime birth { get; set; }
        public DateTime email_verified_at { get; set; }
        public string avatar { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string token { get; set; }
        public string avatar_path { get; set; }
    }

    public class LoginResponse
    {
        public int code { get; set; }
        public UserData data { get; set; }
        public string message { get; set; }
    }
}
