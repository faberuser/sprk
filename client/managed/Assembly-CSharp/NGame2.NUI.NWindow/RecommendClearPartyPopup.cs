using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.GetRecommendDeckList;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RecommendClearPartyPopup : BaseWindow, ClearPartyListComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/RecommendClearParty/RecommendClearPartyPopup";

	private static Dictionary<NGame2.NUI.NWindow.RecommendPartyInfoKey, RecommendDeckInfo[]> RecommendInfoManager = new Dictionary<NGame2.NUI.NWindow.RecommendPartyInfoKey, RecommendDeckInfo[]>();

	public UIScrollGrid Grid_PartyList;

	public UIToggle Toggle_Level;

	public UIToggle Toggle_Time;

	public UILabel Label_Desc;

	public UIWidget Widget_Root;

	public UIPanel Panel_TopRoot;

	public UILabel Label_EmptyString;

	private List<ClearPartyListComponent> ClearPartyListComponents = new List<ClearPartyListComponent>();

	private RecommendDeckType CurrentDeckType;

	private RecommendDeckInfo[] CurrentDeckInfos;

	private int MaxHeroCount { get; set; }

	public void Open(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		base.Open();
		NGame2.NUI.NWindow.RecommendPartyInfoKey key = new NGame2.NUI.NWindow.RecommendPartyInfoKey(chapterIndex, dungeonIndex, difficulty);
		if (RecommendInfoManager.ContainsKey(key))
		{
			RecommendDeckInfo[] infos = RecommendInfoManager[key];
			CreatePartyList(chapterIndex, dungeonIndex, CurrentDeckType, infos);
		}
		else
		{
			CreatePartyList(chapterIndex, dungeonIndex, CurrentDeckType, null);
			RequestGetRecommendDeckList(chapterIndex, dungeonIndex, difficulty);
		}
	}

	public override void Reset()
	{
		base.Reset();
		CurrentDeckInfos = null;
		CurrentDeckType = RecommendDeckType.Level;
		SelectDeckType(CurrentDeckType);
	}

	private void RequestGetRecommendDeckList(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		NShared.GetRecommendDeckList.Request request = new NShared.GetRecommendDeckList.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			Difficulty = (int)difficulty
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetRecommendDeckList.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				RecommendInfoManager.Add(new NGame2.NUI.NWindow.RecommendPartyInfoKey(chapterIndex, dungeonIndex, difficulty), response.RecommendDecks);
				CreatePartyList(chapterIndex, dungeonIndex, CurrentDeckType, response.RecommendDecks);
			}
		});
	}

	private void CreatePartyList(int chapterIndex, int dungeonIndex, RecommendDeckType type, RecommendDeckInfo[] infos)
	{
		int deckCount = GetDeckCount(chapterIndex, dungeonIndex);
		CreatePartyList(type, infos, deckCount);
	}

	private void CreatePartyList(RecommendDeckType type, RecommendDeckInfo[] infos, int deckCount)
	{
		ClearPartyList();
		if (deckCount == 0)
		{
			deckCount = 4;
		}
		int num = 0;
		if (__Extension_Method_ICollection__.COUNT(infos) == 0)
		{
			ClearPartyListComponent clearPartyListComponent = UIHelper.AddGridItem(Grid_PartyList, ClearPartyListComponent.PrefabAuto);
			if (clearPartyListComponent != null)
			{
				clearPartyListComponent.Init(null, deckCount, this);
				ClearPartyListComponents.Add(clearPartyListComponent);
				num = Mathf.Max(num, clearPartyListComponent.Width);
				UIHelper.EnableGameObject(clearPartyListComponent, enable: false);
			}
		}
		else
		{
			foreach (RecommendDeckInfo recommendDeckInfo in infos)
			{
				if (recommendDeckInfo.Type == type)
				{
					ClearPartyListComponent clearPartyListComponent2 = UIHelper.AddGridItem(Grid_PartyList, ClearPartyListComponent.PrefabAuto);
					if (clearPartyListComponent2 != null)
					{
						clearPartyListComponent2.Init(recommendDeckInfo, deckCount, this);
						UIHelper.EnableGameObject(clearPartyListComponent2, enable: true);
						ClearPartyListComponents.Add(clearPartyListComponent2);
						num = Mathf.Max(num, clearPartyListComponent2.Width);
					}
				}
			}
			UIHelper.RepositionGrid(Grid_PartyList);
		}
		UIHelper.EnableGameObject(Label_EmptyString, __Extension_Method_ICollection__.COUNT(infos) == 0);
		MaxHeroCount = deckCount;
		CurrentDeckType = type;
		SetDesc(type);
		ResizeBG(num);
		CurrentDeckInfos = infos;
	}

	private void ResizeBG(int width)
	{
		if (!(Widget_Root == null))
		{
			Widget_Root.width = width;
			Widget_Root.UpdateAnchors();
		}
	}

	private void ClearPartyList()
	{
		FactoryBase.ReleaseAndClearList(ClearPartyListComponents);
	}

	public void OnToggleLevel()
	{
		if (Toggle_Level.value && CurrentDeckType != RecommendDeckType.Level)
		{
			CreatePartyList(RecommendDeckType.Level, CurrentDeckInfos, MaxHeroCount);
		}
	}

	public void OnToggleTime()
	{
		if (Toggle_Time.value && CurrentDeckType != RecommendDeckType.Time)
		{
			CreatePartyList(RecommendDeckType.Time, CurrentDeckInfos, MaxHeroCount);
		}
	}

	void ClearPartyListComponent.IListener.OnClickComponent(RecommendDeckInfo info, CachedHeroInfo[] heroInfos)
	{
		SceneAutoBehaviour<InspectUserManagement>.instance.OpenInspectUserList(null, heroInfos);
	}

	public void SelectDeckType(RecommendDeckType type)
	{
		UIToggle uIToggle = null;
		switch (type)
		{
		case RecommendDeckType.Level:
			uIToggle = Toggle_Level;
			break;
		case RecommendDeckType.Time:
			uIToggle = Toggle_Time;
			break;
		}
		if (uIToggle != null)
		{
			uIToggle.value = true;
		}
	}

	public void SetDesc(RecommendDeckType type)
	{
		string text = LocalizationManager.GetStringDirect("RECOMMEND_PARTY_DECK_DESC");
		string key = "";
		switch (type)
		{
		case RecommendDeckType.Level:
			key = "RECOMMEND_PARTY_DECK_DESC_LEVEL";
			break;
		case RecommendDeckType.Time:
			key = "RECOMMEND_PARTY_DECK_DESC_TIME";
			break;
		}
		string stringDirect = LocalizationManager.GetStringDirect(key);
		if (!String_Extension_Method.IsNullOrEmpty(stringDirect))
		{
			text = text + "\n" + stringDirect;
		}
		UIHelper.SetLabelText(Label_Desc, text);
	}

	public int GetDeckCount(int chapterIndex, int dungeonIndex)
	{
		int num = 0;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return 0;
		}
		num = data.DeckCount;
		switch (data.BattleType)
		{
		case BattleType.Raid:
		case BattleType.GuildRaid:
		case BattleType.RaidSingle:
		case BattleType.ChallengeRaidSingle:
		case BattleType.GuildSuppress:
		case BattleType.HardRaid:
		case BattleType.ChallengeRaid:
		{
			RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapterIndex, dungeonIndex);
			if (raidDataByDungeon != null)
			{
				num = raidDataByDungeon.DeckCount;
			}
			break;
		}
		}
		return num;
	}
}
