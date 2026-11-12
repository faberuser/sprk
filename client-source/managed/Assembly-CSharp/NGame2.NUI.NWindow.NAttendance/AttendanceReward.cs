using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI3.NAttendance;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NAttendance;

public class AttendanceReward : AttendanceWindow, AttendanceRewardComponent.Listener, IUserObserver<AttendanceInfo>, IUserObserver
{
	public static class Helper
	{
		public static bool CanPurchaseAttendance(AttendanceInfo attendanceInfo, AttendanceData attendanceData)
		{
			bool num = AttendanceHelper.IsAttendanceProgress(attendanceInfo, attendanceData);
			bool result = AttendanceHelper.IsPayShopProductSelling(attendanceData.Index);
			if (!num)
			{
				return result;
			}
			return false;
		}
	}

	public interface Listener
	{
		void OnReceive(AttendanceReward attendanceReward);
	}

	[Serializable]
	public class SpecialRewardItemGridInfo
	{
		public int SlotCount;

		public UISimpleGrid RewardItemGrid;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Attendance/AttendanceReward";

	[Header("[[ --- BaseDbRewardPopup ]]")]
	public UIScrollGrid RewardItemGrid;

	public bool AnimateGet;

	public float RewardPopInterval;

	public List<SpecialRewardItemGridInfo> ListRewardItemGridInfos;

	[Header("[[ --- AttendanceReward ]]")]
	public Transform VisibleRoot;

	public UILabel TitleLabel;

	public bool ManualReceive;

	public UITexture BackgroundTexture;

	public float RewardPopupDelay = 1f;

	public int RewardSlotCount = 31;

	public float MaxWaitLoadingTime = 5f;

	public List<Transform> IntegrationWindowHideButtons = new List<Transform>();

	public List<Transform> IntegrationWindowDisableColliders = new List<Transform>();

	protected List<DbRewardComponent> _rewardList = new List<DbRewardComponent>();

	protected ItemAnimationContainer _getItemAnimationContainer = new ItemAnimationContainer(open3dModelPopup: true);

	private AttendanceKeywordGroup _attendanceGroup;

	private string _title;

	protected AttendanceRewardComponent _todayComponent;

	private bool _effectStarted;

	private float _effectStartedTime;

	private Action _onEffectEnd;

	protected bool _received;

	private bool _backgroundImageRequested;

	private float _openedTime;

	private bool _onReceived;

	private bool _is_in_AttendanceIntegration;

	public AttendanceData AttendanceData { get; set; }

	public AttendanceInfo AttendanceInfo { get; set; }

	public int RewardableDay { get; set; }

	public string Title
	{
		get
		{
			return _title;
		}
		protected set
		{
			_title = value;
		}
	}

	public sealed override void Setup_for_AttendanceIntegration()
	{
		_is_in_AttendanceIntegration = true;
		foreach (Transform integrationWindowHideButton in IntegrationWindowHideButtons)
		{
			UIHelper.EnableGameObject(integrationWindowHideButton, enable: false);
		}
		foreach (Transform integrationWindowDisableCollider in IntegrationWindowDisableColliders)
		{
			UIHelper.EnableCollider(integrationWindowDisableCollider, isEnable: false);
		}
	}

	public sealed override void OpenAttendance(AttendanceKeywordGroup attendanceGroup, bool openedFromReservePopup, Action onComplete = null)
	{
		_attendanceGroup = attendanceGroup;
		AttendanceData attendanceData = attendanceGroup.AttendanceData;
		AttendanceInfo attendanceInfo = attendanceGroup.AttendanceInfo;
		int rewardableDays = attendanceGroup.RewardableDays;
		OpenAttendanceReward(attendanceData, attendanceInfo, rewardableDays, onComplete);
	}

	public override void RefreshAttendance(AttendanceKeywordGroup attendanceGroup)
	{
		OpenAttendance(_attendanceGroup, openedFromReservePopup: false, delegate
		{
			Setup_for_AttendanceIntegration();
		});
	}

