using System;
using System.Collections.Generic;
using System.Linq;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserPetManager : NShared.UserPetManager
{
	public enum EChangedPetInfoType
	{
		None,
		AddedPet,
		PetInfoChanged,
		PetInterAction,
		CreateMainAvatarPet,
		ChangeMainAvatarPet,
		ChangePetLayout,
		RemoveAddedPet
	}

	public class ChangedPetInfo
	{
		public int PetIndex;

		public EChangedPetInfoType EChangedPetInfoType;
	}

	private readonly UserObserverManager<ChangedPetInfo> _observerManager = new UserObserverManager<ChangedPetInfo>();

	public Dictionary<PetMiscType, PetMiscInfo> _petMiscInfoDictionary = new Dictionary<PetMiscType, PetMiscInfo>();

	public const string KEY_NEW_PET_INDEX = "NewPetIndex_";

	public const int VALUE_INVALID = -1;

	private HashSet<int> _AddedPetIndices = new HashSet<int>();

	public IUserObserverManager ObserverManager => _observerManager;

	public bool NotHavePet
	{
		get
		{
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return true;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.PetManager == null)
			{
				return true;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.PetManager.Count() == 0;
		}
	}

	public int MainPetAvatarIndex
	{
		get
		{
			PetMiscInfo petMiscInfo = GetPetMiscInfo(PetMiscType.AvatarPetIndex);
			if (petMiscInfo == null)
			{
				return 0;
			}
			return Vespa_String_Extension_Method.TOINT(petMiscInfo.MiscValue, 0);
		}
	}

	public long RemainPetFoodCount => GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.PetFeedKey);

	public long MaxPetFoodCount => GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.PetFeedKey);

	public int PetFoodValue => GameRoot.Instance.ConstantDataContainer.GetValueInt("PetFoodValue");

	public List<int> PlacePetIndexList
	{
		get
		{
			PetMiscInfo petMiscInfo = GetPetMiscInfo(PetMiscType.PetLayout);
			if (petMiscInfo == null)
			{
				return null;
			}
			if (String_Extension_Method.IsNullOrEmpty(petMiscInfo.MiscValue))
			{
				return null;
			}
			return Array.ConvertAll(petMiscInfo.MiscValue.Split(','), int.Parse).ToList();
		}
	}

	public int PlacePetCount
	{
		get
		{
			int num = 0;
			List<int> placePetIndexList = PlacePetIndexList;
			if (__Extension_Method_ICollection__.IsNullOrEmpty(placePetIndexList))
			{
				return num;
			}
			for (int i = 0; i < placePetIndexList.Count; i++)
			{
				if (placePetIndexList[i] > 0)
				{
					num++;
				}
			}
			return num;
		}
	}

	private void AnnounceToObserver(int petIndex, EChangedPetInfoType eChangedPetInfoType)
	{
		if (_observerManager != null)
		{
			_observerManager.AnnounceToObservers(new ChangedPetInfo
			{
				PetIndex = petIndex,
				EChangedPetInfoType = eChangedPetInfoType
			});
		}
	}

	public override void Set(PetInfo[] values)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values); i++)
		{
			PetInfo petInfo = values[i];
			if (petInfo != null)
			{
				Add(petInfo);
				if (IsNew(petInfo.PetIndex))
				{
					_AddedPetIndices.Add(petInfo.PetIndex);
				}
			}
		}
	}

	public void Apply(PetInfo info, EChangedPetInfoType changedPetType = EChangedPetInfoType.None)
	{
		if (info != null)
		{
			EChangedPetInfoType eChangedPetInfoType = ((changedPetType == EChangedPetInfoType.None) ? GetChangeType(info) : changedPetType);
			Add(info);
			if (eChangedPetInfoType == EChangedPetInfoType.AddedPet)
			{
				_AddedPetIndices.Add(info.PetIndex);
				LocalAccountInt localAccountInt = new LocalAccountInt(-1, "NewPetIndex_" + info.PetIndex);
				int petIndex = info.PetIndex;
				localAccountInt.Value = petIndex;
			}
			if (info != null)
			{
				AnnounceToObserver(info.PetIndex, eChangedPetInfoType);
			}
		}
	}

	public void Apply(PetInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Apply(PetAddResultInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Apply(PetAddResultInfo info)
	{
		if (info.PetResult != null)
		{
			Apply(info.PetResult);
		}
	}

	public void Apply(PetMiscInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Apply(PetMiscInfo info)
	{
		if (info == null || info.MiscKey == PetMiscType.None)
		{
			return;
		}
		int mainPetAvatarIndex = MainPetAvatarIndex;
		if (!_petMiscInfoDictionary.ContainsKey(info.MiscKey))
		{
			_petMiscInfoDictionary.Add(info.MiscKey, info);
		}
		else
		{
			_petMiscInfoDictionary[info.MiscKey] = info;
		}
		if (info.MiscKey == PetMiscType.PetLayout)
		{
			AnnounceToObserver(0, EChangedPetInfoType.ChangePetLayout);
		}
		else
		{
			if (info.MiscKey != PetMiscType.AvatarPetIndex)
			{
				return;
			}
			if (Vespa_String_Extension_Method.TOINT(info.MiscValue, 0) == 0)
			{
				_petMiscInfoDictionary.Remove(info.MiscKey);
			}
			int num = MainPetAvatarIndex;
			if (mainPetAvatarIndex != num)
			{
				if (num == 0)
				{
					num = mainPetAvatarIndex;
				}
				AnnounceToObserver(num, (mainPetAvatarIndex > 0) ? EChangedPetInfoType.ChangeMainAvatarPet : EChangedPetInfoType.CreateMainAvatarPet);
			}
		}
	}

	public PetMiscInfo GetPetMiscInfo(PetMiscType type)
	{
		if (_petMiscInfoDictionary.ContainsKey(type))
		{
			return _petMiscInfoDictionary[type];
		}
		return null;
	}

	private EChangedPetInfoType GetChangeType(PetInfo petInfo)
	{
		if (Get(petInfo.PetIndex) == null)
		{
			return EChangedPetInfoType.AddedPet;
		}
		return EChangedPetInfoType.PetInfoChanged;
	}

	public int GetAddedPetCount()
	{
		return _AddedPetIndices.Count;
	}

	public bool AddedPetListExists(int petIndex)
	{
		return _AddedPetIndices.Contains(petIndex);
	}

	public bool IsNew(int petIndex)
	{
		LocalAccountInt localAccountInt = new LocalAccountInt(-1, "NewPetIndex_" + petIndex);
		if (localAccountInt.Value == -1)
		{
			return false;
		}
		if (petIndex == localAccountInt.Value)
		{
			return true;
		}
		return false;
	}

	public void RemoveAddedPet(int petIndex)
	{
		_AddedPetIndices.Remove(petIndex);
		new LocalAccountInt(-1, "NewPetIndex_" + petIndex).Remove();
		AnnounceToObserver(petIndex, EChangedPetInfoType.RemoveAddedPet);
	}

	public void RemoveAllAddedPet()
	{
		foreach (int addedPetIndex in _AddedPetIndices)
		{
			new LocalAccountInt(-1, "NewPetIndex_" + addedPetIndex).Remove();
		}
		_AddedPetIndices.Clear();
		AnnounceToObserver(0, EChangedPetInfoType.RemoveAddedPet);
	}

	public bool IsPlacedPet(int petIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PlacePetIndexList))
		{
			return false;
		}
		return PlacePetIndexList.Contains(petIndex);
	}

	public TimeSpan? GetRemainPetEggSupplyTimeSpan()
	{
		PetMiscInfo petMiscInfo = GetPetMiscInfo(PetMiscType.PetIncubatorOpenTime);
		if (petMiscInfo == null)
		{
			return null;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(petMiscInfo.MiscValue, out var o_dateTime))
		{
			return null;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (o_dateTime.CompareTo(currentServerUTCTime) < 0)
		{
			return null;
		}
		return o_dateTime - currentServerUTCTime;
	}
}
