using System.Collections.Generic;
using NGame2.NUI.NComponent2.Nvalance;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NValance;

public abstract class BaseValanceFilterPanel<T> : ValanceFilterGroupComponent.IListener, ValanceFilterComponent.IListener where T : IValanceFilterImpl, new()
{
	public GameObject Root;

	public UITable FilterTable;

	public UIScrollView FilterScrollView;

	protected List<ValanceFilterGroupComponent> _listGroupComp = new List<ValanceFilterGroupComponent>();

	protected T _filterImpl = new T();

	public void OnOffRoot(bool isOn)
	{
		UIHelper.EnableGameObject(Root, isOn);
		if (isOn)
		{
			Reposition();
		}
	}

	public void Reposition()
	{
		if (!(FilterScrollView == null) && !(FilterTable == null))
		{
			FilterTable.Reposition();
			FilterScrollView.ResetPosition();
		}
	}

	public virtual void SetFilterGroupComponent()
	{
		if (FilterScrollView == null || FilterTable == null)
		{
			return;
		}
		IList<ValanceFilterGroupData> dataListByGroupType = GameRoot.Instance.ValanceFilterGroupDataContainer.GetDataListByGroupType(EValanceFilterGroupType.TechnoMagic);
		for (int i = 0; i < dataListByGroupType.Count; i++)
		{
			ValanceFilterGroupData valanceFilterGroupData = dataListByGroupType[i];
			if (valanceFilterGroupData != null)
			{
				ValanceFilterGroupComponent valanceFilterGroupComponent = ValanceFilterGroupComponent.PrefabAuto.Factory.Create(FilterTable.transform);
				valanceFilterGroupComponent.Open(valanceFilterGroupData, this);
				_listGroupComp.Add(valanceFilterGroupComponent);
			}
		}
		FilterTable.Reposition();
		FilterScrollView.ResetPosition();
	}

	public virtual void Reset()
	{
		_filterImpl.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listGroupComp); i++)
		{
			ValanceFilterGroupComponent valanceFilterGroupComponent = _listGroupComp[i];
			if (!(valanceFilterGroupComponent == null))
			{
				valanceFilterGroupComponent.Reset();
			}
		}
	}

	public void ReleaseFilterComponents()
	{
		FactoryBase.ReleaseAndClearList(_listGroupComp);
	}

	public void Close()
	{
		ReleaseFilterComponents();
		_filterImpl.Clear();
	}

	public abstract void OnClickFoldOut(bool isFoldOut);

	public abstract void OnClickCreatureFilter(ValanceFilterComponent comp, IValanceCreatureFilterInfo info);

	public abstract void OnClickEquipFilter(ValanceFilterComponent comp, IValanceEquipFilterInfo info);
}