	private void OpenAttendanceReward(AttendanceData data, AttendanceInfo attendanceInfo, int rewardableDay, Action onComplete = null)
	{
		_openedTime = Time.time;
		GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.ObserverManager.AddObserver(this);
		Open(delegate
		{
			AttendanceData = data;
			AttendanceInfo = attendanceInfo;
			RewardableDay = rewardableDay;
			_title = GetTitle(AttendanceData);
			UIHelper.EnableGameObject(VisibleRoot, enable: false);
			WaitingPopup.Open(this);
			List<NShared.AttendanceReward> rewardDataList = GetRewardDataList();
			HashSet<NShared.AttendanceReward> hashSet = new HashSet<NShared.AttendanceReward>();
			foreach (NShared.AttendanceReward item in rewardDataList)
			{
				if (item != null && item.Invisible)
				{
					hashSet.Add(item);
				}
			}
			rewardDataList.RemoveAll(hashSet.Contains);
			List<DbRewardData> list = new List<DbRewardData>(rewardDataList.ToArray());
			Init(list, GetRewardComponentPrefabAuto(), GetSpecialRewardComponentPrefabAuto(), list.Count, OnCreateComponent);
			UIHelper.SetLabelText(TitleLabel, _title, unwrapNewline: true);
			InitBackground();
			SetupCustom();
		}, delegate
		{
			UIHelper.EnableGameObject(VisibleRoot, enable: true);
			WaitingPopup.Close(this);
			if (!ManualReceive)
			{
				ReceiveReward();
			}
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}, () => !AllAssetLoaded());
	}

	protected void Init<T>(IList<DbRewardData> dbRewardDatas, PrefabAuto<T> prefab, PrefabAuto<T> specialPrefab, int slotCount = 0, Action<int, T> onEachComponent = null) where T : DbRewardComponent
	{
		if (slotCount == 0)
		{
			slotCount = __Extension_Method_ICollection__.COUNT(dbRewardDatas);
		}
		IGrid grid = null;
		PrefabAuto<T> prefabAuto = null;
		int index;
		for (index = 0; index < slotCount; index++)
		{
			SpecialRewardItemGridInfo specialRewardItemGridInfo = ListRewardItemGridInfos.Find((SpecialRewardItemGridInfo info) => info.SlotCount == index);
			if (specialRewardItemGridInfo != null)
			{
				grid = specialRewardItemGridInfo.RewardItemGrid;
				prefabAuto = specialPrefab;
			}
			else
			{
				grid = RewardItemGrid;
				prefabAuto = prefab;
			}
			if (prefabAuto == null)
			{
				prefabAuto = prefab;
			}
			T val = UIHelper.AddGridItem(grid, prefabAuto);
			if (val == null)
			{
				VespaDebug.LogError("Failed to create component of type=" + typeof(T).Name);
				continue;
			}
			_rewardList.Add(val);
			if (index < __Extension_Method_ICollection__.COUNT(dbRewardDatas))
			{
				DbRewardData dbRewardData = dbRewardDatas[index];
				if (dbRewardData != null)
				{
					val.Init(dbRewardData);
					IIconComponent iconComponent = val as IIconComponent;
					if (AnimateGet && iconComponent != null)
					{
						_getItemAnimationContainer.Add(iconComponent.CreateGetAnimation());
					}
				}
			}
			Vespa_System_Action_Extension_Method.fire(onEachComponent, index, val);
		}
		UIHelper.RepositionGrid(RewardItemGrid);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			_todayComponent = null;
			_onEffectEnd = null;
		}
	}

	public override void Reset()
	{
		base.Reset();
		_getItemAnimationContainer.Reset();
		FactoryBase.ReleaseAndClearList(_rewardList);
		_todayComponent = null;
		UIHelper.SetLabelText(TitleLabel, string.Empty);
		UIHelper.ClearPlaneAssetTexture(BackgroundTexture);
		_effectStarted = false;
		_effectStartedTime = 0f;
		_onEffectEnd = null;
		_received = false;
		_backgroundImageRequested = false;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_getItemAnimationContainer.ChainUpdate();
		UpdateEffectEndCallback();
	}

	protected virtual PrefabAuto<AttendanceRewardComponent> GetRewardComponentPrefabAuto()
	{
		return AttendanceRewardComponent.PrefabAuto;
	}

	protected virtual PrefabAuto<AttendanceRewardComponent> GetSpecialRewardComponentPrefabAuto()
	{
		return null;
	}

	protected virtual void SetupCustom()
	{
	}

	public int GetCompletedDay()
	{
		if (AttendanceInfo == null)
		{
			return 0;
		}
		return AttendanceInfo.CompletedDay;
	}

