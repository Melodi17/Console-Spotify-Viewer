using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Melodi.Networking
{
    public class HTTPServer
    {
        private HttpListener listener;
        private readonly string[] Addresses;
        private readonly Type Tp;
        public bool Running;
        public IHTTPServerFilter Filter;

        public HTTPServer(Type tp, params string[] addresses)
        {
            Addresses = addresses;
            Running = false;
            Tp = tp;
        }

        public void Start()
        {
            if (Running)
                throw new Exception("Server already started");

            listener = new();

            foreach (string item in Addresses)
                listener.Prefixes.Add(item);

            listener.Start();

            Running = true;

            Thread t = new(StartAsync);
            t.IsBackground = true;
            t.Start();
        }
        public void Stop()
        {
            Running = false;
        }
        public void StartAsync()
        {
            while (Running)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();

                    if (Filter != null)
                    {
                        if (Filter.FilterRequest(ref context))
                            continue;
                    }

                    MethodInfo[] methods = Tp.GetMethods()
                        .Where(x => x.GetCustomAttribute<HTTPRequestAttribute>() != null
                            && x.ReturnType == typeof(byte[]))
                        .Where(x =>
                        {
                            HTTPRequestAttribute attribute = x.GetCustomAttribute<HTTPRequestAttribute>();
                            return attribute.Path == context.Request.Url.AbsolutePath
                                && attribute.Protocol == context.Request.HttpMethod.ToLower();
                        })
                        .ToArray();

                    MethodInfo[] defaultMethods = Tp.GetMethods()
                        .Where(x => x.GetCustomAttribute<HTTPDefaultAttribute>() != null
                            && x.ReturnType == typeof(byte[]))
                        .Where(x =>
                        {
                            HTTPDefaultAttribute attribute = x.GetCustomAttribute<HTTPDefaultAttribute>();
                            return attribute.Protocol == context.Request.HttpMethod.ToLower();
                        })
                        .ToArray();

                    byte[] buffer;
                    if (methods.Any())
                    {
                        buffer = (byte[])methods.First().Invoke(null, new object[] { context });
                    }
                    else if (defaultMethods.Any())
                    {
                        buffer = (byte[])defaultMethods.First().Invoke(null, new object[] { context });
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    context.Response.ContentType = "text/html";
                    context.Response.ContentLength64 = buffer.Length;

                    Stream output = context.Response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    // You must close the output stream.
                    output.Close();
                }
                catch (Exception e)
                {
                    MethodInfo[] errorMethods = Tp.GetMethods()
                        .Where(x => x.GetCustomAttribute<HTTPErrorAttribute>() != null)
                        .ToArray();

                    if (errorMethods.Any())
                        errorMethods.First().Invoke(null, new object[] { e });
                }
            }
        }
    }
    public interface IHTTPServerFilter
    {
        bool FilterRequest(ref HttpListenerContext ctx);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HTTPRequestAttribute : Attribute
    {
        public string Path => _path;
        private string _path;
        public string Protocol => _protocol;
        private string _protocol;
        public HTTPRequestAttribute(string path, string protocol)
        {
            _path = path;
            _protocol = protocol.ToLower();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HTTPDefaultAttribute : Attribute
    {
        public string Protocol => _protocol;
        private string _protocol;
        public HTTPDefaultAttribute(string protocol)
        {
            _protocol = protocol.ToLower();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HTTPErrorAttribute : Attribute
    {
        public HTTPErrorAttribute() { }
    }
}
