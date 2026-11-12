using NGame2.NLogin;
using NShared;
using NVespa;

namespace NGame2.IAP;

public class IAPWrapper : SingletonAuto<IAPWrapper>
{
	public enum EStoreType
	{
		Editor,
		PTS,
		GooglePlay,
		OneStore,
		AppStore,
		UDP,
		Samsung,
		Steam
	}

	public static EStoreType StoreType;

	public bool IsReady => default(bool);

	public IAPManagerInterface IAPManager { get; private set; }

	public IAPWrapper()
	{
		if (SingletonUpdater<LoginManager>.isValidInstance)
		{
			_ = SingletonUpdater<LoginManager>.instance.VersionInfo;
		}
		SetIAPManager();
	}

	public void ForceSetIAPManager(int type)
	{
		IAPManager = null;
	}

	public void SetIAPManager()
	{
		IAPManager = null;
	}

	public void SetIAPManager(EStoreType storeType)
	{
		IAPManager = null;
	}

	public void SetEditorIAPManager()
	{
		IAPManager = null;
	}

	public void SetPTSIAPManager()
	{
		IAPManager = null;
	}

	public void SetOneStoreIAPManager()
	{
		IAPManager = null;
	}

	public void SetUDPIAPManager()
	{
		IAPManager = null;
	}

	public void SetAppStoreIAPManager()
	{
		IAPManager = null;
	}

	public void SetUnityIAPManager()
	{
		IAPManager = null;
	}

	public void SetSamsungIAPManager()
	{
		IAPManager = null;
	}

	public void SetSteamIAPManager()
	{
		IAPManager = null;
	}

	public SkuInfo GetSkuInfo(string productId, NewPayShopProductInfo productInfo = null)
	{
		return null;
	}
}
