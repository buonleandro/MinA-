using System;

public class MinHeap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentSize;

    public MinHeap(int maxSize)
    {
        items = new T[maxSize];
    }

    public int Count
    {
        get
        {
            return currentSize;
        }
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    public void Add(T item)
    {
        item.HeapIndex = currentSize;
        items[currentSize] = item;
        SortUp(item);
        currentSize++;
    }

    public T Pop()
    {
        T firstItem = items[0];
        currentSize--;
        items[0] = items[currentSize];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    void SortDown(T item)
    {
        while (true)
        {
            int leftChildIndex = item.HeapIndex * 2 + 1;
            int rightChildIndex = item.HeapIndex * 2 + 2;
            int minIndex = 0;

            if (leftChildIndex < currentSize)
            {
                minIndex = leftChildIndex;
                if (rightChildIndex < currentSize && items[leftChildIndex].CompareTo(items[rightChildIndex]) < 0)
                {
                    minIndex = rightChildIndex;
                }

                if (item.CompareTo(items[minIndex]) < 0)
                {
                    Swap(item, items[minIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
    
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}