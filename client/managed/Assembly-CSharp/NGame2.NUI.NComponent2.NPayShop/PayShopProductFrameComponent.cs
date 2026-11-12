using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopProductFrameComponent : PayShopBaseFrameComponent<PayShopProductComponent, NewPayShopProductInfo>, PayShopProductComponent.IListener, PayShopBaseComponent.IListener, InitOnCreate
{
	public new interface IListener : PayShopBaseComponent.IListener
	{
		void OnClickProduct(int originalIndex, int comboIndex);

		void OnChangeComboProduct(int productIndex, int step);
	}

	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopProductFrameComponent";

	public static PrefabAuto<PayShopProductFrameComponent> PrefabAuto = new PrefabAuto<PayShopProductFrameComponent>("Prefab2/Component/PayShop2/PayShopProductFrameComponent");

	private new IListener _listener;

	public override void Open(PayShopPairData<NewPayShopProductInfo> data, PayShopBaseComponent.IListener listener)
	{
		_listener = listener as IListener;
		base.Open(data, listener);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseComponent();
	}

	void PayShopProductComponent.IListener.OnClickProduct(int originalIndex, int comboIndex)
	{
		if (_listener != null)
		{
			_listener.OnClickProduct(originalIndex, comboIndex);
		}
	}

	protected override PrefabAuto<PayShopProductComponent> GetPrefabAuto(int cardType)
	{
		return PayShopProductComponent.GetPrefabAuto(cardType);
	}

	void PayShopProductComponent.IListener.OnChangeComboProduct(int productIndex, int step)
	{
		if (_listener != null)
		{
			_listener.OnChangeComboProduct(productIndex, step);
		}
	}
}
