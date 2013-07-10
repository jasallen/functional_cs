using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Helpers;

namespace functional_cs
{
    class Program
    {
        private const int PageSize = 10;

        static void Main(string[] args)
        {
            // Set up Stream
            var http = new HttpClient();
            var url = "http://api.thriftdb.com/api.hnsearch.com/items/_search?start={0}&limit={1}&q=nsa";

            var s = GetHNhits(http, url, 0).ToObservable(NewThreadScheduler.Default)
                //.Take(40)
                .GroupBy(_ => (int)_.item.num_comments > 5)
                .SelectMany(grp => grp
                    //.Average(_ => _.item.points)
                    .Scan(new { count = 0, points = 0d }, (working, current) => new { count = working.count + 1, points = ((double)current.item.points - working.points) / (working.count + 1) + working.points })
                    .Select(_ => new {num_comments = grp.Key, points = (int)_.points}))                    
                .Subscribe(Console.WriteLine);
            
            Console.ReadLine();
            Console.WriteLine("Cancelling...");
            s.Dispose();

            Console.ReadLine();
        }

        private static IEnumerable<dynamic> GetHNhits(HttpClient http, string urltemplate, int nextResult)
        {
            var url = string.Format(urltemplate, nextResult, PageSize);
            var res = http.GetStringAsync(url).Result;
            dynamic hn = Newtonsoft.Json.Linq.JObject.Parse(res);

            foreach (dynamic hit in hn.results)
            {                
                yield return hit;
            }
            
            Thread.Sleep(2000);

            foreach (var subhit in GetHNhits(http, urltemplate, nextResult + PageSize))
            {
                yield return subhit;
            }            
        }
    }
}
