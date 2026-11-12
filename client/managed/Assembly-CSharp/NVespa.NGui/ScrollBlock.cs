using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NGui;

public class ScrollBlock : IDisposable
{
	private static Dictionary<Tuple<object, string>, Vector2> s_scrollDic = new Dictionary<Tuple<object, string>, Vector2>();

	private bool _beginScroll;

	public static Vector2 GetScroll(object i_keyObj, string i_keyName)
	{
		Tuple<object, string> key = Tuple.Create(i_keyObj, i_keyName);
		Vector2 value;
		lock (s_scrollDic)
		{
			if (!s_scrollDic.TryGetValue(key, out value))
			{
				value = Vector2.zero;
				s_scrollDic.Add(key, Vector2.zero);
			}
		}
		return value;
	}

	public static void SetScroll(object i_keyObj, string i_keyName, Vector2 i_scrollPos)
	{
		lock (s_scrollDic)
		{
			s_scrollDic[Tuple.Create(i_keyObj, i_keyName)] = i_scrollPos;
		}
	}

	public ScrollBlock()
	{
		if (Event.current != null && Event.current.type == EventType.Used)
		{
			_beginScroll = false;
			return;
		}
		GUILayout.BeginScrollView(Vector2.zero, false, false);
		_beginScroll = true;
	}

	public ScrollBlock(object i_keyObj, string i_keyName)
	{
		if (Event.current != null && Event.current.type == EventType.Used)
		{
			_beginScroll = false;
			return;
		}
		Vector2 scroll = GetScroll(i_keyObj, i_keyName);
		scroll = GUILayout.BeginScrollView(scroll);
		SetScroll(i_keyObj, i_keyName, scroll);
		_beginScroll = true;
	}

	public ScrollBlock(object i_keyObj, string i_keyName, bool alwaysShowHorizontal, bool alwaysShowVertical)
	{
		if (Event.current != null && Event.current.type == EventType.Used)
		{
			_beginScroll = false;
			return;
		}
		Vector2 scroll = GetScroll(i_keyObj, i_keyName);
		scroll = GUILayout.BeginScrollView(scroll, alwaysShowHorizontal, alwaysShowVertical);
		SetScroll(i_keyObj, i_keyName, scroll);
		_beginScroll = true;
	}

	public virtual void Dispose()
	{
		if (!_beginScroll)
		{
			return;
		}
		_beginScroll = false;
		try
		{
			GUILayout.EndScrollView();
		}
		catch (InvalidOperationException ex)
		{
			Debug.LogWarning($"Exception: {Event.current.rawType}\n{ex.Message}");
		}
	}
}
