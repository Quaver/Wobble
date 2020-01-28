using System;
using System.Collections.Generic;

namespace Wobble.Bindables
{
    public class BindableList<T> : Bindable<List<T>>
    {
        /// <summary>
        ///     Event invoked when an item has been added to the list
        /// </summary>
        public EventHandler<BindableListItemAddedEventArgs<T>> ItemAdded;

        /// <summary>
        ///     Event invoked when an item has been removed from the list.
        /// </summary>
        public EventHandler<BindableListItemRemovedEventArgs<T>> ItemRemoved;

        /// <summary>
        ///     Event invoked when multiple items have been added to the list when calling <see cref="AddRange"/>
        /// </summary>
        public EventHandler<BindableListMultipleItemsAddedEventArgs<T>> MultipleItemsAdded;

        /// <summary>
        ///    Event invoked when the list has been completely cleared of all elements
        /// </summary>
        public EventHandler<BindableListClearedEventArgs> ListCleared;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="action"></param>
        public BindableList(List<T> defaultVal, EventHandler<BindableValueChangedEventArgs<List<T>>> action = null) : base(defaultVal, action)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultVal"></param>
        /// <param name="action"></param>
        public BindableList(string name, List<T> defaultVal, EventHandler<BindableValueChangedEventArgs<List<T>>> action = null) : base(name, defaultVal, action)
        {
        }

        /// <summary>
        ///     Adds an object to the list and invokes <see cref="ItemAdded"/>
        /// </summary>
        /// <param name="obj"></param>
        public void Add(T obj)
        {
            Value.Add(obj);
            ItemAdded?.Invoke(this, new BindableListItemAddedEventArgs<T>(obj));
        }

        /// <summary>
        ///     Removes an object from the list and invokes <see cref="ItemRemoved"/>
        /// </summary>
        /// <param name="obj"></param>
        public void Remove(T obj)
        {
            Value.Remove(obj);
            ItemRemoved?.Invoke(this, new BindableListItemRemovedEventArgs<T>(obj));
        }

        /// <summary>
        ///     Adds multiple objects to the list and invokes <see cref="MultipleItemsAdded"/>
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(List<T> list)
        {
            Value.AddRange(list);
            MultipleItemsAdded?.Invoke(this, new BindableListMultipleItemsAddedEventArgs<T>(list));
        }

        /// <summary>
        ///     Clears an object from the list and invokes
        /// </summary>
        public void Clear()
        {
            Value.Clear();
            ListCleared?.Invoke(this, new BindableListClearedEventArgs());
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableListItemAddedEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     The object that was added to the list
        /// </summary>
        public T Item { get; }

        public BindableListItemAddedEventArgs(T item) => Item = item;
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableListItemRemovedEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     The object that was removed from the list
        /// </summary>
        public T Item { get; }

        public BindableListItemRemovedEventArgs(T item) => Item = item;
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableListMultipleItemsAddedEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     The list of objects that were added to the list
        /// </summary>
        public List<T> Items { get; }

        public BindableListMultipleItemsAddedEventArgs(List<T> items) => Items = items;
    }

    /// <summary>
    /// </summary>
    public class BindableListClearedEventArgs : EventArgs
    {
    }
}