using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.Services
{
    public static class SessionManager
    {
        private static readonly string FolderPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "Vibra");

        private static readonly string FilePath =
            Path.Combine(FolderPath, "session.json");

        public static void SaveUser(User user)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            var session = new UserSession
            {
                id = user.id,
                name = user.name,
                email = user.email,
                token = user.token,
                avatar_path = user.avatar_path
            };

            string json =
                JsonSerializer.Serialize(session);

            File.WriteAllText(FilePath, json);
        }

        public static async Task<User?> LoadUserAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return null;

                string json = File.ReadAllText(FilePath);

                UserSession? session =
                    JsonSerializer.Deserialize<UserSession>(json);

                if (session == null)
                    return null;

                //var user = await ApiManager.GetInstance().LoginFromSession(session);

                User user = new User
                {
                    id = session.id,
                    name = session.name,
                    email = session.email,
                    token = session.token,
                    avatar_path = session.avatar_path
                };

                ApiManager.GetInstance().SetCurrentUser(user);

                return user;
            }
            catch
            {
                return null;
            }
        }

        public static void Clear()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}
