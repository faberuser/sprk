using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI.NComponent2.NSubStory;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SubStoryCharacterScene : BaseWindow, CharacterComponent.Listener, ITouchDisabler
{
	public interface Listener
	{
		void OnSelectCharacter(SubStoryData subStorydata);
	}

	public const string Command_ShowItemIcon = "EquipGachaScene/ShowItemIcon";

	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/SubStoryCharacterScene";

	private const string PACK_CODE = "Ui/Action";

	private const string APPEAR_NAME = "SubstoryBookAppear";

	private const string DISAPPEAR_NAME = "SubstoryBookDisAppear";

	private Listener _listener;

	public TweenPosition[] TweenPosition_Ary;

	public TweenAlpha[] TweenAlpha_Ary;

	public TweenScale[] TweenScale_Ary;

	public UILabel Label_HeroName;

	public UILabel Label_HeroDesc;

	public UIBundleLoader Texture_Hero;

	public UIButton Button_Select;

	public UIBaseGrid CharacterListGrid;

	private List<CharacterComponent> _componentList = new List<CharacterComponent>();

	private SubStoryData _selectedSubStory;

	public Transform Root_Contents;

	public UIWidget Widget_Root;

	public UITexture Texture_Cinema;

	private RenderTargetCinema _cinema;

	private Action _onCloseScene;

	protected Holder _receiverHolder = new Holder();

	bool ITouchDisabler.IsValid => IsOpen();

	public override void Awake()
	{
		base.Awake();
		UIPanel.lateUpdateBeforePanelList.Add(base.gameObject);
	}

	public override void Reset()
	{
		base.Reset();
		ClearComponentList();
		_receiverHolder.Clear();
		if (Texture_Cinema != null)
		{
			Texture_Cinema.mainTexture = null;
		}
		if (_cinema != null)
		{
			RenderTargetCinema.ReleaseInstance(_cinema);
			_cinema = null;
		}
	}

	private void ClearComponentList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public override void OnSystemBackButton()
	{
		VespaCoroutine.Start(CoClose(this));
	}

	public override void OnClosing()
	{
		base.OnClosing();
		VespaCoroutine.StopAll(this);
	}

	public void Open(Listener listener, List<SubStoryData> selectedClassDataList, int heroIndex, Action onCloseScene = null)
	{
		if (IsOpen())
		{
			Close();
		}
		_onCloseScene = onCloseScene;
		_listener = listener;
		base.Open();
		OpenWithData(selectedClassDataList, heroIndex);
		Play(null);
	}

	private void OpenWithData(List<SubStoryData> selectedClassDataList, int heroIndex)
	{
		if (__Extension_Method_ICollection__.COUNT(selectedClassDataList) == 0)
		{
			VespaDebug.LogError("SelectedClassDataList.COUNT() == 0");
			return;
		}
		selectedClassDataList.Sort((SubStoryData l, SubStoryData r) => l.CreatureIndex.CompareTo(r.CreatureIndex));
		Init(selectedClassDataList, heroIndex);
	}

	private void Init(List<SubStoryData> selectedClassDataList, int heroIndex)
	{
		_selectedSubStory = GetSeletedData(selectedClassDataList, heroIndex);
		SetCharacterInfo();
		SetCharacterList(selectedClassDataList);
	}

	private SubStoryData GetSeletedData(List<SubStoryData> selectedClassDataList, int heroIndex)
	{
		SubStoryData subStoryData = selectedClassDataList.Find((SubStoryData e) => e.CreatureIndex == heroIndex);
		if (subStoryData == null)
		{
			return selectedClassDataList[0];
		}
		return subStoryData;
	}

	private void SetCharacterInfo()
	{
		if (_selectedSubStory != null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_selectedSubStory.CreatureIndex);
			UIHelper.SetLabelText(Label_HeroName, data.Name);
			UIHelper.SetLabelText(Label_HeroDesc, LocalizationManager.GetStringDirect("CREATURE_DESC_" + _selectedSubStory.CreatureIndex));
			UIHelper.SetBundleTexture(Texture_Hero, _selectedSubStory.IllustPath);
		}
	}

	private void SetCharacterList(List<SubStoryData> selectedClassDataList)
	{
		ClearComponentList();
		selectedClassDataList.Sort(delegate(SubStoryData lhs, SubStoryData rhs)
		{
			if (lhs.ReqChapterIndex < rhs.ReqChapterIndex)
			{
				return -1;
			}
			if (lhs.ReqChapterIndex > rhs.ReqChapterIndex)
			{
				return 1;
			}
			if (lhs.ReqDungeonIndex < rhs.ReqDungeonIndex)
			{
				return -1;
			}
			if (lhs.ReqDungeonIndex > rhs.ReqDungeonIndex)
			{
				return 1;
			}
			if (lhs.CreatureIndex < rhs.CreatureIndex)
			{
				return -1;
			}
			return (lhs.CreatureIndex > rhs.CreatureIndex) ? 1 : 0;
		});
		foreach (SubStoryData selectedClassData in selectedClassDataList)
		{
			CharacterComponent characterComponent = CharacterListGrid.AddGridItem(CharacterComponent.PrefabAuto);
			CachedHeroInfo heroInfo = new CachedHeroInfo
			{
				HeroIndex = selectedClassData.CreatureIndex,
				CostumeIndex = selectedClassData.CostumeIndex
			};
			characterComponent.Init(this, selectedClassData, heroInfo);
			characterComponent.Select(_selectedSubStory != null && _selectedSubStory == selectedClassData);
			bool flag = selectedClassData.ReqChapterIndex != 0 && selectedClassData.ReqDungeonIndex != 0;
			bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(selectedClassData.ReqChapterIndex, selectedClassData.ReqDungeonIndex);
			characterComponent.SetGrayPortrait(flag && !flag2);
			_componentList.Add(characterComponent);
		}
		CharacterListGrid.RepositionGrid();
	}

	private void Play(Action onEndDisAppear)
	{
		if (!(Texture_Cinema == null))
		{
			_receiverHolder.Add<CommandMessage>(new ObjVerifier(this), OnMessageFast);
			ActiveBookContents(show: false);
			VespaCoroutine.Start(this, CoAppear(onEndDisAppear));
		}
	}

	private void ActiveBookContents(bool show)
	{
		if (show)
		{
			PlayTween();
		}
		UIHelper.EnableGameObject(Root_Contents, show);
	}

	private IEnumerator CoAppear(Action onEndAppear)
	{
		UIHelper.SetWidgetAlpha(Widget_Root, 0f);
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		UIHelper.SetWidgetAlpha(Widget_Root, 1f);
		if (_cinema == null)
		{
			RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
			SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
			stock.NodeTreePackCode = "Ui/Action";
			stock.NodeTreeName = "SubstoryBookAppear";
			_cinema = RenderTargetCinema.CreateInstance(stock);
		}
		else
		{
			_cinema.LoadNodeTree("Ui/Action", "SubstoryBookAppear");
		}
		if (!(_cinema == null))
		{
			do
			{
				yield return null;
			}
			while (!_cinema.IsCompleteLoad());
			_cinema.StartNodeTree(onEndAppear);
			Texture_Cinema.mainTexture = _cinema.RenderTarget;
			Texture_Cinema.alpha = 1f;
			sceneChanger.Close();
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return null;
			}
		}
	}

	private void PlayTween()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TweenAlpha_Ary); i++)
		{
			if (!(TweenAlpha_Ary[i] == null))
			{
				TweenAlpha_Ary[i].ResetToBeginning();
				TweenAlpha_Ary[i].PlayForward();
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(TweenPosition_Ary); j++)
		{
			if (!(TweenPosition_Ary[j] == null))
			{
				TweenPosition_Ary[j].ResetToBeginning();
				TweenPosition_Ary[j].PlayForward();
			}
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(TweenScale_Ary); k++)
		{
			if (!(TweenScale_Ary[k] == null))
			{
				TweenScale_Ary[k].ResetToBeginning();
				TweenScale_Ary[k].PlayForward();
			}
		}
	}

	private void PlayDisapear(Action onEndDisAppear)
	{
		VespaCoroutine.Start(this, CoDisapear(onEndDisAppear));
	}

	protected IEnumerator CoDisapear(Action onEndDisAppear)
	{
		if (!(_cinema == null))
		{
			_cinema.LoadNodeTree("Ui/Action", "SubstoryBookDisAppear");
			do
			{
				yield return null;
			}
			while (!_cinema.IsCompleteLoad());
			_cinema.StartNodeTree(onEndDisAppear);
		}
	}

	protected void OnMessageFast(CommandMessage message)
	{
		if (message.IsValidMessage() && message.IsValidScene(_cinema) && message.Command == "EquipGachaScene/ShowItemIcon")
		{
			ActiveBookContents(show: true);
		}
	}

	void CharacterComponent.Listener.OnClick(CharacterComponent caller)
	{
		if (caller != null && caller.SubStoryData != null)
		{
			int reqChapterIndex = caller.SubStoryData.ReqChapterIndex;
			int reqDungeonIndex = caller.SubStoryData.ReqDungeonIndex;
			bool num = reqChapterIndex != 0 && reqDungeonIndex != 0;
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(reqChapterIndex, reqDungeonIndex);
			if (num && !flag)
			{
				string stringDirect = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DUNGEON", reqChapterIndex, reqDungeonIndex);
				string stringDirect2 = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_TITLE");
				string message = string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DESC"), stringDirect);
				MessagePopup.Open(stringDirect2, message);
				return;
			}
		}
		_componentList.ForEach(delegate(CharacterComponent component)
		{
			component.Select(select: false);
		});
		caller.Select(select: true);
		_selectedSubStory = caller.SubStoryData;
		SetCharacterInfo();
	}

	public void OnClickBook()
	{
		if (_selectedSubStory == null)
		{
			VespaDebug.LogError("SubStoryData == null");
			return;
		}
		bool num = _selectedSubStory.ReqChapterIndex != 0 && _selectedSubStory.ReqDungeonIndex != 0;
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_selectedSubStory.ReqChapterIndex, _selectedSubStory.ReqDungeonIndex);
		if (num && !flag)
		{
			string stringDirect = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DUNGEON", _selectedSubStory.ReqChapterIndex, _selectedSubStory.ReqDungeonIndex);
			string stringDirect2 = LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_TITLE");
			string message = string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DESC"), stringDirect);
			MessagePopup.Open(stringDirect2, message);
		}
		else if (_listener != null)
		{
			_listener.OnSelectCharacter(_selectedSubStory);
		}
	}

	public void OnClickClose()
	{
		VespaCoroutine.Start(CoClose(this, _onCloseScene));
	}

	private static IEnumerator CoClose(SubStoryCharacterScene This, Action AfterClose = null)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (This != null && This.IsOpen())
		{
			UIHelper.SetWidgetAlpha(This.Widget_Root, 0f);
			This.Close();
		}
		sceneChanger.Close();
		Vespa_System_Action_Extension_Method.fire(AfterClose);
	}
}
