using System;
using System.Collections.Generic;

namespace NGame2.NBattleField.NManager.NUtil;

public class PendingContainer<T>
{
	private Dictionary<T, DateTime> _container = new Dictionary<T, DateTime>();

	public bool IsValidInstance { get; private set; }

	public float PendingExpireSeconds { get; private set; }

	public void SetValid(bool isValid)
	{
		IsValidInstance = isValid;
	}

	public void SetExpireTime(float pendingExpireTime)
	{
		PendingExpireSeconds = pendingExpireTime;
	}

	public void Add(T key)
	{
		if (key != null)
		{
			_container[key] = DateTime.UtcNow;
		}
	}

	public void Remove(T key)
	{
		if (key != null)
		{
			_container.Remove(key);
		}
	}

	public bool IsPending(T key)
	{
		if (!_container.TryGetValue(key, out var value))
		{
			return false;
		}
		return (value - DateTime.UtcNow).TotalSeconds < (double)PendingExpireSeconds;
	}

	public void Clear()
	{
		_container.Clear();
	}
}
