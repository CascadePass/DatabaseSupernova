using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CascadePass.CascadeCore.UI
{

    public class RegistryProvider : IRegistryProvider
    {
        private readonly RegistryHive hive;

        public delegate void RegistryAccessedHandler(object sender, RegistryAccessEventArgs e);
        public delegate Task RegistryAccessedAsyncHandler(object sender, RegistryAccessEventArgs e);

        public event RegistryAccessedHandler RegistryAccessed;
        public event RegistryAccessedAsyncHandler RegistryAccessedAsync;

        #region Constructors

        public RegistryProvider()
        {
            this.hive = RegistryHive.CurrentUser;
        }

        public RegistryProvider(RegistryHive targetHive)
        {
            this.hive = targetHive;
        }

        #endregion

        private RegistryKey OpenKey(string keyName, bool writable)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException(null, nameof(keyName));
            }

            return RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(keyName, writable);
        }

        public object GetValue(string keyName, string valueName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException(null, nameof(keyName));
            }

            try
            {
                using var key = this.OpenKey(keyName, writable: false);
                object value = key?.GetValue(valueName);

                this.OnRegistryAccessed(new(this.hive, keyName, valueName, RegistryAccessType.Read));
                return value;
            }
            catch (Exception ex)
            {
                this.OnRegistryAccessed(new(this.hive, keyName, valueName, RegistryAccessType.Read, ex));
                return null;
            }
        }

        public bool SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException(null, nameof(keyName));
            }

            try
            {
                using var key = RegistryKey.OpenBaseKey(hive, RegistryView.Default).CreateSubKey(keyName);
                key?.SetValue(valueName, value, valueKind);

                this.OnRegistryAccessed(new(this.hive, keyName, valueName, RegistryAccessType.Write));
                return true;
            }
            catch (Exception ex)
            {
                this.OnRegistryAccessed(new(this.hive, keyName, valueName, RegistryAccessType.Write, ex));
                return false;
            }
        }

        public bool DeleteValue(string keyName, string valueName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException(null, nameof(keyName));
            }

            try
            {
                using var key = this.OpenKey(keyName, writable: true);
                key?.DeleteValue(valueName, throwOnMissingValue: false);

                this.OnRegistryAccessed(new(this.hive, keyName, valueName, RegistryAccessType.Delete));
                return true;
            }
            catch (Exception ex)
            {
                this.OnRegistryAccessed(new(this.hive, keyName, valueName, RegistryAccessType.Delete, ex));
                return false;
            }
        }

        public string[] GetSubKeyNames(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException(null, nameof(keyName));
            }

            try
            {
                using var key = this.OpenKey(keyName, writable: false);
                var result = key?.GetSubKeyNames() ?? [];

                this.OnRegistryAccessed(new(this.hive, keyName, RegistryAccessType.EnumerateKeys));
                return result;
            }
            catch (Exception ex)
            {
                this.OnRegistryAccessed(new(this.hive, keyName, RegistryAccessType.EnumerateKeys, ex));
                return null;
            }
        }

        public string[] GetValueNames(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException(null, nameof(keyName));
            }

            try
            {
                using var key = this.OpenKey(keyName, writable: false);
                var result = key?.GetValueNames() ?? [];

                this.OnRegistryAccessed(new(this.hive, keyName, RegistryAccessType.EnumerateValues));
                return result;
            }
            catch (Exception ex)
            {
                this.OnRegistryAccessed(new(this.hive, keyName, RegistryAccessType.EnumerateValues, ex));
                return null;
            }
        }

        protected virtual void OnRegistryAccessed(RegistryAccessEventArgs e)
        {
            this.RegistryAccessed?.Invoke(this, e);
            this.OnRegistryAccessedAsync(e);
        }

        protected virtual void OnRegistryAccessedAsync(RegistryAccessEventArgs e)
        {
            if (this.RegistryAccessedAsync != null)
            {
                var invocationList = this.RegistryAccessedAsync.GetInvocationList().Cast<RegistryAccessedAsyncHandler>();
                foreach (var handler in invocationList)
                {
                    _ = handler(this, e); // Fire-and-forget async
                }
            }
        }
    }

    /// <summary>
    /// Provides data for events related to Windows Registry access operations, 
    /// including key and value names, access type, outcome, and any associated exception.
    /// </summary>
    public class RegistryAccessEventArgs : EventArgs
    {
        #region Constructors

        internal RegistryAccessEventArgs(
            RegistryHive hive,
            string keyName,
            RegistryAccessType accessType)
        {
            this.Hive = hive;
            this.KeyName = keyName;
            this.AccessType = accessType;
        }

        internal RegistryAccessEventArgs(
            RegistryHive hive,
            string keyName,
            string valueName,
            RegistryAccessType accessType)
        {
            this.Hive = hive;
            this.KeyName = keyName;
            this.ValueName = valueName;
            this.AccessType = accessType;
        }

        internal RegistryAccessEventArgs(
            RegistryHive hive,
            string keyName,
            RegistryAccessType accessType,
            Exception exception)
        {
            this.Hive = hive;
            this.KeyName = keyName;
            this.AccessType = accessType;
            this.Exception = exception;
        }

        internal RegistryAccessEventArgs(
            RegistryHive hive,
            string keyName,
            string valueName,
            RegistryAccessType accessType,
            Exception exception)
        {
            this.Hive = hive;
            this.KeyName = keyName;
            this.ValueName = valueName;
            this.AccessType = accessType;
            this.Exception = exception;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the registry hive involved in the operation.
        /// </summary>
        public RegistryHive Hive { get; }

        /// <summary>
        /// Gets the name of the registry key involved in the operation.
        /// </summary>
        public string KeyName { get; }

        /// <summary>
        /// Gets the name of the value within the registry key that was accessed or modified.
        /// </summary>
        public string ValueName { get; }

        /// <summary>
        /// Gets the type of registry access operation that was performed.
        /// </summary>
        public RegistryAccessType AccessType { get; }

        /// <summary>
        /// Gets a value indicating whether the registry operation completed without throwing an exception.
        /// </summary>
        public bool WasSuccessful => this.Exception == null;

        /// <summary>
        /// Gets the exception thrown during the registry operation, if any. 
        /// Returns <c>null</c> if the operation was successful.
        /// </summary>
        public Exception Exception { get; }

        #endregion
    }

    public enum RegistryAccessType
    {
        None,

        Read,

        Write,

        Delete,

        EnumerateKeys,

        EnumerateValues,
    }
}
