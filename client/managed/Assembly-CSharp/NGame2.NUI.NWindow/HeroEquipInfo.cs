using System;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NInventory;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroEquipInfo : InspectEquipInfo, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver
{
	public interface Listener
	{
		bool OnCloseRequest(HeroEquipInfo closeThis);

		void OnClose(EquipItemInfo equipItemInfo);

		void OnAwaken(HeroEquipInfo closeThis);

		void OnEquip(HeroEquipInfo closeThis);

		void OnUnequip(HeroEquipInfo closeThis);

		void OnSell(HeroEquipInfo closeThis);

		void OnLock(HeroEquipInfo closeThis, byte Locked);
	}

	public interface IHeroIterator
	{
		bool IsActive();

		void Visit(Action<HeroPortraitComponent> onVisit);
	}

	[Space(10f)]
	[Header("HeroEquipInfo")]
	public UIButton UpgradeButton;

	public UIButton AwakenButton;

	public UIButton EquipButton;

	public UIButton UnequipButton;

	public UIButton Button_Lock;

	public UIButton Button_UnLock;

	public GameObject UpgradeButtonTween;

	protected InvenEquipInfoUIContainer.WindowPosType _windowPosType = InvenEquipInfoUIContainer.WindowPosType.Right;

	private Listener _listener;

	private IHeroIterator _heroIterator;

	public InvenEquipInfoUIContainer.WindowPosType WindowPosType => _windowPosType;

	private bool _isDispatched => DispatchHelper.IsDispatchedHero(EquipItemInfo.EquippedHeroInfo);

	public void Open(CachedHeroInfo cachedHeroInfo, EquipItemInfo equipItemInfo, Listener listener, IHeroIterator heroIterator)
	{
		Open(delegate
		{
			Init(cachedHeroInfo, equipItemInfo, listener, heroIterator);
		}, null);
	}

	public void Init(CachedHeroInfo cachedHeroInfo, EquipItemInfo equipItemInfo, Listener listener, IHeroIterator heroIterator)
	{
		_listener = listener;
		_heroIterator = heroIterator;
		Init(GlobalAutoBehaviour<AccountManager>.instance, cachedHeroInfo, equipItemInfo);
	}

	public override void Refresh()
	{
		if (IsOpen())
		{
			base.Refresh();
			SetupEquipState();
			ShowHeroesEquipState(show: true);
			SetupEquipLockButtons();
		}
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ShowHeroesEquipState(show: false);
		_heroIterator = null;
	}

	public virtual bool IsInventoryMode()
	{
		return false;
	}

	protected override void OnShowRoot(bool isShow)
	{
		base.OnShowRoot(isShow);
		ShowHeroesEquipState(isShow);
	}

	private void ShowHeroesEquipState(bool show)
	{
		if (_heroIterator == null)
		{
			return;
		}
		_heroIterator.Visit(delegate(HeroPortraitComponent component)
		{
			bool show2 = false;
			if (show && component.HeroInfo != null && EquipItemData != null)
			{
				show2 = component.HeroInfo.CheckEquipResult(EquipItemData) == CachedHeroInfo.EquipResult.Success;
			}
			component.ShowEquipable(show2);
		});
	}

	protected virtual void SetupEquipState()
	{
		if (EquipItemData != null)
		{
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(EquipItemInfo.SlotIndex) != null;
			bool enable = true;
			if (CachedHeroInfo != null)
			{
				enable = !CachedHeroInfo.IsEquipSlotLocked((int)EquipItemData.PartType) && CachedHeroInfo.CheckEquipResult(EquipItemData) == CachedHeroInfo.EquipResult.Success && EquipItemInfo.IsIdentified;
			}
			UIHelper.EnableGameObject(EquipButton, !flag);
			UIHelper.EnableGameObject(UnequipButton, flag);
			UIHelper.EnableButton(flag ? UnequipButton : EquipButton, enable);
		}
	}

	private void SetupEquipLockButtons()
	{
		UIHelper.EnableGameObject(Button_Lock, EquipItemInfo.Locked == 0);
		UIHelper.EnableGameObject(Button_UnLock, EquipItemInfo.Locked != 0);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
		if (equipStatInfo == null)
		{
			return;
		}
		if (equipStatInfo.EquipItemInfo == null && equipStatInfo.SlotIndex == EquipItemInfo.SlotIndex)
		{
			Close();
		}
		else
		{
			if (equipStatInfo.SlotIndex != EquipItemInfo.SlotIndex)
			{
				return;
			}
			if (equipStatInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Locked)
			{
				SetupEquipLockButtons();
				SetupEquipState();
				if (_itemIconComponent != null)
				{
					_itemIconComponent.InitItemLocked();
				}
			}
			else
			{
				Refresh();
			}
		}
	}

	public override void OnClickClose()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public override void OnClosing()
	{
		if (_listener != null)
		{
			_listener.OnClose(EquipItemInfo);
		}
		base.OnClickClose();
	}

	public void OnClickUpgrade()
	{
	}

	public void OnClickAwaken()
	{
		if (_listener != null)
		{
			_listener.OnAwaken(this);
		}
	}

	public void OnClickEquip()
	{
		if (_listener != null)
		{
			_listener.OnEquip(this);
		}
	}

	public void OnClickUnequip()
	{
		if (_listener != null)
		{
			_listener.OnUnequip(this);
		}
	}

	public virtual void OnClickSellButton()
	{
		if (_listener != null)
		{
			_listener.OnSell(this);
		}
	}

	public void OnClickLockButton()
	{
		if (_listener != null)
		{
			_listener.OnLock(this, 1);
		}
		OnChangeLockState();
	}

	public void OnClickUnLockButton()
	{
		if (_listener != null)
		{
			_listener.OnLock(this, 0);
		}
		OnChangeLockState();
	}

	protected virtual void OnChangeLockState()
	{
	}
}
