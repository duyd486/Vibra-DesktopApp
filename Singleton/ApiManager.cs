using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using Vibra_DesktopApp.Models;

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
                MessageBox.Show("Đăng nhập thành công" + res.data?.name);
                currentUser = res?.data;
                return true;
            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại");
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

            MessageBox.Show(result);

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

        #endregion

        #region HomePage

        public async Task<List<Song>> GetListSong()
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + currentUser?.token);
            HttpResponseMessage response = await client.GetAsync(baseUrl + "home/list-song");

            string result = await response.Content.ReadAsStringAsync();

            //ListSongResponse? res = JsonSerializer.Deserialize<ListSongResponse>(result);

            ResponseBase<List<Song>>? res = JsonSerializer.Deserialize<ResponseBase<List<Song>>>(result);
            //if (res?.code == 200)
            //{
            //    MessageBox.Show("Lấy danh sách nhạc thành công");
            //}
            //else
            //{
            //    MessageBox.Show("Call api thất bại");
            //}
            return res?.data ?? new List<Song>();
        }



        #endregion


        public User? GetCurrentUser()
        {
            return currentUser;
        }
    }

    public class ResponseBase<T>
    {
        public int? code { get; set; }
        public T? data { get; set; }
        public string? message { get; set; }
    }
}
