using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NContents;

public class ContentsValueManager : SingletonAuto<ContentsValueManager>
{
	private Dictionary<string, ContentsValueInfo> _dic = new Dictionary<string, ContentsValueInfo>();

	private readonly UserObserverManager<ContentsValueInfo> _observerManager;

	public Dictionary<string, ContentsValueInfo> Dic
	{
		get
		{
			if (_dic == null)
			{
				_dic = new Dictionary<string, ContentsValueInfo>();
			}
			return _dic;
		}
	}

	public IUserObserverManager ObserverManager => _observerManager;

	public ContentsValueManager()
	{
		_observerManager = new UserObserverManager<ContentsValueInfo>();
		ObserverManager.AddObserver(SceneAutoBehaviour<PayShopManagement>.instance);
	}

	public void Add(ContentsValueInfo info)
	{
		if (info != null)
		{
			string key = (info.Key = info.Key.ToUpper());
			_dic[key] = info;
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Add(ContentsValueInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Add(infos[i]);
		}
	}

	public ContentsValueInfo GetInfo(string key)
	{
		if (key == null)
		{
			return null;
		}
		key = key.ToUpper();
		ContentsValueInfo value = null;
		if (!_dic.TryGetValue(key.ToString(), out value))
		{
			return null;
		}
		return value;
	}

	public string GetContentsValue(string key)
	{
		ContentsValueInfo info = GetInfo(key);
		if (info == null)
		{
			return "";
		}
		return info.Value;
	}

	public int GetContentsIntValue(string key, int defaultValue = 0)
	{
		return GetInfo(key)?.GetIntValue(defaultValue) ?? defaultValue;
	}

	public bool GetContentsBoolValue(string key, bool defaultValue = false)
	{
		return GetInfo(key)?.GetBoolValue(defaultValue) ?? defaultValue;
	}

	public string[] GetContentsStringArray(string key)
	{
		ContentsValueInfo info = GetInfo(key);
		if (info == null)
		{
			return new string[0];
		}
		return info.GetStringArray();
	}

	public T GetContentsEnumValue<T>(string key, T defuatValue = default(T))
	{
		ContentsValueInfo info = GetInfo(key);
		if (info == null)
		{
			return defuatValue;
		}
		return info.GetEnumValue(defuatValue);
	}

	public void ApplyContentsValue(IUserObserver<ContentsValueInfo> observer, string[] keys)
	{
		if (keys == null || keys.Length == 0)
		{
			return;
		}
		foreach (string key in keys)
		{
			ContentsValueInfo info = GetInfo(key);
			if (info != null)
			{
				observer.OnChanged(info);
			}
		}
	}
}
