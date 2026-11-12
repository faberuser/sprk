using System.Collections;
using System.Collections.Generic;

namespace MiniJSON;

public class JsonPool
{
	private static object Sync = new object();

	private static JsonPool _instance;

	private Queue<List<object>> _listQueue = new Queue<List<object>>();

	private Queue<Dictionary<string, object>> _dicQueue = new Queue<Dictionary<string, object>>();

	public static JsonPool instance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	public static void CreateInstance()
	{
		if (_instance != null)
		{
			return;
		}
		lock (Sync)
		{
			if (_instance == null)
			{
				_instance = new JsonPool();
			}
		}
	}

	public List<object> CreateList()
	{
		lock (_listQueue)
		{
			if (_listQueue.Count > 0)
			{
				return _listQueue.Dequeue();
			}
		}
		return new List<object>();
	}

	public List<object> CreateList(int capacity)
	{
		List<object> list = CreateList();
		if (list.Capacity < capacity)
		{
			list.Capacity = capacity;
		}
		return list;
	}

	public Dictionary<string, object> CreateDic(int capacity = 0)
	{
		lock (_dicQueue)
		{
			if (_dicQueue.Count > 0)
			{
				return _dicQueue.Dequeue();
			}
		}
		return new Dictionary<string, object>(capacity);
	}

	private void ReleaseList(IList list)
	{
		if (!(list is List<object> list2))
		{
			return;
		}
		list2.Clear();
		lock (_listQueue)
		{
			_listQueue.Enqueue(list2);
		}
	}

	private void ReleaseDic(IDictionary dic)
	{
		if (!(dic is Dictionary<string, object> dictionary))
		{
			return;
		}
		dictionary.Clear();
		lock (_dicQueue)
		{
			_dicQueue.Enqueue(dictionary);
		}
	}

	public void Release(object value)
	{
		if (value is IList list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				object value2 = list[i];
				Release(value2);
			}
			ReleaseList(list);
		}
		else
		{
			if (!(value is IDictionary dictionary))
			{
				return;
			}
			foreach (object value3 in dictionary.Values)
			{
				Release(value3);
			}
			ReleaseDic(dictionary);
		}
	}

	public void OnSceneChanged()
	{
		lock (_listQueue)
		{
			_listQueue.Clear();
		}
		lock (_dicQueue)
		{
			_dicQueue.Clear();
		}
	}
}
