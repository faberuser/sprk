using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public abstract class PayShopBaseFrameComponent<TComp, TData> : PayShopBaseComponent<PayShopPairData<TData>>, PayShopBaseComponent.IListener where TComp : PayShopBaseComponent<TData> where TData : IDataKeySelector<int>, IPayShopCardData
{
	public UISimpleGrid Grid;

	private List<TComp> _components = new List<TComp>();

	public bool IsSmallCard { get; private set; }

	protected override void Init(PayShopPairData<TData> pairData)
	{
		foreach (TData data in pairData.Datas)
		{
			if (data.CardType != 1)
			{
				IsSmallCard = true;
			}
			TComp val = CreateProdctComponent(data.CardType);
			val.Open(data, this);
			_components.Add(val);
		}
		if (IsSmallCard)
		{
			UIHelper.RepositionGrid(Grid);
		}
	}

	protected TComp CreateProdctComponent(int cardType)
	{
		PrefabAuto<TComp> prefabAuto = GetPrefabAuto(cardType);
		TComp val;
		if (cardType == 1)
		{
			val = prefabAuto.Factory.Create(base.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(val.transform);
		}
		else
		{
			val = UIHelper.AddGridItem(Grid, prefabAuto);
		}
		return val;
	}

	protected abstract PrefabAuto<TComp> GetPrefabAuto(int cardType);

	void IListener.OnClickProduct(int index)
	{
		if (_listener != null)
		{
			_listener.OnClickProduct(index);
		}
	}

	public override void ReleaseComponent()
	{
		FactoryBase.ReleaseAndClearList(_components);
	}

	public TComp FindComponent(int index)
	{
		return _components.Find((TComp comp) => comp.Index == index);
	}

	public override void UpdateDerived()
	{
		foreach (TComp component in _components)
		{
			component.UpdateDerived();
		}
	}
}
