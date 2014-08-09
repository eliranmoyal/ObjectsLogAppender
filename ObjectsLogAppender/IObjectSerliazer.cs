using System.Security.Cryptography.X509Certificates;

namespace ObjectsLogAppender
{
    public interface IObjectSerliazer
    {
        string SerializeObject(object obj);
    }
}