using System;
using System.Collections.Generic;
using NShared;
using NVespa;

namespace NGame2.NEditor.NViewer;

public class ViewerFactory : SingletonAuto<ViewerFactory>
{
	private Dictionary<string, Func<IViewer>> _dicViewer;

	public ViewerFactory()
	{
		_dicViewer = new Dictionary<string, Func<IViewer>>();
		Regist<AttendanceDataViewer>();
		Regist<PetInfoViewer>();
		Regist<PetExploreInfoViewer>();
	}

	public IViewer CreatetListViewer<TValue>(IEnumerable<TValue> values)
	{
		if (values == null)
		{
			return null;
		}
		string name = typeof(TValue).Name;
		_dicViewer.TryGetValue(name, out var value);
		IViewer viewer = Vespa_System_Action_Extension_Method.fire(value);
		if (viewer == null)
		{
			return new ListDefaultViewer<TValue>(values);
		}
		viewer.Set(values);
		return viewer;
	}

	public IViewer CreateAccountViwer<TKey, TValue>(AccountAttrManager<TKey, TValue> manager) where TValue : IAccountAttrKey<TKey>
	{
		if (manager == null)
		{
			return null;
		}
		string name = typeof(TValue).Name;
		_dicViewer.TryGetValue(name, out var value);
		IViewer viewer = Vespa_System_Action_Extension_Method.fire(value);
		if (viewer == null)
		{
			return new UserDefaultViewer<TKey, TValue>(manager);
		}
		viewer.Set(manager);
		return viewer;
	}

	private void Regist<T>() where T : IViewer, new()
	{
		T val = new T();
		if (val != null)
		{
			string name = val.Name;
			Regist<T>(name);
		}
	}

	private void Regist<T>(string key) where T : IViewer, new()
	{
		if (key != null && !String_Extension_Method.IsNullOrEmpty(key))
		{
			_dicViewer.Remove(key);
			_dicViewer.Add(key, () => new T());
		}
	}
}
