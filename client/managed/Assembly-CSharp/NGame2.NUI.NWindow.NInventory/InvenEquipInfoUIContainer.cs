using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NInventory;

public class InvenEquipInfoUIContainer : BaseWindow, InvenEquipInfo.IInvenModeListener
{
	public enum WindowPosType
	{
		Center,
		Right,
		Max
	}

	public const string PREFAB_PATH_THIS = "Resources/Prefab2/Window/Inventory/invenEquipInfouiContainer";

	public Transform[] PivotList = new Transform[2];

	public UISprite SpriteDimmedBG;

	private InvenEquipInfo[] _instantiatedInvenEquipInfoComps = new InvenEquipInfo[2];

	private PrefabAuto<InvenEquipInfo> _invenEquipPrefabAuto = new PrefabAuto<InvenEquipInfo>("Resources/Prefab2/Window/Inventory/InvenEquipInfo");

	private HeroSpecialEquipInfo[] _instantiatedHeroSpecialEquipInfo = new HeroSpecialEquipInfo[2];

	private PrefabAuto<HeroSpecialEquipInfo> _heroSpecialEquipInfoPreafabAuto = new PrefabAuto<HeroSpecialEquipInfo>("Resources/Prefab2/Window/Hero/HeroSpecialEquipInfo");

	public override void OnSystemBackButton()
	{
		if (!CheckCloseOpenedHeroEquipItemInfo())
		{
			Close();
		}
	}

	public void OnClickClose()
	{
		if (!CheckCloseOpenedHeroEquipItemInfo())
		{
			EnableBackground(enable: false);
			Close();
		}
	}

	public InvenEquipInfo Open(WindowPosType windowPosition, CachedHeroInfo cachedHeroInfo, EquipItemInfo equipItemInfo, HeroEquipInfo.Listener listener, HeroEquipInfo.IHeroIterator heroIterator)
	{
		if (equipItemInfo == null)
		{
			return null;
		}
		base.Open();
		InvenEquipInfo invenEquipInfo = GetInvenEquipInfo(windowPosition);
		if (invenEquipInfo == null)
		{
			invenEquipInfo = InstantiateEquipInfoPrefab(windowPosition);
		}
		if (invenEquipInfo != null)
		{
			invenEquipInfo.Open(cachedHeroInfo, equipItemInfo, listener, heroIterator, windowPosition);
		}
		ResetHeroSpecialEquipItemInfo(windowPosition, equipItemInfo);
		CheckCloseOpenedHeroEquipItemInfo();
		return invenEquipInfo;
	}

	private void ResetHeroSpecialEquipItemInfo(WindowPosType windowPosType, EquipItemInfo equipItemInfo)
	{
		HeroSpecialEquipInfo heroSpecialEquipInfo = GetHeroSpecialEquipInfo(windowPosType);
		if (!(heroSpecialEquipInfo == null) && UIHelper.IsEnabledGameObjet(heroSpecialEquipInfo))
		{
			if (CheckHeroSpecialEquipInfo(equipItemInfo))
			{
				heroSpecialEquipInfo.OpenInventoryMode(equipItemInfo);
			}
			else
			{
				heroSpecialEquipInfo.Close();
			}
		}
	}

	public InvenEquipInfo OpenInventoryMode(EquipItemInfo equipItemInfo, InvenEquipInfo.Listener listener)
	{
		if (equipItemInfo == null)
		{
			return null;
		}
		base.Open();
		InvenEquipInfo invenEquipInfo = GetInvenEquipInfo(WindowPosType.Right);
		if (invenEquipInfo == null)
		{
			invenEquipInfo = InstantiateEquipInfoPrefab(WindowPosType.Right);
		}
		if (invenEquipInfo != null)
		{
			invenEquipInfo.OpenInventoryMode(equipItemInfo, listener, this);
		}
		EnableBackground(enable: true);
		return invenEquipInfo;
	}

	public HeroSpecialEquipInfo Open(WindowPosType windowPosType, EquipItemInfo equipItemInfo)
	{
		if (!CheckHeroSpecialEquipInfo(equipItemInfo))
		{
			return null;
		}
		base.Open();
		HeroSpecialEquipInfo heroSpecialEquipInfo = GetHeroSpecialEquipInfo(windowPosType);
		if (heroSpecialEquipInfo == null)
		{
			heroSpecialEquipInfo = InstantiateHeroSpecialEquipInfoPrefab(windowPosType);
		}
		if (heroSpecialEquipInfo != null)
		{
			heroSpecialEquipInfo.OpenInventoryMode(equipItemInfo);
		}
		return heroSpecialEquipInfo;
	}

	public void CloseWithPosition(WindowPosType position)
	{
		InvenEquipInfo invenEquipInfo = GetInvenEquipInfo(position);
		if (invenEquipInfo != null)
		{
			invenEquipInfo.Close();
		}
	}

	public InvenEquipInfo GetInvenEquipInfo(WindowPosType position)
	{
		InvenEquipInfo invenEquipInfo = _instantiatedInvenEquipInfoComps[(int)position];
		if (invenEquipInfo != null)
		{
			return invenEquipInfo;
		}
		return null;
	}

