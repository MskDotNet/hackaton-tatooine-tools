using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Threading;
#if !PUBLIC
using MachineheadTetsujin;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Pathfinder;
using Tatooine;

namespace BotVision
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 0)
                Startup.ServerUri = args[0];
            BuildAvaloniaApp().Start(AppMain, args);
        }


        static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UsePlatformDetect();

        private static BotVisionViewModel Vm;
        static void AppMain(Application app, string[] args)
        {
            Vm = new BotVisionViewModel();
            
            
            new Thread(HttpServe){IsBackground = true}.Start();
            
            app.Run(new BotVisionWindow{DataContext = Vm});
        }

        private static void AutoBot()
        {
#if !PUBLIC
            var client = new ApiClient("http://localhost:5000/raceapi/");
            var bot = new Bot(client, "MachineheadTetsujin", "Redline", "the_maze");
            Thread.Sleep(1000);
            while (bot.CanContinue)
            {
                bot.MakeTurn();
            }
#endif
        }

        public static void DispatchMapUpdate(ApiRaceStateDto state, bool newRace)
        {
            Dispatcher.UIThread.InvokeAsync(() => Vm.WeatherUpdate(state, newRace));
        }
        
        static void HttpServe()
        {
            var wh = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();
            wh.Start();
            new Thread(AutoBot){IsBackground = true}.Start();
            wh.WaitForShutdown();
        }
    }

    class Startup
    {
        //public static string ServerUri = "http://51.15.100.12:5000";
        public static string ServerUri = "http://51.158.109.80:5000";
        static readonly HttpClient Client = new HttpClient();
        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(async (context, _) =>
            {

                var req = new HttpRequestMessage(new HttpMethod(context.Request.Method),
                    ServerUri + context.Request.Path + context.Request.QueryString);
                req.Headers.Clear();
                if (context.Request.Headers.ContentLength.HasValue)
                {
                    req.Content = new StreamContent(context.Request.Body);
                    req.Content.Headers.Clear();
                }

                foreach (var hdr in context.Request.Headers)
                {
                    if (hdr.Key.ToLowerInvariant() == "host"
                        || hdr.Key.ToLowerInvariant() == "content-length")
                        continue;
                    
                    if (!req.Headers.TryAddWithoutValidation(hdr.Key, hdr.Value.ToArray())
                        && req.Content != null)
                        req.Content.Headers.TryAddWithoutValidation(hdr.Key, hdr.Value.ToArray());
                }

                var res = await Client.SendAsync(req);
                var data = await res.Content.ReadAsByteArrayAsync();

                if (res.IsSuccessStatusCode && context.Request.Path == "/raceapi/race")
                {
                    var s = Encoding.UTF8.GetString(data);
                    var state = JsonConvert.DeserializeObject<ApiRaceStateDto>(s);
                    Program.DispatchMapUpdate(state, context.Request.Method.ToUpperInvariant() == "POST");
                }

                context.Response.StatusCode = (int) res.StatusCode;
                foreach (var hdr in res.Headers.Concat(res.Content.Headers))
                {
                    if (hdr.Key.ToLowerInvariant() != "transfer-encoding")
                        context.Response.Headers[hdr.Key] = new StringValues(hdr.Value.ToArray());
                }

                context.Response.ContentLength = data.Length;
                await context.Response.Body.WriteAsync(data, 0, data.Length);
            });
        }
    }
}