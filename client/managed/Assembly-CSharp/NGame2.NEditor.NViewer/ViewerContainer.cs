using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGui;
using UnityEngine;

namespace NGame2.NEditor.NViewer;

public class ViewerContainer
{
	private List<IViewer> _list;

	private int _currentPage;

	public ViewerContainer()
	{
		_list = new List<IViewer>();
	}

	private bool IsContains<T>()
	{
		string name = typeof(T).Name;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			if (_list[i].Name == name)
			{
				return true;
			}
		}
		return false;
	}

	public void AddAccountViewer<TKey, TValue>(AccountAttrManager<TKey, TValue> manager) where TValue : IAccountAttrKey<TKey>
	{
		if (manager != null && !IsContains<TValue>())
		{
			IViewer viewer = SingletonAuto<ViewerFactory>.instance.CreateAccountViwer(manager);
			if (viewer != null)
			{
				_list.Add(viewer);
			}
		}
	}

	public void AddListViewer<TValue>(IEnumerable<TValue> values)
	{
		if (!IsContains<TValue>())
		{
			IViewer viewer = SingletonAuto<ViewerFactory>.instance.CreatetListViewer(values);
			if (viewer != null)
			{
				_list.Add(viewer);
			}
		}
	}

	public void DrawEditor()
	{
		if (_list == null || _list.Count == 0)
		{
			return;
		}
		int num = __Extension_Method_ICollection__.COUNT(_list);
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("<<", GUILayout.Width(30f)))
			{
				_currentPage = System.Math.Max(_currentPage - 1, 0);
			}
			GUILayout.Label($"Page : {_currentPage + 1} / {num}", GUILayout.Width(100f));
			if (GUILayout.Button(">>", GUILayout.Width(30f)))
			{
				int val = ((num != 0) ? (num - 1) : 0);
				_currentPage = System.Math.Min(_currentPage + 1, val);
			}
		}
		for (int i = 0; i < num; i++)
		{
			IViewer viewer = _list[i];
			if (i == _currentPage)
			{
				viewer?.DrawGUI();
			}
		}
	}
}
