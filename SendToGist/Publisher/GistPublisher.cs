using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace SendToGist.Publisher
{
    public class GistPublisher
    {
        public GistPublishResult Publish(GistPublishRequest gistRequest)
        {
            var json = gistRequest.Serialize();
            var postResult = PostJson(json);

            dynamic result = JsonConvert.DeserializeObject(postResult);

            return new GistPublishResult { Url = result.html_url };
        }

        private static string PostJson(string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.github.com/gists");
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
