using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenBattlePartySetting : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime, BattlePartySetting.IDataProvider, BattlePartySetting.Listener
{
	private EnterDungeonData _enterDungeon = new EnterDungeonData(1, 1, 0);

	private int[] _heroIndices;

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 4);
		TutorialUtility.GetArgBool(tutorialData, 5, defaultValue: true);
		_heroIndices = new int[4] { argInt, argInt2, argInt3, argInt4 };
		if (SceneAutoBehaviour<LobbyCampaignDungeon>.instance.IsOpened<BattleDungeonInfo>())
		{
			BattleDungeonInfo instance = SceneAutoBehaviour<LobbyCampaignDungeon>.instance.GetInstance<BattleDungeonInfo>();
			_enterDungeon = instance.EnterDungeon;
		}
		BattlePartySetting instance2 = SceneAutoBehaviour<LobbyCampaignDungeon>.instance.GetInstance<BattlePartySetting>();
		instance2.Close();
		instance2.Open(this, this);
		if (argBool && SceneAutoBehaviour<LobbyCampaignDungeon>.isValidInstance)
		{
			BattlePartySetting.IDataProvider dataProvider = SceneAutoBehaviour<LobbyCampaignDungeon>.instance.GetDataProvider(_enterDungeon);
			if (dataProvider != null)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_heroIndices); i++)
				{
					if (_heroIndices[i] != 0)
					{
						dataProvider.SetPartyMemberHeroIndex(i, _heroIndices[i]);
					}
				}
			}
		}
		BattlePlayerCreateInfo playerCreateInfo = new BattlePlayerCreateInfo
		{
			Id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
			TeamType = TeamType.Left
		};
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		int[] heroIndices = _heroIndices;
		foreach (int key in heroIndices)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key);
			if (cachedHeroInfo != null)
			{
				list.Add(new BattleCreatureCreateInfo
				{
					HeroInfo = cachedHeroInfo,
					StatData = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(cachedHeroInfo)
				});
			}
		}
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
		playerEnterBattleLog.PlayerCreateInfo = playerCreateInfo;
		playerEnterBattleLog.CreatureCreateInfos = list.ToArray();
		SingletonAuto<CustomCampaignContext>.instance.SetEnterPlayerBattleLog(playerEnterBattleLog);
	}

	EnterDungeonData BattlePartySetting.IDataProvider.GetEnterDungeon()
	{
		return _enterDungeon;
	}

	bool BattlePartySetting.IDataProvider.EnableOnetimeBooster()
	{
		return false;
	}

	int[] BattlePartySetting.IDataProvider.GetForcedPartyMemberHeroIndices()
	{
		return null;
	}

	int[] BattlePartySetting.IDataProvider.GetPartyMemberHeroIndices(bool isMainParty, int slotCount)
	{
		return _heroIndices;
	}

	int BattlePartySetting.IDataProvider.GetPartyMemberHeroIndex(int slotIndex, bool isMainParty = true)
	{
		if (slotIndex < __Extension_Method_ICollection__.COUNT(_heroIndices))
		{
			return _heroIndices[slotIndex];
		}
		return 0;
	}

	void BattlePartySetting.IDataProvider.SetPartyMemberHeroIndex(int slotIndex, int heroIndex, bool isMainParty = true)
	{
	}

	void BattlePartySetting.IDataProvider.ResetPartyMember()
	{
	}

	EHeroState BattlePartySetting.IDataProvider.GetPartyHeroState()
	{
		return EHeroState.Free;
	}

	string BattlePartySetting.IDataProvider.GetStartButtonLabelText()
	{
		return LocalizationManager.GetString(Battle.START_BATTLE);
	}

	StaminaType BattlePartySetting.IDataProvider.GetStaminaType()
	{
		return StaminaType.None;
	}

	int BattlePartySetting.IDataProvider.GetStaminaConsumeCount()
	{
		return 0;
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetSelectCreatureTagTypeList()
	{
		return null;
	}

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetAcceptedCreatureTagTypeList()
	{
		return null;
	}

	List<CreatureData> BattlePartySetting.IDataProvider.GetBannedHeroDataList()
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
}
