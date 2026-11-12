using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LobbyRightGridWindow : BaseWindow
{
	public interface IRightGridSort
	{
		bool IsDestroy();

		void ChainUpdate();

		int GetSortIndex();
	}

	public enum EPositionType
	{
		None,
		Top,
		Bottom
	}

	[Serializable]
	public class PositionInfo
	{
		public EPositionType PositionType;

		public UITable Table;

		public bool IsSort;
	}

	private class BannerInfo
	{
		public Action<Action> FirstAction;

		public Func<Transform, IRightGridSort> CreateBanner;

		public float CreateDestroyCheckTime = 1f;

		public EPositionType PositionType = EPositionType.Top;

		public IRightGridSort comp;

		public NGame2.NUI.NWindow.IntervalUpdater BannerCreateUpdater;

		public NGame2.NUI.NWindow.IntervalUpdater BannerDestroyUpdater;

		public bool IsCreateable = true;

		public bool IsDestroy;
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/LobbyRightGridWindow";

	public PositionInfo[] PositionInfos;

	private List<BannerInfo> _compList;

	public void OpenInternal()
	{
		Open(Init, null);
	}

	private void Init()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PositionInfos))
		{
			VespaDebug.LogError("PositionInfo empty");
			return;
		}
		InitTableCustomSort();
		InitCompList();
	}

	private void InitTableCustomSort()
	{
		int num = __Extension_Method_ICollection__.COUNT(PositionInfos);
		for (int i = 0; i < num; i++)
		{
			PositionInfo positionInfo = PositionInfos[i];
			if (positionInfo.IsSort)
			{
				positionInfo.Table.sorting = UITable.Sorting.Custom;
				positionInfo.Table.onCustomSort = RightMenuTableSort;
			}
		}
	}

	private void InitCompList()
	{
		_compList = new List<BannerInfo>();
		_compList.Add(new BannerInfo
		{
			CreateBanner = StaminaConsumePointCreateBanner,
			CreateDestroyCheckTime = 1f,
			PositionType = EPositionType.Top
		});
		_compList.Add(new BannerInfo
		{
			FirstAction = RightAwayCreateBannerFirst,
			CreateBanner = RightAwayCreateBanner,
			CreateDestroyCheckTime = 0.25f,
			PositionType = EPositionType.Top
		});
		_compList.Add(new BannerInfo
		{
			CreateBanner = LegendCostumeCreateBanner,
			CreateDestroyCheckTime = 0.25f,
			PositionType = EPositionType.Top
		});
		foreach (BannerInfo comp in _compList)
		{
			UITable table = GetTable(comp.PositionType);
			if (table == null)
			{
				VespaDebug.LogError("UITable is null");
				_compList.Remove(comp);
				continue;
			}
			comp.BannerCreateUpdater = new NGame2.NUI.NWindow.IntervalUpdater(delegate
			{
				comp.comp = comp.CreateBanner(table.transform);
				comp.IsDestroy = false;
			}, comp.CreateDestroyCheckTime);
			comp.BannerDestroyUpdater = new NGame2.NUI.NWindow.IntervalUpdater(delegate
			{
				if (comp.comp.IsDestroy())
				{
					comp.IsDestroy = true;
				}
			}, comp.CreateDestroyCheckTime);
			if (comp.FirstAction != null)
			{
				comp.IsCreateable = false;
				comp.FirstAction(delegate
				{
					comp.IsCreateable = true;
				});
			}
		}
	}

	public static int RightMenuTableSort(Transform a, Transform b)
	{
		int num = 0;
		num = LeftMenu.NullCompare(a, b);
		if (num != 0)
		{
			return num;
		}
		IRightGridSort component = a.GetComponent<IRightGridSort>();
		IRightGridSort component2 = b.GetComponent<IRightGridSort>();
		num = LeftMenu.NullCompare(component, component2);
		if (num != 0)
		{
			return num;
		}
		int sortIndex = component.GetSortIndex();
		int sortIndex2 = component2.GetSortIndex();
		return sortIndex.CompareTo(sortIndex2);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		foreach (BannerInfo comp2 in _compList)
		{
			IRightGridSort comp = comp2.comp;
			if (comp == null)
			{
				NGame2.NUI.NWindow.IntervalUpdater bannerCreateUpdater = comp2.BannerCreateUpdater;
				if (bannerCreateUpdater == null)
				{
					Destroy(comp2, isRemoveList: true);
					GetTable(comp2.PositionType).Reposition();
					break;
				}
				if (comp2.IsCreateable)
				{
					bannerCreateUpdater.Update();
					if (comp2.comp != null)
					{
						GetTable(comp2.PositionType).Reposition();
					}
				}
			}
			else
			{
				comp2.BannerDestroyUpdater.Update();
				if (comp2.IsDestroy)
				{
					Destroy(comp2);
					GetTable(comp2.PositionType).Reposition();
				}
				else
				{
					comp.ChainUpdate();
				}
			}
		}
	}

	private void Destroy(BannerInfo bannerInfo, bool isRemoveList = false)
	{
		IRightGridSort comp = bannerInfo.comp;
		if (comp != null)
		{
			if (comp is Component)
			{
				FactoryBase.ReleaseComponent(comp as Component);
			}
			else
			{
				VespaDebug.LogError("comp is not component");
			}
		}
		bannerInfo.comp = null;
		if (isRemoveList)
		{
			bannerInfo.FirstAction = null;
			bannerInfo.CreateBanner = null;
			bannerInfo.BannerCreateUpdater = null;
			bannerInfo.BannerDestroyUpdater = null;
			_compList.Remove(bannerInfo);
		}
	}

	private UITable GetTable(EPositionType positionType)
	{
		int num = __Extension_Method_ICollection__.COUNT(PositionInfos);
		for (int i = 0; i < num; i++)
		{
			PositionInfo positionInfo = PositionInfos[i];
			if (positionInfo.PositionType == positionType)
			{
				return positionInfo.Table;
			}
		}
		return null;
	}

	public override void OnClosing()
	{
		base.OnClosing();
		foreach (BannerInfo comp in _compList)
		{
			Destroy(comp);
		}
		_compList.Clear();
	}

	public IRightGridSort StaminaConsumePointCreateBanner(Transform parent)
	{
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance)
		{
			return null;
		}
		List<CurrencyType> eventTypeList = StaminaConsumePointInfoComponent.GetEventTypeList(EventBonusType.GrowWorldTree);
		__Extension_Method_ICollection__.IsNullOrEmpty(eventTypeList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(eventTypeList))
		{
			return null;
		}
		CurrencyType currenyType = eventTypeList[0];
		StaminaConsumePointInfoComponent staminaConsumePointInfoComponent = StaminaConsumePointInfoComponent.PrefabAuto.Factory.Create(parent);
		staminaConsumePointInfoComponent.Open(currenyType);
		return staminaConsumePointInfoComponent;
	}

	public void RightAwayCreateBannerFirst(Action onComplete)
	{
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			((NGame2.NUI.NWindow.RightAwayProductMenu.Listener)SceneAutoBehaviour<LobbyManagement>.instance).InitRightAwayProductInfo(onComplete, onComplete);
		}
	}

	public IRightGridSort RightAwayCreateBanner(Transform parent)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager == null)
		{
			return null;
		}
		if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return null;
		}
		int num = GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.Count();
		if (num == 0)
		{
			return null;
		}
		RightAwayProductInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.RightAwayProductManager.GetValues();
		for (int i = 0; i < num; i++)
		{
			RightAwayProductInfo rightAwayProductInfo = values[i];
			if (rightAwayProductInfo != null && !String_Extension_Method.IsNullOrEmpty(rightAwayProductInfo.EndTime) && !String_Extension_Method.IsNullOrEmpty(rightAwayProductInfo.BannerPath))
			{
				DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
				if (!((Convert.ToDateTime(rightAwayProductInfo.EndTime) - currentServerUTCTime).TotalSeconds <= 0.0))
				{
					NGame2.NUI.NWindow.RightAwayProductMenu rightAwayProductMenu = NGame2.NUI.NWindow.RightAwayProductMenu.PrefabAuto.Factory.Create(parent);
					rightAwayProductMenu.Open(SceneAutoBehaviour<LobbyManagement>.instance);
					return rightAwayProductMenu;
				}
			}
		}
		return null;
	}

	public IRightGridSort LegendCostumeCreateBanner(Transform parent)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance)
		{
			return null;
		}
		EventBonusType eventBonusType = EventBonusType.PayShopDiscount;
		if (!GlobalAutoBehaviour<DropBonusEventManager>.instance.AnyEffectiveEventByEventBonusType(eventBonusType))
		{
			return null;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null && !GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(10202))
		{
			return null;
		}
		LegendCostumeBannerComponent legendCostumeBannerComponent = LegendCostumeBannerComponent.PrefabAuto.Factory.Create(parent);
		legendCostumeBannerComponent.Open(delegate
		{
			LegendCostumeReservationPopup.EShowOption showOption = LegendCostumeReservationPopup.EShowOption.ChangePage | LegendCostumeReservationPopup.EShowOption.MoveToHeroCostumeView;
			SceneAutoBehaviour<CostumeManagement>.instance.RequestLegendCostumeReservationPopup(0, showOption);
		});
		return legendCostumeBannerComponent;
	}
}
