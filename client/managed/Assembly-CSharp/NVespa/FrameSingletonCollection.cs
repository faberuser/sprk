using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa;

public static class FrameSingletonCollection
{
	private static int _frameCount;

	private static Dictionary<Type, IFrameSingleton> _dic = new Dictionary<Type, IFrameSingleton>();

	private static int CurrentFrameCount
	{
		get
		{
			if (!SystemStatus.IsPlaying)
			{
				return Time.renderedFrameCount;
			}
			return Time.frameCount;
		}
	}

	public static void ChainUpdate()
	{
		lock (_dic)
		{
			if (_frameCount != Time.frameCount)
			{
				_dic.Clear();
				_frameCount = Time.frameCount;
			}
		}
	}

	public static bool IsValidInstance<T>() where T : class, IFrameSingleton, new()
	{
		lock (_dic)
		{
			if (_frameCount != Time.frameCount)
			{
				return false;
			}
			return _dic.ContainsKey(typeof(T));
		}
	}

	public static T GetInstance<T>() where T : class, IFrameSingleton, new()
	{
		lock (_dic)
		{
			if (_frameCount != Time.frameCount)
			{
				_dic.Clear();
				_frameCount = Time.frameCount;
			}
			Type typeFromHandle = typeof(T);
			if (!_dic.TryGetValue(typeFromHandle, out var value))
			{
				T val = new T();
				_dic.Add(typeFromHandle, val);
				return val;
			}
			return value as T;
		}
	}

	public static void ReleaseInstance<T>()
	{
		lock (_dic)
		{
			Type typeFromHandle = typeof(T);
			_dic.Remove(typeFromHandle);
		}
	}
}
