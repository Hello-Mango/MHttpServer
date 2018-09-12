# MHttpServer
##使用方法：
     MHttpServer.Listen listen = new MHttpServer.Listen(8088);
     ContainerBuilder builder = new ContainerBuilder();
     builder.RegisterType<TestService>().PropertiesAutowired();
     listen.InitController(builder);
     listen.start();