using System.Windows;

namespace CascadePass.CascadeCore.UI
{
    public class ThemeDetector : IThemeDetector
    {
        private readonly IRegistryProvider registryProvider;

        #region Constructors

        public ThemeDetector()
        {
            this.registryProvider = new RegistryProvider();
        }

        public ThemeDetector(IRegistryProvider registryProviderToUse)
        {
            this.registryProvider = registryProviderToUse;
        }

        #endregion

        public IRegistryProvider RegistryProvider => this.registryProvider;

        public bool IsInLightMode
        {
            get
            {
                return
                    this.RegistryProvider.GetValue(
                        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                        "AppsUseLightTheme"
                        )
                    ?.Equals(1) ?? true;
            }
        }

        public bool IsHighContrastEnabled
        {
            get
            {
                return SystemParameters.HighContrast;
            }
        }

        public ThemeType GetTheme()
        {
            if (this.IsHighContrastEnabled)
            {
                return ThemeType.HighContrast;
            }

            if (this.IsInLightMode)
            {
                return ThemeType.Light;
            }

            return ThemeType.Dark;
        }
    }
}
