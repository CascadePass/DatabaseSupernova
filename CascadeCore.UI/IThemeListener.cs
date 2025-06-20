using System;

namespace CascadePass.CascadeCore.UI
{
    public interface IThemeListener
    {
        event EventHandler ThemeChanged;
    }
}