using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NShared;
using NShared.GetShopList;
using NShared.NLocalization;
using NShared.RequestShopList;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NWindow.NShop;

public class ShopFacade : SceneAutoBehaviour<ShopFacade>
{
	public DateTime RestockTime;

	private int _RestockSecond;

	public int ShopIndex { get; private set; }

	public bool IsValidRestockTime { get; private set; }

	public DateTime NextRestockTime
	{
		get
		{
			if (!IsValidRestockTime)
			{
				return DateTime.MinValue;
			}
			return RestockTime.AddSeconds(_RestockSecond);
		}
	}

	public void Reset(int shopIndex)
	{
		if (ShopIndex != shopIndex)
		{
			ShopIndex = shopIndex;
			RestockTime = DateTime.MinValue;
			ShopData data = GameRoot.Instance.ShopDataContainer.GetData(ShopIndex);
			_RestockSecond = ((data != null && data.RestockSecond > 0) ? data.RestockSecond : 3600);
			UpdateRestockTime();
		}
	}

	public ITask FreeRestockIfNeed()
	{
		return new BranchTask().If(() => NeedRestock()).Then(FreeRestock(null));
	}

	private void UpdateRestockTime()
	{
		IsValidRestockTime = false;
		ShopRestockTimeInfo shopRestockTimeInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopRestockTimeManager.Get(ShopIndex);
		if (shopRestockTimeInfo != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(shopRestockTimeInfo.RestockTime, out RestockTime))
		{
			IsValidRestockTime = true;
		}
	}

	public bool NeedRestock()
	{
		ShopData data = GameRoot.Instance.ShopDataContainer.GetData(ShopIndex);
		if (data != null && data.MaxStock == 0)
		{
			return false;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (!IsValidRestockTime)
		{
			return true;
		}
		if (NextRestockTime.CompareTo(currentServerUTCTime) < 0)
		{
			return true;
		}
		return !GlobalAutoBehaviour<AccountManager>.instance.ShopItemManager.ShopItemExists(ShopIndex);
	}

	public ITask FreeRestock(Func<NShared.GetShopList.Response, ITask> onResponse)
	{
		return new WebServiceTask<NShared.GetShopList.Request, NShared.GetShopList.Response>(new NShared.GetShopList.Request
		{
			ShopIndex = ShopIndex
		}, delegate(NShared.GetShopList.Request request, NShared.GetShopList.Response response)
		{
			NShared.GetShopList.Response response2 = response;
			if (response2.Result != NShared.GetShopList.ResultType.Success)
			{
				return new DelegateTask(delegate
				{
					MessagePopup.OpenError(response2.Result);
				});
			}
			GlobalAutoBehaviour<AccountManager>.instance.ShopItemManager.Set(response2.ShopItems);
			SprkNetworkPerformance.ShopListApplied(response.ShopItems);
			GlobalAutoBehaviour<AccountManager>.instance.ShopRestockTimeManager.Add(response2.RestockTimeInfo);
			UpdateRestockTime();
			return Vespa_System_Action_Extension_Method.fire(onResponse, response2);
		});
	}

	public ITask PaidRestock(Func<NShared.RequestShopList.Response, ITask> onResponse)
	{
		return new WebServiceTask<NShared.RequestShopList.Request, NShared.RequestShopList.Response>(new NShared.RequestShopList.Request
		{
			ShopIndex = ShopIndex
		}, delegate(NShared.RequestShopList.Request request, NShared.RequestShopList.Response response)
		{
			NShared.RequestShopList.Response response2 = response;
			if (response2.Result == NShared.RequestShopList.ResultType.NotEnoughGem)
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
				return new DelegateTask(delegate
				{
					MessagePopup.Open(title, message, delegate
					{
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
					}, null);
				});
			}
			if (response2.Result != NShared.RequestShopList.ResultType.Success)
			{
				return new DelegateTask(delegate
				{
					MessagePopup.OpenError(response2.Result);
				});
			}
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response2.CurrencyResult);
			GlobalAutoBehaviour<AccountManager>.instance.ShopItemManager.Set(response2.ShopItems);
			SprkNetworkPerformance.ShopListApplied(response.ShopItems);
			GlobalAutoBehaviour<AccountManager>.instance.ShopRestockTimeManager.Set(new ShopRestockTimeInfo[1] { response2.RestockTimeInfo });
			UpdateRestockTime();
			return Vespa_System_Action_Extension_Method.fire(onResponse, response2);
		});
	}
}
