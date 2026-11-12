using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NShop;
using NShared;
using NShared.GetAllShopItemPurchaseCount;
using NShared.GetGuildRaidAllBootyItems;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NManager;

public class ShopManagement : BaseManagement<ShopManagement>, Shop.Listener
{
	private ShopData _openedShopData;

	private DateTime _lastGuildRaidBootyItemRequestedTime = DateTime.MinValue;

	private Dictionary<int, List<ShopItemInfo>> _shopItemPurchaseCountDic = new Dictionary<int, List<ShopItemInfo>>();

	protected override void Awake()
	{
		base.Awake();
		AddInstance<Shop>("Resources/Prefab2/Window/Shop/Shop");
	}

	public void OpenShop(ShopData shopData, NPCData npcData)
	{
		if (shopData.BuyCostType == ShopCostType.GuildPoint && GlobalAutoBehaviour<GuildManager>.instance.JoinedGuildId != 0L)
		{
			RequestGuildRaidBootyItems(forceUpdate: false, OpenShopAsync(shopData, npcData));
		}
		else
		{
			OpenShopAsync(shopData, npcData).Commit();
		}
	}

	public void RequestGuildRaidBootyItems(bool forceUpdate = false, ITask task = null, Action action = null)
	{
		DateTime utcNow = DateTime.UtcNow;
		if ((utcNow - _lastGuildRaidBootyItemRequestedTime).TotalSeconds < 60.0 && !forceUpdate)
		{
			if (task != null)
			{
				task.Commit();
			}
			if (action != null)
			{
				Vespa_System_Action_Extension_Method.fire(action);
			}
			return;
		}
		_lastGuildRaidBootyItemRequestedTime = utcNow;
		NShared.GetGuildRaidAllBootyItems.Request request = new NShared.GetGuildRaidAllBootyItems.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildRaidAllBootyItems.Response response)
		{
			if (response.Result != NShared.GetGuildRaidAllBootyItems.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildRaidBootyEquipItems(response.EquipItemInfos);
				GlobalAutoBehaviour<GuildManager>.instance.InitGuildRaidBootyItems(response.ItemInfos);
				if (task != null)
				{
					task.Commit();
				}
				if (action != null)
				{
					Vespa_System_Action_Extension_Method.fire(action);
				}
			}
		});
	}

	private ITask OpenShopAsync(ShopData shopData, NPCData npcData)
	{
		SceneAutoBehaviour<ShopFacade>.instance.Reset(shopData.Index);
		return Vespa_Task_ExtensionMethod.NAME(new GroupTask().ADDTASK(SceneAutoBehaviour<ShopFacade>.instance.FreeRestockIfNeed()), "FreeRestockIfNeed").ADDTASK(base.TaskFactory.Run(delegate(Shop window)
		{
			window.title.Key = "SHOP_NAME_" + shopData.Code.ToUpper();
		})).ADDTASK(base.TaskFactory.Run(delegate(Shop window)
		{
			window.SetPlayerAssetInfo(shopData);
		}))
			.ADDTASK(base.TaskFactory.Open(delegate(Shop window)
			{
				_openedShopData = shopData;
				window.Init(this, _openedShopData, npcData);
			}))
			.WrapperTouchControl()
			.DisableTouch();
	}

	bool Shop.Listener.OnCloseRequest(Shop closeThis)
	{
		return true;
	}

	public void OnNPCSpeak()
	{
		AddTask(base.TaskFactory.Branch(base.TaskFactory.IsOpen<Shop>(), base.TaskFactory.Run(delegate(Shop window)
		{
			window.SetNextSpeechTime(3.0);
		})));
	}

	public static void ResetShopItemPurchaseCountDictionary()
	{
		if (SceneAutoBehaviour<ShopManagement>.isValidInstance)
		{
			SceneAutoBehaviour<ShopManagement>.instance._shopItemPurchaseCountDic.Clear();
		}
	}

	public void SetShopItemInfos(int shopIndex, ShopItemInfo[] shopItemInfos)
	{
		List<ShopItemInfo> list = new List<ShopItemInfo>();
		list.AddRange(shopItemInfos);
		if (_shopItemPurchaseCountDic.ContainsKey(shopIndex))
		{
			_shopItemPurchaseCountDic[shopIndex] = list;
		}
		else
		{
			_shopItemPurchaseCountDic.Add(shopIndex, list);
		}
	}

	public void UpdateShopItemInfo(int shopIndex, ShopItemInfo shopItemInfo)
	{
		if (shopItemInfo == null)
		{
			return;
		}
		List<ShopItemInfo> list = null;
		if (_shopItemPurchaseCountDic.ContainsKey(shopIndex))
		{
			list = _shopItemPurchaseCountDic[shopIndex];
		}
		else
		{
			list = new List<ShopItemInfo>();
			_shopItemPurchaseCountDic.Add(shopIndex, list);
		}
		bool flag = true;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			ShopItemInfo shopItemInfo2 = list[i];
			if (shopItemInfo2.ShopIndex == shopItemInfo.ShopIndex && shopItemInfo2.ListNo == shopItemInfo.ListNo)
			{
				list[i] = shopItemInfo;
				flag = false;
				break;
			}
		}
		if (flag)
		{
			list.Add(shopItemInfo);
		}
	}

	public bool HasShopItemInfos(int shopIndex)
	{
		return _shopItemPurchaseCountDic.ContainsKey(shopIndex);
	}

	public ShopItemInfo GetShopItemPurchaseInfo(int shopIndex, int shopItemIndex)
	{
		if (!_shopItemPurchaseCountDic.ContainsKey(shopIndex))
		{
			return null;
		}
		return _shopItemPurchaseCountDic[shopIndex].Find((ShopItemInfo info) => info.ListNo == shopItemIndex);
	}

	public void RequestPurchasableCount(ShopData shopData, Action callback = null)
	{
		if (shopData == null)
		{
			return;
		}
		int shopIndex = shopData.Index;
		NShared.GetAllShopItemPurchaseCount.Request request = new NShared.GetAllShopItemPurchaseCount.Request();
		request.ShopIndex = shopIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetAllShopItemPurchaseCount.Response iResponse)
		{
			if (iResponse.Result == NShared.GetAllShopItemPurchaseCount.ResultType.Success)
			{
				SetShopItemInfos(shopIndex, iResponse.ShopItems);
				Vespa_System_Action_Extension_Method.fire(callback);
			}
		});
	}
}
