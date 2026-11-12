using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NWindow.NBattle;
using NNguiExtension;
using NShared;
using NShared.PetCancelExplore;
using NShared.PetEndExplore;
using NShared.PetStartExplore;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetExplorePartyPopup : BaseWindow, PetPortraitComponent.IListener, PetManagement.IPetExploreWebListener
{
	private enum EPetExplorableStatus
	{
		Enable,
		OverMaxCount,
		NotEnoughFood,
		AlreadyContains,
		InvalidPetType,
		DisableStatus,
		Exploring,
		Invalid
	}

	public interface IListener
	{
		void OnClickExploreStart(int exploreIndex, int deckIndex, int[] petIndices);

		void OnClickExploreCancel(int exploreIndex, int deckIndex);

		void OnClickReceiveEnd(int exploreIndex, int deckIndex);

		void OnClickOpenRewardInfoPopup(int itemIndex);
	}

	public static string PrefabPath = "Prefab2/Window/Pet/Popup/PetExplorePartyPopup";

	private const string TimeFormat = "{0:D2}:{1:D2}:{2:D2}";

	public UIScrollGrid PetListGrid;

	private List<PetPortraitComponent> _listPetListComponent = new List<PetPortraitComponent>();

	public UIScrollGrid SelectPetGrid;

	private List<PetPortraitComponent> _listSelectPetListComponent = new List<PetPortraitComponent>();

	public UIComboBox ComboBoxDeckSelect;

	public UIButton ButtonStart;

	public UIButton ButtonCancel;

	public UIButton ButtonEnd;

	public UIButton ButtonReward;

	public UIBundleLoader ExploreThumbnail;

	public UILabel LabelExploreName;

	public UILabel LabelExploreStatus;

	public UILabel LabelReqFood;

	public UILabel LabelTime;

	public UILabel LabelDisableDesc;

	public Transform TransReward;

	public GameObject PetDisableRoot;

	private ItemIconComponent _itemIconComponent;

	private IListener _listener;

	private int _exploreIndex;

	private int _deckIndex;

	private NGame2.NUI.NWindow.IntervalUpdater _intervalUpdater;

	private PetHelper.EPetExploreInfoStatus _status;

	private Dictionary<int, EPetExplorableStatus> _dicCachedStatus = new Dictionary<int, EPetExplorableStatus>();

	public static void Open(int exploreIndex, int deckIndex, IListener listener)
	{
		WindowPrefab<PetExplorePartyPopup> windowPrefab = new WindowPrefab<PetExplorePartyPopup>(PrefabPath, GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(exploreIndex, deckIndex, listener);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(int exploreIndex, int deckIndex, IListener listener)
	{
		_listener = listener;
		_exploreIndex = exploreIndex;
		_deckIndex = deckIndex;
		SceneAutoBehaviour<PetManagement>.instance.AddPetExploreWebListener(this);
		Open(delegate
		{
			new List<int>();
			SetComboBox(deckIndex);
			Set(exploreIndex, deckIndex);
		}, delegate
		{
		});
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (ComboBoxDeckSelect != null)
		{
			ComboBoxDeckSelect.OnChanged = null;
			ComboBoxDeckSelect.Clear();
		}
		SceneAutoBehaviour<PetManagement>.instance.RemovePetExploreWebListener(this);
		ReleasePetListComonent();
		ReleaseSelectPetListComponent();
		ReleaseItemIconComponent();
		_listener = null;
		_intervalUpdater = null;
		_exploreIndex = 0;
		_deckIndex = 0;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_intervalUpdater != null)
		{
			_intervalUpdater.Update();
		}
	}

	private void ReleasePetListComonent()
	{
		FactoryBase.ReleaseAndClearList(_listPetListComponent);
	}

	private void ReleaseSelectPetListComponent()
	{
		FactoryBase.ReleaseAndClearList(_listSelectPetListComponent);
	}

	private void ReleaseItemIconComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	public void Set(int exploreIndex, int deckIndex)
	{
		PetExploreInfo exploreInfo = GetExploreInfo(exploreIndex, deckIndex);
		Set(exploreInfo);
	}

	private void Set(PetExploreInfo petExploreInfo)
	{
		int key = petExploreInfo?.ExploreIndex ?? _exploreIndex;
		PetAdventureData data = GameRoot.Instance.PetAdventureDataContainer.GetData(key);
		PetAdventureRewardData data2 = GameRoot.Instance.PetAdventureRewardDataContainer.GetData(key);
		List<int> petIndexList = GetPetIndexList(petExploreInfo);
		SetSelectPetListComponent(petIndexList);
		SetExploreRewardInfo(petExploreInfo, data, data2);
		SetPetListComponent(petIndexList);
	}

	private void SetExploreRewardInfo(PetExploreInfo exploreInfo, PetAdventureData adventureData, PetAdventureRewardData adventureRewardData)
	{
		PetHelper.EPetExploreInfoStatus ePetExploreInfoStatus = (_status = PetHelper.GetPetExploreInfoStatus(exploreInfo));
		_intervalUpdater = null;
		bool enable = ePetExploreInfoStatus == PetHelper.EPetExploreInfoStatus.Start;
		bool flag = ePetExploreInfoStatus == PetHelper.EPetExploreInfoStatus.Cancel;
		bool flag2 = ePetExploreInfoStatus == PetHelper.EPetExploreInfoStatus.End;
		bool show = flag || flag2;
		UIHelper.EnableGameObject(ButtonStart, enable);
		UIHelper.EnableGameObject(ButtonCancel, flag);
		UIHelper.EnableGameObject(ButtonEnd, flag2);
		UIHelper.EnableGameObject(ButtonReward, enable);
		string exploreName = ((adventureData != null) ? adventureData.NameKey : string.Empty);
		string exploreThumbnail = ((adventureData != null) ? adventureData.ThumbnailImagePath : string.Empty);
		int reqFood = adventureData?.ReqFood ?? 0;
		int rewardItem = adventureRewardData?.RewardItemIndex ?? 0;
		string descKey = (flag2 ? "EXPLORE_END" : "EXPLORE_DOING");
		SetExploreName(exploreName);
		SetExploreThumbnail(exploreThumbnail);
		SetReqFood(reqFood);
		SetRewardItem(rewardItem);
		SetExploreStatusLabel(ePetExploreInfoStatus);
		SetPetListDiable(show, descKey);
		switch (ePetExploreInfoStatus)
		{
		case PetHelper.EPetExploreInfoStatus.Start:
		{
			int exploreTime = adventureRewardData?.Time ?? 0;
			UpdateStartButton();
			SetExploreTime(exploreTime);
			break;
		}
		case PetHelper.EPetExploreInfoStatus.Cancel:
		{
			string text = ((exploreInfo != null) ? exploreInfo.EndTime : string.Empty);
			TimeSpan? petGetUpRemainTime = PetHelper.GetPetGetUpRemainTime(text);
			SetExploreTime(petGetUpRemainTime);
			SetUpdateExploreTime(text);
			break;
		}
		case PetHelper.EPetExploreInfoStatus.End:
			SetExploreTime(0);
			break;
		case PetHelper.EPetExploreInfoStatus.Error:
			break;
		}
	}

	private void SetPetListComponent(List<int> petList)
	{
		List<PetInfo> petInfoList = GetPetInfoList();
		int makeCompCount = __Extension_Method_ICollection__.COUNT(petInfoList);
		UIHelper.CreateOrReleaseCompnent(_listPetListComponent, makeCompCount, PetListGrid, PetPortraitComponent.SmallPrefabAuto);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petInfoList); i++)
		{
			PetInfo petInfo = petInfoList[i];
			if (petInfo != null)
			{
				PetPortraitComponent petPortraitComponent = _listPetListComponent[i];
				if (petPortraitComponent == null)
				{
					break;
				}
				petPortraitComponent.OpenAndRefresh(petInfo, this);
				SetPetPortraitComponent(petPortraitComponent);
			}
		}
		UIHelper.ResetGridPosition(PetListGrid);
	}

	private void UpdatePetListComponent()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listPetListComponent); i++)
		{
			PetPortraitComponent petPortraitComponent = _listPetListComponent[i];
			if (!(petPortraitComponent == null))
			{
				SetPetPortraitComponent(petPortraitComponent);
			}
		}
	}

	private void UpdatePetPortraitComponent(int petIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listPetListComponent); i++)
		{
			PetPortraitComponent petPortraitComponent = _listPetListComponent[i];
			if (!(petPortraitComponent == null) && petPortraitComponent.PetIndex == petIndex)
			{
				SetPetPortraitComponent(petPortraitComponent);
			}
		}
	}

	private void SetPetPortraitComponent(PetPortraitComponent comp)
	{
		if (!(comp == null))
		{
			int petIndex = comp.PetIndex;
			PetInfo petInfo = comp.PetInfo;
			bool flag = IsContainsSelectPetListComponent(petIndex);
			comp.SetSelection(flag);
			if (flag)
			{
				comp.ShowPetDiableStatus(disable: false);
			}
			else if (_status == PetHelper.EPetExploreInfoStatus.Start && !PetHelper.CanExplorePetCount(__Extension_Method_ICollection__.COUNT(_listSelectPetListComponent), _exploreIndex))
			{
				comp.ShowPetDiableStatus(disable: true);
			}
			else if (PetHelper.IsExploring(petIndex))
			{
				comp.ShowPetDiableStatus(disable: true);
			}
			else if (!PetHelper.CanExplorePetStatus(petInfo))
			{
				comp.ShowPetDiableStatus(disable: true);
			}
			else if (!PetHelper.CanExplorePetType(petInfo))
			{
				comp.ShowPetDiableStatus(disable: true);
			}
			else if (!PetHelper.IsEnoughExploreReqFood(petInfo, _exploreIndex))
			{
				comp.ShowPetDiableStatus(disable: true);
			}
			else
			{
				comp.ShowPetDiableStatus(disable: false);
			}
		}
	}

	private void SetComboBox(int deckIndex)
	{
		if (!(ComboBoxDeckSelect == null))
		{
			int maxExploreCount = PetHelper.GetMaxExploreCount();
			for (int i = 0; i < maxExploreCount; i++)
			{
				ComboBoxDeckSelect.AddItem(i + 1, "");
			}
			ComboBoxDeckSelect.SelectItem(deckIndex, isNotify: false);
			ComboBoxDeckSelect.OnChanged = OnChagnedDeck;
		}
	}

	private PetExploreInfo GetExploreInfo(int exploreIndex, int deckIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.Get(exploreIndex, deckIndex);
	}

	public List<PetInfo> GetPetInfoList()
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		List<PetInfo> petInfoLisExceptLinePet = GlobalAutoBehaviour<AccountManager>.instance.GetPetInfoLisExceptLinePet();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(petInfoLisExceptLinePet))
		{
			petInfoLisExceptLinePet.Sort(delegate(PetInfo lhs, PetInfo rhs)
			{
				EPetExplorableStatus ePetExplorableStatus = TryGetPetStatus(lhs);
				EPetExplorableStatus ePetExplorableStatus2 = TryGetPetStatus(rhs);
				int num = ePetExplorableStatus.CompareTo(ePetExplorableStatus2);
				if (num != 0)
				{
					return num;
				}
				int num2 = lhs?.PetIndex ?? int.MaxValue;
				int value = rhs?.PetIndex ?? int.MaxValue;
				return num2.CompareTo(value);
			});
			_dicCachedStatus.Clear();
		}
		return petInfoLisExceptLinePet;
	}

	private EPetExplorableStatus TryGetPetStatus(PetInfo petInfo)
	{
		int key = petInfo?.PetIndex ?? 0;
		EPetExplorableStatus value = EPetExplorableStatus.Invalid;
		if (!_dicCachedStatus.TryGetValue(key, out value))
		{
			value = GetExplorableStatus(petInfo);
			_dicCachedStatus.Add(key, value);
		}
		return value;
	}

	public void SetSelectPetListComponent(List<int> petList)
	{
		int makeCompCount = __Extension_Method_ICollection__.COUNT(petList);
		UIHelper.CreateOrReleaseCompnent(_listSelectPetListComponent, makeCompCount, SelectPetGrid, PetPortraitComponent.SmallPrefabAuto);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petList); i++)
		{
			int num = petList[i];
			if (num == 0)
			{
				continue;
			}
			PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(num);
			if (petInfo != null)
			{
				PetPortraitComponent petPortraitComponent = _listSelectPetListComponent[i];
				if (!(petPortraitComponent == null))
				{
					petPortraitComponent.OpenAndRefresh(petInfo, this);
				}
			}
		}
		UIHelper.ResetGridPosition(SelectPetGrid);
	}

	public List<int> GetPetIndexList(PetExploreInfo info)
	{
		return info?.GetPetIndexList();
	}

	private bool IsContainsSelectPetListComponent(int petIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSelectPetListComponent); i++)
		{
			PetPortraitComponent petPortraitComponent = _listSelectPetListComponent[i];
			if (!(petPortraitComponent == null) && petPortraitComponent.PetIndex == petIndex)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsSelectPetPortraitComp(PetPortraitComponent comp)
	{
		if (comp == null)
		{
			return false;
		}
		return _listSelectPetListComponent.Contains(comp);
	}

	private void RemoveSelectPetListComponent(int petIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSelectPetListComponent); i++)
		{
			PetPortraitComponent petPortraitComponent = _listSelectPetListComponent[i];
			if (!(petPortraitComponent == null) && petPortraitComponent.PetIndex == petIndex)
			{
				_listSelectPetListComponent.RemoveAt(i);
				FactoryBase.ReleaseComponent(petPortraitComponent);
				break;
			}
		}
		UIHelper.RepositionGrid(SelectPetGrid);
	}

	private void AddSelectPetListComponent(PetInfo petInfo)
	{
		if (petInfo != null)
		{
			PetPortraitComponent petPortraitComponent = UIHelper.AddGridItem(SelectPetGrid, PetPortraitComponent.SmallPrefabAuto);
			if (!(petPortraitComponent == null))
			{
				petPortraitComponent.Open(petInfo, this);
				_listSelectPetListComponent.Add(petPortraitComponent);
				UIHelper.RepositionGrid(SelectPetGrid);
			}
		}
	}

	private EPetExplorableStatus GetExplorableStatus(PetInfo petInfo)
	{
		if (petInfo == null)
		{
			return EPetExplorableStatus.Invalid;
		}
		int petIndex = petInfo.PetIndex;
		if (!PetHelper.CanExplorePetCount(__Extension_Method_ICollection__.COUNT(_listSelectPetListComponent), _exploreIndex))
		{
			return EPetExplorableStatus.OverMaxCount;
		}
		if (IsContainsSelectPetListComponent(petIndex))
		{
			return EPetExplorableStatus.AlreadyContains;
		}
		if (PetHelper.IsExploring(petIndex))
		{
			return EPetExplorableStatus.Exploring;
		}
		if (!PetHelper.CanExplorePetStatus(petInfo))
		{
			return EPetExplorableStatus.DisableStatus;
		}
		if (!PetHelper.CanExplorePetType(petInfo))
		{
			return EPetExplorableStatus.InvalidPetType;
		}
		if (!PetHelper.IsEnoughExploreReqFood(petInfo, _exploreIndex))
		{
			return EPetExplorableStatus.NotEnoughFood;
		}
		return EPetExplorableStatus.Enable;
	}

	private string GetDiablePetExploreMessageKey(EPetExplorableStatus explorableStatus)
	{
		switch (explorableStatus)
		{
		case EPetExplorableStatus.OverMaxCount:
		case EPetExplorableStatus.AlreadyContains:
			return string.Empty;
		case EPetExplorableStatus.Exploring:
			return LocalizationManager.GetStringDirect("ALREADYEXPLOREPET");
		case EPetExplorableStatus.NotEnoughFood:
			return LocalizationManager.GetStringDirect("NOTENOUGHFULLPOINT");
		case EPetExplorableStatus.DisableStatus:
			return LocalizationManager.GetStringDirect("NOTENOUGHSLEEP");
		default:
			return LocalizationManager.GetStringDirect("EXPLORE_REQ_TEXT");
		}
	}

	private void SetExploreName(string nameKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(nameKey);
		UIHelper.SetLabelText(LabelExploreName, stringDirect);
	}

	private void SetExploreStatusLabel(PetHelper.EPetExploreInfoStatus status)
	{
		string text = "";
		switch (status)
		{
		case PetHelper.EPetExploreInfoStatus.Start:
			text = LocalizationManager.GetStringDirect("EXPLORE_TIME");
			break;
		case PetHelper.EPetExploreInfoStatus.Cancel:
			text = LocalizationManager.GetStringDirect("EXPLORE_DOING");
			break;
		case PetHelper.EPetExploreInfoStatus.End:
			text = LocalizationManager.GetStringDirect("EXPLORE_END");
			break;
		}
		UIHelper.SetLabelText(LabelExploreStatus, text);
	}

	private void SetExploreThumbnail(string imagePath)
	{
		if (!(ExploreThumbnail == null) && !String_Extension_Method.IsNullOrEmpty(imagePath))
		{
			ExploreThumbnail.SetAssetPath(imagePath);
		}
	}

	private void SetPetListDiable(bool show, string descKey)
	{
		UIHelper.EnableGameObject(PetDisableRoot, show);
		UIHelper.SetLabelTextLocalizeDirect(LabelDisableDesc, descKey);
	}

	private void UpdateStartButton()
	{
		bool enable = PetHelper.EnoughExplorePetCount(__Extension_Method_ICollection__.COUNT(_listSelectPetListComponent), _exploreIndex);
		UIHelper.EnableButton(ButtonStart, enable, grayscaleIfDisable: true);
	}

	private void SetExploreTime(int time)
	{
		TimeSpan timeSpan = new TimeSpan(0, 0, time);
		string text = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		UIHelper.SetLabelText(LabelTime, text);
	}

	private void SetExploreTime(TimeSpan? timeSpan)
	{
		int num = (timeSpan.HasValue ? ((int)timeSpan.Value.TotalHours) : 0);
		int num2 = (timeSpan.HasValue ? timeSpan.Value.Minutes : 0);
		int num3 = (timeSpan.HasValue ? timeSpan.Value.Seconds : 0);
		string text = $"{num:D2}:{num2:D2}:{num3:D2}";
		UIHelper.SetLabelText(LabelTime, text);
	}

	private void SetRewardItem(int itemIndex)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			ReleaseItemIconComponent();
			return;
		}
		if (_itemIconComponent == null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(TransReward);
			_itemIconComponent.ResetPosition();
		}
		if (_itemIconComponent != null)
		{
			_itemIconComponent.Open(data, null, null);
		}
	}

	private void SetReqFood(int reqFood)
	{
		UIHelper.SetLabelText(LabelReqFood, reqFood.ToString());
	}

	private void SetUpdateExploreTime(string remainTimeStr)
	{
		if (String_Extension_Method.IsNullOrEmpty(remainTimeStr))
		{
			_intervalUpdater = null;
			return;
		}
		_intervalUpdater = new NGame2.NUI.NWindow.IntervalUpdater(delegate
		{
			TimeSpan? petGetUpRemainTime = PetHelper.GetPetGetUpRemainTime(remainTimeStr);
			SetExploreTime(petGetUpRemainTime);
			if (!petGetUpRemainTime.HasValue)
			{
				PetExploreInfo exploreInfo = GetExploreInfo(_exploreIndex, _deckIndex);
				if (exploreInfo != null && PetHelper.GetPetExploreInfoStatus(exploreInfo) == PetHelper.EPetExploreInfoStatus.End)
				{
					Set(_exploreIndex, _deckIndex);
					string title = LocalizationManager.GetStringDirect("EXPLORE_STOP");
					string message = LocalizationManager.GetStringDirect("EXPLORE_STOP_TEXT");
					GlobalAutoBehaviour<PopupManegement>.instance.Find(delegate(IWindowPrefab popup)
					{
						if (!(popup is WindowPrefab<MessagePopup3Button> windowPrefab))
						{
							return false;
						}
						return (string.Equals(windowPrefab.instance.Title, title) && string.Equals(windowPrefab.instance.Message, message)) ? true : false;
					})?.CloseWindow();
				}
			}
		});
		_intervalUpdater.SetInterval(0.5f);
	}

	private int[] GetSelectPetInidices()
	{
		int[] array = new int[__Extension_Method_ICollection__.COUNT(_listSelectPetListComponent)];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSelectPetListComponent); i++)
		{
			PetPortraitComponent petPortraitComponent = _listSelectPetListComponent[i];
			if (!(petPortraitComponent == null))
			{
				array[i] = petPortraitComponent.PetIndex;
			}
		}
		return array;
	}

	void PetPortraitComponent.IListener.OnClick(PetPortraitComponent comp)
	{
		if (_status != PetHelper.EPetExploreInfoStatus.Start)
		{
			return;
		}
		int petIndex = comp.PetIndex;
		if (IsSelectPetPortraitComp(comp) || IsContainsSelectPetListComponent(petIndex))
		{
			RemoveSelectPetListComponent(petIndex);
			UpdatePetPortraitComponent(petIndex);
			UpdateStartButton();
			return;
		}
		PetInfo petInfo = comp.PetInfo;
		EPetExplorableStatus explorableStatus = GetExplorableStatus(petInfo);
		if (explorableStatus == EPetExplorableStatus.Enable)
		{
			comp.SetSelection(select: true);
			AddSelectPetListComponent(petInfo);
			UpdateStartButton();
			return;
		}
		string diablePetExploreMessageKey = GetDiablePetExploreMessageKey(explorableStatus);
		if (!String_Extension_Method.IsNullOrEmpty(diablePetExploreMessageKey))
		{
			CenterMessage.Open(diablePetExploreMessageKey);
		}
	}

	private void OnChagnedDeck(int value)
	{
		if (value != _deckIndex)
		{
			_deckIndex = value;
			Set(_exploreIndex, _deckIndex);
		}
	}

	public void OnClickStart()
	{
		if (_listener != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_INFO_POPUP_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("RAIDER_CONTENTS_DELETE_DESC");
			MessagePopup.Open(stringDirect, stringDirect2);
		}
	}

	public void OnClickCancel()
	{
		string stringDirect = LocalizationManager.GetStringDirect("EXPLORE_STOP");
		string stringDirect2 = LocalizationManager.GetStringDirect("EXPLORE_STOP_TEXT");
		MessagePopup3Button.Open(stringDirect, stringDirect2, delegate
		{
			if (_listener != null)
			{
				_listener.OnClickExploreCancel(_exploreIndex, _deckIndex);
			}
		}, delegate
		{
		});
	}

	public void OnClickEnd()
	{
		if (_listener != null)
		{
			_listener.OnClickReceiveEnd(_exploreIndex, _deckIndex);
		}
	}

	public void OnClickRewardInfoPopup()
	{
		if (_listener != null)
		{
			int itemIndex = ((_itemIconComponent != null) ? _itemIconComponent.ItemIndex : 0);
			_listener.OnClickOpenRewardInfoPopup(itemIndex);
		}
	}

	void PetManagement.IPetExploreWebListener.OnStart(NShared.PetStartExplore.Response reponse)
	{
		PetExploreInfo petExploreInfo = reponse.PetExploreInfo;
		Set(petExploreInfo);
		PopupUIDispatchBattleFx.OpenPet(PopupUIDispatchBattleFx.AniState.Start, null);
	}

	void PetManagement.IPetExploreWebListener.OnCancel(NShared.PetCancelExplore.Response reponse)
	{
		Set(_exploreIndex, _deckIndex);
	}

	void PetManagement.IPetExploreWebListener.OnEnd(NShared.PetEndExplore.Response reponse)
	{
		Set(_exploreIndex, _deckIndex);
		BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetCurrencyResultInfos3(reponse.CurrencyResults).SetItemResultInfos(reponse.ItemResults);
		PetExploreResultPopup.Open(reponse.PetExploreInfo, popupOpenData, null);
	}
}
