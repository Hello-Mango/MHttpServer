using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp.Server;

namespace MHttpServer
{
    public interface IExceptionFilter
    {
        void OnException(Exception ex, HttpRequestEventArgs e);
    }
}
