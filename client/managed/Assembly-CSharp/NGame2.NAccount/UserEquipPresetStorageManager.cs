using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserEquipPresetStorageManager : NShared.UserEquipPresetStorageManager
{
	private readonly UserObserverManager<EquipStoreagChangeInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public int CurrentSlotCount => Count();

	public UserEquipPresetStorageManager()
	{
		_observerManager = new UserObserverManager<EquipStoreagChangeInfo>();
	}

	public void Apply(EquipPresetInfo[] infos)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(infos))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void Apply(EquipPresetInfo info)
	{
		Apply(info, EEquipStorageChangeType.Add);
	}

	private void Apply(EquipPresetInfo info, EEquipStorageChangeType changeType)
	{
		if (info != null)
		{
			Remove(info.EquipStorageSlotIndex);
			Add(info);
			EquipStoreagChangeInfo changedInfo = new EquipStoreagChangeInfo
			{
				ChangeType = changeType,
				PresetInfo = info
			};
			_observerManager.AnnounceToObservers(changedInfo);
		}
	}

	public void ApplyReset(EquipPresetInfo info)
	{
		Apply(info, EEquipStorageChangeType.Reset);
	}

	public void ApplyName(int slotIndex, string name)
	{
		EquipPresetInfo equipPresetInfo = Get(slotIndex);
		if (equipPresetInfo != null)
		{
			equipPresetInfo.Name = name;
			EquipStoreagChangeInfo changedInfo = new EquipStoreagChangeInfo
			{
				ChangeType = EEquipStorageChangeType.ChangeName,
				PresetInfo = equipPresetInfo
			};
			_observerManager.AnnounceToObservers(changedInfo);
		}
	}

	public EquipPresetInfo GetConatinStorageSlotInfo(string uid, int partIndex)
	{
		if (String_Extension_Method.IsNullOrEmpty(uid))
		{
			return null;
		}
		using (IEnumerator<EquipPresetInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				EquipPresetInfo current = enumerator.Current;
				string equipItemUid = current.GetEquipItemUid(partIndex);
				if (!String_Extension_Method.IsNullOrEmpty(equipItemUid) && equipItemUid == uid)
				{
					return current;
				}
			}
		}
		return null;
	}

	public EquipPresetInfo GetContainStorageSlotInfo(string uid)
	{
		if (String_Extension_Method.IsNullOrEmpty(uid))
		{
			return null;
		}
		using (IEnumerator<EquipPresetInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				EquipPresetInfo current = enumerator.Current;
				EquipPartType[] saveEquipParts = EquipPresetInfo.SaveEquipParts;
				foreach (EquipPartType partIndex in saveEquipParts)
				{
					string equipItemUid = current.GetEquipItemUid((int)partIndex);
					if (!String_Extension_Method.IsNullOrEmpty(equipItemUid) && equipItemUid == uid)
					{
						return current;
					}
				}
			}
		}
		return null;
	}
}
