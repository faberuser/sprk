using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopSpecialCurrencyComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopSpecialCurrencyComponent";

	public static PrefabAuto<PayShopSpecialCurrencyComponent> PrefabAuto = new PrefabAuto<PayShopSpecialCurrencyComponent>("Prefab2/Component/PayShop2/PayShopSpecialCurrencyComponent");

	public UIWidget IconProxyRoot;

	public UILabel LabelPointCount;

	private IconProxy _iconProxy;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseIconProxy();
	}

	public void Open(CurrencyType currencyType, int count)
	{
		CreateIconProxy();
		SetIcon(currencyType);
		SetCount(count);
	}

	private void SetIcon(CurrencyType currencyType)
	{
		if (_iconProxy != null)
		{
			_iconProxy.SetupCurrencyNoBg(currencyType);
		}
	}

	private void SetCount(int count)
	{
		string text = string.Empty;
		if (count != 0)
		{
			text = "+ " + Vespa_Int_Extension_Method.ToCommaString(count);
		}
		UIHelper.SetLabelText(LabelPointCount, text);
	}

	private void CreateIconProxy()
	{
		if (!(IconProxyRoot == null) && _iconProxy == null)
		{
			_iconProxy = IconProxy.Create(IconProxyRoot);
		}
	}

	private void ReleaseIconProxy()
	{
		if (_iconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
			_iconProxy = null;
		}
	}
}
