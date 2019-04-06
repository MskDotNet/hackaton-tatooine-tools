using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace Tatooine
{
    [DoNotNotify]
    public class EditorWindow : Window
    {
        public EditorWindowViewModel Model => DataContext as EditorWindowViewModel;
        
        public EditorWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<GameFieldView>("field").PaintEvent += pt => Model?.OnPaint(pt);

        }
        
        
    }
}