	public HeroSpecialEquipInfo GetHeroSpecialEquipInfo(WindowPosType position)
	{
		HeroSpecialEquipInfo heroSpecialEquipInfo = _instantiatedHeroSpecialEquipInfo[(int)position];
		if (heroSpecialEquipInfo != null)
		{
			return heroSpecialEquipInfo;
		}
		return null;
	}

	private bool CheckHeroSpecialEquipInfo(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null || equipItemInfo.ItemData == null)
		{
			return false;
		}
		if (equipItemInfo.ItemData.Type != ItemType.Equip)
		{
			return false;
		}
		if (equipItemInfo.EquipItemData == null)
		{
			return false;
		}
		if (!equipItemInfo.EquipItemData.IsSpecialWeapon && !equipItemInfo.EquipItemData.IsSpecialTreasure)
		{
			return false;
		}
		return true;
	}

	public bool IsOpenInvenEquipInfo(WindowPosType position)
	{
		InvenEquipInfo invenEquipInfo = _instantiatedInvenEquipInfoComps[(int)position];
		if (invenEquipInfo == null)
		{
			return false;
		}
		return invenEquipInfo.IsOpen();
	}

	private bool CheckCloseOpenedHeroEquipItemInfo()
	{
		if (CheckOpenedHeroSpecialEquipInfo())
		{
			CloseOpenedHeroSpecialEquipInfo();
			return true;
		}
		return false;
	}

	private bool CheckOpenedHeroSpecialEquipInfo(bool close = false)
	{
		HeroSpecialEquipInfo heroSpecialEquipInfo = null;
		int i = 0;
		for (int num = _instantiatedHeroSpecialEquipInfo.Length; i < num; i++)
		{
			heroSpecialEquipInfo = _instantiatedHeroSpecialEquipInfo[i];
			if (!(heroSpecialEquipInfo == null) && heroSpecialEquipInfo.IsOpen())
			{
				if (close)
				{
					heroSpecialEquipInfo.Close();
				}
				return true;
			}
		}
		return false;
	}

	private void CloseOpenedHeroSpecialEquipInfo()
	{
		HeroSpecialEquipInfo heroSpecialEquipInfo = null;
		int i = 0;
		for (int num = _instantiatedHeroSpecialEquipInfo.Length; i < num; i++)
		{
			heroSpecialEquipInfo = _instantiatedHeroSpecialEquipInfo[i];
			if (!(heroSpecialEquipInfo == null) && heroSpecialEquipInfo.IsOpen())
			{
				heroSpecialEquipInfo.Close();
			}
		}
	}

	public override void OnClosing()
	{
		int i = 0;
		for (int num = _instantiatedInvenEquipInfoComps.Length; i < num; i++)
		{
			InvenEquipInfo invenEquipInfo = _instantiatedInvenEquipInfoComps[i];
			if (!(invenEquipInfo == null))
			{
				invenEquipInfo.Close();
			}
		}
	}

	private InvenEquipInfo InstantiateEquipInfoPrefab(WindowPosType position)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(PivotList, (int)position))
		{
			return null;
		}
		Transform transform = PivotList[(int)position];
		if (transform == null)
		{
			return null;
		}
		InvenEquipInfo invenEquipInfo = _instantiatedInvenEquipInfoComps[(int)position];
		if (invenEquipInfo != null)
		{
			FactoryBase.ReleaseComponent(invenEquipInfo);
			invenEquipInfo = null;
		}
		invenEquipInfo = _invenEquipPrefabAuto.Factory.Create(transform);
		_instantiatedInvenEquipInfoComps[(int)position] = invenEquipInfo;
		return invenEquipInfo;
	}

	private HeroSpecialEquipInfo InstantiateHeroSpecialEquipInfoPrefab(WindowPosType position)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(PivotList, (int)position))
		{
			return null;
		}
		Transform transform = PivotList[(int)position];
		if (transform == null)
		{
			return null;
		}
		HeroSpecialEquipInfo heroSpecialEquipInfo = _instantiatedHeroSpecialEquipInfo[(int)position];
		if (heroSpecialEquipInfo != null)
		{
			FactoryBase.ReleaseComponent(heroSpecialEquipInfo);
			heroSpecialEquipInfo = null;
		}
		heroSpecialEquipInfo = _heroSpecialEquipInfoPreafabAuto.Factory.Create(transform);
		_instantiatedHeroSpecialEquipInfo[(int)position] = heroSpecialEquipInfo;
		return heroSpecialEquipInfo;
	}

	private void EnableBackground(bool enable)
	{
		UIHelper.EnableGameObject(SpriteDimmedBG, enable);
	}

	void InvenEquipInfo.IInvenModeListener.OnClickClose()
	{
		OnClickClose();
	}

	public void ChangeHero(CachedHeroInfo heroinfo)
	{
		CloseWithPosition(WindowPosType.Center);
		if (IsOpenInvenEquipInfo(WindowPosType.Right))
		{
			GetInvenEquipInfo(WindowPosType.Right).ChangeHero(heroinfo);
		}
		CloseOpenedHeroSpecialEquipInfo();
	}
}
