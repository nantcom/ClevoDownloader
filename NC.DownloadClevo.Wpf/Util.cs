using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NC.DownloadClevo
{
    public static class Util
    {
        private static string _AppDataFolder;

        /// <summary>
        /// App Data Folder
        /// </summary>
        public static string AppDataFolder
        {
            get
            {
                if (_AppDataFolder == null)
                {
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    _AppDataFolder = Path.Combine(appData, "downloadclevo");

                    Directory.CreateDirectory(_AppDataFolder);
                }

                return _AppDataFolder;
            }
        }

        /// <summary>
        /// Load a content of text file in app data folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<string> LoadString(string fileName)
        {
            var path = Path.Combine(Util.AppDataFolder, fileName);
            if (File.Exists(path))
            {
                return await File.ReadAllTextAsync(path);
            }

            return null;
        }

        /// <summary>
        /// Save a content of text file in app data folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Task SaveString(string fileName, string content)
        {
            var path = Path.Combine(Util.AppDataFolder, fileName);
            return File.WriteAllTextAsync(path, content);
        }

        public static HashSet<T> ToHashSet<T>( this IEnumerable<T> input)
        {
            var hs = new HashSet<T>();
            foreach (var item in input)
            {
                hs.Add(item);
            }

            return hs;
        }

        public static T CloneWithJson<T>( this T input )
        {
            var json = JsonConvert.SerializeObject(input);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string GetSHA256(this string rawData)
        {
            // Create a SHA256   
            using (var sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}
