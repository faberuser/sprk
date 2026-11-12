using System.Collections.Generic;
using NGame2.NContents;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa;

namespace NGame2.NAccount;

public class UserClassBuffManager : NShared.UserClassBuffManager
{
	private readonly UserObserverManager<ClassBuffInfo> _observerManager = new UserObserverManager<ClassBuffInfo>();

	private Dictionary<CreatureTagType, long> _classBuffPointDic = new Dictionary<CreatureTagType, long>();

	private Dictionary<CreatureTagType, long> _prevClassBuffPointDic = new Dictionary<CreatureTagType, long>();

	private Dictionary<CreatureTagType, Dictionary<int, ClassBuffInfo>> _classBuffInfoDic = new Dictionary<CreatureTagType, Dictionary<int, ClassBuffInfo>>();

	public IUserObserverManager ObserverManager => _observerManager;

	public static bool ReservedFirstOpen { get; set; }

	public Dictionary<CreatureTagType, long> ClassBuffPointDic => _classBuffPointDic;

	public Dictionary<CreatureTagType, long> PrevClassBuffPointDic => _prevClassBuffPointDic;

	public Dictionary<CreatureTagType, Dictionary<int, ClassBuffInfo>> ClassBuffInfoDic => _classBuffInfoDic;

	protected override void OnAddData(ClassBuffKey key, ClassBuffInfo value)
	{
		base.OnAddData(key, value);
		_observerManager.AnnounceToObservers(value);
	}

	public void InitClassBuffPoint(ClassBuffPointInfo[] pointInfos)
	{
		ReservedFirstOpen = false;
		_prevClassBuffPointDic.Clear();
		_classBuffPointDic.Clear();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(pointInfos))
		{
			return;
		}
		CreatureTagType type = CreatureTagType.None;
		foreach (ClassBuffPointInfo classBuffPointInfo in pointInfos)
		{
			if (classBuffPointInfo != null && BanRuleData.GetCreatureTagType(classBuffPointInfo.TagType.ToString(), out type))
			{
				_classBuffPointDic.Add(type, classBuffPointInfo.BuffPoint);
			}
		}
		if (ClassBuffWrapper.IsFirstOpenStorage)
		{
			ReservedFirstOpen = true;
			if (SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.ClassBuff))
			{
				ApplyPrevPoint();
			}
		}
	}

	public void InitClassBuffInfo(ClassBuffInfo[] infos)
	{
		_classBuffInfoDic.Clear();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(infos))
		{
			return;
		}
		CreatureTagType type = CreatureTagType.None;
		foreach (ClassBuffInfo classBuffInfo in infos)
		{
			if (classBuffInfo == null || !BanRuleData.GetCreatureTagType(classBuffInfo.TagType.ToString(), out type))
			{
				continue;
			}
			if (_classBuffInfoDic.ContainsKey(type))
			{
				if (!_classBuffInfoDic[type].ContainsKey(classBuffInfo.ClassBuffIndex))
				{
					_classBuffInfoDic[type].Add(classBuffInfo.ClassBuffIndex, classBuffInfo);
				}
				else
				{
					_classBuffInfoDic[type][classBuffInfo.ClassBuffIndex] = classBuffInfo;
				}
			}
			else
			{
				AddInitClassBuffInfo(type, classBuffInfo);
			}
		}
	}

	public void ApplyByCreatureList(List<CreatureTagType> list = null)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			_prevClassBuffPointDic.Clear();
			{
				foreach (KeyValuePair<CreatureTagType, long> item in _classBuffPointDic)
				{
					_prevClassBuffPointDic.Add(item.Key, item.Value);
				}
				return;
			}
		}
		foreach (CreatureTagType item2 in list)
		{
			if (_classBuffPointDic.ContainsKey(item2))
			{
				if (!_prevClassBuffPointDic.ContainsKey(item2))
				{
					_prevClassBuffPointDic.Add(item2, _classBuffPointDic[item2]);
				}
				else
				{
					_prevClassBuffPointDic[item2] = _classBuffPointDic[item2];
				}
			}
		}
	}

	public void ApplyPrevPoint()
	{
		_prevClassBuffPointDic.Clear();
		foreach (KeyValuePair<CreatureTagType, long> item in _classBuffPointDic)
		{
			_prevClassBuffPointDic.Add(item.Key, item.Value);
		}
	}

	public void Apply(ClassBuffPointInfo[] pointInfos)
	{
		CreatureTagType type = CreatureTagType.None;
		foreach (ClassBuffPointInfo classBuffPointInfo in pointInfos)
		{
			if (classBuffPointInfo != null && BanRuleData.GetCreatureTagType(classBuffPointInfo.TagType.ToString(), out type) && _classBuffPointDic.ContainsKey(type))
			{
				_classBuffPointDic[type] = classBuffPointInfo.BuffPoint;
			}
		}
		_observerManager.AnnounceToObservers(null);
	}

	public void Apply(ClassBuffInfo[] infos)
	{
		InitClassBuffInfo(infos);
		_observerManager.AnnounceToObservers(null);
	}

	private void AddInitClassBuffInfo(CreatureTagType type, ClassBuffInfo info)
	{
		_classBuffInfoDic.Add(type, new Dictionary<int, ClassBuffInfo>());
		_classBuffInfoDic[type].Clear();
		_classBuffInfoDic[type].Add(info.ClassBuffIndex, info);
	}

	public long GetClassBuffPointByCreatureType(CreatureTagType type)
	{
		long value = 0L;
		if (!_classBuffPointDic.ContainsKey(type))
		{
			return value;
		}
		if (!_classBuffPointDic.TryGetValue(type, out value))
		{
			return 0L;
		}
		return value;
	}

	public Dictionary<int, ClassBuffInfo> GetClassBuffInfoDicByCreatureType(CreatureTagType type)
	{
		if (!ClassBuffInfoDic.ContainsKey(type))
		{
			return null;
		}
		return ClassBuffInfoDic[type];
	}

	public ClassBuffInfo GetClassBuffInfo(CreatureTagType type, int index)
	{
		if (!_classBuffInfoDic.ContainsKey(type))
		{
			return null;
		}
		if (!_classBuffInfoDic[type].ContainsKey(index))
		{
			return null;
		}
		return _classBuffInfoDic[type][index];
	}
}
