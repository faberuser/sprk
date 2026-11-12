using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MessagePack;
using NGame2.NConsole;
using NGame2.NEditor.NViewer.NBase;
using NVespa;
using NVespa.NGui;
using UnityEngine;

namespace NGame2.NEditor.NViewer;

public abstract class BaseAbstarctViewer2<TValue> : IViewer, IProvider<TValue>, IProvider
{
	private static StringBuilder _sb;

	protected string _name;

	private PropertyInfo[] _cachedPropertyInfos;

	private LocalEditorBool _showHead = new LocalEditorBool(i_default: true, "Editor_Viewer_" + typeof(TValue).Name);

	protected List<TValue> _cachedList;

	private NGame2.NEditor.NViewer.NBase.SimplePage _page;

	private NGame2.NEditor.NViewer.NBase.ViewerSort<TValue> _sort;

	private ViewerSearch<TValue> _search;

	private ViewerProperties<TValue> _properties;

	protected static StringBuilder Sb
	{
		get
		{
			if (_sb == null)
			{
				_sb = new StringBuilder(1000);
			}
			return _sb;
		}
	}

	string IViewer.Name => _name;

	public BaseAbstarctViewer2()
	{
		_name = typeof(TValue).Name;
	}

	public abstract void Set(object obj);

	protected abstract void Refresh();

	protected void Initilize()
	{
		InitCachedPropertyInfos();
		if (_page == null)
		{
			_page = new NGame2.NEditor.NViewer.NBase.SimplePage(this);
		}
		if (_search == null)
		{
			_search = new ViewerSearch<TValue>(this);
		}
		if (_sort == null)
		{
			_sort = new NGame2.NEditor.NViewer.NBase.ViewerSort<TValue>(this);
		}
		if (_properties == null)
		{
			_properties = new ViewerProperties<TValue>(this);
		}
	}

