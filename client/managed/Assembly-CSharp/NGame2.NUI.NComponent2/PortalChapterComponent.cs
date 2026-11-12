using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NUI.NComponent2.ChapterClearReward;
using NGame2.NUI.NHelper.NPortal;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PortalChapterComponent : PortalContentsComponent, ChapterMarkerComponent.Listener, PortalMoveToPopup.Listener
{
	[Serializable]
	public class PageButtonControl
	{
		public UIButton ControlButton;

		public UISprite LockedSprite;

		public bool IsLocked { get; private set; }

		public void ChangeLockedState(bool isLocked)
		{
			IsLocked = isLocked;
			UIHelper.EnableGameObject(LockedSprite, IsLocked);
			UIHelper.EnableButton(ControlButton, !IsLocked);
		}
	}

	[Serializable]
	public class ChapterButtonData
	{
		public bool IsLocked;

		public UIButton Button;

		public Transform Locked;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalChapterComponent";

	public static PrefabAuto<PortalChapterComponent> PrefabAuto = new PrefabAuto<PortalChapterComponent>("Resources/Prefab2/Window/Portal/PortalRenewalComponents/PortalChapterComponent");

	[Header("[Chapter Reward]")]
	public Transform ChapterRewardRoot;

	public UILabel ChapterRewardLabel;

	[Header("[Chapter Info]")]
	public UILabel ChapterTitleLabel;

	public UIScrollView ChapterScrollView;

	public UICenterOnChild CenterOnChild;

	public UISprite MarkerBackgorundLine;

	public UIGrid ChapterMarkerGrid;

	public UIGrid ChapterTextureGrid;

	public UILabel LastNodeLabel;

	[Header("[Chapter Buttons]")]
	public ChapterButtonData PartyPlayButtonData;

	public ChapterButtonData MoveLastNodeButtonData;

	public ChapterButtonData LatelyBattleButtonData;

	[Space(5f)]
	public Transform NoticeRoot;

	public UISpriteSelector PartyPlayNotice;

	[Header("[Page Control Buttons Info]")]
	public PageButtonControl PrevButton;

	public PageButtonControl NextButton;

	public List<ChapterRewardComponent> RewardComponents;

	private List<ChapterMarkerComponent> _markerComponents = new List<ChapterMarkerComponent>();

	private List<ContentsTextureComponent> _textureComponents = new List<ContentsTextureComponent>();

	private Dictionary<int, string> _chapterTitleDic = new Dictionary<int, string>();

	private Dictionary<int, string> _visualChapterIndexDic = new Dictionary<int, string>();

	private int _currChapterPageIndex;

	private int _minChatper = int.MaxValue;

	private int _maxChapter = int.MinValue;

	private int _canMaxChapter = int.MinValue;

	private ChapterMarkerComponent _currMarkerComp;

	protected override void Init()
	{
		base.Init();
		Dictionary<int, CampaignChapterData> mainChapterDataDic = GameRoot.Instance.CampaignChapterDataContainer.GetMainChapterDataDic();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(mainChapterDataDic))
		{
			return;
		}
		IList<PortalRenewalData> data = GameRoot.Instance.PortalRenewalDataContainer.GetData(base._portalRenewalData.Category);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(data))
		{
			return;
		}
		foreach (KeyValuePair<int, CampaignChapterData> item in mainChapterDataDic)
		{
			CampaignChapterData value = item.Value;
			int chapterIndex = value.Index;
			PortalRenewalData portalRenewalData = data.FirstOrDefault((PortalRenewalData x) => x.ContentsShowIndex == chapterIndex);
			if (portalRenewalData != null)
			{
				_minChatper = Mathf.Min(_minChatper, chapterIndex);
				_maxChapter = Mathf.Max(_maxChapter, chapterIndex);
				if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(chapterIndex))
				{
					_canMaxChapter = Mathf.Max(_canMaxChapter, chapterIndex);
				}
				ChapterMarkerComponent chapterMarkerComponent = ChapterMarkerComponent.PrefabAuto.Factory.Create(ChapterMarkerGrid.transform);
				chapterMarkerComponent.Open(chapterIndex, this);
				_markerComponents.Add(chapterMarkerComponent);
				string path = "PortalRenewal/Image/" + portalRenewalData.MainImagePath;
				ContentsTextureComponent contentsTextureComponent = ContentsTextureComponent.PrefabAuto.Factory.Create(ChapterTextureGrid.transform);
				contentsTextureComponent.Open(chapterIndex, path);
				_textureComponents.Add(contentsTextureComponent);
				_chapterTitleDic.Add(chapterIndex, LocalizationManager.GetStringDirect(portalRenewalData.TitleLocalKey));
				string value2 = ((!String_Extension_Method.IsNullOrEmpty(portalRenewalData.VisualChapterIndex)) ? portalRenewalData.VisualChapterIndex : chapterIndex.ToString());
				_visualChapterIndexDic.Add(chapterIndex, value2);
			}
		}
		ChapterMarkerGrid.cellWidth = (float)MarkerBackgorundLine.width / (float)(__Extension_Method_ICollection__.COUNT(mainChapterDataDic) - 1);
		SetChapterPageFocus(_canMaxChapter);
		InitPartyPlayButton();
	}

	protected override void UpdateResetTime()
	{
	}

	protected override void Close()
	{
		base.Close();
		_textureComponents.ForEach(delegate(ContentsTextureComponent x)
		{
			x.Clear();
		});
		FactoryBase.ReleaseAndClearList(_markerComponents);
		FactoryBase.ReleaseAndClearList(_textureComponents);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_chapterTitleDic);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_visualChapterIndexDic);
	}

	private void InitPartyPlayButton()
	{
		bool flag = MultiPartyManagement.CheckPartyDungeonContentsStatus() && SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.PartyPlayDungeon);
		PartyPlayNotice.SelectEnum(PortalRenewal.NoticeState.NONE);
		if (flag)
		{
			LocalAccountBool localAccountBool = new LocalAccountBool(i_default: false, $"PortalRenewal_NewState_{base._portalRenewalData.Category}_PartyPlay");
			if (!localAccountBool.Value)
			{
				localAccountBool.Value = true;
				UIHelper.EnableGameObject(NoticeRoot, enable: true);
				PartyPlayNotice.SelectEnum(PortalRenewal.NoticeState.NEW);
			}
			else
			{
				UIHelper.EnableGameObject(NoticeRoot, enable: false);
			}
		}
		PartyPlayButtonData.IsLocked = !flag;
		UIHelper.EnableGameObject(PartyPlayButtonData.Locked, PartyPlayButtonData.IsLocked);
	}

	private void RefreshPageController()
	{
		PrevButton.ChangeLockedState(_currChapterPageIndex <= _minChatper);
		NextButton.ChangeLockedState(_currChapterPageIndex >= _canMaxChapter);
	}

	private void SetChapterPageFocus(int chapterIndex)
	{
		ContentsTextureComponent contentsTextureComponent = _textureComponents.Find((ContentsTextureComponent x) => x.ChapterIndex == chapterIndex);
		if (contentsTextureComponent == null)
		{
			return;
		}
		ChapterMarkerComponent chapterMarkerComponent = _markerComponents.Find((ChapterMarkerComponent x) => x.ChapterIndex == chapterIndex);
		if (chapterMarkerComponent == null || chapterMarkerComponent.IsLocked)
		{
			return;
		}
		_currChapterPageIndex = chapterIndex;
		string visualChapterIndex = GetVisualChapterIndex(chapterIndex);
		if (_chapterTitleDic.ContainsKey(chapterIndex))
		{
			string stringDirect = LocalizationManager.GetStringDirect("PORTAL_UI_TEXT_CHAPTER_CHPATERNAME", visualChapterIndex, _chapterTitleDic[chapterIndex]);
			UIHelper.SetLabelText(ChapterTitleLabel, stringDirect);
		}
		if (_currMarkerComp != null)
		{
			_currMarkerComp.ChangeActiveState(isActive: false);
		}
		_currMarkerComp = chapterMarkerComponent;
		_currMarkerComp.ChangeActiveState(isActive: true);
		List<ChapterClearRewardData> rewardDataByChapterIndex = GameRoot.Instance.ChapterClearRewardDataContainer.GetRewardDataByChapterIndex(chapterIndex);
		UIHelper.EnableGameObject(ChapterRewardRoot, !__Extension_Method_ICollection__.IsNullOrEmpty(rewardDataByChapterIndex));
		foreach (ChapterClearRewardData item in rewardDataByChapterIndex)
		{
			if (item != null)
			{
				RewardComponents[item.StepIndex - 1].Open(null, null, item);
			}
		}
		int totalStarByChapterClearReward = GameRoot.Instance.CampaignDungeonDataContainer.GetTotalStarByChapterClearReward(chapterIndex);
		int num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.ClearDungeonStar(chapterIndex, totalStarByChapterClearReward);
		string stringDirect2 = LocalizationManager.GetStringDirect("PORTAL_UI_TEXT_CHAPTER_CLEAR_STAR_REWARD", num, totalStarByChapterClearReward);
		UIHelper.SetLabelText(ChapterRewardLabel, stringDirect2);
		int dungeonIndex = Mathf.Max(GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLatelyPlayDungeonIndex_Chapter(chapterIndex), 1);
		string nodeCode = PortalHelper.GetNodeCode(chapterIndex, dungeonIndex, combineChapterIndex: true, visualChapterIndex);
		UIHelper.SetLabelText(LastNodeLabel, nodeCode);
		int latelyBattleChapterIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleChapterIndex;
		int latelyBattleDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleDungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(latelyBattleChapterIndex, latelyBattleDungeonIndex);
		UIHelper.EnableButton(LatelyBattleButtonData.Button, data != null);
		UIHelper.EnableGameObject(LatelyBattleButtonData.Locked, data == null);
		VespaCoroutine.Start(coCenterOn(contentsTextureComponent.transform));
		RefreshPageController();
	}

	public void OnClickPrevPageButton()
	{
		if (_listener == null || !_listener.IsRunningEditMode())
		{
			int num = _currChapterPageIndex - 1;
			if (num >= _minChatper)
			{
				SetChapterPageFocus(num);
			}
		}
	}

	public void OnClickNextPageButton()
	{
		if (_listener == null || !_listener.IsRunningEditMode())
		{
			int num = _currChapterPageIndex + 1;
			if (num <= _maxChapter)
			{
				SetChapterPageFocus(num);
			}
		}
	}

	public void OnClickPartyPlay()
	{
		if (_listener != null && _listener.IsRunningEditMode())
		{
			return;
		}
		if (PartyPlayButtonData.IsLocked)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("SUBCATEGORY_OPEN_CONDITION_RAID"));
			return;
		}
		PortalHelper.OpenContentsMovePopup(LocalizationManager.GetStringDirect("PARTY_PLAY"), delegate
		{
			SceneAutoBehaviour<MultiPartyManagement>.instance.OpenPartyDungeonRoomList();
		});
	}

	public void OnClickMoveChapter()
	{
		if (_listener == null || !_listener.IsRunningEditMode())
		{
			int chapterIndex = _currChapterPageIndex;
			int dungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLatelyPlayDungeonIndex_Chapter(chapterIndex);
			dungeonIndex = Mathf.Max(dungeonIndex, 1);
			PortalHelper.OpenContentsMovePopup(chapterIndex, dungeonIndex, delegate
			{
				_listener.MoveNode(chapterIndex, dungeonIndex);
			});
		}
	}

	public void OnClickMoveLatelyBattleNode()
	{
		if (_listener == null || !_listener.IsRunningEditMode())
		{
			int chapterIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleChapterIndex;
			int dungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.LatelyBattleDungeonIndex;
			PortalHelper.OpenContentsMovePopup(chapterIndex, dungeonIndex, delegate
			{
				_listener.MoveNode(chapterIndex, dungeonIndex);
			});
		}
	}

	private IEnumerator coCenterOn(Transform target)
	{
		yield return null;
		CenterOnChild.CenterOn(target.transform);
	}

	private string GetVisualChapterIndex(int chapterIndex)
	{
		if (!_visualChapterIndexDic.TryGetValue(chapterIndex, out var value))
		{
			return chapterIndex.ToString();
		}
		return value;
	}

	void ChapterMarkerComponent.Listener.OnClickDirectMoveButton(ChapterMarkerComponent comp)
	{
		if (_listener == null || !_listener.IsRunningEditMode())
		{
			SetChapterPageFocus(comp.ChapterIndex);
		}
	}

	void PortalMoveToPopup.Listener.MoveNode(int ChapterIndex, int DungeonIndex)
	{
		if (_listener != null && !_listener.IsRunningEditMode())
		{
			_listener.MoveNode(ChapterIndex, DungeonIndex);
		}
	}
}
