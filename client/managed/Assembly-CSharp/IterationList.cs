using System.Collections;
using System.Collections.Generic;

public class IterationList<TList, T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where TList : IList<T>, new()
{
	private TList _list;

	bool ICollection<T>.IsReadOnly => _list.IsReadOnly;

	public int Count => _list.Count;

	public int Index { get; private set; }

	public T this[int i]
	{
		get
		{
			return _list[i];
		}
		set
		{
			_list[i] = value;
		}
	}

	public IterationList()
	{
		_list = new TList();
	}

	public IterationList(TList list)
	{
		_list = list;
	}

	public bool IterateBegin(int beginIndex)
	{
		Index = beginIndex;
		return Vespa_Generic_IList_Extension_Method.IsValidIndex(_list, beginIndex);
	}

	public bool IterateNext(int step = 1)
	{
		Index += step;
		return Vespa_Generic_IList_Extension_Method.IsValidIndex(_list, Index);
	}

	public bool IterateValid()
	{
		return Vespa_Generic_IList_Extension_Method.IsValidIndex(_list, Index);
	}

	public T GetCurrent()
	{
		ref TList list = ref _list;
		int index = Index;
		return list[index];
	}

	public void RemoveCurrent()
	{
		RemoveAt(Index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Vespa_Common_Class_Extension_Method.Interface((IEnumerable)_list).GetEnumerator();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	public void Clear()
	{
		_list.Clear();
	}

	public void Add(T item)
	{
		_list.Add(item);
	}

	public void Insert(int pos, T item)
	{
		_list.Insert(pos, item);
		if (pos <= Index)
		{
			Index++;
		}
	}

	public bool Contains(T item)
	{
		return _list.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_list.CopyTo(array, arrayIndex);
	}

	public int IndexOf(T item)
	{
		return _list.IndexOf(item);
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num < 0)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public void RemoveAt(int indexAt)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_list, indexAt))
		{
			_list.RemoveAt(indexAt);
			if (indexAt <= Index)
			{
				Index--;
			}
		}
	}
}
