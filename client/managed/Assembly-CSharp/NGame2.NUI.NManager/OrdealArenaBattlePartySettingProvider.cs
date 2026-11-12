using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NOrdeal;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class OrdealArenaBattlePartySettingProvider : OrdealArenaBattlePartySetting.IDataProvider, BattlePartySetting.IDataProvider
{
	private readonly DefaultBattlePartySettingDataProdiver.LocalStorageDeckSetting _localStorage;

	private readonly EnterDungeonData _enterDungeonData;

	private readonly List<CachedHeroInfo> _enemyHeroList;

	public OrdealArenaBattlePartySettingProvider(string storageKey, EnterDungeonData enterDungeonData, List<CachedHeroInfo> enemyList)
	{
		if (!String_Extension_Method.IsNullOrEmpty(storageKey))
		{
			_localStorage = new DefaultBattlePartySettingDataProdiver.LocalStorageDeckSetting(storageKey);
		}
		_enterDungeonData = enterDungeonData;
		_enemyHeroList = enemyList;
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

	List<CachedHeroInfo> OrdealArenaBattlePartySetting.IDataProvider.GetEnemyHeroList()
	{
		return _enemyHeroList;
	}

	bool OrdealArenaBattlePartySetting.IDataProvider.IsDeadHero(int heroIndex)
	{
		return GlobalAutoBehaviour<OrdealArenaManager>.instance.IsDeadHero(heroIndex);
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
		return null;
	}

	void BattlePartySetting.IDataProvider.SetAffixIndices(int[] affixIndices)
	{
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
			bool flag = GlobalAutoBehaviour<OrdealArenaManager>.instance.IsDeadHero(partyMemberHeroIndex);
			if (partyMemberHeroIndex <= 0)
			{
				continue;
			}
			if (flag)
			{
				if (_localStorage != null)
				{
					if (isMainParty)
					{
						_localStorage.SetMainHeros(i, partyMemberHeroIndex);
					}
					else
					{
						_localStorage.SetSubHeros(i, partyMemberHeroIndex);
					}
				}
			}
			else
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
		return StaminaType.None;
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
}
