using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Vibra_DesktopApp.Singleton
{
    class ApiManager
    {
        public static ApiManager Instance { get; private set; }

        public static ApiManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ApiManager();
            }
            return Instance;
        }


        public async Task LoginAsync(string email, string password)
        {  
            using HttpClient client = new HttpClient();

            var payload = new { email = email, password = password };
            string json = JsonSerializer.Serialize(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("http://spotify_clone_api.test/api/login", content);

            //response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            LoginResponse login = JsonSerializer.Deserialize<LoginResponse>(result);

            MessageBox.Show(login.code.ToString());


            //if (((int)response.StatusCode) == 200)
            //{
            //    MessageBox.Show("Đăng nhập thành công");
            //}
            //else
            //{
            //    MessageBox.Show("Đăng nhập thất bại");
            //}

            //if (response.IsSuccessStatusCode)
            //{
            //    MessageBox.Show("Đăng nhập thành công");
            //}
            //else
            //{
            //    MessageBox.Show("Đăng nhập thất bại");
            //}
        }
    }

    public class LoginResponse
    {
        public int code { get; set; }
    }
}
