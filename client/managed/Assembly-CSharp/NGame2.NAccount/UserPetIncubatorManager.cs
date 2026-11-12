using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserPetIncubatorManager : NShared.UserPetIncubatorManager
{
	private PetIncubatorInfoKey _cachedKey = new PetIncubatorInfoKey(0, 0);

	private List<PetIncubatorInfo> _list = new List<PetIncubatorInfo>();

	protected override void OnAddData(PetIncubatorInfoKey key, PetIncubatorInfo value)
	{
		base.OnAddData(key, value);
		AddList(value);
	}

	public void Apply(PetIncubatorInfo info)
	{
		Add(info);
	}

	public void Apply(PetIncubatorInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public override bool Remove(PetIncubatorInfo value)
	{
		if (value == null)
		{
			return false;
		}
		_cachedKey.Item1 = value.SlotIndex;
		_cachedKey.Item2 = value.IncubatorIndex;
		return Remove(_cachedKey);
	}

	public override bool Remove(PetIncubatorInfoKey key)
	{
		RemoveList(key);
		return base.Remove(key);
	}

	public override void RemoveAll()
	{
		base.RemoveAll();
		_list.Clear();
	}

	private void AddList(PetIncubatorInfo info)
	{
		if (info == null)
		{
			return;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			PetIncubatorInfo petIncubatorInfo = _list[i];
			if (petIncubatorInfo == null)
			{
				_list.RemoveAt(i);
				i--;
			}
			else if (petIncubatorInfo.IncubatorIndex == info.IncubatorIndex && petIncubatorInfo.SlotIndex == info.SlotIndex)
			{
				_list[i] = info;
				return;
			}
		}
		_list.Add(info);
	}

	private bool RemoveList(PetIncubatorInfoKey key)
	{
		if (key == null)
		{
			return false;
		}
		int item = key.Item1;
		for (int i = 0; i < _list.Count; i++)
		{
			PetIncubatorInfo petIncubatorInfo = _list[i];
			if (petIncubatorInfo == null)
			{
				_list.RemoveAt(i);
				i--;
			}
			else if (petIncubatorInfo.SlotIndex == item)
			{
				_list.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public List<PetIncubatorInfo> GetPetIncubatorInfoList()
	{
		return _list;
	}

	public List<PetIncubatorInfo> GetPetIncubatorInfoListBySlotIndex(int slotIndex)
	{
		List<PetIncubatorInfo> list = null;
		for (int i = 0; i < _list.Count; i++)
		{
			PetIncubatorInfo petIncubatorInfo = _list[i];
			if (petIncubatorInfo != null && petIncubatorInfo.SlotIndex == slotIndex)
			{
				if (list == null)
				{
					list = new List<PetIncubatorInfo>();
				}
				list.Add(petIncubatorInfo);
			}
		}
		return list;
	}

	public PetIncubatorInfo Get(int slotIndex, int incubatorIndex)
	{
		_cachedKey.Item1 = slotIndex;
		_cachedKey.Item2 = incubatorIndex;
		return Get(_cachedKey);
	}
}
