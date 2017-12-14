using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TestAPI
{
    public class MyMiddleWare : OwinMiddleware
    {
        public MyMiddleWare(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            var response = context.Response;
            var request = context.Request;

            if (request.Headers["Origin"] != null)
            {
                response.OnSendingHeaders(state =>
                {
                    var resp = (OwinResponse)state;
                    resp.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

                    if(request.Method == "OPTIONS")
                    {
                        resp.Headers.Add("Access-Control-Allow-Headers", new[] { "Accept, Content-Type, Origin" });
                        resp.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
                    }

                }, response);
            }

            if (request.Method != "OPTIONS")
            {
                await Next.Invoke(context);
            }   
        }
    }
}