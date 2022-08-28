using Newtonsoft.Json;
using PU_Test.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PU_Test.Common
{
        public static class ServerInfoGetter
        {


            public static async Task<string> HttpGet(string url, Dictionary<string, string> dic = null)

            {


                HttpResponseMessage response;
                //参数添加
                StringBuilder builder = new StringBuilder();
                builder.Append(url);
                if (dic != null)
                {

                    if (dic.Count > 0)
                    {
                        builder.Append("?");
                        int i = 0;
                        foreach (var item in dic)
                        {
                            if (i > 0)
                                builder.Append("&");
                            builder.AppendFormat("{0}={1}", item.Key, item.Value);
                            i++;
                        }
                    }
                }

                try
                {

                    response = await new HttpClient().GetAsync(new Uri(builder.ToString()));
                }
                catch (Exception e)
                {
                    return null;
                }
                string result = await response.Content.ReadAsStringAsync();

                return result;
            }

            class REPDT
            {
                public class Status
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public int playerCount { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string version { get; set; }
                }

                public class Root
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public int retcode { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public Status status { get; set; }
                }

            }


            public static async Task<ServerInfo> GetAsync(string ip)
            {

                var Url = $"https://{ip}/status/server";
                Stopwatch sw = new Stopwatch();
                sw.Start();

                var r = await HttpGet(url: Url);

                sw.Stop();
                REPDT.Root dt;
                try
                {
                    dt = JsonConvert.DeserializeObject<REPDT.Root>(r);

                }
                catch
                {
                    dt = new REPDT.Root();
                }

                var SI= new ServerInfo();
                SI.players = dt.status.playerCount.ToString();
                SI.ver = dt.status.version;
                SI.timeout = sw.ElapsedMilliseconds.ToString();


                return SI;



            }

        }
    
}
