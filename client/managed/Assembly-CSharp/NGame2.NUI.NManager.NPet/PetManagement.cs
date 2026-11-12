using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NPet;
using NGame2.NUI.NPet;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUI.NWindow.NPet;
using NGame2.NUI.NWorldMap;
using NGameAsset;
using NShared;
using NShared.AddEquipStorageSlot;
using NShared.BuyPetIncubator;
using NShared.ChangePetAvatar;
using NShared.EggSupplier;
using NShared.GetEggRewards;
using NShared.NLocalization;
using NShared.PetAwaken;
using NShared.PetCancelExplore;
using NShared.PetEndExplore;
using NShared.PetIncubatorSlotExpansion;
using NShared.PetStartExplore;
using NShared.PetUpgradeTier;
using NShared.SetPetIncubator;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.NPet;

public class PetManagement : BaseManagement<PetManagement>, PetHouseWindow.IListener, PetList.IListener, PetSoulInven.IListener, PetListBook.Listener, PetInteractionWindow.IListener, SelectPlacePetPopup.IListener, PetIncubatorScene.Listener, PetIncubatorManageWindow.IListener, PetJukebox.IListener, PetFeedPopup.IListener, PetExploreListWindow.IListener, PetExploreListComponent.IListener, PetExplorePartyPopup.IListener, HeroFullSceneViewBase.Listener, PetIncubatorPopup.IListener
{
	public interface IPetExploreWebListener
	{
		void OnStart(NShared.PetStartExplore.Response reponse);

		void OnCancel(NShared.PetCancelExplore.Response reponse);

		void OnEnd(NShared.PetEndExplore.Response reponse);
	}

	private PetWebManager _petWebManager = new PetWebManager();

	private string _beforeBGM = string.Empty;

	private List<IPetExploreWebListener> _listPetExploreWebListener = new List<IPetExploreWebListener>();

	private const string StartIncubatingCode = "Pet.System.Incubator.Start";

	private const string FinishIncubatingCode = "Pet.System.Incubator";

	private const float MaxRaiderHouseOpenWaitTime = 10f;

	private float _startTime;

	public bool IsOpenPetHouse { get; set; }

	protected override void Awake()
	{
		base.Awake();
		AddInstance<PetList>("Resources/Prefab2/Window/Pet/PetList");
		AddInstance<PetAwakenResultWindow>("Resources/Prefab2/Window/Pet/PetAwakenResultWindow");
		AddInstance<PetListBook>("Resources/Prefab2/Window/Pet/PetListBook");
		AddInstance<PetSoulInven>("Resources/Prefab2/Window/Pet/PetSoulInven");
		AddInstance<PetHouseWindow>("Resources/Prefab2/Window/Pet/PetHouseWindow");
		AddInstance<PetJukebox>("Resources/Prefab2/Window/Pet/PetJukebox");
		AddInstance<PetInteractionWindow>("Resources/Prefab2/Window/Pet/PetInteractionWindow");
		AddInstance<PetDetailView>("Resources/Prefab2/Window/Pet/PetDetailView");
		AddInstance<SelectPlacePetPopup>("Resources/Prefab2/Window/Pet/Popup/SelectPlacePetPopup");
		AddInstance<PetIncubatorScene>("Resources/Prefab2/Window/Pet/PetIncubatorScene");
		AddInstance<PetFeedPopup>(PetFeedPopup.PrefabPath);
		AddInstance<PetExploreListWindow>(PetExploreListWindow.PrefabPath);
		AddInstance<PetIncubatorManageWindow>("Resources/Prefab2/Window/Pet/PetIncubatorManageWindow");
		AddInstance<PetIncubatorRewardPopup>(PetIncubatorRewardPopup.PrefabPath);
		AddInstance<PetIncubatorPopup>(PetIncubatorPopup.PrefabPath);
	}

	public void OpenPetList(PetInfo petInfo = null)
	{
		PetList petList = GetInstance<PetList>();
		if (!(petList == null) && !petList.IsOpen())
		{
			petList.Open(petInfo, this);
		}
	}

	public void OpenPetAwakenResultWindow(PetInfo petInfo)
	{
		PetAwakenResultWindow petAwakenResultWindow = GetInstance<PetAwakenResultWindow>();
		if (!(petAwakenResultWindow == null) && !petAwakenResultWindow.IsOpen())
		{
			petAwakenResultWindow.Open(petInfo);
		}
	}

	public void OpenPetArchiveWindow(int petIndex = 0)
	{
		PetListBook petListBook = GetInstance<PetListBook>();
		if (!(petListBook == null) && !petListBook.IsOpen())
		{
			petListBook.Open(this, petIndex);
		}
	}

	public void OpenPetSoulInven()
	{
		PetSoulInven petSoulInven = GetInstance<PetSoulInven>();
		if (!(petSoulInven == null) && !petSoulInven.IsOpen())
		{
			petSoulInven.Open(this);
		}
	}

	public void OpenPetIncubatorWindow()
	{
		PetIncubatorScene petIncubatorScene = GetInstance<PetIncubatorScene>();
		if (!(petIncubatorScene == null) && !petIncubatorScene.IsOpen())
		{
			petIncubatorScene.Open(this);
		}
	}

