using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class BoosterInfoList : BaseWindow, BoosterInfoListComponent.Listener, IBoosterListObserver
{
	public interface Listener
	{
		void OnClickBoosterInfo(BoosterInfoListComponent comp);

		void RefreshBoosterInfoList(int totalBoosterInfoCount, int newBoosterInfoCount, bool isBoosterInfoListOpen);

		void EnableList();
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/BoosterInfoList";

	public UIScrollGrid BoosterInfoListGrid;

	private List<BoosterInfoListComponent> _ListBoosterComponent = new List<BoosterInfoListComponent>();

	private DateTime _lastBoosterCheckTime = DateTime.UtcNow;

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		GlobalAutoBehaviour<BoosterManager>.instance.AddObserver(this);
		RefreshBoosterList();
	}

	public void OpenBoosterInfoList()
	{
		bool flag = UIHelper.IsEnabledGameObjet(BoosterInfoListGrid);
		SetBoosterInfoListGridVisible(!flag);
	}

	public override void Reset()
	{
		base.Reset();
		UIHelper.EnableGameObject(BoosterInfoListGrid, enable: false);
		FactoryBase.ReleaseAndClearList(_ListBoosterComponent);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			if (GlobalAutoBehaviour<BoosterManager>.isValidInstance)
			{
				GlobalAutoBehaviour<BoosterManager>.instance.RemoveObserver(this);
			}
			base.OnDestroy();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateBooster();
	}

	private void UpdateBooster()
	{
		if ((DateTime.UtcNow - _lastBoosterCheckTime).TotalSeconds < 1.0)
		{
			return;
		}
		_lastBoosterCheckTime = DateTime.UtcNow;
		foreach (BoosterInfoListComponent item in _ListBoosterComponent)
		{
			item.UpdateTooltipText();
		}
	}

	private void RefreshBoosterList()
	{
		FactoryBase.ReleaseAndClearList(_ListBoosterComponent);
		List<BoosterInfo> allEffectiveBooster = GlobalAutoBehaviour<BoosterManager>.instance.GetAllEffectiveBooster();
		if (__Extension_Method_ICollection__.COUNT(allEffectiveBooster) > 0)
		{
			foreach (BoosterInfo item in allEffectiveBooster)
			{
				if (item.BuffPosition == DropBonusEvnetBuffPosition.None)
				{
					if (!NGame2.NUI.Common.IsShowBoosterInfoList(item.BonusType))
					{
						continue;
					}
				}
				else if (item.BuffPosition != DropBonusEvnetBuffPosition.Top)
				{
					continue;
				}
				BoosterInfoListComponent boosterInfoListComponent = UIHelper.AddGridItem(BoosterInfoListGrid, BoosterInfoListComponent.PrefabAuto);
				boosterInfoListComponent.Open(this, item);
				_ListBoosterComponent.Add(boosterInfoListComponent);
			}
		}
		UIHelper.RepositionGrid(BoosterInfoListGrid);
		if (_listener != null)
		{
			int num = __Extension_Method_ICollection__.COUNT(_ListBoosterComponent);
			int newBoosterInfoCount = 0;
			if (num > 0 && !UIHelper.IsEnabledGameObjet(BoosterInfoListGrid))
			{
				newBoosterInfoCount = __Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<BoosterManager>.instance.GetNewBoosterItemIndexList());
			}
			bool isBoosterInfoListOpen = UIHelper.IsEnabledGameObjet(BoosterInfoListGrid);
			_listener.RefreshBoosterInfoList(num, newBoosterInfoCount, isBoosterInfoListOpen);
		}
	}

	void IBoosterListObserver.OnChanged(BoosterInfo expiredBooster)
	{
		RefreshBoosterList();
	}

	public void LostFocus()
	{
		SetBoosterInfoListGridVisible(visible: false);
	}

	public void OnClickBoosterInfo(BoosterInfoListComponent comp)
	{
		if (_listener != null)
		{
			SetBoosterInfoListGridVisible(visible: false);
			_listener.OnClickBoosterInfo(comp);
		}
	}

	private void SetBoosterInfoListGridVisible(bool visible)
	{
		if (visible)
		{
			UIHelper.RepositionGrid(BoosterInfoListGrid);
			UIHelper.EnableGameObject(BoosterInfoListGrid, enable: true);
			ShowSpriteNew();
		}
		else
		{
			foreach (BoosterInfoListComponent item in _ListBoosterComponent)
			{
				item.SetNewSprite(visible: false);
			}
			UIHelper.EnableGameObject(BoosterInfoListGrid, enable: false);
		}
		if (_listener != null)
		{
			_listener.EnableList();
		}
	}

	private void ShowSpriteNew()
	{
		foreach (int newInfo in GlobalAutoBehaviour<BoosterManager>.instance.GetNewBoosterItemIndexList())
		{
			BoosterInfoListComponent boosterInfoListComponent = _ListBoosterComponent.Find((BoosterInfoListComponent comp) => (comp.GetBoosterInfo().Type == BoosterInfoType.BoosterItem && newInfo == ((ItemBoosterInfo)comp.GetBoosterInfo()).ItemIndex) ? true : false);
			if (boosterInfoListComponent != null)
			{
				boosterInfoListComponent.SetNewSprite(visible: true);
			}
		}
		GlobalAutoBehaviour<BoosterManager>.instance.ClearNewBoosterItemIndexList();
	}
}
