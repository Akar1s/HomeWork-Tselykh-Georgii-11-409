using MiniHttpServer.Core.Attributes;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class HomeEndpoint
    {
        [HttpGet]
        public string Index()
        {
            Console.WriteLine("HomeEndpoint: Serving index.html");
            return "Public/index.html";
        }
    }
}
