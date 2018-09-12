using MHttpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace HttpServerTest
{
    public class MExceptionAttribute : Attribute, IExceptionFilter
    {
        public void OnException(Exception ex, HttpRequestEventArgs e)
        {
            byte[] buffer = Encoding.UTF8.GetBytes("emmmmmmmmm" + ex.Message + ex.StackTrace);
            e.Response.StatusCode = 500;
            e.Response.ContentType = "application/text; charset =utf-8";
            e.Response.WriteContent(buffer);
        }
    }
}
