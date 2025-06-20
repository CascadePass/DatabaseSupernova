using Microsoft.Win32;

namespace CascadePass.CascadeCore.UI
{
    public interface IRegistryProvider
    {
        event RegistryProvider.RegistryAccessedHandler RegistryAccessed;
        event RegistryProvider.RegistryAccessedAsyncHandler RegistryAccessedAsync;

        bool DeleteValue(string keyName, string valueName);
        string[] GetSubKeyNames(string keyName);
        object GetValue(string keyName, string valueName);
        string[] GetValueNames(string keyName);
        bool SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind);
    }
}
