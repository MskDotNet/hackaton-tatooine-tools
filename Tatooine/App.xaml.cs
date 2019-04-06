using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
namespace Tatooine
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
#if !PUBLIC
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start(AppMain, args);
        }

        private static void AppMain(Application app, string[] args)
        {
            var w = new EditorWindow() {DataContext = new EditorWindowViewModel()};
            w.AttachDevTools();
            app.Run(w);
        }


        static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UsePlatformDetect();
#endif
        
    }
}

