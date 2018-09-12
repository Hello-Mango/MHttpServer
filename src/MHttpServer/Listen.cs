using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MHttpServer
{
    public class Listen
    {
        Dictionary<string, MAction> dic = new Dictionary<string, MAction>();
        IContainer container;
        public HttpServer httpsv;
        private int port;
        /// <summary>
        /// 跨域参数设置
        /// </summary>
        public string corsOptions { get; set; }
        public Listen(int _port)
        {
            port = _port;
        }
        /// <summary>
        /// 开启监听
        /// </summary>
        public void start()
        {
            httpsv = new HttpServer(IPAddress.Any, port);
            httpsv.Log.Level = LogLevel.Trace;
            httpsv.OnGet += Httpsv_OnGet;
            httpsv.OnPost += Httpsv_OnPost;
            httpsv.OnOptions += Httpsv_OnOptions;
            httpsv.Start();
        }
        /// <summary>
        /// 停止监听
        /// </summary>
        public void stop()
        {
            httpsv.OnGet -= Httpsv_OnGet;
            httpsv.OnPost -= Httpsv_OnPost;
            httpsv.OnOptions -= Httpsv_OnOptions;
            httpsv.Stop();
        }
        /// <summary>
        /// 跨域请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Httpsv_OnOptions(object sender, HttpRequestEventArgs e)
        {
            e.Response.AddHeader("Access-Control-Allow-Origin", "*");
            e.Response.AddHeader("Access-Control-Allow-Methods", "POST,PUT,DELETE,GET");
            e.Response.AddHeader("Access-Control-Allow-Headers", "*");
            e.Response.AddHeader("Access-Control-Max-Age", "1728000");
            e.Response.Close();
        }

        private void Httpsv_OnPost(object sender, HttpRequestEventArgs e)
        {
            MAction emm = new MAction();
            string logResult = string.Empty;
            try
            {
                byte[] bf = new byte[e.Request.ContentLength64];
                string result = Encoding.UTF8.GetString(GetResult(e, bf));   //获取http请求内容
                logResult = result;
                if (dic.ContainsKey(e.Request.RawUrl))
                {
                    emm = dic[e.Request.RawUrl];
                    object obj;
                    object[] objs = new object[emm.parameterInfo.Length];
                    if (emm.parameterInfo.Length > 1)
                    {
                        throw new Exception("POST方法只允许有一个参数");
                    }
                    else if (emm.parameterInfo.Length == 0)
                    {
                        obj = new object();
                    }
                    else
                    {
                        obj = Newtonsoft.Json.JsonConvert.DeserializeObject(result, emm.parameterInfo[0].ParameterType);
                        objs[0] = obj;
                    }
                    var action = container.Resolve(emm.controllerType);
                    var data = emm.action.Invoke(action, objs);
                    e.Response.ContentType = "application/json";
                    if (data == null)
                    {
                        e.Response.StatusCode = 204;
                    }
                    else
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                        e.Response.WriteContent(buffer);
                    }
                }
                else
                {
                    e.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                if (emm.exceptionFilter == null)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(ex.Message + ex.StackTrace);
                    e.Response.WriteContent(buffer);
                    e.Response.StatusCode = 500;
                }
                else
                {
                    if (emm.exceptionFilter != null)
                    {
                        if (ex.InnerException != null)
                        {
                            emm.exceptionFilter.OnException(ex.InnerException, e);
                        }
                        else
                        {
                            emm.exceptionFilter.OnException(ex, e);
                        }
                    }
                }
            }
        }

        private void Httpsv_OnGet(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;
            string rawUrl = req.RawUrl;
            MAction emm = new MAction();
            if (Path.GetExtension(req.RawUrl).Length > 0)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + req.RawUrl;
                string contentType = MimeMapping.GetMimeMapping(path);
                byte[] buffer = File.ReadAllBytes(path);
                res.ContentType = contentType;
                res.WriteContent(buffer);
            }
            else
            {
                int index = rawUrl.IndexOf('?');
                if (index > 0)
                {
                    rawUrl = rawUrl.Substring(0, index);
                }
                string result = string.Empty;
                try
                {
                    if (dic.ContainsKey(rawUrl))
                    {
                        emm = dic[rawUrl];
                        var action = container.Resolve(emm.controllerType);
                        object[] objs = new object[emm.parameterInfo.Length];
                        int i = 0;
                        foreach (var item in emm.parameterInfo)
                        {
                            if (item.ParameterType == typeof(int))
                            {
                                objs[i] = int.Parse(e.Request.QueryString.Get(item.Name));
                            }
                            else
                            {
                                objs[i] = e.Request.QueryString.Get(item.Name);
                            }
                            i = i + 1;
                        }
                        var data = emm.action.Invoke(action, objs);
                        e.Response.ContentType = "application/json";
                        if (data == null)
                        {
                            e.Response.StatusCode = 204;
                        }
                        else
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                            e.Response.WriteContent(buffer);
                        }
                    }
                    else
                    {
                        e.Response.StatusCode = 404;
                    }
                }
                catch (Exception ex)
                {
                    if (emm.exceptionFilter == null)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(ex.Message + ex.StackTrace);
                        e.Response.WriteContent(buffer);
                        e.Response.StatusCode = 500;
                    }
                    else
                    {
                        if (emm.exceptionFilter != null)
                        {
                            if (ex.InnerException != null)
                            {
                                emm.exceptionFilter.OnException(ex.InnerException, e);
                            }
                            else
                            {
                                emm.exceptionFilter.OnException(ex, e);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取数据结果
        /// </summary>
        /// <param name="e"></param>
        /// <param name="bf"></param>
        /// <returns></returns>
        private byte[] GetResult(HttpRequestEventArgs e, byte[] bf)
        {
            System.IO.Stream st = e.Request.InputStream;
            st.Read(bf, 0, bf.Length);

            string value = Encoding.UTF8.GetString(bf).TrimEnd((char)0);
            byte[] newbf = Encoding.UTF8.GetBytes(value);

            //判断数据是否已经下载完整
            if (newbf.Length < bf.Length)
            {
                GetResult(e, new byte[bf.Length - newbf.Length]).CopyTo(bf, newbf.Length);
            }
            return bf;
        }

        public void InitController(ContainerBuilder builder)
        {
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in asses)
            {
                var typesToRegister = item.GetTypes()
                                        .Where(type => !string.IsNullOrEmpty(type.Namespace))
                                        .Where(type => type.BaseType == typeof(Controller));
                if (typesToRegister.Count() > 0)
                {
                    foreach (var item1 in typesToRegister)
                    {
                        builder.RegisterType(item1);

                        foreach (var item2 in item1.GetMethods())
                        {
                            IExceptionFilter _exceptionFilter = null;
                            foreach (var item3 in item2.GetCustomAttributes(true))
                            {
                                Attribute temp = (Attribute)item3;
                                Type type = temp.GetType().GetInterface(typeof(IExceptionFilter).Name);
                                if (typeof(IExceptionFilter) == type)
                                {
                                    _exceptionFilter = item3 as IExceptionFilter;
                                }
                            }
                            MAction mAction = new MAction()
                            {
                                requestRawUrl = @"/" + item1.Name.Replace("Controller", "") + @"/" + item2.Name,
                                action = item2,
                                typeName = item1.GetType().Name,
                                controllerType = item1,
                                parameterInfo = item2.GetParameters(),
                                exceptionFilter = _exceptionFilter
                            };
                            dic.Add(mAction.requestRawUrl, mAction);
                        }
                    }
                }
            }
            container = builder.Build();
        }
    }
    public class MAction
    {
        public string requestRawUrl { get; set; }
        public string typeName { get; set; }
        public Type controllerType { get; set; }
        public MethodInfo action { get; set; }
        public IExceptionFilter exceptionFilter { get; set; }
        public ParameterInfo[] parameterInfo { get; set; }
    }
}
