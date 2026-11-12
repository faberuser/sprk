using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NVespa;
using UnityEngine;

public class VespaNew
{
	[Conditional("UNITY_EDITOR")]
	public static void AddMem<T>(T item) where T : class
	{
		_ = item;
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

	public static IncreaseDic<TKey, TValue> NewIncreaseDic<TKey, TValue>()
	{
		return new IncreaseDic<TKey, TValue>();
	}

	public static IncreaseDic<TKey, TValue> NewIncreaseDic<TKey, TValue>(int capacity)
	{
		return new IncreaseDic<TKey, TValue>(capacity);
	}

	public static IncreaseDic<TKey, TValue> NewIncreaseDic<TKey, TValue>(IEqualityComparer<TKey> comparer)
	{
		return new IncreaseDic<TKey, TValue>(comparer);
	}

	public static IncreaseDic<TKey, TValue> NewIncreaseDic<TKey, TValue>(int capacity, IEqualityComparer<TKey> comparer)
	{
		return new IncreaseDic<TKey, TValue>(capacity, comparer);
	}

	public static IncreaseDic<TKey, TValue> NewIncreaseDic2<TKey, TValue>()
	{
		return NewIncreaseDic<TKey, TValue>();
	}

	public static IncreaseDic<TKey, TValue> NewIncreaseDic2<TKey, TValue>(int capacity)
	{
		return NewIncreaseDic<TKey, TValue>(capacity);
	}

	public static IncreaseDic<TKey, TValue> NewIncreaseDic2<TKey, TValue>(IEqualityComparer<TKey> comparer)
	{
		return NewIncreaseDic<TKey, TValue>(comparer);
	}

	public static IncreaseDic<TKey, TValue> NewIncreaseDic2<TKey, TValue>(int capacity, IEqualityComparer<TKey> comparer)
	{
		return NewIncreaseDic<TKey, TValue>(capacity, comparer);
	}

	public static CircularBuffer<T> NewCircularBuffer<T>(int capacity)
	{
		return new CircularBuffer<T>(capacity);
	}

	public static RenderingList<T> NewRenderingList<T>() where T : struct
	{
		return new RenderingList<T>();
	}

	public static RenderingList<T> NewRenderingList<T>(int cacheBaseCount) where T : struct
	{
		return new RenderingList<T>(cacheBaseCount);
	}

	public static VespaList<T> NewVespaList<T>()
	{
		return new VespaList<T>();
	}

	public static VespaList<T> NewVespaList<T>(int minReserveSize)
	{
		return new VespaList<T>(minReserveSize);
	}

	public static BetterList<T> NewBetterList<T>()
	{
		return new BetterList<T>();
	}

	public static Material NewMaterial(Shader shader)
	{
		return new Material(shader);
	}

	public static Material NewMaterial(Material source)
	{
		return new Material(source);
	}

	public static Material NewMaterial(string contents)
	{
		return new Material(contents);
	}

	public static GameObject NewGameObject()
	{
		return new GameObject();
	}

	public static GameObject NewGameObject(string name)
	{
		return new GameObject(name);
	}

	public static GameObject NewGameObject(string name, params Type[] components)
	{
		return new GameObject(name, components);
	}

	public static Mesh NewMesh()
	{
		return new Mesh();
	}
}
