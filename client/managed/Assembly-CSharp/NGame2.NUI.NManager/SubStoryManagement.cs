using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NSubStory;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class SubStoryManagement : BaseManagement<SubStoryManagement>, SubStoryStarter.Listener, SubStoryCharacterScene.Listener, IEventListener
{
	private Dictionary<int, SubStoryData> _dicsCreatureLastSubStoryData = new Dictionary<int, SubStoryData>();

	private SubStoryStarter _subStoryStarter;

	public bool OnSubStoryMap { get; private set; }

	public int LastSubStoryCreatureIndex { get; private set; }

	public CampaignNodeData EntryNode { get; private set; }

	public CampaignNodeData EntryChapterNode { get; private set; }

	public void SetEntryNode(CampaignNodeData entryNode)
	{
		EntryNode = entryNode;
	}

	public void SetEntryChapterNode(CampaignNodeData entryNode)
	{
		EntryChapterNode = entryNode;
	}

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		_subStoryStarter = new SubStoryStarter(this);
		SingletonAuto<EventTriggerManager>.instance.AddEventListener(this);
		AddInstance<SubStoryCharacterScene>("Resources/Prefab2/Window/FullScene/SubStoryCharacterScene");
	}

	protected override void OnDestroy()
	{
		SingletonAuto<EventTriggerManager>.instance.RemoveEventListener(this);
		base.OnDestroy();
	}

	public void OpenSubStoryCharacterScene(Action onEndScene = null)
	{
		List<SubStoryData> onlySubStoryDataList = GameRoot.Instance.SubStoryDataContainer.GetOnlySubStoryDataList();
		SetCreatureSubStoryData(onlySubStoryDataList);
		int lastSubStoryCreatureIndex = SceneAutoBehaviour<SubStoryManagement>.instance.LastSubStoryCreatureIndex;
		GetInstance<SubStoryCharacterScene>().Open(this, _dicsCreatureLastSubStoryData.Values.ToList(), lastSubStoryCreatureIndex, onEndScene);
	}

	public void OpenSubStoryCharacterScene(int heroIndex)
	{
		List<SubStoryData> onlySubStoryDataList = GameRoot.Instance.SubStoryDataContainer.GetOnlySubStoryDataList();
		SetCreatureSubStoryData(onlySubStoryDataList);
		_ = SceneAutoBehaviour<SubStoryManagement>.instance.LastSubStoryCreatureIndex;
		GetInstance<SubStoryCharacterScene>().Open(this, _dicsCreatureLastSubStoryData.Values.ToList(), heroIndex);
	}

	private void CloseWindow<T>() where T : Component, IWindow
	{
		T val = GetInstance<T>();
		if (val != null)
		{
			val.Close();
		}
	}

	public bool CanStart(NodeFunctionType functionType)
	{
		SubStoryStarter.CheckStartRetunType checkStartRetunType = _subStoryStarter.CheckStart(functionType);
		ShowFailPopup(checkStartRetunType);
		return checkStartRetunType == SubStoryStarter.CheckStartRetunType.CanStart;
	}

	private void ShowFailPopup(SubStoryStarter.CheckStartRetunType checkResult)
	{
		switch (checkResult)
		{
		case SubStoryStarter.CheckStartRetunType.HeroRequireFail:
		{
			string stringDirect3 = LocalizationManager.GetStringDirect("SUBSTORY_HERO_STAR_LOW_TITLE");
			string stringDirect4 = LocalizationManager.GetStringDirect("SUBSTORY_HERO_STAR_LOW_DESC");
			MessagePopup.Open(stringDirect3, stringDirect4);
			break;
		}
		case SubStoryStarter.CheckStartRetunType.DungeonRequireFail:
		{
			string stringDirect = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DESC");
			MessagePopup.Open(stringDirect, stringDirect2);
			break;
		}
		}
	}

	public void MoveSubStoryChapter(int creatureIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		if (data == null)
		{
			VespaDebug.LogError(" Index of [" + creatureIndex + "] creauture is null ");
			return;
		}
		List<SubStoryData> listForCreatureTagType = GameRoot.Instance.SubStoryDataContainer.GetListForCreatureTagType(data.TagType);
		SetCreatureSubStoryData(listForCreatureTagType);
		if (!_dicsCreatureLastSubStoryData.ContainsKey(creatureIndex))
		{
			VespaDebug.LogError(" Index of [" + creatureIndex + "] subStory is null ");
			return;
		}
		SubStoryData subStoryData = _dicsCreatureLastSubStoryData[creatureIndex];
		bool num = subStoryData.ReqChapterIndex != 0 && subStoryData.ReqDungeonIndex != 0;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(subStoryData.ReqChapterIndex, subStoryData.ReqDungeonIndex);
		if (num && !flag)
		{
			string stringDirect = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DUNGEON", subStoryData.ReqChapterIndex, subStoryData.ReqDungeonIndex);
			string stringDirect2 = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_TITLE");
			string message = string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DESC"), stringDirect);
			MessagePopup.Open(stringDirect2, message);
		}
		else
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows(immediately: true);
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(subStoryData.ChapterIndex, subStoryData.DungeonIndex);
		}
	}

	public bool CheckMoveSubStoryChapter(int creatureIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		if (data == null)
		{
			VespaDebug.LogError(" Index of [" + creatureIndex + "] creauture is null ");
			return false;
		}
		List<SubStoryData> listForCreatureTagType = GameRoot.Instance.SubStoryDataContainer.GetListForCreatureTagType(data.TagType);
		SetCreatureSubStoryData(listForCreatureTagType);
		if (!_dicsCreatureLastSubStoryData.ContainsKey(creatureIndex))
		{
			VespaDebug.LogError(" Index of [" + creatureIndex + "] subStory is null ");
			return false;
		}
		SubStoryData subStoryData = _dicsCreatureLastSubStoryData[creatureIndex];
		bool num = subStoryData.ReqChapterIndex != 0 && subStoryData.ReqDungeonIndex != 0;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(subStoryData.ReqChapterIndex, subStoryData.ReqDungeonIndex);
		if (num && !flag)
		{
			string stringDirect = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DUNGEON", subStoryData.ReqChapterIndex, subStoryData.ReqDungeonIndex);
			string stringDirect2 = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_TITLE");
			string message = string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DESC"), stringDirect);
			MessagePopup.Open(stringDirect2, message);
			return false;
		}
		return true;
	}

	private void SetCreatureSubStoryData(List<SubStoryData> listForCreatureTagType)
	{
		_dicsCreatureLastSubStoryData.Clear();
		listForCreatureTagType.Sort((SubStoryData l, SubStoryData r) => l.DungeonIndex.CompareTo(r.DungeonIndex));
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(listForCreatureTagType); num++)
		{
			int creatureIndex = listForCreatureTagType[num].CreatureIndex;
			if (!_dicsCreatureLastSubStoryData.ContainsKey(creatureIndex))
			{
				_dicsCreatureLastSubStoryData[creatureIndex] = listForCreatureTagType[num];
				continue;
			}
			int chapterIndex = listForCreatureTagType[num].ChapterIndex;
			int dungeonIndex = listForCreatureTagType[num].DungeonIndex;
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, dungeonIndex))
			{
				_dicsCreatureLastSubStoryData[creatureIndex] = listForCreatureTagType[num];
			}
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	private void OnLoadMap()
	{
		OnSubStoryMap = GameRoot.Instance.SubStoryDataContainer.IsSubStoryChapter(NGame2.NAccount.UserCampaignManager.ChapterIndex);
	}

	private void SetWorldMapAvatar(int chapterIndex)
	{
		if (!GameRoot.Instance.SubStoryDataContainer.IsSubStoryChapter(chapterIndex))
		{
			return;
		}
		int creatureIndexForChapter = GameRoot.Instance.SubStoryDataContainer.GetCreatureIndexForChapter(chapterIndex);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndexForChapter);
		if (data == null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetAvatarHero(GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex);
			return;
		}
		List<SubStoryData> listForCreatureTagType = GameRoot.Instance.SubStoryDataContainer.GetListForCreatureTagType(data.TagType);
		SetCreatureSubStoryData(listForCreatureTagType);
		LastSubStoryCreatureIndex = creatureIndexForChapter;
		if (!_dicsCreatureLastSubStoryData.ContainsKey(creatureIndexForChapter))
		{
			VespaDebug.LogError("SetWorldMapAvatar  index[" + creatureIndexForChapter + "] canVisit node not exist");
			SetAvatar(creatureIndexForChapter, isCostumeIndex: false);
		}
		else
		{
			SetAvatar(_dicsCreatureLastSubStoryData[creatureIndexForChapter].CostumeIndex, isCostumeIndex: true);
		}
	}

	private void SetAvatar(int index, bool isCostumeIndex)
	{
		if (isCostumeIndex)
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetAvatarHero(index + 10000);
		}
		else
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetAvatarHero(index * 10);
		}
	}

	void SubStoryCharacterScene.Listener.OnSelectCharacter(SubStoryData subStoryData)
	{
		SelectCharacter(subStoryData, delegate
		{
			SubStoryCharacterScene subStoryCharacterScene = GetInstance<SubStoryCharacterScene>();
			if (subStoryCharacterScene != null)
			{
				subStoryCharacterScene.Close();
			}
			CloseAllActiveWindow();
		});
	}

	private void CloseAllActiveWindow()
	{
		if (SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<MultiPartyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MultiPartyManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<RaidManagement>.isValidInstance)
		{
			SceneAutoBehaviour<RaidManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance)
		{
			SceneAutoBehaviour<SoulWeaponManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.isValidInstance)
		{
			SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
	}

	private void SelectCharacter(SubStoryData subStoryData, Action OnEndMoveMap)
	{
		SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
		if (GameRoot.Instance.CampaignChapterDataContainer.GetData(subStoryData.ChapterIndex) == null)
		{
			VespaDebug.LogError("intex[" + subStoryData.ChapterIndex + "] of chapterData is null");
		}
		else
		{
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(subStoryData.ChapterIndex, subStoryData.DungeonIndex, OnEndMoveMap);
		}
	}

	void IEventListener.OnEvent(IEvent triggerEvent)
	{
		if (triggerEvent is ChapterMove chapterMove)
		{
			OnLoadMap();
			SetWorldMapAvatar(chapterMove.ChapterIndex);
		}
	}

	public void StartSubStoryBattle(int chapterIndex, int dungeonIndex, Action onCallback = null)
	{
		_subStoryStarter.StartSubStoryBattle(chapterIndex, dungeonIndex, onCallback);
	}

	public void ExecuteNodeFunction(NodeFunctionType functionType)
	{
		switch (functionType)
		{
		case NodeFunctionType.SubStoryBattle:
			_subStoryStarter.StartBattle();
			break;
		case NodeFunctionType.SubStoryEvent:
			_subStoryStarter.StartEvent();
			break;
		}
	}

	void SubStoryStarter.Listener.OnEndEventRewardPopup()
	{
		GlobalAutoBehaviour<CostumeOpenChecker>.instance.OpenUnlockPopup();
		if (GameRoot.Instance.SubStoryDataContainer.IsSubStoryChapter(NGame2.NAccount.UserCampaignManager.ChapterIndex))
		{
			SetWorldMapAvatar(NGame2.NAccount.UserCampaignManager.ChapterIndex);
		}
	}
}
