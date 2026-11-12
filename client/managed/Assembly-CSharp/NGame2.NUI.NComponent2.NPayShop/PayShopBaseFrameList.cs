using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NComponent2.NPayShop;

public abstract class PayShopBaseFrameList
{
}
public abstract class PayShopBaseFrameList<TData, TComp, TFrameComp, TCategory> : PayShopBaseList<PayShopPairData<TData>, TFrameComp, TCategory> where TData : class, IDataKeySelector<int>, IPayShopCardData where TComp : PayShopBaseComponent<TData> where TFrameComp : PayShopBaseFrameComponent<TComp, TData>
{
	public override void CreateList(TCategory category)
	{
		_selectedCategory = category;
		List<TData> realDataList = GetRealDataList(category);
		CreateComponentList(realDataList);
	}

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
			TData realData = GetRealData(index);
			if (realData != null)
			{
				list.Add(realData);
			}
		}
		CreateComponentList(list, data.Indices);
	}

	public override List<PayShopPairData<TData>> GetDataList(TCategory category)
	{
		return null;
	}

	protected override PayShopPairData<TData> GetData(int index)
	{
		return null;
	}

	public abstract List<TData> GetRealDataList(TCategory category);

	protected virtual TData GetRealData(int index)
	{
		return null;
	}

	protected virtual void CreateComponentList(List<TData> dataList, int[] indices = null)
	{
		ReleaseComponents();
		List<TData> list = Filtering(dataList);
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			OnEndCreateList(isEmptyList: true);
			return;
		}
		Sort(list, indices);
		_datas = CreatePairData(list);
		SetScrollGrid();
		OnEndCreateList(isEmptyList: false);
	}

	protected virtual List<TData> Filtering(List<TData> dataList)
	{
		return dataList;
	}

	protected virtual void Sort(List<TData> dataList, int[] indices = null)
	{
	}

	protected virtual List<PayShopPairData<TData>> CreatePairData(List<TData> filteredList)
	{
		List<PayShopPairData<TData>> list = new List<PayShopPairData<TData>>();
		foreach (TData filtered in filteredList)
		{
			if (filtered.CardType == 1)
			{
				PayShopPairData<TData> payShopPairData = new PayShopPairData<TData>();
				payShopPairData.Datas.Add(filtered);
				list.Add(payShopPairData);
				continue;
			}
			if (__Extension_Method_ICollection__.COUNT(list) == 0)
			{
				PayShopPairData<TData> payShopPairData2 = new PayShopPairData<TData>();
				payShopPairData2.Datas.Add(filtered);
				list.Add(payShopPairData2);
				continue;
			}
			PayShopPairData<TData> last = Vespa_Generic_IList_Extension_Method.GetLast(list);
			if (__Extension_Method_ICollection__.COUNT(last.Datas) == 1)
			{
				if (last.Datas[0].CardType == 1)
				{
					PayShopPairData<TData> payShopPairData3 = new PayShopPairData<TData>();
					payShopPairData3.Datas.Add(filtered);
					list.Add(payShopPairData3);
				}
				else
				{
					last.Datas.Add(filtered);
				}
			}
			else
			{
				PayShopPairData<TData> payShopPairData4 = new PayShopPairData<TData>();
				payShopPairData4.Datas.Add(filtered);
				list.Add(payShopPairData4);
			}
		}
		return list;
	}

	protected TComp FindComponent(int index)
	{
		foreach (TFrameComp component in _componentList)
		{
			TComp val = component.FindComponent(index);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}
}
