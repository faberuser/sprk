using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.GetGlobalMailList;
using NShared.GetMailList;
using NShared.NLocalization;
using NShared.ReceiveMail;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;

namespace NGame2.NUI.NManager;

public class MailManagement : BaseManagement<MailManagement>, MailListPopup.Listener, NGame2.NUI.NWindow.MailInfo.Listener
{
	public delegate void onErrorFunc();

	public const int AllReceiveMaxCount = 50;

	private const int MailCountPerRead = 10;

	private const int NewMailCountPerRead = 5;

	private static LocalAccountLong _topmostMailIndexStorage = new LocalAccountLong(0L, typeof(MailManagement).Name + "TopmostMailIndex");

	private static LocalAccountLong _lastCheckedMailIndexStorage = new LocalAccountLong(0L, typeof(MailManagement).Name + "LastCheckedMailIndex");

	public BaseWindow rewardPopup;

	public static long TopmostMailIndex
	{
		get
		{
			return _topmostMailIndexStorage.Value;
		}
		set
		{
			_topmostMailIndexStorage.Value = value;
		}
	}

	public static long LastCheckedMailIndex
	{
		get
		{
			return _lastCheckedMailIndexStorage.Value;
		}
		set
		{
			_lastCheckedMailIndexStorage.Value = value;
		}
	}

	public static bool IsNewMailArrived { get; set; }

	public static bool IsNewGlobalMailArrived { get; set; }