	private void InitCachedPropertyInfos()
	{
		if (_cachedPropertyInfos != null)
		{
			return;
		}
		PropertyInfo[] properties = typeof(TValue).GetProperties();
		List<PropertyInfo> list = new List<PropertyInfo>(properties.Length);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(properties); i++)
		{
			PropertyInfo propertyInfo = properties[i];
			if (!(propertyInfo == null) && __Extension_Method_ICollection__.IsNullOrEmpty(propertyInfo.GetCustomAttributes(typeof(IgnoreMemberAttribute), inherit: false)))
			{
				list.Add(propertyInfo);
			}
		}
		_cachedPropertyInfos = list.ToArray();
	}

	protected void InitCachedList()
	{
		if (_cachedList == null)
		{
			_cachedList = new List<TValue>();
		}
		else
		{
			_cachedList.Clear();
		}
	}

	protected void SetCachedList(IEnumerable<TValue> values)
	{
		if (values != null)
		{
			_cachedList.AddRange(values);
			_sort.Sort();
		}
	}

	protected virtual TValue DrawValue(int index, TValue value)
	{
		if (value != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_cachedPropertyInfos); i++)
			{
				PropertyInfo propertyInfo = _cachedPropertyInfos[i];
				object value2 = propertyInfo.GetValue(value, null);
				Sb.Append((i + 1).ToString()).Append(". ");
				Sb.Append(propertyInfo.Name);
				if (value2 != null)
				{
					Sb.Append("  :  ").Append(value2.ToString());
				}
				else
				{
					Sb.Append("  :  (null)");
				}
				if (i < __Extension_Method_ICollection__.COUNT(_cachedPropertyInfos) - 1)
				{
					Sb.AppendLine();
				}
			}
			GUILayout.Label(Sb.ToString());
			Sb.Length = 0;
		}
		return value;
	}

	protected virtual string GetBtnName()
	{
		if (_cachedList != null)
		{
			return "초기화";
		}
		return "초기화";
	}

	private void DrawHead()
	{
		using (new BlockManager().Vertical(GUI.skin.box))
		{
			DrawInfo();
			if (_cachedList == null)
			{
				return;
			}
			if (_page != null)
			{
				_page.DrawGUI();
			}
			if (_showHead.Value)
			{
				if (_search != null)
				{
					_search.DrawGUI();
				}
				if (_sort != null)
				{
					_sort.DrawGUI();
				}
				if (_properties != null)
				{
					_properties.DrawGUI();
				}
			}
		}
	}

	private void DrawInfo()
	{
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label($"{_name} 개수 : ({__Extension_Method_ICollection__.COUNT(_cachedList)})");
			string name = (_showHead.Value ? "기능 감추기" : "기능 보기");
			_showHead.Value = DrawHelper.ToggleAutoSize(name, _showHead.Value);
			if (GUILayout.Button(GetBtnName(), GUILayout.Width(80f)))
			{
				Refresh();
			}
		}
	}

	private void DrawBody()
	{
		if (_cachedList == null)
		{
			return;
		}
		using (new BlockManager().Scroll(this, _name))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_cachedList); i++)
			{
				TValue value = _cachedList[i];
				if (!CanDrawValue(i, value))
				{
					continue;
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label((i + 1).ToString(), GUI.skin.box, GUILayout.Width(50f));
					using (new BlockManager().Vertical(GUI.skin.box))
					{
						_cachedList[i] = DrawValue(i, value);
					}
				}
			}
		}
	}

	private bool CanDrawValue(int index, TValue value)
	{
		if (value == null)
		{
			return false;
		}
		if (_page == null)
		{
			return true;
		}
		return _page.IsInner(index);
	}

	void IViewer.DrawGUI()
	{
		DrawTools.DrawSeparator(Color.gray);
		DrawHead();
		DrawTools.DrawSeparator(Color.gray);
		DrawBody();
	}

	int IProvider.GetListCount()
	{
		return __Extension_Method_ICollection__.COUNT(_cachedList);
	}

	PropertyInfo[] IProvider.GeCachedtPropertyInfos()
	{
		return _cachedPropertyInfos;
	}

	PropertyInfo IProvider.GetPropertyInfo(int index)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_cachedPropertyInfos, index))
		{
			return _cachedPropertyInfos[index];
		}
		return null;
	}

	void IProvider.RemoveProperty(int index)
	{
		if (RemoveCachedProperty(index))
		{
			_search.Refresh();
			_sort.Refresh();
			_properties.Refresh();
		}
	}

	void IProvider.ResetProperties()
	{
		_cachedPropertyInfos = null;
		InitCachedPropertyInfos();
		_search.Refresh();
		_sort.Refresh();
		_properties.Refresh();
	}

	private bool RemoveCachedProperty(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_cachedPropertyInfos, index))
		{
			return false;
		}
		int num = __Extension_Method_ICollection__.COUNT(_cachedPropertyInfos) - 1;
		if (num < 1)
		{
			return false;
		}
		PropertyInfo[] array = new PropertyInfo[num];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_cachedPropertyInfos); i++)
		{
			if (i < index)
			{
				array[i] = _cachedPropertyInfos[i];
			}
			else if (i > index)
			{
				array[i - 1] = _cachedPropertyInfos[i];
			}
		}
		_cachedPropertyInfos = array;
		return true;
	}

	List<TValue> IProvider<TValue>.GetList()
	{
		return _cachedList;
	}

	protected void Cheat(string cheatMessage, Action<string> response)
	{
		if (!SingletonAuto<ConsoleInputManager>.isValidInstance)
		{
			return;
		}
		Action<string> action = null;
		if (response != null)
		{
			action = response;
			action = (Action<string>)Delegate.Combine(action, (Action<string>)delegate
			{
				Refresh();
			});
		}
		else
		{
			action = delegate
			{
				Refresh();
			};
		}
		SingletonAuto<ConsoleInputManager>.instance.OnConsoleInput(cheatMessage, action);
	}
}
