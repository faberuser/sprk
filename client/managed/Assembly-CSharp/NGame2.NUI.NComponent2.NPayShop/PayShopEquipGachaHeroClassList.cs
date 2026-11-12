using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaHeroClassList : MonoBehaviour, PayShopEquipGachaHeroClassComponent.IListener
{
	public interface IListener
	{
		void OnClickTab(HighGachaCategoryType type);
	}

	public UISimpleGrid Grid;

	public List<HighGachaCategoryType> CategoryList;

	private List<PayShopEquipGachaHeroClassComponent> _compList = new List<PayShopEquipGachaHeroClassComponent>();

	private PayShopEquipGachaHeroClassComponent _selectedComp;

	private IListener _listener;

	public void Open(IListener listener)
	{
		_listener = listener;
		foreach (HighGachaCategoryType category in CategoryList)
		{
			PayShopEquipGachaHeroClassComponent payShopEquipGachaHeroClassComponent = UIHelper.AddGridItem(Grid, PayShopEquipGachaHeroClassComponent.PrefabAuto);
			payShopEquipGachaHeroClassComponent.Open(this, category);
			payShopEquipGachaHeroClassComponent.Select(isSelect: false);
			_compList.Add(payShopEquipGachaHeroClassComponent);
		}
		UIHelper.RepositionGrid(Grid);
	}

	void PayShopEquipGachaHeroClassComponent.IListener.OnClickTab(HighGachaCategoryType type)
	{
		Select(type);
		if (_listener != null)
		{
			_listener.OnClickTab(type);
		}
	}

	public void Select(HighGachaCategoryType type)
	{
		if (_selectedComp != null)
		{
			_selectedComp.Select(isSelect: false);
		}
		_selectedComp = GetComponent(type);
		_selectedComp.Select(isSelect: true);
	}

	public PayShopEquipGachaHeroClassComponent GetComponent(HighGachaCategoryType type)
	{
		return _compList.Find((PayShopEquipGachaHeroClassComponent comp) => comp.Type == type);
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_compList);
		_selectedComp = null;
	}
}
