using System.Collections.Generic;

namespace CascadePass.CascadeCore.UI.Tests
{
    [TestClass]
    public class ObservableTests
    {
        [TestMethod]
        public void SetPropertyValue_ShouldRaisePropertyChanged()
        {
            var model = new ConcreteObservable();
            bool eventRaised = false;
            model.PropertyChanged += (sender, e) => { eventRaised = true; };

            model.TestProperty = "New Value";

            Assert.IsTrue(eventRaised, "PropertyChanged event should have fired.");
        }

        [TestMethod]
        public void SetPropertyValue_ShouldNotRaisePropertyChanged_WhenValueIsSame()
        {
            string initialValue = "Initial Value";
            var model = new ConcreteObservable { TestProperty = initialValue };
            bool eventRaised = false;
            model.PropertyChanged += (sender, e) => { eventRaised = true; };

            model.TestProperty = initialValue;  // No actual change.

            Assert.IsFalse(eventRaised, "PropertyChanged event should NOT fire when the value remains unchanged.");
        }

        [TestMethod]
        public void SetPropertyValue_ShouldRaiseMultiplePropertyChangedEvents()
        {
            var model = new ConcreteObservable();
            List<string> changedProperties = [];
            model.PropertyChanged += (sender, e) => changedProperties.Add(e.PropertyName);

            model.MultiNotifyProperty = "Updated Value";

            CollectionAssert.AreEquivalent(new[] { "TestProperty", "MultiNotifyProperty" }, changedProperties,
                "PropertyChanged should fire for each specified property.");
        }

    }


    public class ConcreteObservable : Observable
    {
        private string _testProperty;
        private string _multiNotifyProperty;

        public string TestProperty
        {
            get => _testProperty;
            set => this.SetPropertyValue(ref _testProperty, value, nameof(TestProperty));
        }

        public string MultiNotifyProperty
        {
            get => _multiNotifyProperty;
            set => this.SetPropertyValue(ref _multiNotifyProperty, value, [nameof(MultiNotifyProperty), nameof(TestProperty)]);
        }
    }
}
