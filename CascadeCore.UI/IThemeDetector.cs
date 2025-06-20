namespace CascadePass.CascadeCore.UI
{
    public interface IThemeDetector
    {
        bool IsHighContrastEnabled { get; }
        bool IsInLightMode { get; }

        ThemeType GetTheme();
    }
}