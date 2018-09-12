using Autofac;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HttpServerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TestEvent.informHandle += TestEvent_informHandle;
            MHttpServer.Listen listen = new MHttpServer.Listen(8088);
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<TestService>().PropertiesAutowired();
            listen.InitController(builder);
            listen.start();
        }

        private void TestEvent_informHandle(object sender)
        {
            this.richTextBox1.BeginInvoke(new Action(() =>
            {
                this.richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + ":" + sender.ToString() + Environment.NewLine);
            }));
        }
    }
}
