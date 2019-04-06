using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PropertyChanged;

namespace BotVision
{
    [DoNotNotify]
    public class BotVisionWindow : Window
    {
        public BotVisionWindow()
        {
            AvaloniaXamlLoader.Load(this);
            
        }

        private bool _skipMaximize;
        public override void Render(DrawingContext context)
        {
            if (!_skipMaximize)
            {
                _skipMaximize = true;
                WindowState = WindowState.Maximized;
            }
            base.Render(context);
        }
    }
}