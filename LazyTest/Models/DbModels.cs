using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Reflection.Metadata;

namespace LazyTest.Models
{
    public class LazyTestContext : DbContext
    {
        public DbSet<WebSite> TestSite { get; set; }
        public DbSet<TestUrl> TestResult { get; set; }

        public string DbPath { get; }

        public LazyTestContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "d:\\home\\site\\wwwroot\\content\\LazyTestSQLlite.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
      /*
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
      */
      protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source = ./Database/LazyTestSQLlite.db;Pooling=True;");
        }
    }
    public class WebSite
    {
        public int WebsiteId { get; set; }
        public string SitemapUrl { get; set; }
        public string? WebsiteGuid {get; set;}
        public DateTime Created { get; set; }
        public int TotalUrls { get; set; } 
        public int TestedUrls { get; set; }
        public bool IsTesting { get; set; }

        [NotMapped]
        public bool IsTestRunning
        {
            get
            {
                return IsTesting && (DateTime.UtcNow - Created).TotalDays <= 1;
            }
        }


    }

    public class TestUrl
    {
        public int TestUrlId { get; set; }
        public string Url { get; set; }
        public long?  ContentLength { get; set; }
        public int? DomCount { get; set; }
        public string StatusCode { get; set; }
        public int?  HttpStatusCode { get; set; }
        public double ResponseTime { get; set; }
        public int WebsiteId { get; set; }
        public WebSite Site { get; set; }

        [NotMapped]
        public LastTestItem? LastTest { get; set; }

    }
}
