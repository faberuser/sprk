using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public abstract class PayShopBaseTabList
{
	public interface IListener : PayShopBaseList.IListener
	{
		void OnClickTab();
	}
}
public abstract class PayShopBaseTabList<TData, TComp, TTabComp, TCategory> : PayShopBaseList<TData, TComp, TCategory>, PayShopBaseTabComponent<TCategory>.IListener where TData : class, IDataKeySelector<int> where TComp : PayShopBaseComponent<TData> where TTabComp : PayShopBaseTabComponent<TCategory>
{
	public UISimpleGrid TabGrid;

	public List<TCategory> CategoryList = new List<TCategory>();

	private Dictionary<TCategory, TTabComp> _tabComponentDic = new Dictionary<TCategory, TTabComp>();

	private List<TTabComp> _tabComponents = new List<TTabComp>();

	protected new PayShopBaseTabList.IListener _listener;

	public override void Open(IListener listener)
	{
		_listener = (PayShopBaseTabList.IListener)listener;
		base.Open(listener);
		CreateTabList();
	}

	protected virtual void CreateTabList()
	{
		foreach (TCategory category in CategoryList)
		{
			TTabComp val = UIHelper.AddGridItem(TabGrid, GetTabPrefabAuto());
			if (val != null)
			{
				val.Open(this, category);
				val.Select(isSelect: false);
				_tabComponentDic.Add(category, val);
				_tabComponents.Add(val);
			}
		}
		UIHelper.RepositionGrid(TabGrid);
	}

	protected abstract PrefabAuto<TTabComp> GetTabPrefabAuto();

	public void OnClickTab(TCategory category)
	{
		CreateList(category);
		if (_listener != null)
		{
			_listener.OnClickTab();
		}
	}

	public override void CreateList(TCategory category)
	{
		TCategory selectedCategory = _selectedCategory;
		base.CreateList(category);
		SelectCategory(selectedCategory, isSelect: false);
		SelectCategory(category, isSelect: true);
	}

	private void SelectCategory(TCategory category, bool isSelect)
	{
		if (category != null)
		{
			TTabComp vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_tabComponentDic, category, null);
			if ((bool)vALUE)
			{
				vALUE.Select(isSelect);
			}
		}
	}

	public override void ClearList()
	{
		base.ClearList();
		SelectCategory(_selectedCategory, isSelect: false);
	}

	public override void ClearAll()
	{
		base.ClearAll();
		_tabComponentDic.Clear();
		FactoryBase.ReleaseAndClearList(_tabComponents);
	}
}
