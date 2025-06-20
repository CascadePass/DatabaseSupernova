using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace CascadePass.CascadeCore.UI
{
    public class ThemeListener : IThemeListener
    {
        public event EventHandler ThemeChanged;

        public ThemeListener()
        {
            SystemEvents.UserPreferenceChanged += this.OnUserPreferenceChanged;
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                this.OnThemeChanged(sender, e);
            }
        }

        protected void OnThemeChanged(object sender, EventArgs e)
        {
            //Task.Delay(100).ContinueWith(_ => this.ApplyTheme());
        }
    }

}
