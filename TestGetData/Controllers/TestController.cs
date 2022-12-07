using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using InfluxDB.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using InfluxDB.Client.Core;
using System.Linq;

namespace TestGetData.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private HttpClient _client;
        public TestController(HttpClient client)
        {
            _client = client;
        }

        [HttpGet(Name = "GetOHLCData")]
        public async Task<Root> Get(string path)
        {
            Root root = new Root();
            List<Ohlcdata> ohlcItems = new List<Ohlcdata>();
            HttpResponseMessage response = await _client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                root = await response.Content.ReadFromJsonAsync<Root>();



                foreach (var items in root.result.XXBTZUSD)
                {
                    Ohlcdata data = new Ohlcdata
                    {
                        Time = UnixTimeStampToDateTime(Convert.ToDouble(items.First().ToString())),
                        Open = Convert.ToString(items[items.Count() - 7].ToString()),
                        High = Convert.ToString(items[items.Count() - 6].ToString()),
                        Low = Convert.ToString(items[items.Count() - 5].ToString()),
                        Close = Convert.ToString(items[items.Count() - 4].ToString()),
                        Vwap = Convert.ToString(items[items.Count() - 3].ToString()),
                        Volume = Convert.ToString(items[items.Count() - 2].ToString()),
                        Count = Convert.ToInt32(items[items.Count() - 1].ToString())
                    };

                    ohlcItems.Add(data);
                }

                using var client = new InfluxDBClient("http://localhost:8086",
                    "Xs4QnJl9922IxZiHZP6VC8V1RrrqFyP2NnizUwPeT2N4mtzLVvpSTVVGUKqFvmBQcli8yUuC4rPLB-_7d1W_kg==");


                using (var writeApi = client.GetWriteApi())
                {
                    writeApi.WriteMeasurements(ohlcItems, WritePrecision.Ns, "Bucket1904", "OlegTest");


                    // write by PointData 

                    // var point = PointData.Measurement("temperature")
                    //.Tag("location", "west")
                    //.Field("value", 55D)
                    //.Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

                    // writeApi.WritePoint(point, "Bucket1904", "OlegTest");
                }
            }


            return root;

        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public class Result
        {
            public int last { get; set; }
            public List<List<object>> XXBTZUSD { get; set; }
        }

        public class Root
        {
            public Result result { get; set; }
            public List<string> error { get; set; }
        }

        [Measurement("ohlcdata")]
        private class Ohlcdata
        {
            //[Column("location", IsTag = true)] public string Location { get; set; }
            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
            [Column("open", IsTag = true)] public string Open { get; set; }
            //[Column("open")] public string Open { get; set; }
            [Column("high")] public string High { get; set; }
            [Column("low")] public string Low { get; set; }
            [Column("close")] public string Close { get; set; }
            [Column("vwap")] public string Vwap { get; set; }
            [Column("Volume")] public string Volume { get; set; }
            [Column("Count")] public int Count { get; set; }
        }
    }
}