	public virtual int GetCurrentPage()
	{
		if (AttendanceData.PageSize == 0)
		{
			return 0;
		}
		return Mathf.Max(((RewardableDay > 0) ? RewardableDay : GetCompletedDay()) - 1, 0) / AttendanceData.PageSize;
	}

	public int GetPageSize()
	{
		if (AttendanceData.PageSize <= 0)
		{
			return __Extension_Method_ICollection__.COUNT(AttendanceData.Reward);
		}
		return AttendanceData.PageSize;
	}

	public bool CanReceiveReward()
	{
		if (RewardableDay == 0)
		{
			return false;
		}
		return GetCompletedDay() != RewardableDay;
	}

	public void OnReceivedReward(bool start)
	{
		_onReceived = start;
	}

	public void ReceiveReward()
	{
		if (!_received && CanReceiveReward())
		{
			SingletonAuto<AttendanceManager>.instance.OnReceive_AttendanceReward(this);
			_received = true;
		}
	}

	private void UpdateEffectEndCallback()
	{
		if (_effectStarted && !(Time.time - _effectStartedTime < RewardPopupDelay))
		{
			Vespa_System_Action_Extension_Method.fire(_onEffectEnd);
			_effectStarted = false;
			_effectStartedTime = 0f;
		}
	}

	protected virtual List<NShared.AttendanceReward> GetRewardDataList()
	{
		List<NShared.AttendanceReward> list = new List<NShared.AttendanceReward>();
		if (AttendanceData == null)
		{
			return list;
		}
		int currentPage = GetCurrentPage();
		int pageSize = GetPageSize();
		int num = pageSize * currentPage;
		int num2 = num + pageSize;
		for (int i = num; i < num2; i++)
		{
			AttendanceRewards attendanceRewards = ((i < __Extension_Method_ICollection__.COUNT(AttendanceData.Reward)) ? AttendanceData.Reward[i] : null);
			if (attendanceRewards != null && __Extension_Method_ICollection__.COUNT(attendanceRewards.Reward) > 0)
			{
				list.Add(attendanceRewards.Reward[0]);
			}
			else
			{
				list.Add(null);
			}
		}
		return list;
	}

	private static AttendanceInfoData GetAttendanceInfoData(int index)
	{
		return GameRoot.Instance.AttendanceInfoDataContainer.GetData(index);
	}

	protected static AttendanceRewardInfoData GetAttendanceRewardInfoData(int index, int day, int no)
	{
		return GameRoot.Instance.AttendanceRewardInfoDataContainer.GetData(index, day, no);
	}

	protected static string GetTitle(AttendanceData data)
	{
		if (data == null)
		{
			return string.Empty;
		}
		AttendanceInfoData attendanceInfoData = GetAttendanceInfoData(data.Index);
		if (attendanceInfoData != null && !String_Extension_Method.IsNullOrEmpty(attendanceInfoData.TitleKey))
		{
			return LocalizationManager.GetStringDirect(attendanceInfoData.TitleKey);
		}
		return string.Empty;
	}

	private void InitBackground()
	{
		if (AttendanceData == null)
		{
			return;
		}
		AttendanceInfoData attendanceInfoData = GetAttendanceInfoData(AttendanceData.Index);
		if (attendanceInfoData != null)
		{
			string backGroundUrl = attendanceInfoData.GetBackGroundUrl(Vespa_String_Extension_Method.GetEnumString(SingletonAuto<LocalizationManager>.instance.Language));
			if (!string.IsNullOrEmpty(backGroundUrl))
			{
				_backgroundImageRequested = true;
				AssetTexture assetTexture = new AssetTexture();
				assetTexture.OpenUrl(backGroundUrl);
				UIHelper.ResetPlaneAssetTextureAsync(this, BackgroundTexture, assetTexture, UIHelper.EAssetTextureAssign.None, delegate
				{
				});
			}
		}
	}

