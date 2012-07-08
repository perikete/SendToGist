using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SendToGist.Publisher
{
    public class GistPublishRequest 
    {
        public string Description { get; set; }
        public string Code { get; set; }
        public bool Public { get { return true; } }
        public string File { get; set; }

        public GistPublishRequest(string description, string code, string file)
        {
            Description = description;
            Code = code;
            File = file;
        }
      
        public string Serialize()
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = Formatting.Indented;

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("public");
                jsonWriter.WriteValue(true);
                
                jsonWriter.WritePropertyName("files");

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName(File);

                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("content");
                jsonWriter.WriteValue(Code);
                jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();
            }

            return sb.ToString();}
    }
}
