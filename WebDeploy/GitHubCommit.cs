using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebDeploy
{
    /// <summary>
    /// Helper to read github commits
    /// </summary>
    public class GitHubCommit
    {
        /// <summary>
        /// Sha
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("sha")]
        public string Sha {get;set;}
        /// <summary>
        /// Commit time stamps
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("commit")]
        public CommitEntry Commit {get;set;}

        /// <summary>
        /// Fetch info from github
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<GitHubCommit> Fetch(string fileName)
        {
            var basePath = "https://api.github.com/repos/hargrave81/aquamonitor/commits?path={0}&page=1&per_page=1";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
            var result = await httpClient.GetStringAsync(new Uri(string.Format(basePath, fileName)));
            return System.Text.Json.JsonSerializer.Deserialize<GitHubCommit[]>(result).First();
        }

        /// <summary>
        /// Download file from github
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="storedFile"></param>
        /// <returns></returns>
        public static async Task Download(string fileName, string storedFile)
        {
            var basePath = "https://github.com/hargrave81/Aquamonitor/raw/master/{0}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
            var result = await httpClient.GetByteArrayAsync(new Uri(string.Format(basePath, fileName)));
            await System.IO.File.WriteAllBytesAsync(storedFile, result);
        }
    }

    /// <summary>
    /// Commit time stamp entry
    /// </summary>
    public class CommitEntry
    {
        /// <summary>
        /// author
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("author")]
        public Person Author {get;set;}

        /// <summary>
        /// committer
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("committer")]
        public Person Committer {get;set;}
    }

    /// <summary>
    /// Person with date
    /// </summary>
    public class Person
    {
        /// <summary>
        /// name
        /// </summary>
        
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name {get;set;}
        /// <summary>
        /// email
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string Email {get;set;}
        /// <summary>
        /// date of change
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public DateTime Date {get;set;}
    }
}