	public void OpenPetHouse(EPetHouseObjectType ePetHouseObjectType = EPetHouseObjectType.None, Action onEnd = null)
	{
		if (IsOpenPetHouse)
		{
			return;
		}
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (!(petHouseWindow == null) && !petHouseWindow.IsOpen())
		{
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (!(mapLayout == null) && mapLayout.Mediator != null)
			{
				petHouseWindow.Open(this, mapLayout.Mediator.MapComponent, ePetHouseObjectType, onEnd);
				IsOpenPetHouse = true;
			}
		}
	}

	public void OpenPetJukebox()
	{
		PetJukebox petJukebox = GetInstance<PetJukebox>();
		if (!(petJukebox == null) && !petJukebox.IsOpen())
		{
			petJukebox.Open(this);
		}
	}

	public void OpenPetInteractionWindow(PetAvatar petAvatar)
	{
		if (petAvatar == null)
		{
			return;
		}
		if (petAvatar != null)
		{
			PetInteractionWindow petInteractionWindow = GetInstance<PetInteractionWindow>();
			if (!(petInteractionWindow == null) && !petInteractionWindow.IsOpen())
			{
				petInteractionWindow.Open(new PetInteractionWindow.PetInteractionInfo
				{
					IListener = this,
					PetInfo = ((IPet)petAvatar).PetInfo,
					IPetAction = petAvatar,
					IPetInteraction = petAvatar
				});
			}
		}
	}

	public void OpenPetDetailView(int petIndex, bool isShowPetInfo = false, Action openAction = null, Action closeAction = null)
	{
		PetDetailView petDetailView = GetInstance<PetDetailView>();
		if (!(petDetailView == null) && !petDetailView.IsOpen())
		{
			PetDetailView.PetDetailViewInfo info = new PetDetailView.PetDetailViewInfo
			{
				PetInfo = new PetInfo
				{
					PetIndex = petIndex
				},
				IsShowPetStat = isShowPetInfo,
				IBaseListner = this,
				OpenAction = openAction,
				CloseAction = closeAction
			};
			petDetailView.Open(info);
		}
	}

	public void OpenPetFeedPopup()
	{
		PetFeedPopup petFeedPopup = GetInstance<PetFeedPopup>();
		if (!(petFeedPopup == null) && !petFeedPopup.IsOpen())
		{
			petFeedPopup.Open(this);
		}
	}

	private void OpenSelectPetPlacePopup()
	{
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (!(petHouseWindow == null))
		{
			SelectPlacePetPopup selectPlacePetPopup = GetInstance<SelectPlacePetPopup>();
			if (!(selectPlacePetPopup == null) && !selectPlacePetPopup.IsOpen())
			{
				selectPlacePetPopup.Open(this, petHouseWindow);
			}
		}
	}