	protected virtual void OnCreateComponent(int index, AttendanceRewardComponent component)
	{
		int pageSize = GetPageSize();
		int num = GetCurrentPage() * pageSize + 1 + index;
		int day = index + 1;
		if (num > __Extension_Method_ICollection__.COUNT(AttendanceData.Reward) || component.DbRewardData == null)
		{
			component.Show(show: false);
			return;
		}
		int completedDay = GetCompletedDay();
		bool flag = ((RewardableDay > 0) ? (num == RewardableDay) : (num == completedDay));
		bool flag2 = RewardableDay > 0 && RewardableDay == completedDay;
		bool flag3 = (flag && flag2) || ((RewardableDay > 0) ? (num < RewardableDay) : (num <= completedDay));
		component.SetListener(this);
		component.Show(show: true);
		component.SetDay(day);
		component.SetAlreadyRewarded(flag3);
		component.SetTodayReward(flag);
		component.SetRecieveButton(flag && !flag3);
		AttendanceRewardInfoData attendanceRewardInfoData = GetAttendanceRewardInfoData(AttendanceData.Index, num, 1);
		if (attendanceRewardInfoData != null)
		{
			component.SetRewardInfo(attendanceRewardInfoData);
		}
		if (flag)
		{
			_todayComponent = component;
		}
	}

	protected int GetComponentDay(AttendanceRewardComponent comp)
	{
		if (comp == null)
		{
			return -1;
		}
		int num = _rewardList.IndexOf(comp);
		if (num != -1)
		{
			int num2 = GetCurrentPage() * GetPageSize() + 1;
			num += num2;
		}
		return num;
	}

	void AttendanceRewardComponent.Listener.OnClick(AttendanceRewardComponent component)
	{
		if (component == _todayComponent && ManualReceive)
		{
			ReceiveReward();
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickClose()
	{
		if (!_effectStarted && !_onReceived)
		{
			GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.ObserverManager.RemoveObserver(this);
			Close();
		}
	}

	public void StartReceiveEffect(Action onEnd)
	{
		if ((bool)_todayComponent)
		{
			_todayComponent.StartReceiveEffect();
		}
		_effectStarted = true;
		_effectStartedTime = Time.time;
		_onEffectEnd = delegate
		{
			if (_todayComponent != null)
			{
				_todayComponent.SetAlreadyRewarded(rewarded: true);
				_todayComponent.SetRecieveButton(isOn: false);
			}
			Vespa_System_Action_Extension_Method.fire(onEnd);
		};
	}

	protected bool AllAssetLoaded()
	{
		if (Time.time - _openedTime > MaxWaitLoadingTime)
		{
			return true;
		}
		if (_backgroundImageRequested && (bool)BackgroundTexture && BackgroundTexture.mainTexture == null)
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_rewardList); i++)
		{
			AttendanceRewardComponent attendanceRewardComponent = _rewardList[i] as AttendanceRewardComponent;
			if (!(attendanceRewardComponent == null) && !attendanceRewardComponent.CustomImageLoaded())
			{
				return false;
			}
		}
		return true;
	}

	public void OnClickPurchase()
	{
		if (Helper.CanPurchaseAttendance(AttendanceInfo, AttendanceData))
		{
			NewPayShopProductInfo attendanceProduct = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetAttendanceProduct(AttendanceData.Index);
			if (attendanceProduct != null)
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(attendanceProduct.Category.ToString());
			}
			if (!_is_in_AttendanceIntegration)
			{
				Close();
			}
		}
	}

	public void OpenNextPageNoticePopup()
	{
		if (AttendanceData.PageSize == 0 || _todayComponent == null)
		{
			return;
		}
		int componentDay = GetComponentDay(_todayComponent);
		if (componentDay == -1)
		{
			return;
		}
		int num = __Extension_Method_ICollection__.COUNT(AttendanceData.Reward);
		if (componentDay % AttendanceData.PageSize == 0 && num != componentDay)
		{
			AttendanceInfoData attendanceInfoData = GetAttendanceInfoData(AttendanceData.Index);
			if (attendanceInfoData != null)
			{
				string stringDirect = LocalizationManager.GetStringDirect(attendanceInfoData.NextStepPopupTitle);
				string stringDirect2 = LocalizationManager.GetStringDirect(attendanceInfoData.NextStepPopupDesc);
				MessagePopup.Open(stringDirect, stringDirect2);
			}
		}
	}

	public override void ShowPurchaseButton(bool isShow)
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<AttendanceInfo>.OnChanged(AttendanceInfo changedInfo)
	{
		if (AttendanceData.Index == changedInfo.AttendanceIndex)
		{
			AttendanceInfo = changedInfo;
		}
	}
}
