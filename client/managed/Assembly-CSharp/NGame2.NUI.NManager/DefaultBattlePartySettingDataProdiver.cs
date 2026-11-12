using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NManager;

public class DefaultBattlePartySettingDataProdiver : BattlePartySetting.IDataProvider
{
	public class LocalStorageDeckSetting
	{
		private readonly LocalAccountIntArray _mainDeckHeros;

		private readonly LocalAccountIntArray _subDeckHeros;

		public LocalStorageDeckSetting(string key)
		{
			string i_keyPrefix = SetMainStorageKey(key);
			string i_keyPrefix2 = SetSubStorageKey(key);
			_mainDeckHeros = new LocalAccountIntArray(0, i_keyPrefix);
			_subDeckHeros = new LocalAccountIntArray(0, i_keyPrefix2);
		}

		private string SetMainStorageKey(string key)
		{
			return $"{key}_PartyMember";
		}

		private string SetSubStorageKey(string key)
		{
			return $"{key}_SubPartyMember";
		}

		public int GetMainDeckHero(int slotIndex)
		{
			return _mainDeckHeros[slotIndex];
		}

		public int GetSubHeros(int slotIndex)
		{
			return _subDeckHeros[slotIndex];
		}

		public void SetMainHeros(int slotIndex, int heroIndex)
		{
			_mainDeckHeros[slotIndex] = heroIndex;
		}

		public void SetSubHeros(int slotIndex, int heroIndex)
		{
			_subDeckHeros[slotIndex] = heroIndex;
		}

		public void ResetMainHeros()
		{
			for (int i = 0; i < _mainDeckHeros.Count; i++)
			{
				_mainDeckHeros[i] = 0;
			}
		}

		public void ResetSubHeros()
		{
			for (int i = 0; i < _subDeckHeros.Count; i++)
			{
				_subDeckHeros[i] = 0;
			}
		}
	}

	private readonly LocalStorageDeckSetting _localStorage;

	private readonly StaminaType _staminaType;

	private readonly long _staminaCount;

	private readonly Enum _startButtonTextKey;

	private readonly EnterDungeonData _enterDungeon;

	private readonly BanRuleData _banRuleData;

	private int[] _affixIndices;

	private readonly BanRuleContainer _banRuleContainer;

	public DefaultBattlePartySettingDataProdiver(string storageKey, StaminaType staminaType, int staminaCount, Enum startButtonTextKey, EnterDungeonData enterDungeon)
	{
		if (!String_Extension_Method.IsNullOrEmpty(storageKey))
		{
			_localStorage = new LocalStorageDeckSetting(storageKey);
		}
		_staminaType = staminaType;
		_staminaCount = staminaCount;
		_startButtonTextKey = startButtonTextKey;
		_enterDungeon = enterDungeon;
		_banRuleContainer = new BanRuleContainer();
	}

	public DefaultBattlePartySettingDataProdiver(string storageKey, StaminaType staminaType, long staminaCount, Enum startButtonTextKey, EnterDungeonData enterDungeon, BanRuleData banRuleData)
	{
		if (!String_Extension_Method.IsNullOrEmpty(storageKey))
		{
			_localStorage = new LocalStorageDeckSetting(storageKey);
		}
		_staminaType = staminaType;
		_staminaCount = staminaCount;
		_startButtonTextKey = startButtonTextKey;
		_enterDungeon = enterDungeon;
		_banRuleContainer = new BanRuleContainer(banRuleData);
	}

	public DefaultBattlePartySettingDataProdiver(string storageKey, StaminaType staminaType, int staminaCount, Enum startButtonTextKey, EnterDungeonData enterDungeon, int[] affixIndices)
	{
		if (!String_Extension_Method.IsNullOrEmpty(storageKey))
		{
			_localStorage = new LocalStorageDeckSetting(storageKey);
		}
		_banRuleContainer = new BanRuleContainer();
		_staminaType = staminaType;
		_staminaCount = staminaCount;
		_startButtonTextKey = startButtonTextKey;
		_enterDungeon = enterDungeon;
		ApplyAffixIndices(affixIndices);
	}

	public EnterDungeonData GetEnterDungeon()
	{
		return _enterDungeon;
	}

