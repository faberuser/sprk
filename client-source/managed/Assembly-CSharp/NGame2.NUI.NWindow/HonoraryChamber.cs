using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NHonoraryChamber;
using NGameAsset;
using NGameAsset.NCreature;
using NNguiExtension;
using NShared;
using NShared.GetRecordsOfHonorRankingList;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HonoraryChamber : BaseWindow, HonoraryChamberMenuComponent.IListener, HonoraryChamberRankComponent.IListener, LobbyTopmostMenu.IMiniGuideButtonObserver
{
	public interface IListener
	{
	}

	[Serializable]
	public class UICustomToggle
	{
		public UIButton Button;

		public UISprite SelectSprtie;

		private bool _active;

		public void SetSelectEnable(bool select, bool active)
		{
			_active = active;
			UIHelper.EnableButton(Button, active, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(SelectSprtie, active && select);
		}

		public void Select(bool select)
		{
			UIHelper.EnableGameObject(SelectSprtie, _active && select);
		}
	}

	public enum ServerType
	{
		None,
		Local,
		World
	}

	public class ResponseCache
	{
		public RecordsOfHonorSeasonInfo[] SeasonInfo { get; set; }

		public RecordsOfHonorRankingInfo[] ServerInfo { get; set; }

		public RecordsOfHonorRankingInfo[] WorldInfo { get; set; }

		public RecordsOfHonorMyInfo[] MyInfo { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Lobby/HonoraryChamber";

	private IListener _listener;

	public Transform TransformEmptyViwer;

	public Transform TransformEmptyRankList;

	public UIScrollView ScrollMenu;

	public UITable TableMenu;

	public UIGrid GridDeck;

	public UITexture HeroPreviewTexture;

	public NameUI UserName;

	public NameUI GuildName;

	public ArenaInfo ArenaInfo;

	public WorldBossInfo WorldBossInfo;

	public NGame2.NUI.NWindow.NHonoraryChamber.GuildSuppressInfo GuildSuppressInfo;

	public UILabel LabelMyName;

	public UILabel LabelMyRank;

	public UILabel LabelDisableCenter;

	public UISprite SpriteMyRankMark;

	public UIScrollGrid GridRank;

	public UICustomToggle CustomToggleServer;

	public UICustomToggle CustomToggleWorld;

	private ServerType _serverType = ServerType.Local;

	public UIComboBox ComboBoxSeason;

	public RecordsOfHonorContentType DefaultOpenType = RecordsOfHonorContentType.Match;

	private RecordsOfHonorContentType _currentContentOpenType;

	private int _currentSeason;

	private Dictionary<int, HonoraryChamberMenuComponent> _menuComponents = new Dictionary<int, HonoraryChamberMenuComponent>();

	private List<HonoraryChamberRankComponent> _rankingComponentList = new List<HonoraryChamberRankComponent>();

	private List<HeroPortraitTextureName> _heroPortraitList = new List<HeroPortraitTextureName>();

	private HeroPreview _heroPreview;

	private HonoraryChamberMenuData _currentMenu;

	private string defaultNamedAction = "Victory";

	private RecordsOfHonorRankingInfo _currentRankingInfo;

	private List<HonoraryInfoBase> _infoList = new List<HonoraryInfoBase>();

	private Dictionary<KeyValuePair<RecordsOfHonorContentType, int>, ResponseCache> _cachedResponse = new Dictionary<KeyValuePair<RecordsOfHonorContentType, int>, ResponseCache>();

	private ResponseCache _lastResponse;

	int LobbyTopmostMenu.IMiniGuideButtonObserver.WindowGuideIndex => 0;

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rankingComponentList); i++)
		{
			_rankingComponentList[i].UpdateComponent();
		}
	}

	public void Open(IListener listener, RecordsOfHonorContentType requestContentType = RecordsOfHonorContentType.None, ServerType serverType = ServerType.Local)
	{
		base.Open();
		_listener = listener;
		SetInfoList();
		CreateMenu();
		ScrollMenu.disableDragIfFits = true;
		requestContentType = ((requestContentType != RecordsOfHonorContentType.None) ? requestContentType : DefaultOpenType);
		_serverType = serverType;
		SelectMenuTypeOf(requestContentType);
	}

	private void SelectMenuTypeOf(RecordsOfHonorContentType contentType)
	{
		foreach (HonoraryChamberMenuComponent value in _menuComponents.Values)
		{
			if (value.MenuData.ContentsType == contentType)
			{
				value.SetSelect(select: true);
				value.OnValueChanged();
			}
		}
	}

	private void SetInfoList()
	{
		_infoList.Add(ArenaInfo);
		_infoList.Add(WorldBossInfo);
		_infoList.Add(GuildSuppressInfo);
	}

	private void CreateMenu()
	{
		ReleaseAndClearMenu();
		Dictionary<int, List<HonoraryChamberMenuComponent>> dictionary = new Dictionary<int, List<HonoraryChamberMenuComponent>>();
		foreach (HonoraryChamberMenuData allDatum in GameRoot.Instance.HonoraryChamberMenuDataContainer.GetAllData())
		{
			HonoraryChamberMenuComponent honoraryChamberMenuComponent = HonoraryChamberMenuComponent.PrefabAuto.Factory.Create(TableMenu.transform);
			honoraryChamberMenuComponent.Open(this, allDatum);
			bool enable = allDatum.ParentIndex == 0;
			UIHelper.EnableGameObject(honoraryChamberMenuComponent, enable);
			if (allDatum.ParentIndex != 0)
			{
				if (!dictionary.ContainsKey(allDatum.ParentIndex))
				{
					dictionary[allDatum.ParentIndex] = new List<HonoraryChamberMenuComponent>();
				}
				dictionary[allDatum.ParentIndex].Add(honoraryChamberMenuComponent);
			}
			_menuComponents[allDatum.Index] = honoraryChamberMenuComponent;
		}
		LinkMenuComponents(dictionary);
		TableMenu.Reposition();
		ScrollMenu.ResetPosition();
	}

	private void LinkMenuComponents(Dictionary<int, List<HonoraryChamberMenuComponent>> parentIndexSubMenuPairs)
	{
		Dictionary<int, HonoraryChamberMenuComponent>.Enumerator enumerator = _menuComponents.GetEnumerator();
		while (enumerator.MoveNext())
		{
			HonoraryChamberMenuComponent value = enumerator.Current.Value;
			HonoraryChamberMenuData menuData = value.MenuData;
			if (parentIndexSubMenuPairs.ContainsKey(menuData.Index))
			{
				value.AddSubMenuList(parentIndexSubMenuPairs[menuData.Index]);
			}
			if (menuData.ParentIndex != 0)
			{
				value.SetParentMenu(_menuComponents[menuData.ParentIndex]);
			}
		}
	}

	private void OnSelectMenu(HonoraryChamberMenuComponent selectedComponent)
	{
		TableMenu.Reposition();
		ScrollMenu.ResetPosition();
		_currentMenu = selectedComponent.MenuData;
		if (_currentContentOpenType != _currentMenu.ContentsType)
		{
			_currentContentOpenType = _currentMenu.ContentsType;
			_currentSeason = 0;
			RequestContentsRankInfo(_currentContentOpenType, _currentSeason, OnResponse);
		}
	}

	private ResponseCache GetCachedResponse(RecordsOfHonorContentType contentsType, int season)
	{
		KeyValuePair<RecordsOfHonorContentType, int> key = new KeyValuePair<RecordsOfHonorContentType, int>(contentsType, season);
		if (!_cachedResponse.ContainsKey(key))
		{
			return null;
		}
		return _cachedResponse[key];
	}

	private void RequestContentsRankInfo(RecordsOfHonorContentType contentsType, int season, Action<RecordsOfHonorContentType, int, ResponseCache> OnResponse)
	{
		ResponseCache cachedResponse = GetCachedResponse(contentsType, season);
		if (cachedResponse != null)
		{
			if (!Define.IsLiveBuild())
			{
				Debug.LogError($"{contentsType} {season} is cached");
			}
			Vespa_System_Action_Extension_Method.fire(OnResponse, contentsType, season, cachedResponse);
			return;
		}
		NShared.GetRecordsOfHonorRankingList.Request request = new NShared.GetRecordsOfHonorRankingList.Request();
		request.ContentType = contentsType;
		request.Season = season;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetRecordsOfHonorRankingList.Response response)
		{
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(OnResponse, contentsType, season, null);
			}
			else if (response.Result == ResultType.Fail)
			{
				Vespa_System_Action_Extension_Method.fire(OnResponse, contentsType, season, null);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(OnResponse, contentsType, season, ResponseToCache(response));
			}
		});
	}

	private ResponseCache ResponseToCache(NShared.GetRecordsOfHonorRankingList.Response response)
	{
		return new ResponseCache
		{
			SeasonInfo = response.SeasonInfo,
			ServerInfo = response.ServerInfo,
			WorldInfo = response.WorldInfo,
			MyInfo = response.MyInfo
		};
	}

	public override void Reset()
	{
		base.Reset();
		Release3dViewer();
		ReleaseClearRankingList();
		ReleaseAndClearMenu();
		_currentMenu = null;
		_currentContentOpenType = RecordsOfHonorContentType.None;
		_currentSeason = 0;
		_currentRankingInfo = null;
		ReleaseInfoList();
		ComboBoxSeason.Clear();
		_cachedResponse.Clear();
	}

	private void ReleaseInfoList()
	{
		foreach (HonoraryInfoBase info in _infoList)
		{
			info.Release();
		}
		_infoList.Clear();
	}

	private void ReleaseClearRankingList()
	{
		FactoryBase.ReleaseAndClearList(_rankingComponentList);
		_rankingComponentList.Clear();
	}

	private void OnResponse(RecordsOfHonorContentType contentType, int season, ResponseCache response)
	{
		if (GameRoot.Instance.ConstantDataContainer.GetValueBool("UseCacheHonoraryChamber"))
		{
			_cachedResponse[new KeyValuePair<RecordsOfHonorContentType, int>(contentType, season)] = response;
		}
		_lastResponse = response;
		SetInfos();
	}

	private void SetInfos()
	{
		if (_lastResponse == null)
		{
			VespaDebug.LogError("response == null");
			Close();
			return;
		}
		SetSeasonComboBox();
		SetServerToggle();
		RecordsOfHonorRankingInfo[] serverInfo = _lastResponse.ServerInfo;
		RecordsOfHonorRankingInfo[] worldInfo = _lastResponse.WorldInfo;
		RecordsOfHonorRankingInfo[] array = ((_serverType == ServerType.Local) ? serverInfo : worldInfo);
		bool flag = __Extension_Method_ICollection__.COUNT(array) == 0;
		UIHelper.EnableGameObject(TransformEmptyViwer, flag);
		UIHelper.EnableGameObject(TransformEmptyRankList, flag);
		if (flag)
		{
			ComboBoxSeason.Clear();
			ComboBoxSeason.AddItem(0, "-");
			ComboBoxSeason.SelectFirstItem(isNotify: false);
			SetRanker(null);
			SetMyInfo(null);
			SetRankingList(null, null);
			VespaDebug.LogError("infoList Count == 0");
		}
		else
		{
			ComboBoxSeason.SelectItem(array[0].Season, isNotify: false);
			RecordsOfHonorRankingInfo recordsOfHonorRankingInfo = array[0];
			SetRanker(recordsOfHonorRankingInfo);
			SetMyInfo(_lastResponse.MyInfo);
			SetRankingList(array, recordsOfHonorRankingInfo);
		}
	}

	private void SetServerToggle()
	{
		bool flag = __Extension_Method_ICollection__.COUNT(_lastResponse.ServerInfo) > 0;
		bool flag2 = __Extension_Method_ICollection__.COUNT(_lastResponse.WorldInfo) > 0;
		if (_serverType == ServerType.Local && !flag)
		{
			_serverType = ServerType.World;
		}
		else if (_serverType == ServerType.World && !flag2)
		{
			_serverType = ServerType.Local;
		}
		bool flag3 = _serverType == ServerType.Local && flag;
		bool flag4 = _serverType == ServerType.World && flag2;
		CustomToggleServer.SetSelectEnable(flag3, flag);
		CustomToggleWorld.SetSelectEnable(flag4, flag2);
	}

	private void SetRanker(RecordsOfHonorRankingInfo rankingData)
	{
		Release3dViewer();
		CreateHeroModel(rankingData);
		SetViewerUI(rankingData);
	}

	private void SetMyInfo(RecordsOfHonorMyInfo[] myRanking)
	{
		RecordsOfHonorMyInfo recordsOfHonorMyInfo = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(myRanking); i++)
		{
			if ((myRanking[i].RankingType == RecordsOfHonorRankingType.Server && _serverType == ServerType.Local) || (myRanking[i].RankingType == RecordsOfHonorRankingType.World && _serverType == ServerType.World) || myRanking[i].RankingType == RecordsOfHonorRankingType.ServerTotal)
			{
				recordsOfHonorMyInfo = myRanking[i];
				break;
			}
		}
		string empty = string.Empty;
		empty = ((_currentContentOpenType != RecordsOfHonorContentType.GuildSuppress) ? GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick : GetGuildName());
		string text = ((recordsOfHonorMyInfo == null) ? string.Empty : LocalizationManager.GetStringDirect("HONOR_RANK_NUM", recordsOfHonorMyInfo.Rank + 1));
		bool flag = String_Extension_Method.IsNullOrEmpty(empty) && String_Extension_Method.IsNullOrEmpty(text);
		UIHelper.EnableGameObject(LabelMyName, !flag);
		UIHelper.EnableGameObject(LabelMyRank, !flag);
		UIHelper.EnableGameObject(LabelDisableCenter, flag);
		if (!flag)
		{
			UIHelper.SetLabelText(LabelMyName, String_Extension_Method.IsNullOrEmpty(empty) ? "-" : empty);
			UIHelper.SetLabelText(LabelMyRank, String_Extension_Method.IsNullOrEmpty(text) ? "-" : text);
			UIHelper.EnableGameObject(SpriteMyRankMark, enable: true);
		}
	}

	private string GetGuildName()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null)
		{
			return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Name;
		}
		return string.Empty;
	}

	private void SetRankingList(RecordsOfHonorRankingInfo[] rankingInfoList, RecordsOfHonorRankingInfo selected)
	{
		ReleaseClearRankingList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rankingInfoList); i++)
		{
			HonoraryChamberRankComponent honoraryChamberRankComponent = GridRank.AddGridItem(HonoraryChamberRankComponent.PrefabAuto);
			honoraryChamberRankComponent.Open(this, rankingInfoList[i]);
			bool flag = selected == rankingInfoList[i];
			honoraryChamberRankComponent.Select = flag;
			_rankingComponentList.Add(honoraryChamberRankComponent);
		}
		UIHelper.RepositionGrid(GridRank);
	}

	private void SetViewerUI(RecordsOfHonorRankingInfo rankingData)
	{
		SetName(rankingData);
		CreatePlayerDeck(rankingData);
		SetInfoDetail(rankingData);
	}

	private void SetName(RecordsOfHonorRankingInfo rankingData)
	{
		bool flag = rankingData != null;
		bool flag2 = flag && rankingData.ContentType == RecordsOfHonorContentType.GuildSuppress;
		UIHelper.EnableGameObject(GuildName.TransformRoot, flag && flag2);
		UIHelper.EnableGameObject(UserName.TransformRoot, flag && !flag2);
		if (flag)
		{
			NameUI obj = (flag2 ? GuildName : UserName);
			obj.SetLevel(rankingData.Level);
			obj.SetName(rankingData.Name);
		}
	}

	private void CreatePlayerDeck(RecordsOfHonorRankingInfo rankingData)
	{
		ClearPlayerDeck();
		if (rankingData == null || rankingData.Deck == null)
		{
			return;
		}
		DeckHeroInfo[] deck = rankingData.Deck;
		foreach (DeckHeroInfo deckHeroInfo in deck)
		{
			if (deckHeroInfo.Index != 0)
			{
				HeroPortraitTextureName heroPortraitTextureName = HeroPortraitTextureName.PrefabAuto.Value.Factory.Create(GridDeck.transform);
				heroPortraitTextureName.Open(deckHeroInfo.Index, deckHeroInfo.Star, deckHeroInfo.Level, printName: false);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroPortraitTextureName.transform);
				_heroPortraitList.Add(heroPortraitTextureName);
			}
		}
		GridDeck.Reposition();
	}

	private void ClearPlayerDeck()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void SetInfoDetail(RecordsOfHonorRankingInfo rankingData)
	{
		foreach (HonoraryInfoBase info in _infoList)
		{
			UpdateInfo(info, rankingData);
		}
	}

	private void UpdateInfo(HonoraryInfoBase info, RecordsOfHonorRankingInfo rankingData)
	{
		bool flag = rankingData != null && info.IsActiveCondition(rankingData);
		UIHelper.EnableGameObject(info.TransformRoot, flag);
		if (flag)
		{
			info.SetInfo(rankingData);
		}
	}

	private void CreateHeroModel(RecordsOfHonorRankingInfo rankingData)
	{
		if (rankingData != null && !(HeroPreviewTexture == null) && rankingData.ContentType != RecordsOfHonorContentType.GuildSuppress)
		{
			UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(rankingData.PictureIndex);
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, avatarInfo.GetCreatureData().CodeName, avatarInfo.Star);
			renderFacadeBuilder.SetCostume(avatarInfo.CostumeIndex);
			renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
			UIHelper.InitHero3dPreview(ref _heroPreview, HeroPreviewTexture, avatarInfo.HeroIndex, 0, renderFacadeBuilder, "Normal", perspective: false);
		}
	}

	private void Release3dViewer()
	{
		UIHelper.CloseHero3dPreview(ref _heroPreview, HeroPreviewTexture);
	}

	private void ReleaseAndClearMenu()
	{
		Dictionary<int, HonoraryChamberMenuComponent>.Enumerator enumerator = _menuComponents.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FactoryBase.ReleaseComponent(enumerator.Current.Value);
		}
		_menuComponents.Clear();
	}

	private void SetSeasonComboBox()
	{
		ComboBoxSeason.Clear();
		ComboBoxSeason.OnChanged = OnChangeSeason;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_lastResponse.SeasonInfo); i++)
		{
			if (_lastResponse.SeasonInfo[i] == null)
			{
				continue;
			}
			if (!Define.IsLiveBuild())
			{
				Debug.LogError($"SetSeasonComboBox {_lastResponse.SeasonInfo[i].ContentType} seasonCount {__Extension_Method_ICollection__.COUNT(_lastResponse.SeasonInfo[i].Season)}");
			}
			if (_lastResponse.SeasonInfo[i].ContentType == _currentContentOpenType)
			{
				int[] season = _lastResponse.SeasonInfo[i].Season;
				int num = __Extension_Method_ICollection__.COUNT(season);
				for (int j = 0; j < num; j++)
				{
					int num2 = season[j];
					ComboBoxSeason.AddItem(num2, LocalizationManager.GetStringDirect("HONOR_RANK_SEASON_INFO", num2));
				}
			}
		}
	}

	private void OnChangeSeason(int season)
	{
		if (_currentSeason != season)
		{
			_currentSeason = season;
			RequestContentsRankInfo(_currentContentOpenType, _currentSeason, OnResponse);
		}
	}

	private void SetServerButton(ServerType serverType)
	{
		CustomToggleServer.Select(_serverType == ServerType.Local);
		CustomToggleWorld.Select(_serverType == ServerType.World);
		if (_serverType != serverType)
		{
			_serverType = serverType;
			SetInfos();
		}
	}

	public void OnClickButtonWorld()
	{
		SetServerButton(ServerType.World);
	}

	public void OnClickButtonLocal()
	{
		SetServerButton(ServerType.Local);
	}

	void HonoraryChamberMenuComponent.IListener.OnClick(HonoraryChamberMenuComponent caller)
	{
		if (caller == null || caller.MenuData == null)
		{
			VespaDebug.LogError(" caller == null || caller.HonoraryChamberMenuData == null");
		}
		else
		{
			OnSelectMenu(caller);
		}
	}

	void HonoraryChamberRankComponent.IListener.OnClick(HonoraryChamberRankComponent caller)
	{
		if (!(caller == null) && caller.RankingInfo != null && _currentRankingInfo != caller.RankingInfo)
		{
			_currentRankingInfo = caller.RankingInfo;
			SetRankingComponentSelect();
			SetRanker(caller.RankingInfo);
		}
	}

	private void SetRankingComponentSelect()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rankingComponentList); i++)
		{
			_rankingComponentList[i].Select = _rankingComponentList[i].RankingInfo == _currentRankingInfo;
		}
	}

	public void OnClickPreview()
	{
		PlayNamedAction();
	}

	private void PlayNamedAction()
	{
		if (!(_heroPreview == null))
		{
			RenderFacade renderFacade = _heroPreview.RenderFacade;
			PlayNamedAction(renderFacade, defaultNamedAction);
		}
	}

	private void PlayNamedAction(RenderFacade renderFacade, string animName)
	{
		if (!(renderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, animName);
			stockData.MotionEnd = DefineCreature.EMotionEnd.End;
			renderFacade.Action.Motion(stockData, blendMotion: true, null);
		}
	}

	private RecordsOfHonorRankingInfo[] CreateDummyList(RecordsOfHonorContentType contentsType, RecordsOfHonorRankingType rankingType)
	{
		List<RecordsOfHonorRankingInfo> list = new List<RecordsOfHonorRankingInfo>();
		IList<CountryCodeData> allData = GameRoot.Instance.CountryCodeDataContainer.GetAllData();
		for (int i = 0; i < 10; i++)
		{
			RecordsOfHonorRankingInfo recordsOfHonorRankingInfo = new RecordsOfHonorRankingInfo();
			list.Add(recordsOfHonorRankingInfo);
			recordsOfHonorRankingInfo.CountryCode = allData[UnityEngine.Random.Range(0, __Extension_Method_ICollection__.COUNT(allData) - 1)].Index;
			recordsOfHonorRankingInfo.ContentType = contentsType;
			recordsOfHonorRankingInfo.RankingType = rankingType;
			recordsOfHonorRankingInfo.Season = _currentSeason;
			recordsOfHonorRankingInfo.Rank = i + 1;
			recordsOfHonorRankingInfo.Id = i + 1;
			recordsOfHonorRankingInfo.Name = "Name" + (i + 1);
			recordsOfHonorRankingInfo.Level = i + 1;
			recordsOfHonorRankingInfo.PictureIndex = i + 1;
			recordsOfHonorRankingInfo.Score = i * 1000;
			recordsOfHonorRankingInfo.MasterAvatarIndex = (i + 1).ToString();
		}
		return list.ToArray();
	}
}
