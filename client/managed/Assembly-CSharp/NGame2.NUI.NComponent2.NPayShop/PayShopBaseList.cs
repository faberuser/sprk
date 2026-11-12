using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public abstract class PayShopBaseList : MonoBehaviour, PayShopBaseComponent.IListener
{
	public interface IListener
	{
		void OnClickProduct(PayShopIndexType type, int index);

		void OnClickOpenDiscountInfo(int productIndex);

		void OnClickBuyDiscountProduct(int productIndex, int discountPrice);
	}

	protected IListener _listener;

	public virtual PayShopIndexType PayShopIndexType => PayShopIndexType.PayShop;

	public virtual void Open(IListener listener)
	{
		_listener = listener;
	}

	public void SelectTag(int tagIndex)
	{
		CreateList(tagIndex);
	}

	public abstract void CreateList();

	public abstract void ReCreateList();

	public abstract void CreateList(int tagIndex);

	public abstract void ClearTag();

	public abstract void Refresh();

	public virtual void ClearList()
	{
		ReleaseComponents();
	}

	public virtual void ClearAll()
	{
		ReleaseComponents();
	}

	protected abstract void ReleaseComponents();

	void PayShopBaseComponent.IListener.OnClickProduct(int index)
	{
		OnClickProduct(index);
	}

	protected virtual void OnClickProduct(int index)
	{
		if (_listener != null)
		{
			_listener.OnClickProduct(PayShopIndexType, index);
		}
	}

	public virtual void UpdateProduct(int index)
	{
	}

	public virtual void UpdateDerived()
	{
	}
}
public abstract class PayShopBaseList<TData, TComp, TCategory> : PayShopBaseList, UIScrollItemBehaviour.IListener where TData : class, IDataKeySelector<int> where TComp : PayShopBaseComponent<TData>
{
	public UIReuseScrollGrid ScrollGrid;

	protected List<TComp> _componentList = new List<TComp>();

	protected List<TData> _datas = new List<TData>();

	protected TCategory _selectedCategory;

	public List<TData> Datas => _datas;

	public TCategory SelectedCategory => _selectedCategory;

	public virtual void CreateList(TCategory category)
	{
		_selectedCategory = category;
		List<TData> dataList = GetDataList(category);
		CreateComponentList(dataList);
	}

	public override void ReCreateList()
	{
		ClearList();
		CreateList(_selectedCategory);
	}

	public abstract List<TData> GetDataList(TCategory category);

	public override void CreateList(int tagIndex)
	{
		NewPayShopTagData data = GameRoot.Instance.NewPayShopTagDataContainer.GetData(tagIndex);
		if (data == null || data.IndexType != PayShopIndexType)
		{
			return;
		}
		if (data.IndexType != PayShopIndexType)
		{
			ReleaseComponents();
			return;
		}
		List<TData> list = new List<TData>();
		int[] indices = data.Indices;
		foreach (int index in indices)
		{
			TData data2 = GetData(index);
			if (data2 != null)
			{
				list.Add(data2);
			}
		}
		CreateComponentList(list, data.Indices);
	}

	protected abstract TData GetData(int index);

	protected virtual void CreateComponentList(List<TData> dataList, int[] sortIndices = null)
	{
		ReleaseComponents();
		List<TData> list = Filtering(dataList);
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			OnEndCreateList(isEmptyList: true);
			return;
		}
		Sort(list, sortIndices);
		_datas = list;
		SetScrollGrid();
		OnEndCreateList(isEmptyList: false);
	}

	protected void SetScrollGrid()
	{
		ScrollGrid.CreateReuseItem(GetComponentPrefabAuto(), _componentList, __Extension_Method_ICollection__.COUNT(_datas), this);
		if (ScrollGrid != null)
		{
			ScrollGrid.RepositionGrid();
		}
	}

	protected virtual void OnEndCreateList(bool isEmptyList)
	{
	}

	protected virtual List<TData> Filtering(List<TData> datas)
	{
		return datas;
	}

	protected virtual void Sort(List<TData> dataList, int[] sortIndices = null)
	{
		dataList.Sort(Sort);
	}

	protected virtual int Sort(TData a, TData b)
	{
		return 0;
	}

	protected abstract PrefabAuto<TComp> GetComponentPrefabAuto();

	public override void Refresh()
	{
		foreach (TComp component in _componentList)
		{
			component.Refresh();
		}
	}

	protected override void ReleaseComponents()
	{
		_datas.Clear();
		ScrollGrid.ClearGridItem<TComp>();
		_componentList.Clear();
	}

	public override void ClearTag()
	{
		CreateList(_selectedCategory);
	}

	void UIScrollItemBehaviour.IListener.ChangedItemIndex(Component comp, int index)
	{
		TComp val = comp as TComp;
		if (!(val == null))
		{
			TData data = _datas[index];
			OnChangedItem(val, data);
		}
	}

	protected virtual void OnChangedItem(TComp comp, TData data)
	{
		comp.ReleaseComponent();
		comp.Open(data, this);
	}

	public virtual int GetDataIndex(TData data)
	{
		return _datas.FindIndex((TData d) => d == data);
	}

	public virtual void ChagneData(int index, TData data)
	{
		if (__Extension_Method_ICollection__.COUNT(_datas) > index)
		{
			_datas[index] = data;
		}
	}

	public override void UpdateDerived()
	{
		foreach (TComp component in _componentList)
		{
			component.UpdateDerived();
		}
	}
}
