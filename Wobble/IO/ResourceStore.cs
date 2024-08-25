﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wobble.IO
{
    public class ResourceStore<T> : IResourceStore<T>
    {
        private readonly Dictionary<string, Action> actionList = new Dictionary<string, Action>();

        private readonly List<IResourceStore<T>> stores = new List<IResourceStore<T>>();

        private readonly List<string> searchExtensions = new List<string>();

        /// <summary>
        /// Initializes a resource store with no stores.
        /// </summary>
        public ResourceStore()
        {
        }

        /// <summary>
        /// Initializes a resource store with a single store.
        /// </summary>
        /// <param name="store">The store.</param>
        public ResourceStore(IResourceStore<T> store = null)
        {
            if (store != null)
                AddStore(store);
        }

        /// <summary>
        /// Initializes a resource store with a collection of stores.
        /// </summary>
        /// <param name="stores">The collection of stores.</param>
        public ResourceStore(IResourceStore<T>[] stores)
        {
            foreach (var resourceStore in stores.Cast<ResourceStore<T>>())
                AddStore(resourceStore);
        }

        /// <summary>
        /// Notifies a bound delegate that the resource has changed.
        /// </summary>
        /// <param name="name">The resource that has changed.</param>
        protected virtual void NotifyChanged(string name)
        {
            if (!actionList.TryGetValue(name, out var action))
                return;

            action?.Invoke();
        }

        /// <summary>
        /// Adds a resource store to this store.
        /// </summary>
        /// <param name="store">The store to add.</param>
        public virtual void AddStore(IResourceStore<T> store)
        {
            lock (stores)
                stores.Add(store);
        }

        /// <summary>
        /// Removes a store from this store.
        /// </summary>
        /// <param name="store">The store to remove.</param>
        public virtual void RemoveStore(IResourceStore<T> store)
        {
            lock (stores)
                stores.Remove(store);
        }

        /// <summary>
        /// Retrieves an object from the store.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <returns>The object.</returns>
        public virtual async Task<T> GetAsync(string name)
        {
            var filenames = GetFilenames(name);

            // required for locking
            IResourceStore<T>[] localStores;

            lock (stores)
                localStores = stores.ToArray();

            // Cache miss - get the resource
            foreach (var store in localStores)
                foreach (var f in filenames)
                {
                    var result = await store.GetAsync(f);
                    if (result != null)
                        return result;
                }

            return default;
        }

        /// <summary>
        /// Retrieves an object from the store.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <returns>The object.</returns>
        public virtual T Get(string name)
        {
            if (name is null)
                return default;

            var filenames = GetFilenames(name);

            // Cache miss - get the resource
            lock (stores)
                foreach (var store in stores)
                    foreach (var f in filenames)
                    {
                        var result = store.Get(f);

                        if (result != null)
                            return result;
                    }

            return default;
        }

        public Stream GetStream(string name)
        {
            var filenames = GetFilenames(name);

            // Cache miss - get the resource
            lock (stores)
                foreach (var store in stores)
                    foreach (var f in filenames)
                    {
                        try
                        {
                            var result = store.GetStream(f);
                            if (result != null)
                                return result;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

            return null;
        }

        protected virtual IEnumerable<string> GetFilenames(string name)
        {
            yield return name;

            if (name.Contains(@".")) yield break;

            //add file extension if it's missing.
            foreach (var ext in searchExtensions)
                yield return $@"{name}.{ext}";
        }

        /// <summary>
        /// Binds a reload function to an object held by the store.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="onReload">The reload function to bind.</param>
        public void BindReload(string name, Action onReload)
        {
            if (onReload == null)
                return;

            // Check if there's already a reload action bound
            if (actionList.ContainsKey(name))
                throw new InvalidOperationException($"A reload delegate is already bound to the resource '{name}'.");

            actionList[name] = onReload;
        }

        /// <summary>
        /// Add a file extension to automatically append to any lookups on this store.
        /// </summary>
        public void AddExtension(string extension)
        {
            extension = extension.Trim('.');

            if (!searchExtensions.Contains(extension))
                searchExtensions.Add(extension);
        }

        #region IDisposable Support

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                lock (stores) stores.ForEach(s => s.Dispose());
            }
        }

        ~ResourceStore() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
