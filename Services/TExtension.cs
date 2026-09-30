using Microsoft.Maui.Controls.Xaml;

namespace DailyExpenseTracker;

[ContentProperty(nameof(Text))]
public class TExtension : IMarkupExtension<string>
{
    public string Text { get; set; } = "";

    public string ProvideValue(IServiceProvider serviceProvider) => L.T(Text);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
