using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPet;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPet;

public class PetSoulInven : RenewResourceWindow, MinorPetSoulUnionPanel.IListener, LegendPetSoulUnionPanel.IListener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<ItemInfo>
{
	public interface IListener
	{
		void OnClickUninon(int[] PetSouls, Action onEnd = null);
	}

	public enum PetSoulTab
	{
		None,
		Minor,
		Legend
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Pet/PetSoulInven";

	public UIBundleLoader BackgroundLoader;

	public UIToggle Toggle_Minor;

	public UISprite Sprite_MinorNew;

	public UIToggle Toggle_Legend;

	public UISprite Sprite_LegendNew;

	public MinorPetSoulUnionPanel MinorPetSoulUnionPanel;

	public LegendPetSoulUnionPanel LegendPetSoulUnionPanel;

	private IListener _listener;

	private PetSoulTab _petSoulTab;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (PayShopResourceList != null)
		{
			PayShopResourceList.OnChanged(changedInfo);
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(changedInfo.ItemIndex);
			if (data != null && data.Type == ItemType.PetSoul)
			{
				InitToggleNewMark();
			}
		}
	}

	private bool IsPlayUnionAnimation()
	{
		if (MinorPetSoulUnionPanel != null && MinorPetSoulUnionPanel.IsPlayUnionAnim)
		{
			return true;
		}
		if (LegendPetSoulUnionPanel != null && LegendPetSoulUnionPanel.IsPlayUnionAnim)
		{
			return true;
		}
		return false;
	}

	public void Open(IListener listener)
	{
		Open(delegate
		{
			_listener = listener;
			AddUserObserver();
			Init();
		}, delegate
		{
			OpenRaiderSoulStorageUI triggerEvent = new OpenRaiderSoulStorageUI();
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		});
	}

	private new void Init()
	{
		base.Init();
		InitToggleButton();
		InitToggleNewMark();
		InitTab();
	}

	private void InitToggleButton()
	{
		_petSoulTab = PetSoulTab.Minor;
		Toggle_Minor.Set(_petSoulTab == PetSoulTab.Minor);
		Toggle_Legend.Set(_petSoulTab == PetSoulTab.Legend);
	}

	private void InitToggleNewMark()
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.PetSoul);
		byte b = 6;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemsByType); i++)
		{
			ItemInfo itemInfo = itemsByType[i];
			if (itemInfo == null)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
			if (data == null)
			{
				continue;
			}
			if (flag && flag2)
			{
				break;
			}
			if (data.Grade >= b)
			{
				if (!flag2)
				{
					flag2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsNewPetSoul(data.Index);
				}
			}
			else if (!flag)
			{
				flag = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsNewPetSoul(data.Index);
			}
		}
		SetToggleBadge(flag, flag2);
	}

	private void InitTab()
	{
		if (MinorPetSoulUnionPanel != null)
		{
			MinorPetSoulUnionPanel.Open(this, _listener);
		}
	}

	private void SetTab(PetSoulTab tab)
	{
		if (tab == PetSoulTab.Minor)
		{
			if (MinorPetSoulUnionPanel != null)
			{
				MinorPetSoulUnionPanel.Open(this, _listener);
			}
			if (LegendPetSoulUnionPanel != null)
			{
				LegendPetSoulUnionPanel.Close();
			}
		}
		else
		{
			if (MinorPetSoulUnionPanel != null)
			{
				MinorPetSoulUnionPanel.Close();
			}
			if (LegendPetSoulUnionPanel != null)
			{
				LegendPetSoulUnionPanel.Open(this, _listener);
			}
		}
	}

	private void SetToggleBadge(bool newMinor, bool newLegend)
	{
		UIHelper.EnableGameObject(Sprite_MinorNew, newMinor);
		UIHelper.EnableGameObject(Sprite_LegendNew, newLegend);
	}

	private void AddUserObserver()
	{
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	private void RemoveObserver()
	{
		GlobalAutoBehaviour<AccountManager>.instance.PetManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	public override void Close()
	{
		if (!IsPlayUnionAnimation())
		{
			base.Close();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (_petSoulTab == PetSoulTab.Minor)
		{
			if (MinorPetSoulUnionPanel != null)
			{
				MinorPetSoulUnionPanel.Close();
			}
		}
		else if (LegendPetSoulUnionPanel != null)
		{
			LegendPetSoulUnionPanel.Close();
		}
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		RemoveObserver();
	}

	public void OnToggleChanged()
	{
		PetSoulTab petSoulTab = PetSoulTab.None;
		if ((bool)Toggle_Minor && Toggle_Minor.value)
		{
			petSoulTab = PetSoulTab.Minor;
		}
		if ((bool)Toggle_Legend && Toggle_Legend.value)
		{
			petSoulTab = PetSoulTab.Legend;
		}
		if (petSoulTab != PetSoulTab.None && petSoulTab != _petSoulTab)
		{
			if (MinorPetSoulUnionPanel != null && MinorPetSoulUnionPanel.IsInsertPetSoul)
			{
				GetToggle(_petSoulTab).value = true;
				return;
			}
			if (IsPlayUnionAnimation())
			{
				GetToggle(_petSoulTab).value = true;
				return;
			}
			_petSoulTab = petSoulTab;
			SetTab(_petSoulTab);
			SetToggleBadge(newMinor: false, newLegend: false);
		}
	}

	private UIToggle GetToggle(PetSoulTab tab)
	{
		return tab switch
		{
			PetSoulTab.Minor => Toggle_Minor, 
			PetSoulTab.Legend => Toggle_Legend, 
			_ => null, 
		};
	}

	void MinorPetSoulUnionPanel.IListener.OnClickItemIcon(ItemIconComponent comp)
	{
		InitToggleNewMark();
	}

	void LegendPetSoulUnionPanel.IListener.OnClickItemIcon(ItemIconComponent comp)
	{
		InitToggleNewMark();
	}
}
