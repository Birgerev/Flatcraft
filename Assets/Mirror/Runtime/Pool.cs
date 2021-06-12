// Pool to avoid allocations (from libuv2k)

using System;
using System.Collections.Generic;

namespace Mirror
{
    public class Pool<T>
    {
        // some types might need additional parameters in their constructor, so
        // we use a Func<T> generator
        private readonly Func<T> objectGenerator;

        // Mirror is single threaded, no need for concurrent collections
        private readonly Stack<T> objects = new Stack<T>();

        public Pool(Func<T> objectGenerator)
        {
            this.objectGenerator = objectGenerator;
        }

        // count to see how many objects are in the pool. useful for tests.
        public int Count => objects.Count;

        // take an element from the pool, or create a new one if empty
        public T Take()
        {
            return objects.Count > 0 ? objects.Pop() : objectGenerator();
        }

        // return an element to the pool
        public void Return(T item)
        {
            objects.Push(item);
        }
    }
}