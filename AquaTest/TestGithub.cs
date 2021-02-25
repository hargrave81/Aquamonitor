using System;
using System.Threading.Tasks;
using WebDeploy;
using Xunit;

namespace AquaTest
{
    public class TestGithub
    {
        [Fact]
        public async Task TestGitMeta()
        {
            var ghc = new GitHubCommit();
            var am32 = await GitHubCommit.Fetch("AquaMonitor32.zip");
            
            if (am32.Commit.Author.Date > DateTime.Now.ToUniversalTime().AddMinutes(-2))
            {
                Assert.True(false);
            }
            Assert.True(true);
        }
    }
}
