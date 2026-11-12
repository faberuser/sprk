using System;
using System.Collections.Generic;

namespace NGame2.NUI.NWindow;

public class WindowOpenQueue<T> where T : BaseWindow
{
	private T _currentWindow;

	private Queue<Tuple<T, Action<T>>> _windowOpenQueue = new Queue<Tuple<T, Action<T>>>();

	public void Add(T window, Action<T> openAction)
	{
		_windowOpenQueue.Enqueue(new Tuple<T, Action<T>>(window, openAction));
	}

	public void Update()
	{
		if (_windowOpenQueue.Count != 0 && (!(_currentWindow != null) || !_currentWindow.IsOpen()))
		{
			Tuple<T, Action<T>> tuple = _windowOpenQueue.Dequeue();
			T item = tuple.Item1;
			Vespa_System_Action_Extension_Method.fire(tuple.Item2, item);
			_currentWindow = item;
		}
	}

	public void Clear()
	{
		_windowOpenQueue.Clear();
	}

	public bool IsEmpty()
	{
		return _windowOpenQueue.Count == 0;
	}

	public void Release()
	{
		if (_currentWindow != null)
		{
			_currentWindow.Close();
		}
		Clear();
	}
}
