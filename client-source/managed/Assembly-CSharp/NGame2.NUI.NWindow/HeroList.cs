using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.BookmarkHero;
using NShared.NJit;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class HeroList : FrameControlWindow, HeroPortraitComponent.Listener, HeroEquipInfo.IHeroIterator, HeroRunehengeIcon.IDataProvider, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, LobbyTopmostMenu.IMiniGuideButtonObserver
{
	public interface Listener
	{
		void OnClickHeroPortrait(HeroPortraitComponent heroPortrait);

		void OnClickAllHero();

		void OnClickRunehengeIcon();

		void ReOpen();

		void OnSaveBookmark();

		void OnPositionChange(int from, int to, bool bookmaked);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroList";

	public UIScrollGrid HeroListGrid;

	public Hero3DViewPanel Hero3DViewPanel;

	public HeroStatPanel HeroStatPanel;

	public HeroSkillListPanel HeroSkillListPanel;

	public HeroEquipListPanel HeroEquipListPanel;

	public HeroTranscendSkillListPanel HeroTranscendSkillListPanel;

	public HeroEquipStoragePanel HeroEquipStoragePanel;

	public ArchiveHeroProfilePanel HeroProfilePanel;

	public HeroRunehengeIcon HeroRunehengeIcon;

	public GameObject BookmarkButton;

	public GameObject GroupOnBookmark;

	public GameObject PromoteButton;

	public GameObject DemoteButton;

	private Dictionary<int, HeroPortraitComponent> _dictHeroes = new Dictionary<int, HeroPortraitComponent>();

	private Listener _listener;

	private int _SelectedHeroIndex;

	private List<int> _ListHeroIndices = new List<int>();

	private bool _enableBookmark;

	private int _numBookmarkedHeros;

	private static bool _isBookmarkChanged;

	private static bool _isBookmarkAutoSaved;

	private static DateTime _lastBookmarkChangeTime;

	private int _selectedGridIndex;

	public int SelectedHeroIndex => _SelectedHeroIndex;

	int LobbyTopmostMenu.IMiniGuideButtonObserver.WindowGuideIndex => 0;

	public void Init(Listener listener, int heroIndex, bool enableBookmark = false)
	{
		_listener = listener;
		_SelectedHeroIndex = heroIndex;
		Setup();
		_enableBookmark = enableBookmark;
		BookmarkButton.transform.parent.gameObject.SetActive(enableBookmark);
		MoveList(heroIndex, 20f);
		UpdateBookmarkButtons();
	}

	public static void BeginNewSession()
	{
		_isBookmarkChanged = false;
	}

	public bool SelectHero(int HeroIndex)
	{
		int oldHeroIndex = _SelectedHeroIndex;
		if (oldHeroIndex == HeroIndex)
		{
			return false;
		}
		_SelectedHeroIndex = HeroIndex;
		if (HeroRunehengeIcon != null)
		{
			HeroRunehengeIcon.Refresh();
		}
		if (_dictHeroes.ContainsKey(oldHeroIndex))
		{
			_ListHeroIndices.FindIndex((int value) => value == oldHeroIndex);
			_dictHeroes[oldHeroIndex].SetSelectSmall(select: false);
		}
		if (_dictHeroes.ContainsKey(_SelectedHeroIndex))
		{
			_dictHeroes[_SelectedHeroIndex].SetSelectSmall(select: true);
		}
		MoveList(HeroIndex);
		UpdateBookmarkButtons();
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		return true;
	}

	private void ClearHeroes()
	{
		FactoryBase.ReleaseAndClearList(_dictHeroes);
		_ListHeroIndices.Clear();
	}

	public override void Reset()
	{
		base.Reset();
		ClearHeroes();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateNotifyUI();
		if (_isBookmarkChanged && !_isBookmarkAutoSaved && (DateTime.Now - _lastBookmarkChangeTime).TotalSeconds > 10.0)
		{
			_isBookmarkAutoSaved = true;
			SaveBookmarks();
		}
	}

	private void ReloadTable()
	{
		if (!Input.GetKey(KeyCode.LeftControl) || !Input.GetKeyDown(KeyCode.F5))
		{
			return;
		}
		TableRoot.Instance.Reset();
		foreach (EquipItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager)
		{
			item.EquipScore.ClearCache();
		}
		VespaDebug.LogError("[EquipAutoData Reload] End");
	}

	public override void OnClosing()
	{
		if (HeroRunehengeIcon != null)
		{
			HeroRunehengeIcon.Close();
		}
		if (_isBookmarkChanged)
		{
			_listener.OnSaveBookmark();
			SaveBookmarks(showError: true);
		}
		base.OnClosing();
		foreach (IHeroPanel heroPanel in GetHeroPanels())
		{
			heroPanel.Close();
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(heroPanel as IUserObserver);
		}
		ClearHeroes();
	}

	private void SaveBookmarks(bool showError = false)
	{
		NShared.BookmarkHero.Request request = new NShared.BookmarkHero.Request();
		request.HeroIndex = _ListHeroIndices.GetRange(0, _numBookmarkedHeros).ToArray();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.SilenceError, delegate(NShared.BookmarkHero.Response response)
		{
			if (response.Result == ResultType.Success)
			{
				_isBookmarkChanged = false;
			}
			else if (showError)
			{
				MessagePopup.OpenError(Bookmark.HERO_BOOKMARK_FAILED_SAVE);
			}
		});
	}

	private void Setup()
	{
		if (HeroRunehengeIcon != null)
		{
			HeroRunehengeIcon.Open(this);
		}
		ClearHeroes();
		_numBookmarkedHeros = 0;
		GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true).ForEach(delegate(CachedHeroInfo heroInfo)
		{
			CreateHeroItem(heroInfo);
		});
		foreach (IHeroPanel heroPanel in GetHeroPanels())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(heroPanel as IUserObserver);
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void UpdateNotifyUI()
	{
		foreach (KeyValuePair<int, HeroPortraitComponent> dictHero in _dictHeroes)
		{
			if (!(dictHero.Value == null))
			{
				List<HeroNotifyComponent.ENotifyInfo> heroNotifyInfoList = SceneAutoBehaviour<HeroNotifyContainer>.instance.GetHeroNotifyInfoList(dictHero.Value.HeroInfo.HeroIndex);
				dictHero.Value.AddHeroNotify(heroNotifyInfoList, portraitSmall: true);
			}
		}
	}

	private void CreateHeroItem(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return;
		}
		if (GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex) == null)
		{
			VespaDebug.LogError("Cannot find creature data : " + heroInfo.HeroIndex);
			return;
		}
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(heroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
		if (_SelectedHeroIndex == heroInfo.HeroIndex)
		{
			heroPortraitComponent.SetSelectSmall(select: true);
		}
		if (heroInfo.BookmarkPriority > 0)
		{
			heroPortraitComponent.SetBookmarkedSmall(select: true);
		}
		string stringDirect = LocalizationManager.GetStringDirect("STATUS_HERO_DISPATCHED");
		heroPortraitComponent.SetDispatched(DispatchHelper.IsDispatchedHero(heroInfo), isSmall: true, stringDirect);
		_dictHeroes.Add(heroInfo.HeroIndex, heroPortraitComponent);
		_ListHeroIndices.Add(heroInfo.HeroIndex);
		if (heroInfo.BookmarkPriority > 0)
		{
			_numBookmarkedHeros++;
		}
	}

	private int FindSelectedGridIndex()
	{
		return _ListHeroIndices.FindIndex((int value) => value == _SelectedHeroIndex);
	}

	private void UpdateBookmarkChange(int index, int from, int to, bool bookmarked = true)
	{
		_isBookmarkChanged = true;
		_isBookmarkAutoSaved = false;
		_lastBookmarkChangeTime = DateTime.Now;
		HeroListGrid.Grid.GetChild(from).SetSiblingIndex(to);
		UIHelper.RepositionGrid(HeroListGrid);
		_listener.OnPositionChange(from, to, bookmarked);
		for (int i = 0; i < _numBookmarkedHeros; i++)
		{
			_dictHeroes[_ListHeroIndices[i]].HeroInfo.BookmarkPriority = _numBookmarkedHeros - i;
		}
		MoveList(_SelectedHeroIndex, 5f);
		UpdateBookmarkButtons();
	}

	public void OnBookmarkHero()
	{
		int num = FindSelectedGridIndex();
		if (num >= _numBookmarkedHeros && num < _ListHeroIndices.Count)
		{
			int num2 = -1;
			int item = _ListHeroIndices[num];
			_ListHeroIndices.RemoveAt(num);
			_ListHeroIndices.Insert(_numBookmarkedHeros, item);
			num2 = _numBookmarkedHeros;
			UpdateBookmarkChange(_SelectedHeroIndex, num, _numBookmarkedHeros++);
			_dictHeroes[_SelectedHeroIndex].SetBookmarkedSmall(select: true);
			Debug.Log("Bookmark from: " + num + " to: " + num2 + ", count = " + _numBookmarkedHeros);
		}
	}

	public void OnPromoteHero()
	{
		int num = FindSelectedGridIndex();
		if (num > 0 && num < _numBookmarkedHeros && num < _ListHeroIndices.Count)
		{
			_ListHeroIndices.Reverse(num - 1, 2);
			UpdateBookmarkChange(_SelectedHeroIndex, num, num - 1);
			Debug.Log("Promote from: " + num + ", count = " + _numBookmarkedHeros);
		}
	}

	public void OnDemoteHero()
	{
		int num = FindSelectedGridIndex();
		if (num >= 0 && num < _numBookmarkedHeros - 1 && num < _ListHeroIndices.Count)
		{
			_ListHeroIndices.Reverse(num, 2);
			UpdateBookmarkChange(_SelectedHeroIndex, num, num + 1);
			if (num == _numBookmarkedHeros - 2)
			{
				GlobalCenterMessage.Open(Bookmark.HERO_DEMOTED_TO_LAST);
			}
			Debug.Log("Demote from: " + num + ", count = " + _numBookmarkedHeros);
		}
	}

	public void OnUnbookmarkHero()
	{
		int num = FindSelectedGridIndex();
		if (num < 0 || num >= _numBookmarkedHeros)
		{
			return;
		}
		_ListHeroIndices.RemoveAt(num);
		_numBookmarkedHeros--;
		CachedHeroInfo heroInfo = _dictHeroes[_SelectedHeroIndex].HeroInfo;
		heroInfo.BookmarkPriority = 0;
		int num2 = _ListHeroIndices.Count;
		for (int i = _numBookmarkedHeros; i < _ListHeroIndices.Count; i++)
		{
			CachedHeroInfo heroInfo2 = _dictHeroes[_ListHeroIndices[i]].HeroInfo;
			if (HeroComparer.Default.Compare(heroInfo, heroInfo2) < 0)
			{
				num2 = i;
				break;
			}
		}
		_ListHeroIndices.Insert(num2, _SelectedHeroIndex);
		UpdateBookmarkChange(_SelectedHeroIndex, num, num2, bookmarked: false);
		_dictHeroes[_SelectedHeroIndex].SetBookmarkedSmall(select: false);
		Debug.Log("Unbookmark from: " + num + " to: " + num2 + ", count = " + _numBookmarkedHeros);
	}

	public void OnClickButton()
	{
		HeroListGrid.ScrollView.MoveRelative(new Vector2(0f, 5f * HeroListGrid.Grid.cellHeight));
		HeroListGrid.ScrollView.RestrictWithinBounds(instant: true);
	}

	public void OnClickAllHero()
	{
		_listener.OnClickAllHero();
	}

	public void OnClickRunehengeIcon()
	{
		if (_listener != null)
		{
			_listener.OnClickRunehengeIcon();
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		if (_listener != null)
		{
			int heroIndex = comp.HeroInfo.HeroIndex;
			if (SelectHero(heroIndex))
			{
				_listener.OnClickHeroPortrait(comp);
			}
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent comp, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	bool HeroEquipInfo.IHeroIterator.IsActive()
	{
		if (IsOpen())
		{
			return IsShowing();
		}
		return false;
	}

	void HeroEquipInfo.IHeroIterator.Visit(Action<HeroPortraitComponent> onVisit)
	{
		if (_dictHeroes == null)
		{
			return;
		}
		foreach (KeyValuePair<int, HeroPortraitComponent> dictHero in _dictHeroes)
		{
			Vespa_System_Action_Extension_Method.fire(onVisit, dictHero.Value);
		}
	}

	public IEnumerable<IHeroPanel> GetHeroPanels()
	{
		yield return Hero3DViewPanel;
		yield return HeroStatPanel;
		yield return HeroSkillListPanel;
		yield return HeroEquipListPanel;
		yield return HeroTranscendSkillListPanel;
		yield return HeroEquipStoragePanel;
		yield return HeroProfilePanel;
	}

	public void MoveList(int HeroIndex, float scrollStrength = 13f)
	{
		if (_ListHeroIndices.Count <= 4)
		{
			return;
		}
		int num = _ListHeroIndices.FindIndex((int value) => value == HeroIndex);
		if (num != -1)
		{
			_selectedGridIndex = num;
			HeroPortraitComponent heroPortraitComponent = _dictHeroes[_ListHeroIndices[num]];
			Transform cachedTransform = HeroListGrid.ScrollView.panel.cachedTransform;
			Vector3[] worldCorners = HeroListGrid.ScrollView.panel.worldCorners;
			Vector3 vector2;
			if (num <= 1)
			{
				Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[2]);
				vector2 = cachedTransform.InverseTransformPoint(_dictHeroes[_ListHeroIndices[0]].transform.position) - vector;
				vector2.y += HeroListGrid.Grid.cellHeight / 2f;
			}
			else if (num >= _ListHeroIndices.Count - 2)
			{
				Vector3 vector3 = cachedTransform.InverseTransformPoint(worldCorners[0]);
				vector2 = cachedTransform.InverseTransformPoint(_dictHeroes[_ListHeroIndices[_ListHeroIndices.Count - 1]].transform.position) - vector3;
				vector2.y -= HeroListGrid.Grid.cellHeight / 2f;
			}
			else
			{
				Vector3 position = (worldCorners[2] + worldCorners[0]) * 0.5f;
				Vector3 vector4 = cachedTransform.InverseTransformPoint(heroPortraitComponent.transform.position);
				Vector3 vector5 = cachedTransform.InverseTransformPoint(position);
				vector2 = vector4 - vector5;
			}
			if (!HeroListGrid.ScrollView.canMoveHorizontally)
			{
				vector2.x = 0f;
			}
			if (!HeroListGrid.ScrollView.canMoveVertically)
			{
				vector2.y = 0f;
			}
			vector2.z = 0f;
			Vector3 pos = cachedTransform.localPosition - vector2;
			SpringPanel.Begin(HeroListGrid.ScrollView.panel.cachedGameObject, pos, scrollStrength).onFinished = delegate
			{
				HeroListGrid.ScrollView.RestrictWithinBounds(instant: true);
			};
		}
	}

	private void UpdateBookmarkButtons()
	{
		if (_enableBookmark)
		{
			int num = _ListHeroIndices.FindIndex((int value) => value == _SelectedHeroIndex);
			bool flag = num < _numBookmarkedHeros;
			BookmarkButton.SetActive(!flag);
			GroupOnBookmark.SetActive(flag);
			if (flag)
			{
				PromoteButton.GetComponent<UIButton>().isEnabled = num > 0;
				DemoteButton.GetComponent<UIButton>().isEnabled = num < _numBookmarkedHeros - 1;
			}
		}
	}

	public void OnClickClose()
	{
		Close();
		SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<RecommendTranscendContainer>().Close();
	}

	public override void OnSystemBackButton()
	{
		if (HeroTranscendSkillListPanel.gameObject.activeSelf)
		{
			HeroTranscendSkillListPanel.ShowRemainTranscendPointAlarm();
			HeroTranscendSkillListPanel.SendPrevSelectedTranscendPage();
		}
		OnClickClose();
	}

	public override void ReOpen()
	{
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (_dictHeroes.ContainsKey(changedInfo.HeroIndex))
		{
			HeroPortraitComponent heroPortraitComponent = _dictHeroes[changedInfo.HeroIndex];
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup)
			{
				heroPortraitComponent.SetLevel(show: true);
			}
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
			{
				heroPortraitComponent.SetHeroImage();
				HeroSkillListPanel.Refresh();
				HeroTranscendSkillListPanel.Refresh();
			}
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLimitBreakLevel)
			{
				heroPortraitComponent.SetupFrameColor();
			}
		}
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.RunePage && HeroRunehengeIcon != null)
		{
			HeroRunehengeIcon.Refresh();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void OnOpenDetaliView()
	{
		if (HeroTranscendSkillListPanel != null)
		{
			HeroTranscendSkillListPanel.SendPrevSelectedTranscendPage(reset: false);
		}
	}

	public ItemIconComponent GetItemIconComponent(Transform tr)
	{
		if (HeroEquipListPanel == null)
		{
			return null;
		}
		return HeroEquipListPanel.GetItemIconComponent(tr);
	}

	public ItemIconComponent GetItemIconComponent(string uid)
	{
		if (HeroEquipListPanel == null)
		{
			return null;
		}
		return HeroEquipListPanel.GetItemIconComponent(uid);
	}

	CachedHeroInfo HeroRunehengeIcon.IDataProvider.GetHeroInfo()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(SelectedHeroIndex);
	}
}
