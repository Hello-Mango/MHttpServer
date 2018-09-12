using HttpServerTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpServerTest
{
    public class TestController : MHttpServer.Controller
    {
        TestService testService;
        public TestController(TestService _testService)
        {
            testService = _testService;
        }
        public void Test(string res)
        {
            testService.DoAction(res);
        }
        public string TestDo(string res)
        {
            TestEvent.OnEvent(res);
            return res;
        }
        [MException]
        public void TestDoExceptin(string res)
        {
            testService.DoException(res);
        }
    }
}
