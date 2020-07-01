using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
        public string sha {get;set;}
        /// <summary>
        /// Commit time stamps
        /// </summary>
        public CommitEntry commit {get;set;}

        /// <summary>
        /// Fetch info from github
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<GitHubCommit> Fetch(string fileName)
        {
            var basePath = "https://api.github.com/repos/hargrave81/aquamonitor/commits?path={1}&page=1&per_page=1";
            using var httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync(new Uri(string.Format(basePath, fileName)));
            return System.Text.Json.JsonSerializer.Deserialize<GitHubCommit>(result);
        }

        /// <summary>
        /// Download file from github
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="storedFile"></param>
        /// <returns></returns>
        public static async Task Download(string fileName, string storedFile)
        {
            var basePath = "https://github.com/hargrave81/Aquamonitor/raw/master/{1}";
            using var httpClient = new HttpClient();
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
        public Person author {get;set;}

        /// <summary>
        /// committer
        /// </summary>
        public Person committer {get;set;}
    }

    /// <summary>
    /// Person with date
    /// </summary>
    public class Person
    {
        /// <summary>
        /// name
        /// </summary>
        public string name {get;set;}
        /// <summary>
        /// email
        /// </summary>
        public string email {get;set;}
        /// <summary>
        /// date of change
        /// </summary>
        public DateTime date {get;set;}
    }
}
