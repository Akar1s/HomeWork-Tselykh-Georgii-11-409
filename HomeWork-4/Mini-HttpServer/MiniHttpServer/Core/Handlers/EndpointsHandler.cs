using System.Text;
using System.Reflection;
using MiniHttpServer.Core.Abstracts;
using MiniHttpServer.Core.Attributes;
using System.Net;

namespace MiniHttpServer.Core.Handlers
{
    internal class EndpointsHandler : Handler
    {
        public override void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            Console.WriteLine($"INCOMING REQUEST: {request.HttpMethod} {request.Url.AbsolutePath}");

            try
            {
                var pathParts = request.Url?.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts is not { Length: > 0 })
                {
                    Successor?.HandleRequest(context);
                    return;
                }

                var assembly = Assembly.GetExecutingAssembly();
                var endpointTypes = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                    .ToList();

                var endpointName = pathParts[0];
                var targetEndpoint = endpointTypes.FirstOrDefault(et =>
                    et.Name.Replace("Endpoint", "", StringComparison.OrdinalIgnoreCase)
                      .Equals(endpointName, StringComparison.OrdinalIgnoreCase));

                if (targetEndpoint == null)
                {
                    Successor?.HandleRequest(context);
                    return;
                }

                var targetMethod = targetEndpoint.GetMethods().FirstOrDefault(m =>
                    m.GetCustomAttributes().Any(attr =>
                    {
                        var method = request.HttpMethod.ToUpperInvariant();
                        var route = pathParts.Length > 1 ? pathParts[1] : null;

                        return (method == "GET" && attr is HttpGet getAttr &&
                                (getAttr.Route == null || getAttr.Route.Equals(route, StringComparison.OrdinalIgnoreCase)))
                            || (method == "POST" && attr is HttpPost postAttr &&
                                (postAttr.Route == null || postAttr.Route.Equals(route, StringComparison.OrdinalIgnoreCase)));
                    }));

                if (targetMethod == null)
                {
                    Successor?.HandleRequest(context);
                    return;
                }

                var instance = Activator.CreateInstance(targetEndpoint);
                object? result;

                if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var body = reader.ReadToEnd();

                    var postParams = body.Split('&', StringSplitOptions.RemoveEmptyEntries)
                        .Select(pair => pair.Split('='))
                        .Where(kv => kv.Length == 2)
                        .ToDictionary(
                            kv => WebUtility.UrlDecode(kv[0]),
                            kv => WebUtility.UrlDecode(kv[1])
                        );

                    var parameters = targetMethod.GetParameters()
                        .Select(p => postParams.TryGetValue(p.Name!, out var value) ? value : null)
                        .ToArray();

                    result = targetMethod.Invoke(instance, parameters);
                }
                else
                {
                    result = targetMethod.Invoke(instance, null);
                }

                if (result != null)
                {
                    var bytes = Encoding.UTF8.GetBytes(result.ToString() ?? "OK");
                    response.ContentType = "text/html; charset=utf-8";
                    response.OutputStream.Write(bytes, 0, bytes.Length);
                }

                response.OutputStream.Close();
                Console.WriteLine($" Request handled successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in EndpointsHandler: {ex}");
                response.StatusCode = 500;
                var errorBytes = Encoding.UTF8.GetBytes($"Error: {ex.Message}");
                response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                response.OutputStream.Close();
            }
        }
    }
}
