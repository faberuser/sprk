using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPunish;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class PunishmentBattlePartySettingProvider : PunishmentRaidPartySetting.IDataProvider, BattlePartySetting.IDataProvider
{
	private readonly DefaultBattlePartySettingDataProdiver.LocalStorageDeckSetting _localStorage;

	private readonly EnterDungeonData _enterDungeonData;

	private readonly PunishmentGroupData _punishmentGroupData;

	private RaidData _raidData;

	private int[] _affixIndices;

	private LocalAccountInt _localStorageShardIndex;

	private LocalAccountInt _localStorageShardCount;

	private int _shardItemIndex;

	private int _shardItemCount;

	public PunishmentBattlePartySettingProvider(string storageKey, EnterDungeonData enterDungeonData, RaidData raidData, PunishmentGroupData punishmentGroupData, int[] affixIndices)
	{
		if (!String_Extension_Method.IsNullOrEmpty(storageKey))
		{
			_localStorage = new DefaultBattlePartySettingDataProdiver.LocalStorageDeckSetting(storageKey);
		}
		_enterDungeonData = enterDungeonData;
		_raidData = raidData;
		_punishmentGroupData = punishmentGroupData;
		_affixIndices = affixIndices;
		_shardItemIndex = 0;
		_shardItemCount = 0;
		InitShardItem();
	}

	private void InitShardItem()
	{
		if (_raidData == null || _punishmentGroupData == null)
		{
			return;
		}
		_localStorageShardIndex = new LocalAccountInt(0, $"ShardItemIndex_{_raidData.Index}");
		_localStorageShardCount = new LocalAccountInt(0, $"ShardItemCount_{_raidData.Index}");
		switch (_punishmentGroupData.PunishmentDungeonType)
		{
		case EPunishmentDungeonType.Karma:
		{
			int value3 = _localStorageShardIndex.Value;
			ItemInfo itemInfo2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(value3);
			if (value3 == 0 || itemInfo2 == null || itemInfo2.Count == 0)
			{
				int defaultFlaskIndex = GameRoot.Instance.ShardFlaskItemDataContainer.GetDefaultFlaskIndex(_punishmentGroupData.GroupIndex);
				if (defaultFlaskIndex != 0)
				{
					_shardItemIndex = defaultFlaskIndex;
				}
				else
				{
					VespaDebug.LogError($"Default Flask Item is Null : GroupIndex = {_punishmentGroupData.GroupIndex}");
				}
			}
			else
			{
				_shardItemIndex = _localStorageShardIndex.Value;
			}
			break;
		}
		case EPunishmentDungeonType.Boss:
		{
			if (_localStorageShardIndex.Value == 0)
			{
				break;
			}
			int value = _localStorageShardIndex.Value;
			int value2 = _localStorageShardCount.Value;
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(value);
			if (itemInfo != null && itemInfo.Count != 0)
			{
				_shardItemIndex = _localStorageShardIndex.Value;
				_shardItemCount = _localStorageShardCount.Value;
				if (itemInfo.Count < value2)
				{
					_shardItemCount = itemInfo.Count;
				}
			}
			break;
		}
		}
	}

	bool BattlePartySetting.IDataProvider.EnableOnetimeBooster()
	{
		return false;
	}

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetAcceptedCreatureTagTypeList()
	{
		return null;
	}

	List<CreatureData> BattlePartySetting.IDataProvider.GetBannedHeroDataList()
	{
		return null;
	}

	EnterDungeonData BattlePartySetting.IDataProvider.GetEnterDungeon()
	{
		return _enterDungeonData;
	}

	int[] BattlePartySetting.IDataProvider.GetForcedPartyMemberHeroIndices()
	{
		return null;
	}

	int[] BattlePartySetting.IDataProvider.GetAffixIndices()
	{
		return _affixIndices;
	}

	void BattlePartySetting.IDataProvider.SetAffixIndices(int[] affixIndices)
	{
		_affixIndices = affixIndices;
	}

	RaidPartySetting.ClassEnterPermissionBanTypeData BattlePartySetting.IDataProvider.GetClassEnterPermissionBanTypeData()
	{
		return null;
	}

	List<BanRuleData> BattlePartySetting.IDataProvider.GetBanRuleDataList()
	{
		return null;
	}

	EHeroState BattlePartySetting.IDataProvider.GetPartyHeroState()
	{
		return EHeroState.Party;
	}

	int BattlePartySetting.IDataProvider.GetPartyMemberHeroIndex(int slotIndex, bool isMainParty)
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

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetSelectCreatureTagTypeList()
	{
		return null;
	}

	int BattlePartySetting.IDataProvider.GetStaminaConsumeCount()
	{
		return 0;
	}

	StaminaType BattlePartySetting.IDataProvider.GetStaminaType()
	{
		return StaminaType.Empty;
	}

	string BattlePartySetting.IDataProvider.GetStartButtonLabelText()
	{
		return LocalizationManager.GetString(Battle.START_BATTLE);
	}

	void BattlePartySetting.IDataProvider.ResetPartyMember()
	{
		if (_localStorage != null)
		{
			_localStorage.ResetMainHeros();
			_localStorage.ResetSubHeros();
		}
	}

	void BattlePartySetting.IDataProvider.SetPartyMemberHeroIndex(int slotIndex, int heroIndex, bool isMainParty)
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

	RaidData PunishmentRaidPartySetting.IDataProvider.GetRaidData()
	{
		return _raidData;
	}

	PunishmentGroupData PunishmentRaidPartySetting.IDataProvider.GetPunishmentGroupData()
	{
		return _punishmentGroupData;
	}

	int PunishmentRaidPartySetting.IDataProvider.GetShardItemIndex()
	{
		return _shardItemIndex;
	}

	void PunishmentRaidPartySetting.IDataProvider.SetShardItemIndex(int itemIndex)
	{
		_shardItemIndex = itemIndex;
	}

	int PunishmentRaidPartySetting.IDataProvider.GetShardItemCount()
	{
		return _shardItemCount;
	}

	void PunishmentRaidPartySetting.IDataProvider.SetShardItemCount(long itemCount)
	{
		_shardItemCount = checked((int)itemCount);
	}

	void PunishmentRaidPartySetting.IDataProvider.SaveLastShardSetting(int itemIndex, int itemCount)
	{
		_localStorageShardIndex.Value = itemIndex;
		_localStorageShardCount.Value = itemCount;
	}
}