	public static int MailCount { get; set; }

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<MailListPopup>("Resources/Prefab2/Window/Mail/MailListPopup");
		AddInstance<NGame2.NUI.NWindow.MailInfo>("Resources/Prefab2/Window/Mail/MailInfoNormal");
		AddInstance<MailInfoAttached>("Resources/Prefab2/Window/Mail/MailInfoAttached");
	}

	public void Reset()
	{
		IsNewMailArrived = false;
		IsNewGlobalMailArrived = false;
		MailCount = 0;
	}

	public void Open()
	{
		AddTask(base.TaskFactory.Run(delegate
		{
			CloseBut();
		}), OpenMailListAsync());
	}

	private ITask OpenMailListAsync()
	{
		return base.TaskFactory.Run(delegate(MailListPopup window)
		{
			window.Open(this);
		}).WrapperTouchControl().DisableTouch();
	}

	public void RequestMailList(long lastMailIndex, int count, Action<NShared.MailInfo[], int[], int> onList, onErrorFunc onError)
	{
		bool forceReload = false;
		bool isFirstRequest = lastMailIndex == 0;
		if (isFirstRequest)
		{
			forceReload = IsNewMailArrived;
			IsNewMailArrived = false;
		}
		NShared.GetMailList.Request request = new NShared.GetMailList.Request
		{
			LastMailIndex = lastMailIndex,
			MaxCount = count,
			ForceReload = forceReload
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetMailList.Response response)
		{
			if (response.Result != NShared.GetMailList.ResultType.Success)
			{
				onError();
			}
			else
			{
				if (isFirstRequest)
				{
					TopmostMailIndex = ((response.MailInfos.Length != 0) ? response.MailInfos[0].MailIndex : 0);
				}
				onList(response.MailInfos, response.OpenedMailIndices, response.TotalCount);
			}
		});
	}

	public void RequestGlobalMailList(Action<NShared.MailInfo[]> onList, onErrorFunc onError)
	{
		IsNewGlobalMailArrived = false;
		NShared.GetGlobalMailList.Request request = new NShared.GetGlobalMailList.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGlobalMailList.Response response)
		{
			if (response.Result != NShared.GetGlobalMailList.ResultType.Success)
			{
				onError();
			}
			else
			{
				onList(response.MailInfos);
			}
		});
	}

	bool MailListPopup.Listener.OnCloseRequest(MailListPopup caller)
	{
		return true;
	}

	void MailListPopup.Listener.OnClickMail(MailListComponent mailListComponent)
	{
		if (mailListComponent.isNormal())
		{
			base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.MailInfo window)
			{
				NShared.MailInfo mailInfo = mailListComponent.MailInfo;
				bool isNew = MailListComponent.IsNewMail(mailInfo.MailIndex);
				window.Open(mailInfo, isNew, this);
			}).Commit();
		}
		else
		{
			base.TaskFactory.Run(delegate(MailInfoAttached window)
			{
				NShared.MailInfo mailInfo = mailListComponent.MailInfo;
				bool isNew = MailListComponent.IsNewMail(mailInfo.MailIndex);
				window.Open(mailInfo, isNew, this);
			}).Commit();
		}
	}

	void MailListPopup.Listener.OnReceiveAll(MailListPopup caller, int TotalCount)
	{
		ItemMaxCap.WarningOrExecuteForAllMail(delegate
		{
			OnReceiveAll_Real(caller, TotalCount);
		});
	}

	private void OnReceiveAll_Real(MailListPopup caller, int TotalCount)
	{
		SprkMailClaimAll.Start(this, caller, TotalCount);
	}

	void MailListPopup.Listener.OpenMailListPopup(MailListPopup mailListPopup)
	{
		if (mailListPopup != null)
		{
			if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
			{
				LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
				mailListPopup.ChangeLayerRecursive(layerInfo);
			}
			else
			{
				LayerInfo layerInfo2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI);
				mailListPopup.ChangeLayerRecursive(layerInfo2);
			}
		}
	}

	bool NGame2.NUI.NWindow.MailInfo.Listener.OnCloseRequest(NGame2.NUI.NWindow.MailInfo caller)
	{
		if (caller is MailInfoAttached)
		{
			AddTask(base.TaskFactory.Run(delegate(MailInfoAttached window)
			{
				MailListComponent.MakeRead(window.Info.MailIndex);
			}), base.TaskFactory.Run(delegate(MailListPopup window)
			{
				window.UpdateReadState();
			}));
		}
		else
		{
			AddTask(base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.MailInfo window)
			{
				MailListComponent.MakeRead(window.Info.MailIndex);
			}), base.TaskFactory.Run(delegate(MailListPopup window)
			{
				window.UpdateReadState();
			}));
		}
		return true;
	}

	void NGame2.NUI.NWindow.MailInfo.Listener.OnDelete(NGame2.NUI.NWindow.MailInfo caller)
	{
		RequestReceiveMail(caller);
	}

	private bool ContainsHeroInMailAndMyHeroes(NGame2.NUI.NWindow.MailInfo caller, out int rewardIndex)
	{
		rewardIndex = 0;
		if (caller.Info.ItemInfos == null)
		{
			return false;
		}
		MailItemInfo[] itemInfos = caller.Info.ItemInfos;
		foreach (MailItemInfo mailItemInfo in itemInfos)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(mailItemInfo.ItemIndex);
			if (data == null || data.Type != ItemType.Hero)
			{
				continue;
			}
			int num = mailItemInfo.ItemIndex;
			int num2 = 0;
			int num3 = 0;
			if (__Extension_Method_ICollection__.COUNT(data.Value) > 0)
			{
				num = ((data.Value[0] != null) ? Vespa_String_Extension_Method.TOINT(data.Value[0], num) : num);
			}
			CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(num);
			if (data2 == null)
			{
				VespaDebug.LogError("CreatureData not found : " + num);
				continue;
			}
			num2 = ((__Extension_Method_ICollection__.COUNT(data.Value) <= 1) ? data2.StartHeroStar : ((data.Value[1] != null) ? Vespa_String_Extension_Method.TOINT(data.Value[1], num2) : num2));
			if (__Extension_Method_ICollection__.COUNT(data.Value) > 3)
			{
				num3 = ((data.Value[3] != null) ? Vespa_String_Extension_Method.TOINT(data.Value[3], num3) : num3);
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(num) != null)
			{
				CreatureStarData data3 = GameRoot.Instance.CreatureStarDataContainer.GetData(num2, num3);
				if (data3 != null)
				{
					rewardIndex = data3.AllOwnedRewardIndex;
					return true;
				}
			}
		}
		return false;
	}

	private void GetCurrencyTypesInMail(NGame2.NUI.NWindow.MailInfo caller, out HashSet<CurrencyType> currencyTypes, out HashSet<int> itemIndices)
	{
		currencyTypes = new HashSet<CurrencyType>();
		itemIndices = new HashSet<int>();
		if (caller.Info.PvpCoin > 0)
		{
			currencyTypes.Add(CurrencyType.PvpCoin);
		}
		if (caller.Info.GuildPoint > 0)
		{
			currencyTypes.Add(CurrencyType.GuildPoint);
		}
		if (caller.Info.WorldBossPoint > 0)
		{
			currencyTypes.Add(CurrencyType.WorldBossPoint);
		}
		if (caller.Info.GloryPoint > 0)
		{
			currencyTypes.Add(CurrencyType.GloryPoint);
		}
		if (caller.Info.CurrencyInfos != null && caller.Info.CurrencyInfos.Length != 0)
		{
			MailCurrencyInfo[] currencyInfos = caller.Info.CurrencyInfos;
			foreach (MailCurrencyInfo mailCurrencyInfo in currencyInfos)
			{
				if (mailCurrencyInfo.Amount > 0)
				{
					currencyTypes.Add(mailCurrencyInfo.CurrencyType);
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(caller.Info.ItemInfos); j++)
		{
			MailItemInfo mailItemInfo = caller.Info.ItemInfos[j];
			itemIndices.Add(mailItemInfo.ItemIndex);
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(caller.Info.EquipItemInfos); k++)
		{
			EquipItemInfo equipItemInfo = caller.Info.EquipItemInfos[k];
			itemIndices.Add(equipItemInfo.ItemIndex);
		}
	}

	void NGame2.NUI.NWindow.MailInfo.Listener.OnReceive(NGame2.NUI.NWindow.MailInfo caller)
	{
		if (caller == null || !(caller is MailInfoAttached))
		{
			return;
		}
		if (ContainsHeroInMailAndMyHeroes(caller, out var rewardIndex))
		{
			UIHelper.OpenHeroAwakeChallengeItemPopup(rewardIndex, delegate
			{
				RequestReceiveMail(caller);
			});
			return;
		}
		GetCurrencyTypesInMail(caller, out var currencyTypes, out var itemIndices);
		ItemMaxCap.WarningOrExecute(currencyTypes, itemIndices, delegate
		{
			RequestReceiveMail(caller);
		});
	}

	private void RequestReceiveMail(NGame2.NUI.NWindow.MailInfo caller)
	{
		if (caller == null)
		{
			return;
		}
		NShared.ReceiveMail.Request request = new NShared.ReceiveMail.Request
		{
			MailIndex = caller.Info.MailIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ReceiveMail.Response response)
		{
			if (!RestoredPortal.ShowMailCapacityError(response))
			{
				if (response.Result != NShared.ReceiveMail.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					caller.Close();
					SingletonAuto<MailInfoContainer>.instance.OnDeleteMail(caller.Info.MailIndex);
					GlobalAutoBehaviour<AccountManager>.instance.ApplyReceiveMail(response.MailReceiveResult);
					BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Mail.RECEIVED_MAIL_ATTACHMENT)).SetItemResultInfos(response.MailReceiveResult.ItemResults).SetEquipResultInfos(response.MailReceiveResult.EquipItemResults)
						.SetGuildPointResult(response.MailReceiveResult.GuildPointResult)
						.SetStaminaResultInfos(response.MailReceiveResult.StaminaResults)
						.SetCurrencyResultInfos3(response.MailReceiveResult.CurrencyResults)
						.AddPetInfo(response.MailReceiveResult.PetResult);
					rewardPopup = BaseRewardPopup.OpenPopup(popupOpen, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI());
				}
			}
		});
	}
}
