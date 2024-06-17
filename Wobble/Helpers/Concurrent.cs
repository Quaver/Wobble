// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Emik
{
    /// <summary>
    /// Provides <see cref="List{T}"/>; the thread-safe equivalent of <see cref="System.Collections.Generic.List{T}"/>.
    /// </summary>
// ReSharper disable NullableWarningSuppressionIsUsed
    public static class Concurrent
    {
        /// <summary>Invoked when a lock acquisition fails.</summary>
        public static event Action OnLockFailure = () => { };

        /// <summary>Gets the interval before lock acquisition fails.</summary>
        public static TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>Represents a thread-safe <see cref="List{T}"/>.</summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        public sealed class List<T> : IList<T>, IList, IDisposable
        {
            private readonly System.Collections.Generic.List<T> _list;

            private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            /// <inheritdoc cref="System.Collections.Generic.List{T}()"/>
            public List() => _list = new System.Collections.Generic.List<T>();

            /// <inheritdoc cref="System.Collections.Generic.List{T}(int)"/>
            public List(int capacity) => _list = new System.Collections.Generic.List<T>(capacity);

            /// <inheritdoc cref="System.Collections.Generic.List{T}(IEnumerable{T})"/>
            public List(IEnumerable<T> collection) => _list = new System.Collections.Generic.List<T>(collection);

            /// <inheritdoc />
            bool IList.IsFixedSize => false;

            /// <inheritdoc />
            bool IList.IsReadOnly => false;

            /// <inheritdoc />
            bool ICollection<T>.IsReadOnly => false;

            /// <inheritdoc />
            bool ICollection.IsSynchronized => true;

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Capacity"/>
            public int Capacity
            {
                get => Read(l => l.Capacity);
                set => Write(value, (l, c) => l.Capacity = c);
            }

            public int Count => Read(l => l.Count);

            /// <inheritdoc />
            object ICollection.SyncRoot => this;

            /// <inheritdoc />
            object IList.this[int index]
            {
                get => Read(index, (l, c) => ((IList)l)[c]);
                set => Write((index, value), (l, c) => ((IList)l)[c.index] = c.value);
            }

            /// <inheritdoc />
            public T this[int index]
            {
                get => Read(index, (l, c) => l[c]);
                set => Write((index, value), (l, c) => l[c.index] = c.value);
            }

            /// <inheritdoc />
            public void Add(T item) => Write(item, (l, c) => l.Add(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.AddRange"/>
            public void AddRange(IEnumerable<T> collection) => Write(collection, (l, c) => l.AddRange(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Clear"/>
            public void Clear() => Write(l => l.Clear());

            /// <inheritdoc cref="System.Collections.Generic.List{T}.CopyTo(T[])"/>
            public void CopyTo(T[] array) => Read(array, (l, c) => l.CopyTo(c));

            /// <inheritdoc />
            public void CopyTo(T[] array, int arrayIndex) =>
                Read((array, arrayIndex), (l, c) => l.CopyTo(c.array, c.arrayIndex));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.CopyTo(int, T[], int, int)"/>
            public void CopyTo(int index, T[] array, int arrayIndex, int count) =>
                Read((index, array, arrayIndex, count), (l, c) => l.CopyTo(c.index, c.array, c.arrayIndex, c.count));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.CopyTo(int, T[], int, int)"/>
            void ICollection.CopyTo(Array array, int arrayIndex) =>
                Read((array, arrayIndex), (l, c) => ((ICollection)l).CopyTo(c.array, c.arrayIndex));

            /// <inheritdoc />
            public void Dispose() => _lock.Dispose();

            /// <inheritdoc />
            void IList.Insert(int index, object item) =>
                Write((index, item), (l, c) => ((IList)l).Insert(c.index, c.item));

            /// <inheritdoc />
            public void Insert(int index, T item) => Write((index, item), (l, c) => l.Insert(c.index, c.item));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.ForEach"/>
            public void ForEach(Action<T> action) => Read(action, (l, c) => l.ForEach(c));

            /// <inheritdoc />
            void IList.Remove(object item) => Write(item, (l, c) => ((IList)l).Remove(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.RemoveAll(Predicate{T})"/>
            public void RemoveAll(Predicate<T> match) => Write(match, (l, c) => l.RemoveAll(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.RemoveAt"/>
            public void RemoveAt(int index) => Write(index, (l, c) => l.RemoveAt(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.RemoveRange(int, int)"/>
            public void RemoveRange(int index, int count) =>
                Write((index, count), (l, c) => l.RemoveRange(c.index, c.count));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.InsertRange"/>
            public void InsertRange(int index, IEnumerable<T> collection) =>
                Write((index, collection), (l, c) => l.InsertRange(c.index, c.collection));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Reverse()"/>
            public void Reverse() => Write(l => l.Reverse());

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Reverse(int, int)"/>
            public void Reverse(int index, int count) =>
                Write((index, count), (l, c) => l.Reverse(c.index, c.count));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Sort()"/>
            public void Sort() => Write(l => l.Sort());

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Sort(Comparison{T})"/>
            public void Sort(Comparison<T> comparison) => Write(comparison, (l, c) => l.Sort(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Sort(IComparer{T})"/>
            public void Sort(IComparer<T> comparer) => Write(comparer, (l, c) => l.Sort(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Sort(int, int, IComparer{T})"/>
            public void Sort(int index, int count, IComparer<T> comparer) =>
                Write((index, count, comparer), (l, c) => l.Sort(c.index, c.count, c.comparer));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.TrimExcess"/>
            public void TrimExcess() => Write(l => l.TrimExcess());

            /// <inheritdoc />
            public bool Contains(T item) => Read(item, (l, c) => l.Contains(c));

            /// <inheritdoc />
            bool IList.Contains(object item) => Read(item, (l, c) => ((IList)l).Contains(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Exists"/>
            public bool Exists(Predicate<T> match) => Read(match, (l, c) => l.Exists(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Remove"/>
            public bool Remove(T item) => Write(item, (l, c) => l.Remove(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.TrueForAll"/>
            public bool TrueForAll(Predicate<T> match) => Read(match, (l, c) => l.TrueForAll(c));

            /// <inheritdoc />
            int IList.Add(object item) => Write(item, (l, c) => ((IList)l).Add(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.BinarySearch(T)"/>
            public int BinarySearch(T item) => Read(item, (l, c) => l.BinarySearch(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.BinarySearch(T, IComparer{T})"/>
            public int BinarySearch(T item, IComparer<T> comparer) =>
                Read((item, comparer), (l, c) => l.BinarySearch(c.item, c.comparer));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.BinarySearch(int, int, T, IComparer{T})"/>
            public int BinarySearch(int index, int count, T item, IComparer<T> comparer) =>
                Read((index, count, item, comparer), (l, c) => l.BinarySearch(c.index, c.count, c.item, c.comparer));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindIndex(Predicate{T})"/>
            public int FindIndex(Predicate<T> match) => Read(match, (l, c) => l.FindIndex(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindIndex(int, Predicate{T})"/>
            public int FindIndex(int startIndex, Predicate<T> match) =>
                Read((startIndex, match), (l, c) => l.FindIndex(c.startIndex, c.match));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindIndex(int, int, Predicate{T})"/>
            public int FindIndex(int startIndex, int count, Predicate<T> match) =>
                Read((startIndex, count, match), (l, c) => l.FindIndex(c.startIndex, c.count, c.match));

            /// <inheritdoc />
            int IList.IndexOf(object item) => Read(item, (l, c) => ((IList)l).IndexOf(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindLastIndex(Predicate{T})"/>
            public int FindLastIndex(Predicate<T> match) => Read(match, (l, c) => l.FindLastIndex(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindLastIndex(int, Predicate{T})"/>
            public int FindLastIndex(int startIndex, Predicate<T> match) =>
                Read((startIndex, match), (l, c) => l.FindLastIndex(c.startIndex, c.match));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindLastIndex(int, int, Predicate{T})"/>
            public int FindLastIndex(int startIndex, int count, Predicate<T> match) =>
                Read((startIndex, count, match), (l, c) => l.FindLastIndex(c.startIndex, c.count, c.match));

            /// <inheritdoc />
            public int IndexOf(T item) => Read(item, (l, c) => l.IndexOf(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.IndexOf(T, int)"/>
            public int IndexOf(T item, int index) => Read((item, index), (l, c) => l.IndexOf(c.item, c.index));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.IndexOf(T, int, int)"/>
            public int IndexOf(T item, int index, int count) =>
                Read((item, index, count), (l, c) => l.IndexOf(c.item, c.index, c.count));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.LastIndexOf(T)"/>
            public int LastIndexOf(T item) => Read(item, (l, c) => l.LastIndexOf(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.LastIndexOf(T, int)"/>
            public int LastIndexOf(T item, int index) =>
                Read((item, index), (l, c) => l.LastIndexOf(c.item, c.index));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.LastIndexOf(T, int, int)"/>
            public int LastIndexOf(T item, int index, int count) =>
                Read((item, index, count), (l, c) => l.LastIndexOf(c.item, c.index, c.count));

            /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
            public Enumerator GetEnumerator() => new Enumerator(this);

            /// <inheritdoc />
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindAll"/>
            public System.Collections.Generic.List<T> FindAll(Predicate<T> match) =>
                Read(match, (l, c) => l.FindAll(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.ConvertAll{TOutput}"/>
            public System.Collections.Generic.List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) =>
                Read(converter, (l, c) => l.ConvertAll(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.GetRange"/>
            public System.Collections.Generic.List<T> GetRange(int start, int length) =>
                Read((start, length), (l, c) => l.GetRange(c.start, c.length));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.Find"/>
            public T Find(Predicate<T> match) => Read(match, (l, c) => l.Find(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.FindLast"/>
            public T FindLast(Predicate<T> match) => Read(match, (l, c) => l.FindLast(c));

            /// <inheritdoc cref="System.Collections.Generic.List{T}.ToArray"/>
            public T[] ToArray() => Read(l => l.ToArray());

            private void Read<TContext>(TContext context, Action<System.Collections.Generic.List<T>, TContext> action)
            {
                if (!_lock.TryEnterReadLock(Timeout))
                {
                    OnLockFailure();
                    return;
                }

                try
                {
                    action(_list, context);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            private void Write(Action<System.Collections.Generic.List<T>> action)
            {
                if (!_lock.TryEnterWriteLock(Timeout))
                {
                    OnLockFailure();
                    return;
                }

                try
                {
                    action(_list);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            private void Write<TContext>(TContext context, Action<System.Collections.Generic.List<T>, TContext> action)
            {
                if (!_lock.TryEnterWriteLock(Timeout))
                {
                    OnLockFailure();
                    return;
                }

                try
                {
                    action(_list, context);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            private TResult Read<TResult>(Func<System.Collections.Generic.List<T>, TResult> converter)
            {
                if (!_lock.TryEnterReadLock(Timeout))
                {
                    OnLockFailure();
                    return default;
                }

                try
                {
                    return converter(_list);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            private TResult Read<TContext, TResult>(
                TContext context,
                Func<System.Collections.Generic.List<T>, TContext, TResult> converter
            )
            {
                if (!_lock.TryEnterReadLock(Timeout))
                {
                    OnLockFailure();
                    return default;
                }

                try
                {
                    return converter(_list, context);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            private TResult Write<TContext, TResult>(
                TContext context,
                Func<System.Collections.Generic.List<T>, TContext, TResult> action
            )
            {
                if (!_lock.TryEnterWriteLock(Timeout))
                {
                    OnLockFailure();
                    return default;
                }

                try
                {
                    return action(_list, context);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            /// <summary>Thread-safe enumerator for <see cref="List{T}"/>.</summary>
            public struct Enumerator : IEnumerator<T>
            {
                private System.Collections.Generic.List<T> _list;

                private bool _hasLock;

                private ReaderWriterLockSlim _lock;

                /// <summary>Initializes a new instance of the <see cref="Enumerator"/> struct.</summary>
                /// <param name="list">The list to enumerate and obtain a lock on.</param>
                public Enumerator(List<T> list)
                {
                    Current = default;
                    _list = list._list;
                    _lock = list._lock;
                    Index = -1;
                    _hasLock = false;
                }

                /// <summary>Gets the current index.</summary>
                public int Index { get; private set; }

                /// <inheritdoc />
                object IEnumerator.Current => Current;

                /// <inheritdoc />
                public T Current { get; private set; }

                /// <inheritdoc />
                public void Dispose()
                {
                    if (_hasLock)
                    {
                        _lock?.ExitReadLock();
                        _hasLock = false;
                    }

                    _lock = null;
                    _list = null;
                }

                /// <inheritdoc />
                public void Reset() => Index = 0;

                /// <inheritdoc />
                public bool MoveNext()
                {
                    if (_lock is null)
                        return false;

                    if (Index is -1)
                    {
                        if (!_lock.TryEnterReadLock(Timeout))
                        {
                            OnLockFailure();
                            return false;
                        }

                        _hasLock = true;
                        Index++;
                    }

                    if (Index < _list.Count)
                    {
                        Current = _list[Index++];
                        return true;
                    }

                    Dispose();
                    return false;
                }
            }
        }
    }
}