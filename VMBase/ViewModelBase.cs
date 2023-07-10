using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VMBase
{
    /// <summary>
    /// Base class for a managed view model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ViewModelBase<T> : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Holds the child VMs of this VM associated with the property that generated it
        /// </summary>
        private List<ChildVMInfo> ChildVMs { get; } = new List<ChildVMInfo>();

        /// <summary>
        /// Holds any connections to notifiable objects that are not the Item of this VM
        /// </summary>
        private Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> ExtraConnections { get; } = new Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler>();

        /// <summary>
        /// Gets whether this managed view model has been disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the <typeparamref name="T"/> that owns the VM
        /// </summary>
        public T Item { get; }

        /// <summary>
        /// Creates a new managed view model
        /// </summary>
        /// <param name="item"></param>
        protected ViewModelBase(T item)
        {
            Item = item;

            if (Item is INotifyPropertyChanged notifier)
                notifier.PropertyChanged += OnItemPropertyChangedRaw;
        }

        ~ViewModelBase()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes the view model and all its children
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (Item is INotifyPropertyChanged notifier)
                notifier.PropertyChanged -= OnItemPropertyChangedRaw;

            foreach (var i in ExtraConnections)
                i.Key.PropertyChanged -= i.Value;

            foreach (var i in ChildVMs)
                i.Dispose();
        }

        /// <summary>
        /// Disposes all children VMs associated with a particular property
        /// </summary>
        /// <param name="property"></param>
        protected void DisposeChildren([CallerMemberName] string property = "")
        {
            foreach (var i in ChildVMs.Where(i => i.OriginPropertyName == property).ToArray())
            {
                i.Dispose();
                ChildVMs.Remove(i);
            }
        }

        /// <summary>
        /// Disposes all children of the provided property and registers the provided child as the new child of the property
        /// </summary>
        /// <typeparam name="Child"></typeparam>
        /// <param name="child"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected Child RegisterChild<Child>(Func<Child> child, [CallerMemberName] string property = "") where Child : IDisposable
        {
            return (Child)RegisterChildren(() => new IDisposable[] { child() }, property).First();
        }

        /// <summary>
        /// Disposes all children of the provided property and registers the provided children as new children of the property
        /// </summary>
        /// <typeparam name="Collection"></typeparam>
        /// <param name="children"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected internal Child[] RegisterChildren<Child>(Func<IEnumerable<Child>> children, [CallerMemberName] string property = "") where Child : IDisposable
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name, "Cannot register new child view-models for a disposed view-model");

            //lock (this)
            //{
                Child[] existingChildren = ChildVMs
                    .Where(i => i.OriginPropertyName == property)
                    .Select(i => i.ChildVM)
                    .OfType<Child>()
                    .ToArray();

                if (existingChildren.Any())
                    return existingChildren;

                existingChildren = children().ToArray();
                ChildVMs.AddRange(existingChildren.Select(i => new ChildVMInfo(property, i)));

                return existingChildren;

            //}
            
        }

        /// <summary>
        /// Handles Item's PropertyChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemPropertyChangedRaw(object? sender, PropertyChangedEventArgs e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Still handling events in disposed view-model");

            TypeStore.GetTypeStore(GetType()).Notify(this, e.PropertyName!);

            OnItemPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// When overridden in a derived class: Handles a PropertyChanged event of this VM's item
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnItemPropertyChanged(string? propertyName)
        {  }

        /// <summary>
        /// Raises the PropertyChanged event on the provided property
        /// </summary>
        /// <param name="propertyName"></param>
        protected internal void Notify([CallerMemberName] string propertyName = "")
        {
            DisposeChildren(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanged event on all provided properties
        /// </summary>
        /// <param name="propertyNames"></param>
        protected internal void Notify(params string[] propertyNames)
        {
            foreach (string i in propertyNames)
                Notify(i);
        }

        /// <summary>
        /// Creates and manages a PropertyChanged handler for the provided source. Use the DependsOnExtraProperty with the same key to set up extra handlers
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        protected void CreateExtraConnection(INotifyPropertyChanged source, string key)
        {
            if (ExtraConnections.ContainsKey(source))
                throw new ArgumentException("A connection has already been made for this source");

            PropertyChangedEventHandler handler = (s, e) => OnExtraConnectionPropertyChangedRaw(key, e.PropertyName);

            ExtraConnections[source] = handler;
            source.PropertyChanged += handler;
        }

        /// <summary>
        /// Handles an extra connection-source's PropertyChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExtraConnectionPropertyChangedRaw(string key, string? propertyName)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Still handling events in disposed view-model");

            TypeStore.GetTypeStore(GetType()).NotifyExtraConnection(this, propertyName!, key);

            OnExtraConnectionPropertyChanged(propertyName!, key);
        }

        /// <summary>
        /// When overridden in a derived class: Handles a PropertyChanged event of any of this VM's extra connections
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnExtraConnectionPropertyChanged(string propertyName, string key)
        {  }

        /// <summary>
        /// Internal information about child VMs and the property they are attached to
        /// </summary>
        private struct ChildVMInfo : IDisposable
        {
            /// <summary>
            /// Gets the name of the property that the VM comes from
            /// </summary>
            public string OriginPropertyName { get; }

            /// <summary>
            /// Gets the VM
            /// </summary>
            public IDisposable ChildVM { get; }

            /// <summary>
            /// Gets whether this child VM is disposed
            /// </summary>
            public bool IsDisposed { get; private set; }

            /// <summary>
            /// Creates a new ChildVMInfo object
            /// </summary>
            /// <param name="originPropertyName"></param>
            /// <param name="child"></param>
            public ChildVMInfo(string originPropertyName, IDisposable child)
            {
                OriginPropertyName = originPropertyName;
                ChildVM = child;
                IsDisposed = false;
            }

            /// <summary>
            /// <inheritdoc />
            /// </summary>
            /// <returns></returns>
            public static bool operator ==(ChildVMInfo left, ChildVMInfo right)
            {
                return left.ChildVM == right.ChildVM;
            }

            /// <summary>
            /// <inheritdoc />
            /// </summary>
            /// <returns></returns>
            public static bool operator !=(ChildVMInfo left, ChildVMInfo right)
            {
                return !(left == right);
            }

            /// <summary>
            /// <inheritdoc />
            /// </summary>
            /// <returns></returns>
            public override bool Equals(object? obj)
            {
                if (obj is ChildVMInfo cvmi)
                    return this == cvmi;

                return false;
            }

            /// <summary>
            /// <inheritdoc />
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return ChildVM.GetHashCode();
            }

            /// <summary>
            /// Disposes the underlaying VM
            /// </summary>
            public void Dispose()
            {
                ChildVM.Dispose();
                IsDisposed = true;
            }
        }
    }
}
