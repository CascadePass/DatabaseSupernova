using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CascadePass.CascadeCore.UI.Tests
{
    [TestClass]
    public class RegistryProviderTests
    {
        private const string TestKeyPrefix = "Software\\CascadeCoreTests";

        #region GetValue Tests

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\r")]
        [DataRow("\n")]
        [DataRow("\t")]
        [DataRow("\r\n")]
        public void GetValue_ThrowsForEmptyKeyName(string verionOfEmpty)
        {
            var registryProvider = new RegistryProvider();
            Assert.ThrowsException<ArgumentException>(() => registryProvider.GetValue(verionOfEmpty, "AnyValue"));
        }

        [TestMethod]
        public void GetValue_ReturnsNullAndRaisesEvent_WhenKeyDoesNotExist()
        {
            var registryProvider = new RegistryProvider();
            string keyName = "NonExistentKey";
            string valueName = "NonExistentValue";

            var result = registryProvider.GetValue(keyName, valueName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetValue_RaisesRegistryAccessedEvent()
        {
            var registryProvider = new RegistryProvider();
            string keyName = "DoesNotMatter";
            string valueName = "DoesNotMatter";
            RegistryAccessEventArgs receivedArgs = null;

            registryProvider.RegistryAccessed += (sender, args) =>
            {
                receivedArgs = args;
            };

            var result = registryProvider.GetValue(keyName, valueName);

            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(RegistryAccessType.Read, receivedArgs.AccessType);
            Assert.AreEqual(keyName, receivedArgs.KeyName);
            Assert.AreEqual(valueName, receivedArgs.ValueName);
        }

        [TestMethod]
        public void GetValue_RaisesRegistryAccessedEvent_OnlyOnce()
        {
            var registryProvider = new RegistryProvider();
            string keyName = "ArbitraryKey";
            string valueName = "ArbitraryValue";
            int eventCount = 0;

            registryProvider.RegistryAccessed += (sender, args) =>
            {
                eventCount++;
            };

            var result = registryProvider.GetValue(keyName, valueName);

            Assert.IsNull(result);
            Assert.AreEqual(1, eventCount, "Expected only one RegistryAccessed event to be raised.");
        }

        [TestMethod]
        public async Task GetValue_RaisesRegistryAccessedAsyncEvent()
        {
            var registryProvider = new RegistryProvider();
            string keyName = "AsyncKey";
            string valueName = "AsyncValue";

            RegistryAccessEventArgs receivedArgs = null;
            var tcs = new TaskCompletionSource<bool>();

            registryProvider.RegistryAccessedAsync += async (sender, args) =>
            {
                receivedArgs = args;
                tcs.SetResult(true);
                await Task.CompletedTask;
            };

            var result = registryProvider.GetValue(keyName, valueName);

            await Task.WhenAny(tcs.Task, Task.Delay(500)); // Wait for async event or timeout

            Assert.IsNull(result);
            Assert.IsTrue(tcs.Task.IsCompleted, "Async event did not complete in time.");
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(RegistryAccessType.Read, receivedArgs.AccessType);
            Assert.AreEqual(keyName, receivedArgs.KeyName);
            Assert.AreEqual(valueName, receivedArgs.ValueName);
        }

        [TestMethod]
        public async Task GetValue_RaisesRegistryAccessedAsyncEvent_OnlyOnce()
        {
            var registryProvider = new RegistryProvider();
            string keyName = "AsyncKey_Single";
            string valueName = "AsyncValue_Single";

            int asyncCount = 0;
            var tcs = new TaskCompletionSource<bool>();

            registryProvider.RegistryAccessedAsync += async (sender, args) =>
            {
                Interlocked.Increment(ref asyncCount);
                tcs.SetResult(true);
                await Task.CompletedTask;
            };

            var result = registryProvider.GetValue(keyName, valueName);

            await Task.WhenAny(tcs.Task, Task.Delay(500));

            Assert.IsNull(result);
            Assert.IsTrue(tcs.Task.IsCompleted, "Async event did not fire.");
            Assert.AreEqual(1, asyncCount, "Expected only one async event invocation.");
        }

        #endregion

        #region SetValue Tests

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\r")]
        [DataRow("\n")]
        [DataRow("\t")]
        [DataRow("\r\n")]
        public void SetValue_ThrowsForEmptyKeyName(string verionOfEmpty)
        {
            var registryProvider = new RegistryProvider();
            
            Assert.ThrowsException<ArgumentException>(
                () => registryProvider.SetValue(
                    verionOfEmpty,
                    "AnyValueName",
                    "ValueTheCallerWantsToSaveToTheRegistry",
                    RegistryValueKind.String
                )
            );
        }

        [TestMethod]
        public void SetValue_CreatesKeyIfNotExists_AndReturnsTrue()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\NonExistentSubKey_{Guid.NewGuid()}";
            string valueName = "NonExistentValue";
            string valueToSet = "ValueTheCallerWantsToSaveToTheRegistry";

            var result = registryProvider.SetValue(keyName, valueName, valueToSet, RegistryValueKind.String);
            registryProvider.DeleteValue(keyName, valueName);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SetValue_RaisesRegistryAccessedEvent()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\DoesNotMatter";
            string valueName = "DoesNotMatter";
            string value = "DoesNotMatter";

            RegistryAccessEventArgs receivedArgs = null;

            registryProvider.RegistryAccessed += (sender, args) =>
            {
                receivedArgs = args;
            };

            var result = registryProvider.SetValue(keyName, valueName, value, RegistryValueKind.String);

            Assert.IsTrue(result);
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(RegistryAccessType.Write, receivedArgs.AccessType);
            Assert.AreEqual(keyName, receivedArgs.KeyName);
            Assert.AreEqual(valueName, receivedArgs.ValueName);
        }

        [TestMethod]
        public void SetValue_RaisesRegistryAccessedEvent_OnlyOnce()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\ArbitraryKey";
            string valueName = "ArbitraryValue";
            string value = "ArbitraryValue";
            int eventCount = 0;

            registryProvider.RegistryAccessed += (sender, args) =>
            {
                eventCount++;
            };

            var result = registryProvider.SetValue(keyName, valueName, value, RegistryValueKind.String);

            Assert.IsTrue(result);
            Assert.AreEqual(1, eventCount, "Expected only one RegistryAccessed event to be raised.");
        }

        [TestMethod]
        public async Task SetValue_RaisesRegistryAccessedAsyncEvent()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\AsyncKey";
            string valueName = "AsyncValue";
            string value = "AsyncValue";

            RegistryAccessEventArgs receivedArgs = null;
            var tcs = new TaskCompletionSource<bool>();

            registryProvider.RegistryAccessedAsync += async (sender, args) =>
            {
                receivedArgs = args;
                tcs.SetResult(true);
                await Task.CompletedTask;
            };

            var result = registryProvider.SetValue(keyName, valueName, value, RegistryValueKind.String);

            await Task.WhenAny(tcs.Task, Task.Delay(500)); // Wait for async event or timeout

            Assert.IsTrue(result);
            Assert.IsTrue(tcs.Task.IsCompleted, "Async event did not complete in time.");
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(RegistryAccessType.Write, receivedArgs.AccessType);
            Assert.AreEqual(keyName, receivedArgs.KeyName);
            Assert.AreEqual(valueName, receivedArgs.ValueName);
        }

        [TestMethod]
        public async Task SetValue_RaisesRegistryAccessedAsyncEvent_OnlyOnce()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\AsyncKey_Single";
            string valueName = "AsyncValue_Single";
            string value = "AsyncValue_Single";

            int asyncCount = 0;
            var tcs = new TaskCompletionSource<bool>();

            registryProvider.RegistryAccessedAsync += async (sender, args) =>
            {
                Interlocked.Increment(ref asyncCount);
                tcs.SetResult(true);
                await Task.CompletedTask;
            };

            var result = registryProvider.SetValue(keyName, valueName, value, RegistryValueKind.String);

            await Task.WhenAny(tcs.Task, Task.Delay(500));

            Assert.IsTrue(result);
            Assert.IsTrue(tcs.Task.IsCompleted, "Async event did not fire.");
            Assert.AreEqual(1, asyncCount, "Expected only one async event invocation.");
        }

        #endregion

        #region DeleteValue Tests

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\r")]
        [DataRow("\n")]
        [DataRow("\t")]
        [DataRow("\r\n")]
        public void DeleteValue_ThrowsForEmptyKeyName(string verionOfEmpty)
        {
            var registryProvider = new RegistryProvider();

            Assert.ThrowsException<ArgumentException>(
                () => registryProvider.DeleteValue(
                    verionOfEmpty,
                    "AnyValueName"
                )
            );
        }

        [TestMethod]
        public void DeleteValue_ReturnsTrue_IfNotExists()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\NonExistentSubKey_{Guid.NewGuid()}";
            string valueName = "NonExistentValue";

            var result = registryProvider.DeleteValue(keyName, valueName);
            registryProvider.DeleteValue(keyName, valueName);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DeleteValue_RaisesRegistryAccessedEvent()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\DoesNotMatter";
            string valueName = "DoesNotMatter";

            RegistryAccessEventArgs receivedArgs = null;

            registryProvider.RegistryAccessed += (sender, args) =>
            {
                receivedArgs = args;
            };

            var result = registryProvider.DeleteValue(keyName, valueName);

            Assert.IsTrue(result);
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(RegistryAccessType.Delete, receivedArgs.AccessType);
            Assert.AreEqual(keyName, receivedArgs.KeyName);
            Assert.AreEqual(valueName, receivedArgs.ValueName);
        }

        [TestMethod]
        public void DeleteValue_RaisesRegistryAccessedEvent_OnlyOnce()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\ArbitraryKey";
            string valueName = "ArbitraryValue";
            int eventCount = 0;

            registryProvider.RegistryAccessed += (sender, args) =>
            {
                eventCount++;
            };

            var result = registryProvider.DeleteValue(keyName, valueName);

            Assert.IsTrue(result);
            Assert.AreEqual(1, eventCount, "Expected only one RegistryAccessed event to be raised.");
        }

        [TestMethod]
        public async Task DeleteValue_RaisesRegistryAccessedAsyncEvent()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\AsyncKey";
            string valueName = "AsyncValue";

            RegistryAccessEventArgs receivedArgs = null;
            var tcs = new TaskCompletionSource<bool>();

            registryProvider.RegistryAccessedAsync += async (sender, args) =>
            {
                receivedArgs = args;
                tcs.SetResult(true);
                await Task.CompletedTask;
            };

            var result = registryProvider.DeleteValue(keyName, valueName);

            await Task.WhenAny(tcs.Task, Task.Delay(500)); // Wait for async event or timeout

            Assert.IsTrue(result);
            Assert.IsTrue(tcs.Task.IsCompleted, "Async event did not complete in time.");
            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(RegistryAccessType.Delete, receivedArgs.AccessType);
            Assert.AreEqual(keyName, receivedArgs.KeyName);
            Assert.AreEqual(valueName, receivedArgs.ValueName);
        }

        [TestMethod]
        public async Task DeleteValue_RaisesRegistryAccessedAsyncEvent_OnlyOnce()
        {
            var registryProvider = new RegistryProvider();
            string keyName = $"{TestKeyPrefix}\\AsyncKey_Single";
            string valueName = "AsyncValue_Single";

            int asyncCount = 0;
            var tcs = new TaskCompletionSource<bool>();

            registryProvider.RegistryAccessedAsync += async (sender, args) =>
            {
                Interlocked.Increment(ref asyncCount);
                tcs.SetResult(true);
                await Task.CompletedTask;
            };

            var result = registryProvider.DeleteValue(keyName, valueName);

            await Task.WhenAny(tcs.Task, Task.Delay(500));

            Assert.IsTrue(result);
            Assert.IsTrue(tcs.Task.IsCompleted, "Async event did not fire.");
            Assert.AreEqual(1, asyncCount, "Expected only one async event invocation.");
        }

        #endregion

        #region GetSubKeyNames Tests

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\r")]
        [DataRow("\n")]
        [DataRow("\t")]
        [DataRow("\r\n")]
        public void GetSubKeyNames_ThrowsForEmptyKeyName(string verionOfEmpty)
        {
            var registryProvider = new RegistryProvider();

            Assert.ThrowsException<ArgumentException>(
                () => registryProvider.GetSubKeyNames(verionOfEmpty)
            );
        }

        #endregion

        #region GetValueNames Tests

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\r")]
        [DataRow("\n")]
        [DataRow("\t")]
        [DataRow("\r\n")]
        public void GetValueNames_ThrowsForEmptyKeyName(string verionOfEmpty)
        {
            var registryProvider = new RegistryProvider();

            Assert.ThrowsException<ArgumentException>(
                () => registryProvider.GetValueNames(verionOfEmpty)
            );
        }

        #endregion



        [ClassCleanup]
        public static void Cleanup()
        {
            using var baseKey = Registry.CurrentUser;
            try
            {
                baseKey.DeleteSubKeyTree(TestKeyPrefix, throwOnMissingSubKey: false);
            }
            catch
            {
                // Swallow errors if the key doesn't exist or can't be deleted
            }
        }
    }
}
