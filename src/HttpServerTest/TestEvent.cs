using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpServerTest
{
    public static class TestEvent
    {
        public  delegate void InformHandle(object sender);
        public static event InformHandle informHandle;
        public static void OnEvent(object data)
        {
            informHandle?.Invoke(data);
        }
    }
}