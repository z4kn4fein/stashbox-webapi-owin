using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;

namespace Stashbox.AspNet.WebApi.Owin.Tests
{
    [TestClass]
    public class WebApiExtensionTests
    {
        [TestMethod]
        public async Task WebApiExtensionTests_Scoping()
        {
            using (var server = TestServer.Create(app =>
             {
                 var config = new HttpConfiguration { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always };
                 var container = new StashboxContainer();
                 container.RegisterType<TestMiddleware>();
                 container.RegisterScoped<Test>();

                 config.MapHttpAttributeRoutes();

                 app.UseStashboxWebApi(config, container).UseStashbox(container).UseWebApi(config);
             }))
            {
                var resp = await server.HttpClient.GetAsync("/api/test/value");
                Assert.AreEqual("test1\"1test1test1\"", await resp.Content.ReadAsStringAsync());

                resp = await server.HttpClient.GetAsync("/api/test/value");
                Assert.AreEqual("test2\"2test2test2\"", await resp.Content.ReadAsStringAsync());
            }
        }
    }

    public class Test
    {
        private static int counter;

        public Test()
        {
            Interlocked.Increment(ref counter);
        }

        public string Value => "test" + counter;
    }

    [RoutePrefix("api/test")]
    public class Test2Controller : ApiController
    {
        private readonly Test test;
        private readonly Test test1;

        private static int controllerCounter;

        public Test2Controller(Test test, Test test1)
        {
            this.test = test;
            this.test1 = test1;
            Interlocked.Increment(ref controllerCounter);
        }

        [HttpGet]
        [Route("value")]
        public IHttpActionResult GetValue()
        {
            return this.Ok(controllerCounter + this.test.Value + this.test1.Value);
        }
    }

    public class TestMiddleware : OwinMiddleware
    {
        private readonly Test test;

        public TestMiddleware(OwinMiddleware next, Test test) : base(next)
        {
            this.test = test;
        }

        public override async Task Invoke(IOwinContext context)
        {
            await context.Response.WriteAsync(this.test.Value);
            await this.Next.Invoke(context);
        }
    }
}