	int[] BattlePartySetting.IDataProvider.GetForcedPartyMemberHeroIndices()
	{
		return _enterDungeon.LinkedDungeonData?.GetForcedHeroIndex(_enterDungeon.IsScenario);
	}

	int[] BattlePartySetting.IDataProvider.GetPartyMemberHeroIndices(bool isMainParty, int slotCount)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < slotCount; i++)
		{
			int partyMemberHeroIndex = ((BattlePartySetting.IDataProvider)this).GetPartyMemberHeroIndex(i, isMainParty);
			if (partyMemberHeroIndex > 0)
			{
				list.Add(partyMemberHeroIndex);
			}
		}
		return list.ToArray();
	}

	int BattlePartySetting.IDataProvider.GetPartyMemberHeroIndex(int slotIndex, bool isMainParty = true)
	{
		if (_localStorage == null)
		{
			return 0;
		}
		if (!isMainParty)
		{
			return _localStorage.GetSubHeros(slotIndex);
		}
		return _localStorage.GetMainDeckHero(slotIndex);
	}

	void BattlePartySetting.IDataProvider.SetPartyMemberHeroIndex(int slotIndex, int heroIndex, bool isMainParty = true)
	{
		if (_localStorage != null)
		{
			if (isMainParty)
			{
				_localStorage.SetMainHeros(slotIndex, heroIndex);
			}
			else
			{
				_localStorage.SetSubHeros(slotIndex, heroIndex);
			}
		}
	}

	EHeroState BattlePartySetting.IDataProvider.GetPartyHeroState()
	{
		return EHeroState.Party;
	}

	void BattlePartySetting.IDataProvider.ResetPartyMember()
	{
		if (_localStorage != null)
		{
			_localStorage.ResetMainHeros();
			_localStorage.ResetSubHeros();
		}
	}

	string BattlePartySetting.IDataProvider.GetStartButtonLabelText()
	{
		return LocalizationManager.GetString(_startButtonTextKey);
	}

	StaminaType BattlePartySetting.IDataProvider.GetStaminaType()
	{
		return _staminaType;
	}

	int BattlePartySetting.IDataProvider.GetStaminaConsumeCount()
	{
		return checked((int)_staminaCount);
	}

	bool BattlePartySetting.IDataProvider.EnableOnetimeBooster()
	{
		if (_enterDungeon != null && _enterDungeon.LinkedDungeonData != null && _enterDungeon.LinkedDungeonData.BattleType == BattleType.Campaign)
		{
			int dungeonRewardIndex = _enterDungeon.DungeonRewardIndex;
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(dungeonRewardIndex);
			if (data == null || data.Item1Code.IsNullOrEmpty())
			{
				return false;
			}
		}
		return true;
	}

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetSelectCreatureTagTypeList()
	{
		return _banRuleContainer.GetSelectClassTagTypeList();
	}

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetAcceptedCreatureTagTypeList()
	{
		return _banRuleContainer.GetAccpetedClassTagTypeList();
	}

	List<CreatureData> BattlePartySetting.IDataProvider.GetBannedHeroDataList()
	{
		return _banRuleContainer.GetBannedHeroDataList();
	}

	int[] BattlePartySetting.IDataProvider.GetAffixIndices()
	{
		return _affixIndices;
	}

	void BattlePartySetting.IDataProvider.SetAffixIndices(int[] affixIndices)
	{
		ApplyAffixIndices(affixIndices);
	}

	private void ApplyAffixIndices(int[] affixIndices)
	{
		_affixIndices = affixIndices;
		int[] banRuleIndices = AffixHelper.GetBanRuleIndices(affixIndices);
		_banRuleContainer.SetAddtionalBanRuleData(banRuleIndices);
	}

	RaidPartySetting.ClassEnterPermissionBanTypeData BattlePartySetting.IDataProvider.GetClassEnterPermissionBanTypeData()
	{
		return _banRuleContainer.GetClassEnterPermissionBanTypeData();
	}

	List<BanRuleData> BattlePartySetting.IDataProvider.GetBanRuleDataList()
	{
		return _banRuleContainer.GetBanRuleDataList();
	}
}
