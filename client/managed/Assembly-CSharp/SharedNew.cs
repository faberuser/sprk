using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class SharedNew
{
	[Conditional("UNITY_EDITOR")]
	public static void AddMem<T>(T item) where T : class
	{
	}

	public static T New<T>() where T : class, new()
	{
		return new T();
	}

	public static ArrayList NewArrayList()
	{
		return new ArrayList();
	}

	public static ArrayList NewArrayList(ICollection collection)
	{
		return new ArrayList(collection);
	}

	public static ArrayList NewArrayList(int capacity)
	{
		return new ArrayList(capacity);
	}

	public static Queue NewNativeQueue()
	{
		return new Queue();
	}

	public static List<T> NewList<T>()
	{
		return new List<T>();
	}

	public static List<T> NewList<T>(IEnumerable<T> collection)
	{
		return new List<T>(collection);
	}

	public static List<T> NewList<T>(int capacity)
	{
		return new List<T>(capacity);
	}

	public static LinkedList<T> NewLinkedList<T>()
	{
		return new LinkedList<T>();
	}

	public static LinkedList<T> NewLinkedList<T>(IEnumerable<T> collection)
	{
		return new LinkedList<T>(collection);
	}

	public static Queue<T> NewQueue<T>()
	{
		return new Queue<T>();
	}

	public static Queue<T> NewQueue<T>(IEnumerable<T> collection)
	{
		return new Queue<T>(collection);
	}

	public static Queue<T> NewQueue<T>(int capacity)
	{
		return new Queue<T>(capacity);
	}

	public static Stack<T> NewStack<T>()
	{
		return new Stack<T>();
	}

	public static Stack<T> NewStack<T>(IEnumerable<T> collection)
	{
		return new Stack<T>(collection);
	}

	public static Stack<T> NewStack<T>(int capacity)
	{
		return new Stack<T>(capacity);
	}

	public static HashSet<T> NewHashSet<T>()
	{
		return new HashSet<T>();
	}

	public static HashSet<T> NewHashSet<T>(IEnumerable<T> collection)
	{
		return new HashSet<T>(collection);
	}

	public static HashSet<T> NewHashSet<T>(IEqualityComparer<T> comparer)
	{
		return new HashSet<T>(comparer);
	}

	public static HashSet<T> NewHashSet<T>(IEnumerable<T> collection, IEqualityComparer<T> comparer)
	{
		return new HashSet<T>(collection, comparer);
	}

	public static Dictionary<TKey, TValue> NewDictionary<TKey, TValue>()
	{
		return new Dictionary<TKey, TValue>();
	}

	public static Dictionary<TKey, TValue> NewDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
	{
		return new Dictionary<TKey, TValue>(dictionary);
	}

	public static Dictionary<TKey, TValue> NewDictionary<TKey, TValue>(IEqualityComparer<TKey> comparer)
	{
		return new Dictionary<TKey, TValue>(comparer);
	}

	public static Dictionary<TKey, TValue> NewDictionary<TKey, TValue>(int capacity)
	{
		return new Dictionary<TKey, TValue>(capacity);
	}

	public static Dictionary<TKey, TValue> NewDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
	{
		return new Dictionary<TKey, TValue>(dictionary, comparer);
	}

	public static Dictionary<TKey, TValue> NewDictionary<TKey, TValue>(int capacity, IEqualityComparer<TKey> comparer)
	{
		return new Dictionary<TKey, TValue>(capacity, comparer);
	}

	public static SortedDictionary<TKey, TValue> NewSortedDictionary<TKey, TValue>()
	{
		return new SortedDictionary<TKey, TValue>();
	}

	public static SortedDictionary<TKey, TValue> NewSortedDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
	{
		return new SortedDictionary<TKey, TValue>(dictionary);
	}

	public static SortedDictionary<TKey, TValue> NewSortedDictionary<TKey, TValue>(IComparer<TKey> comparer)
	{
		return new SortedDictionary<TKey, TValue>(comparer);
	}

	public static SortedDictionary<TKey, TValue> NewSortedDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
	{
		return new SortedDictionary<TKey, TValue>(dictionary, comparer);
	}
}
