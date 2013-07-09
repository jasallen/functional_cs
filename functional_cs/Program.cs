using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var hits = GetHNhits(http, url, 0);

            var map = hits.Select(_ => (int)_.item.points);

            var reduction = map.Take(20).Aggregate(new{count=0,points=0}, (working , current) => new{count=working.count+1, points=(current - working.points) / (working.count+1) + working.points } );

            Console.WriteLine(reduction);
            Console.ReadLine();

            //var titles = hits.Select(_ => _.item.title == null ? _.item.text : _.item.title);
            //foreach (var title in titles)
            //{
            //    Console.WriteLine(title);
            //}

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

            foreach (var subhit in GetHNhits(http, urltemplate, nextResult + PageSize))
            {
                yield return subhit;
            }            
        }
    }
}
