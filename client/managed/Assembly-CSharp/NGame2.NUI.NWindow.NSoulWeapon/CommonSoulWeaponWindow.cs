using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class CommonSoulWeaponWindow : BaseWindow
{
	private const string SoulWeaponKey = "SoulWeapon";

	public SoulWeaponList SoulWeaponList;

	public AllSoulWeaponList AllSoulWeaponList;

	public Transform SelectItemIconComponentRootTransform;

	protected ItemIconComponent _selectEquipItemIconComponent;

	private EquipItemInfo _equipItemInfo;

	private string _beforeBgmCode;

	public EquipItemInfo EquipItemInfo
	{
		get
		{
			return _equipItemInfo;
		}
		protected set
		{
			_equipItemInfo = value;
			if (_equipItemInfo != null)
			{
				SoulWeaponInfo = _equipItemInfo.SoulWeaponInfo;
			}
			else
			{
				SoulWeaponInfo = null;
			}
		}
	}

	protected SoulWeaponInfo SoulWeaponInfo { get; set; }

	protected bool ExistList => SoulWeaponList.ExistList;

	protected void Open(EquipItemInfo equipItemInfo, Action onComplete = null)
	{
		EquipItemInfo = equipItemInfo;
		ReleaseSelectSoulWeaponIcon();
		Open(null, onComplete, null);
		PlayBGM();
	}

	protected void InitSoulWeaponList(SoulWeaponList.IListener iListener, EquipItemInfo initEquipItemInfo, SoulWeaponList.ListType listType, bool isSelectFirst = false)
	{
		SetSoulWeaponList(iListener, initEquipItemInfo, listType, isSelectFirst);
		AllSoulWeaponList.Open(iListener, GetAllSoulWeaponListTypeByListType(listType));
		EnableAllSoulWeaponList(enable: false);
	}

	private void SetSoulWeaponList(SoulWeaponList.IListener iListener, EquipItemInfo initEquipItemInfo, SoulWeaponList.ListType listType, bool isSelectFirst)
	{
		SoulWeaponList.Open(iListener, initEquipItemInfo, listType, isSelectFirst);
		if (isSelectFirst && SoulWeaponList.SelectedSoulWeaponItemIconComponent != null)
		{
			EquipItemInfo = SoulWeaponList.SelectedSoulWeaponItemIconComponent.EquipItemInfo;
		}
	}

	protected void RefreshList(EquipItemInfo equipItemInfo = null)
	{
		SoulWeaponList.Refresh(equipItemInfo);
		AllSoulWeaponList.Refresh(null);
	}

	protected void RefreshSoulWeaponListItemIconComponent(EquipItemInfo equipItemInfo)
	{
		SoulWeaponList.RefreshItemIconComponent(equipItemInfo);
	}

	public override void Close()
	{
		base.Close();
		ReleaseSelectSoulWeaponIcon();
		SoulWeaponList.Close();
		AllSoulWeaponList.Close();
		PlayPrevBGM();
	}

	private SoulWeaponList.ListType GetAllSoulWeaponListTypeByListType(SoulWeaponList.ListType listType)
	{
		if (listType == SoulWeaponList.ListType.Liberation)
		{
			return SoulWeaponList.ListType.LiberationAll;
		}
		return SoulWeaponList.ListType.SoulWeaponAll;
	}

	protected void ReleaseSelectSoulWeaponIcon()
	{
		FactoryBase.SafeReleaseComponent(ref _selectEquipItemIconComponent);
	}

	protected void SetSelectSoulWeapon(ItemIconComponent.Listener listener)
	{
		if (EquipItemInfo == null)
		{
			return;
		}
		if (_selectEquipItemIconComponent == null)
		{
			_selectEquipItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(SelectItemIconComponentRootTransform);
			if (_selectEquipItemIconComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_selectEquipItemIconComponent.transform);
		}
		_selectEquipItemIconComponent.Open(EquipItemInfo, listener);
	}

	protected void EnableAllSoulWeaponList(bool enable)
	{
		AllSoulWeaponList.EnableAllSoulWeaponList(enable);
	}

	protected void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	protected void PlayBGM()
	{
		string currentEventAssetBgmCode = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		if (!(currentEventAssetBgmCode == "EntryMenu.KainAscensionBGM"))
		{
			SingletonAuto<BgmManager2>.instance.PendingRequest = false;
			_beforeBgmCode = currentEventAssetBgmCode;
			if (_beforeBgmCode != "EntryMenu.KainAscensionBGM")
			{
				SingletonAuto<BgmManager2>.instance.Request("EntryMenu.KainAscensionBGM", isEvent: true);
			}
		}
	}

	protected void PlayPrevBGM()
	{
		List<BaseWindow> openedWindowList = GlobalAutoBehaviour<GlobalManagement>.instance.OpenedWindowList;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(openedWindowList))
		{
			int num = openedWindowList.FindIndex((BaseWindow window) => window.Equals(this));
			if (num >= 1 && !openedWindowList[num - 1].name.Contains("SoulWeapon") && _beforeBgmCode != "EntryMenu.KainAscensionBGM")
			{
				SingletonAuto<BgmManager2>.instance.StopEvent("EntryMenu.KainAscensionBGM");
				SingletonAuto<BgmManager2>.instance.Request(_beforeBgmCode, isEvent: true);
			}
		}
	}

	public void OnClickShowAllSoulWeaponList()
	{
		AllSoulWeaponList.EnableAllSoulWeaponList(enable: true);
	}

	public void OnClickCloseAllSoulWeaponList()
	{
		AllSoulWeaponList.EnableAllSoulWeaponList(enable: false);
	}
}
