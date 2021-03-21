using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021.Pathfinding.Dijkstra
{
    class FibonacciMap<T, TKey> where TKey : IComparable<TKey>
    {
        FibonacciHeap<T, TKey> Heap;
        Dictionary<T, FibonacciHeapNode<T, TKey>> HeapLookup;

        public int Count => Heap.Size();

        public FibonacciMap(TKey min)
        {
            Heap = new FibonacciHeap<T, TKey>(min);
            HeapLookup = new Dictionary<T, FibonacciHeapNode<T, TKey>>();
        }

        public void Insert(T value, TKey key)
        {
            var node = new FibonacciHeapNode<T, TKey>(value, key);
            Heap.Insert(node);
            HeapLookup.Add(value, node);
        }

        public T RemoveMin()
        {
            return Heap.RemoveMin().Data;
        }

        public void DecreaseKey(T value, TKey key)
        {
            var node = HeapLookup[value];
            Heap.DecreaseKey(node, key);
        }
    }
}