	public void OpenPetTotalBuffPopup()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PET_USE_FACILITY_CONDITION_TEXT"));
		}
		else
		{
			PetTotalBuffPopup.OpenPopup();
		}
	}

	public void OpenPetIncubatorManageWindow(int selectedIndex)
	{
		PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
		if (!(petIncubatorManageWindow == null) && !petIncubatorManageWindow.IsOpen())
		{
			petIncubatorManageWindow.Open(this, selectedIndex);
		}
	}

	public void OpenPetIncubatorRewardPopup(int eggIndex)
	{
		PetIncubatorRewardPopup petIncubatorRewardPopup = GetInstance<PetIncubatorRewardPopup>();
		if (!(petIncubatorRewardPopup == null) && !petIncubatorRewardPopup.IsOpen())
		{
			petIncubatorRewardPopup.Open(eggIndex);
		}
	}

	public void OpenPetIncubatorPopup(int slotIndex)
	{
		PetIncubatorPopup petIncubatorPopup = GetInstance<PetIncubatorPopup>();
		if (!(petIncubatorPopup == null) && !petIncubatorPopup.IsOpen())
		{
			petIncubatorPopup.Open(this, slotIndex);
		}
	}

	public void Request<T, V>(T tRequest, V vResponse) where T : NShared.Request where V : NShared.Response
	{
	}

	public void PlayPetHouseBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_beforeBGM = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		string value = PetJukebox.PetHouseBGM.Value;
		if (_beforeBGM != value)
		{
			SingletonAuto<BgmManager2>.instance.Request(value, isEvent: true);
		}
	}

	public void ReturnPetHouseBGM()
	{
		string value = PetJukebox.PetHouseBGM.Value;
		SingletonAuto<BgmManager2>.instance.StopEvent(value);
		if (_beforeBGM != value)
		{
			SingletonAuto<BgmManager2>.instance.Request(_beforeBGM, isEvent: true);
		}
	}

	public void PlayPetHouseNodeTree(string effectName, Action endAction)
	{
		if (IsOpenPetHouse)
		{
			PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
			if (petHouseWindow == null || petHouseWindow.IsOpen())
			{
				petHouseWindow.PlayNodeTree(effectName, endAction);
			}
		}
	}

	private void PetHouseSetInitCamera()
	{
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (!(petHouseWindow == null))
		{
			petHouseWindow.SetInitCamera();
		}
	}

	public override void CloseAllActiveWindow(bool immediately = false, IEnumerable<IWindow> exceptions = null)
	{
		base.CloseAllActiveWindow(immediately, exceptions);
		ChangeTopMostMenuLayer(GameUIRoot.ELayerType.UI);
	}

	public void ChangeTopMostMenuLayer(GameUIRoot.ELayerType layerType)
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<LobbyTopmostMenu>())
		{
			LobbyTopmostMenu window = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<LobbyTopmostMenu>();
			if (window != null)
			{
				LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(layerType);
				window.ChangeLayerRecursive(layerInfo);
			}
		}
	}

	IEnumerator PetHouseWindow.IListener.RequestGetPetHouseInfo()
	{
		if (_petWebManager == null)
		{
			_petWebManager = new PetWebManager();
		}
		_startTime = Time.time;
		bool isComplete = false;
		Action endAction = delegate
		{
			isComplete = true;
		};
		_petWebManager.RequestGetPetHouseInfo(endAction);
		yield return null;
		while (!isComplete && !(Time.time - _startTime > 10f))
		{
			yield return null;
		}
	}

	void PetHouseWindow.IListener.OpenPetInteraction(PetAvatar petAvatar)
	{
		if (petAvatar != null)
		{
			OpenPetInteractionWindow(petAvatar);
		}
	}

	void PetHouseWindow.IListener.OpenPetHouseObject(EPetHouseObjectType type)
	{
		switch (type)
		{
		case EPetHouseObjectType.Mirror:
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.PetGacha.ToString(), openAnimation: false, PetHouseSetInitCamera);
			break;
		case EPetHouseObjectType.Management:
			OpenPetList();
			break;
		case EPetHouseObjectType.Jukebox:
			OpenPetJukebox();
			break;
		case EPetHouseObjectType.IncubatorHouse:
			OpenPetIncubatorWindow();
			break;
		case EPetHouseObjectType.FeedNormal:
		case EPetHouseObjectType.FeedEmpty:
		case EPetHouseObjectType.FeedMax:
			OpenPetFeedPopup();
			break;
		}
	}

	void PetHouseWindow.IListener.OpenSelectPetPlacePopup()
	{
		OpenSelectPetPlacePopup();
	}

	void PetHouseWindow.IListener.OpenPetSoulInven()
	{
		OpenPetSoulInven();
	}

	void PetHouseWindow.IListener.OpenPetListBookWindow()
	{
		OpenPetArchiveWindow();
	}

	void PetHouseWindow.IListener.OpenPetExploreWindow()
	{
		OpenPetExploreWindow();
	}

	void PetHouseWindow.IListener.RequestPetEggGacha()
	{
		NShared.EggSupplier.Request request = new NShared.EggSupplier.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EggSupplier.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.EggSupplier.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetGachaItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetMiscInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.PetGachaItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.PetGachaItemResults) || !__Extension_Method_ICollection__.IsNullOrEmpty(response.ItemResults))
				{
					List<ItemResultInfo> list = new List<ItemResultInfo>();
					for (int i = 0; i < __Extension_Method_ICollection__.COUNT(response.PetGachaItemResults); i++)
					{
						PetAddResultInfo petAddResultInfo = response.PetGachaItemResults[i];
						if (!__Extension_Method_ICollection__.IsNullOrEmpty(petAddResultInfo.PetSoulResults))
						{
							list.AddRange(petAddResultInfo.PetSoulResults);
						}
					}
					list.AddRange(response.ItemResults);
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("RAIDER_EGG_RECEIVE_RESULT")).SetItemResultInfos(list.ToArray()), isGlobal: false);
				}
			}
		});
	}

	void PetList.IListener.OnClickAwaken(int petIndex, Action onCallback)
	{
		NShared.PetAwaken.Request request = new NShared.PetAwaken.Request
		{
			PetIndex = petIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PetAwaken.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.PetAwaken.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.PetAwaken.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				OpenPetAwakenResultWindow(response.PetResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetResult);
				Vespa_System_Action_Extension_Method.fire(onCallback);
			}
		});
	}

	void PetList.IListener.OnClickMainPet(int petIndex)
	{
		NShared.ChangePetAvatar.Request request = new NShared.ChangePetAvatar.Request
		{
			PetIndex = petIndex,
			AvatarPetIndex = petIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ChangePetAvatar.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.ChangePetAvatar.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.ChangePetAvatar.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetMiscInfos);
			}
		});
	}

	void PetList.IListener.OnClickSelectPetPlacePopup()
	{
		OpenSelectPetPlacePopup();
	}

	void PetList.IListener.OnClickDetailView(int petIndex, Action openAction, Action closeAction)
	{
		OpenPetDetailView(petIndex, isShowPetInfo: false, openAction, closeAction);
	}

	void PetList.IListener.OnClickArchive(int petIndex)
	{
		OpenPetArchiveWindow(petIndex);
	}

	void PetList.IListener.OnClose()
	{
		PetHouseSetInitCamera();
	}

	void PetList.IListener.OnClickOpenPetTotalBuffPopup()
	{
		OpenPetTotalBuffPopup();
	}

	void PetSoulInven.IListener.OnClickUninon(int[] PetSouls, Action onEnd = null)
	{
		NShared.PetUpgradeTier.Request request = new NShared.PetUpgradeTier.Request();
		request.PetSouls = PetSouls;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PetUpgradeTier.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.PetUpgradeTier.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.PetUpgradeTier.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetResult);
				Vespa_System_Action_Extension_Method.fire(onEnd);
				if (response.PetResult != null)
				{
					ItemData itemDataByTypeAndValue = GameRoot.Instance.ItemDataContainer.GetItemDataByTypeAndValue(ItemType.Pet, 0, response.PetResult.PetIndex);
					if (itemDataByTypeAndValue == null)
					{
						VespaDebug.LogError("petItemData == null");
					}
					Item3dModelPopup.PreLoad(itemDataByTypeAndValue, itemDataByTypeAndValue.Item3dModelNodeTree).OpenPopup(null);
				}
				else if (response.ItemResults != null)
				{
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("GET_TIERUPGRADE_REWARD_CENTER_TEXT")).SetItemResultInfos(response.ItemResults), isGlobal: false);
				}
			}
		});
	}

	void PetInteractionWindow.IListener.PetEat(int petIndex, int eatCount, Action<bool> endAction)
	{
		if (_petWebManager != null)
		{
			_petWebManager.RequestFeedThePet(petIndex, eatCount, endAction);
		}
	}

	void PetInteractionWindow.IListener.NextPetInteraction(int currentPeIndex, int nextIndex)
	{
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (!(petHouseWindow == null) && petHouseWindow.IsOpen())
		{
			petHouseWindow.NextPetInteraction(currentPeIndex, nextIndex);
		}
	}

	void PetInteractionWindow.IListener.OpenPetDetailView(int petIndex)
	{
		OpenPetDetailView(petIndex);
	}

	void PetInteractionWindow.IListener.OpenSelectPetPlacePopup()
	{
		OpenSelectPetPlacePopup();
	}

	void PetInteractionWindow.IListener.OpenManagePetWindow(PetInfo petInfo)
	{
		OpenPetList(petInfo);
	}

	void PetIncubatorScene.Listener.OnClickIncubator(int slotIndex)
	{
		OpenPetIncubatorManageWindow(slotIndex);
	}

	void PetIncubatorScene.Listener.OnClickAddSlot(int slotIndex)
	{
		PetIncubatorSlotData slotData = GameRoot.Instance.PetIncubatorSlotDataContainer.GetData(slotIndex);
		Action<bool> callback = delegate(bool isSuccess)
		{
			if (isSuccess && CheckEnoughCurrency(slotData.BuyCurrencyType, slotData.BuyCurrencyValue))
			{
				PetIncubatorScene petIncubatorScene = GetInstance<PetIncubatorScene>();
				if (petIncubatorScene != null && petIncubatorScene.IsOpen())
				{
					petIncubatorScene.OnCompleteAddSlot();
				}
			}
		};
		IncubatorPriceMessagePopup.Open(IncubatorPriceMessagePopup.EIncubatorPriceType.Slot, slotData.BuyCurrencyType, slotData.BuyCurrencyValue, slotData.Mileage, callback);
	}

	void PetIncubatorScene.Listener.RequestAddSlot(int slotIndex)
	{
		if (GameRoot.Instance.PetIncubatorSlotDataContainer.GetData(slotIndex) == null)
		{
			return;
		}
		NShared.PetIncubatorSlotExpansion.Request request = new NShared.PetIncubatorSlotExpansion.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PetIncubatorSlotExpansion.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.PetIncubatorSlotExpansion.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfos);
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorManager.Apply(response.PetIncubatorInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				PetIncubatorScene petIncubatorScene = GetInstance<PetIncubatorScene>();
				if (petIncubatorScene != null && petIncubatorScene.IsOpen())
				{
					petIncubatorScene.RefreshComponents();
				}
				PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
				if (petHouseWindow != null)
				{
					petHouseWindow.AddNewIncubatorObject(slotIndex);
				}
				OpenPetIncubatorManageWindow(slotIndex);
			}
		});
	}

	void PetIncubatorScene.Listener.OnClickOpenPetTotalBuffPopup()
	{
		OpenPetTotalBuffPopup();
	}

	void PetIncubatorScene.Listener.OnClose()
	{
		PetHouseSetInitCamera();
	}

	void PetIncubatorScene.Listener.OnClickFinishIncubating(PetIncubatorSlotInfo slotInfo)
	{
		NShared.GetEggRewards.Request request = new NShared.GetEggRewards.Request();
		request.SlotIndex = slotInfo.SlotIndex;
		request.IncubatorIndex = slotInfo.MountingIndex;
		request.ItemIndex = slotInfo.SetItemIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetEggRewards.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetEggRewards.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfo);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetAddResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.PetAddResultInfos);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.PetAddResultInfos))
				{
					List<ItemResultInfo> list = new List<ItemResultInfo>();
					List<PetInfo> petResultInfoList = new List<PetInfo>();
					for (int i = 0; i < __Extension_Method_ICollection__.COUNT(response.PetAddResultInfos); i++)
					{
						PetAddResultInfo petAddResultInfo = response.PetAddResultInfos[i];
						if (!__Extension_Method_ICollection__.IsNullOrEmpty(petAddResultInfo.PetSoulResults))
						{
							list.AddRange(petAddResultInfo.PetSoulResults);
						}
						if (petAddResultInfo.PetResult != null)
						{
							petResultInfoList.Add(petAddResultInfo.PetResult);
						}
					}
					PetIncubatorScene petIncubatorScene = GetInstance<PetIncubatorScene>();
					if (petIncubatorScene != null)
					{
						petIncubatorScene.EnableTouch(isEnable: false);
						petIncubatorScene.OnStartPlayOpenIncubator(response.PetIncubatorSlotInfo.SlotIndex);
					}
					BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("GET_INCUBATOR_REWARD_CENTER_TEXT")).SetItemResultInfos(list.ToArray());
					Action onEnd = delegate
					{
						EndPlayOpenIncubator(response.PetIncubatorSlotInfo.SlotIndex);
					};
					PlayOpenIncubator(response.PetIncubatorSlotInfo.SlotIndex, delegate
					{
						PetIncubatorScene petIncubatorScene2 = GetInstance<PetIncubatorScene>();
						if (petIncubatorScene2 != null)
						{
							petIncubatorScene2.EnableTouch(isEnable: true);
						}
						VespaCoroutine.Start(coPlayPetIncubatorResult(petResultInfoList, popupOpen, onEnd));
					});
				}
			}
		});
	}

	private void PlayOpenIncubator(int slotIndex, Action onEnd)
	{
		PlayIncubatorSound("Pet.System.Incubator");
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (petHouseWindow != null)
		{
			petHouseWindow.SetIncubatorState(slotIndex, PetIncubatorObjectInfo.IncubatorStateType.Open, onEnd);
		}
	}

	private void EndPlayOpenIncubator(int slotIndex)
	{
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (petHouseWindow != null)
		{
			petHouseWindow.SetIncubatorState(slotIndex, PetIncubatorObjectInfo.IncubatorStateType.Phase0, null, needInfoUpdate: true);
		}
	}

	private void PlayIncubatorSound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode,
			LocalTimer = MainTimer.DefaultLocal
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	private IEnumerator coPlayPetIncubatorResult(List<PetInfo> petInfoList, BaseRewardPopup.PopupOpenData popupOpenData, Action onEnd)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petInfoList); i++)
		{
			CreatureData creatureData = petInfoList[i].CreatureData;
			ItemData itemDataByTypeAndValue = GameRoot.Instance.ItemDataContainer.GetItemDataByTypeAndValue(ItemType.Pet, 0, creatureData.Index);
			if (itemDataByTypeAndValue != null)
			{
				bool isClose3DModelPopup = false;
				Item3dModelPopup.PreLoad(itemDataByTypeAndValue, itemDataByTypeAndValue.Item3dModelNodeTree).OpenPopup(delegate
				{
					isClose3DModelPopup = true;
				});
				while (!isClose3DModelPopup)
				{
					yield return null;
				}
			}
		}
		BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false, onEnd);
	}

	void SelectPlacePetPopup.IListener.CompletePlacePet(bool isRequest, List<PlacePetInfo> placePetInfoList)
	{
		SelectPlacePetPopup selectPetPlacePopup = GetInstance<SelectPlacePetPopup>();
		if (selectPetPlacePopup == null)
		{
			return;
		}
		if (isRequest)
		{
			PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
			if (petHouseWindow == null)
			{
				return;
			}
			if (_petWebManager == null)
			{
				_petWebManager = new PetWebManager();
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(placePetInfoList))
			{
				return;
			}
			List<int> list = new List<int>();
			list.Clear();
			PlacePetInfo placePetInfo = null;
			int num = 0;
			for (int i = 0; i < placePetInfoList.Count; i++)
			{
				num = 0;
				placePetInfo = placePetInfoList[i];
				if (placePetInfo != null)
				{
					num = placePetInfo.PetIndex;
				}
				list.Add(num);
			}
			_petWebManager.RequestChangePetLayout(list.ToArray(), delegate
			{
				selectPetPlacePopup.Close();
				petHouseWindow.PlacePet(0f, placePetInfoList);
			});
		}
		else
		{
			selectPetPlacePopup.Close();
		}
	}

	void PetJukebox.IListener.OnClickBackButton()
	{
		PetHouseSetInitCamera();
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
	}

	void PetIncubatorPopup.IListener.OnClickEquipIncubator(int incubatorIndex, int slotIndex, Action<PetIncubatorSlotInfo> onSuccess)
	{
		PetIncubatorSlotInfo petIncubatorSlotInfo = GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Get(slotIndex);
		if (petIncubatorSlotInfo == null)
		{
			VespaDebug.LogError("SlotData is null");
		}
		else if (PetHouseObjectManager.GetIncubatorState(petIncubatorSlotInfo) != PetIncubatorObjectInfo.IncubatorStateType.Phase0)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("PETINCUBATOR_REPLACEMENT_IMPOSSIBLE_NOTI");
			PayShopMessagePopup.Open(title, stringDirect);
		}
		else
		{
			RequestSetPetIncubator(incubatorIndex, slotIndex, onSuccess);
		}
	}

	private void RequestSetPetIncubator(int incubatorIndex, int slotIndex, Action<PetIncubatorSlotInfo> onSuccess)
	{
		NShared.SetPetIncubator.Request request = new NShared.SetPetIncubator.Request
		{
			IncubatorIndex = incubatorIndex,
			SlotIndex = slotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetPetIncubator.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetPetIncubator.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfo);
				Vespa_System_Action_Extension_Method.fire(onSuccess, response.PetIncubatorSlotInfo);
				PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
				if (petIncubatorManageWindow != null && petIncubatorManageWindow.IsOpen())
				{
					petIncubatorManageWindow.OnSuccessAddIncubatorSlot(slotIndex);
				}
			}
		});
	}

	void PetIncubatorPopup.IListener.OnClickBuyIncubator(int incubatorIndex, int slotIndex, Action<PetIncubatorInfo> onSuccess)
	{
		PetIncubatorData incubatorData = GameRoot.Instance.PetIncubatorDataContainer.GetData(incubatorIndex);
		if (incubatorData == null)
		{
			return;
		}
		IncubatorPriceMessagePopup.Open(IncubatorPriceMessagePopup.EIncubatorPriceType.Incubator, incubatorData.BuyCurrencyType, incubatorData.BuyCurrencyValue, incubatorData.Mileage, delegate(bool isSuccess)
		{
			if (isSuccess && CheckEnoughCurrency(incubatorData.BuyCurrencyType, incubatorData.BuyCurrencyValue))
			{
				RequestBuyPetIncubator(incubatorIndex, slotIndex, onSuccess);
			}
		}, incubatorIndex);
	}

	private void RequestBuyPetIncubator(int incubatorIndex, int slotIndex, Action<PetIncubatorInfo> onSuccess)
	{
		NShared.BuyPetIncubator.Request request = new NShared.BuyPetIncubator.Request
		{
			IncubatorIndex = incubatorIndex,
			SlotIndex = slotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyPetIncubator.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result == NShared.BuyPetIncubator.ResultType.NotEnoughCost && response.Result != NShared.BuyPetIncubator.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorManager.Apply(response.PetIncubatorInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				Vespa_System_Action_Extension_Method.fire(onSuccess, response.PetIncubatorInfo);
			}
		});
	}

	private bool CheckEnoughCurrency(CurrencyType currencyType, int requirePrice)
	{
		switch (currencyType)
		{
		case CurrencyType.Gold:
			if (requirePrice > GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold)
			{
				OpenPopupNotEnoughMoney(currencyType);
				return false;
			}
			break;
		case CurrencyType.Gem:
		{
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			if (requirePrice > totalGem)
			{
				OpenPopupNotEnoughMoney(currencyType);
				return false;
			}
			break;
		}
		}
		return true;
	}

	private void OpenPopupNotEnoughMoney(CurrencyType currencyType)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString((currencyType == CurrencyType.Gem) ? NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM : NShared.NLocalization.System.MOVE_GOLD_SHOP_CONFIRM);
		PayShopMessage3ButtonPopup.Open(title, message, delegate
		{
			if (currencyType == CurrencyType.Gem)
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
			}
			else if (currencyType == CurrencyType.Gold)
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
			}
			PetIncubatorPopup petIncubatorPopup = GetInstance<PetIncubatorPopup>();
			if (petIncubatorPopup != null && petIncubatorPopup.IsOpen())
			{
				petIncubatorPopup.Close();
			}
		});
	}

	void PetFeedPopup.IListener.RequestFeedingPet(PetInfo petInfo, int amount, Action<bool> callBack)
	{
		if (_petWebManager != null)
		{
			_petWebManager.RequestFeedThePet(petInfo.PetIndex, amount, callBack);
		}
	}

	void PetFeedPopup.IListener.OnClickBackButton()
	{
		PetHouseSetInitCamera();
	}

	IEnumerator PetListBook.Listener.OnEnterIncubator()
	{
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (petHouseWindow == null)
		{
			yield break;
		}
		PetList petList = GetInstance<PetList>();
		if (petList != null && petList.IsOpen())
		{
			petList.Close();
			while (petHouseWindow.IsFocusObject())
			{
				yield return null;
			}
		}
		if (!IsOpenPetHouse)
		{
			OpenPetHouse(EPetHouseObjectType.IncubatorHouse);
		}
		else
		{
			petHouseWindow.FocusPetHouseObject(EPetHouseObjectType.IncubatorHouse, null);
		}
	}

	IEnumerator PetListBook.Listener.OnEnterSoulInven()
	{
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (petHouseWindow == null)
		{
			yield break;
		}
		PetList petList = GetInstance<PetList>();
		if (petList != null && petList.IsOpen())
		{
			petList.Close();
			while (petHouseWindow.IsFocusObject())
			{
				yield return null;
			}
		}
		if (!IsOpenPetHouse)
		{
			OpenPetHouse(EPetHouseObjectType.None, delegate
			{
				OpenPetSoulInven();
			});
		}
		else
		{
			OpenPetSoulInven();
		}
	}

	public void OpenPetExploreWindow()
	{
		PetExploreListWindow petExploreListWindow = GetInstance<PetExploreListWindow>();
		if (!(petExploreListWindow == null) && !petExploreListWindow.IsOpen())
		{
			petExploreListWindow.Open(this);
		}
	}

	public void AddPetExploreWebListener(IPetExploreWebListener petExploreWebListener)
	{
		if (petExploreWebListener != null)
		{
			_listPetExploreWebListener.Remove(petExploreWebListener);
			_listPetExploreWebListener.Add(petExploreWebListener);
		}
	}

	public void RemovePetExploreWebListener(IPetExploreWebListener petExploreWebListener)
	{
		if (petExploreWebListener != null)
		{
			_listPetExploreWebListener.Remove(petExploreWebListener);
		}
	}

	void PetExploreListComponent.IListener.OnClickButton(PetExploreListComponent caller)
	{
		if (!(caller == null))
		{
			PetExplorePartyPopup.Open(caller.ExploreIndex, 1, this);
		}
	}

	void PetExplorePartyPopup.IListener.OnClickExploreStart(int exploreIndex, int deckIndex, int[] petIndices)
	{
		PetHelper.RequestPetExploreStart(exploreIndex, deckIndex, petIndices, delegate(NShared.PetStartExplore.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.PetStartExplore.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(res.PetResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.Apply(res.PetExploreInfo);
				_listPetExploreWebListener.ForEach(delegate(IPetExploreWebListener listener)
				{
					listener.OnStart(res);
				});
			}
		});
	}

	void PetExplorePartyPopup.IListener.OnClickExploreCancel(int exploreIndex, int deckIndex)
	{
		PetHelper.RequestPetExploreCancel(exploreIndex, deckIndex, delegate(NShared.PetCancelExplore.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.PetCancelExplore.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(res.PetResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.Remove(res.PetExploreInfo);
				_listPetExploreWebListener.ForEach(delegate(IPetExploreWebListener listener)
				{
					listener.OnCancel(res);
				});
			}
		});
	}

	void PetExplorePartyPopup.IListener.OnClickReceiveEnd(int exploreIndex, int deckIndex)
	{
		PetHelper.RequestPetExploreEnd(exploreIndex, deckIndex, delegate(NShared.PetEndExplore.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.PetEndExplore.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(res.PetResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.Remove(res.PetExploreInfo);
				_listPetExploreWebListener.ForEach(delegate(IPetExploreWebListener listener)
				{
					listener.OnEnd(res);
				});
			}
		});
	}

	void PetExplorePartyPopup.IListener.OnClickOpenRewardInfoPopup(int itemIndex)
	{
		PetExploreRewardPopup.Open(itemIndex);
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null)
		{
			PackageItemData data2 = GameRoot.Instance.PackageItemDataContainer.GetData(data.Code);
			if (data2 != null)
			{
				GameRoot.Instance.RewardDataContainer.GetData(data2.RewardIndex1);
			}
		}
	}

	void PetIncubatorManageWindow.IListener.OnClickEggRewardPopup(int eggIndex)
	{
		OpenPetIncubatorRewardPopup(eggIndex);
	}

	void PetIncubatorManageWindow.IListener.OnClickStartIncubating(int slotIndex, int incubatorIndex, int eggIndex)
	{
		if (_petWebManager == null)
		{
			return;
		}
		Action successAction = delegate
		{
			PlayIncubatorSound("Pet.System.Incubator.Start");
			PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
			if (petHouseWindow != null)
			{
				petHouseWindow.SetIncubatorState(slotIndex, PetIncubatorObjectInfo.IncubatorStateType.Phase1Start, null, needInfoUpdate: true);
			}
			PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
			if (petIncubatorManageWindow != null)
			{
				petIncubatorManageWindow.OnEndStartIncubating();
			}
		};
		_petWebManager.RequestStartIncubating(slotIndex, incubatorIndex, eggIndex, successAction);
	}

	void PetIncubatorManageWindow.IListener.OnClickCancelIncubating(int slotIndex, int incubatorIndex)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("PETINCUBATOR_HATCHING_CANCEL_NOTI");
		PayShopMessage3ButtonPopup.Open(title, stringDirect, delegate
		{
			if (_petWebManager != null)
			{
				Action successAction = delegate
				{
					PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
					if (petHouseWindow != null)
					{
						petHouseWindow.SetIncubatorState(slotIndex, PetIncubatorObjectInfo.IncubatorStateType.Phase0, null, needInfoUpdate: true);
					}
					PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
					if (petIncubatorManageWindow != null)
					{
						petIncubatorManageWindow.OnEndCancelIncubating();
					}
				};
				_petWebManager.RequestCancelIncubating(slotIndex, incubatorIndex, successAction);
			}
		});
	}

	void PetIncubatorManageWindow.IListener.OnClickChangeIncubator(int slotIndex)
	{
		OpenPetIncubatorPopup(slotIndex);
	}

	void PetIncubatorManageWindow.IListener.OnClickAddSlot(int slotIndex)
	{
		PetIncubatorSlotData slotData = GameRoot.Instance.PetIncubatorSlotDataContainer.GetData(slotIndex);
		Action<bool> callback = delegate(bool isSuccess)
		{
			if (isSuccess && CheckEnoughCurrency(slotData.BuyCurrencyType, slotData.BuyCurrencyValue))
			{
				PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
				if (petIncubatorManageWindow != null && petIncubatorManageWindow.IsOpen())
				{
					petIncubatorManageWindow.OnCompleteAddSlot();
				}
			}
		};
		IncubatorPriceMessagePopup.Open(IncubatorPriceMessagePopup.EIncubatorPriceType.Slot, slotData.BuyCurrencyType, slotData.BuyCurrencyValue, slotData.Mileage, callback);
	}

	void PetIncubatorManageWindow.IListener.RequestAddSlot(int slotIndex)
	{
		if (GameRoot.Instance.PetIncubatorSlotDataContainer.GetData(slotIndex) == null)
		{
			return;
		}
		NShared.PetIncubatorSlotExpansion.Request request = new NShared.PetIncubatorSlotExpansion.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.PetIncubatorSlotExpansion.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.PetIncubatorSlotExpansion.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfos);
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorManager.Apply(response.PetIncubatorInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
				if (petIncubatorManageWindow != null && petIncubatorManageWindow.IsOpen())
				{
					petIncubatorManageWindow.OnSuccessAddIncubatorSlot(slotIndex);
				}
				PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
				if (petHouseWindow != null)
				{
					petHouseWindow.AddNewIncubatorObject(slotIndex);
				}
			}
		});
	}

	void PetIncubatorManageWindow.IListener.OnClickFinishIncubating(int slotIndex, int incubatorIndex, int itemIndex)
	{
		NShared.GetEggRewards.Request request = new NShared.GetEggRewards.Request();
		request.SlotIndex = slotIndex;
		request.IncubatorIndex = incubatorIndex;
		request.ItemIndex = itemIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetEggRewards.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(NShared.AddEquipStorageSlot.ResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetEggRewards.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetIncubatorSlotManager.Apply(response.PetIncubatorSlotInfo);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetAddResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.PetAddResultInfos);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.PetAddResultInfos))
				{
					List<ItemResultInfo> list = new List<ItemResultInfo>();
					List<PetInfo> petResultInfoList = new List<PetInfo>();
					for (int i = 0; i < __Extension_Method_ICollection__.COUNT(response.PetAddResultInfos); i++)
					{
						PetAddResultInfo petAddResultInfo = response.PetAddResultInfos[i];
						if (!__Extension_Method_ICollection__.IsNullOrEmpty(petAddResultInfo.PetSoulResults))
						{
							list.AddRange(petAddResultInfo.PetSoulResults);
						}
						if (petAddResultInfo.PetResult != null)
						{
							petResultInfoList.Add(petAddResultInfo.PetResult);
						}
					}
					PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
					if (petIncubatorManageWindow != null)
					{
						petIncubatorManageWindow.EnableTouch(isEnable: false);
					}
					BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("GET_INCUBATOR_REWARD_CENTER_TEXT")).SetItemResultInfos(list.ToArray());
					Action onEnd = delegate
					{
						EndPlayOpenIncubatorByManageWindow(response.PetIncubatorSlotInfo.SlotIndex);
					};
					PlayOpenIncubatorByManageWindow(delegate
					{
						PetIncubatorManageWindow petIncubatorManageWindow2 = GetInstance<PetIncubatorManageWindow>();
						if (petIncubatorManageWindow2 != null)
						{
							petIncubatorManageWindow2.EnableTouch(isEnable: true);
						}
						VespaCoroutine.Start(coPlayPetIncubatorResult(petResultInfoList, popupOpen, onEnd));
					});
				}
			}
		});
	}

	private void PlayOpenIncubatorByManageWindow(Action onEnd)
	{
		PlayIncubatorSound("Pet.System.Incubator");
		PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
		if (petIncubatorManageWindow != null)
		{
			petIncubatorManageWindow.OnStartPlayFinishIncubating(onEnd);
		}
	}

	private void EndPlayOpenIncubatorByManageWindow(int slotIndex)
	{
		PetHouseWindow petHouseWindow = GetInstance<PetHouseWindow>();
		if (petHouseWindow != null)
		{
			petHouseWindow.SetIncubatorState(slotIndex, PetIncubatorObjectInfo.IncubatorStateType.Phase0, null, needInfoUpdate: true);
		}
		PetIncubatorManageWindow petIncubatorManageWindow = GetInstance<PetIncubatorManageWindow>();
		if (petIncubatorManageWindow != null)
		{
			petIncubatorManageWindow.OnEndPlayFinishIncubating();
		}
	}
}
