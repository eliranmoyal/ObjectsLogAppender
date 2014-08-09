using System.Web.Script.Serialization;

namespace ObjectsLogAppender
{
    public class JsonSerliazer : IObjectSerliazer
    {
        private JavaScriptSerializer _javaScriptSerializer;

        public JsonSerliazer()
        {
            _javaScriptSerializer = new JavaScriptSerializer();
        }

        public string SerializeObject(object obj)
        {
            return _javaScriptSerializer.Serialize(obj);
        }
    }
}