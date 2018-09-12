using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HttpServerTest
{
    public class TestService
    {
        public void DoAction(string res)
        {
            MessageBox.Show(res);
        }
        public void DoException(string res)
        {
            throw new Exception("hello exception");
        }
    }
}
