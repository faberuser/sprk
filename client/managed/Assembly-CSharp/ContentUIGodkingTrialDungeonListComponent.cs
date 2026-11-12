using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NShared.OpenGodkingTrialDungeon;
using NShared.RechargeStamina;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

public class ContentUIGodkingTrialDungeonListComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void OnOpening(ContentUIGodkingTrialDungeonListComponent comp);

		void OnClosing(ContentUIGodkingTrialDungeonListComponent comp);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDungeonListComponent";

	public static PrefabAuto<ContentUIGodkingTrialDungeonListComponent> PrefabAuto = new PrefabAuto<ContentUIGodkingTrialDungeonListComponent>("Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDungeonListComponent");

	public ContentUIGodkingTrialDungeonGate[] DungeonGates;

	public UILabel LabelDungeonName;

	public UILabel LabelClassRequireNotice;

	public UILabel LabelOpenButton;

	public UIButton ButtonOpen;

	public UIButton ButtonEnter;

	private IListener _listener;

	private GodkingTrialGroupData _data;

	private GodkingTrialDungeonInfo _info;

	private ContentUIGodkingTrialDungeonGate _selectedGate;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_data = null;
		_info = null;
		_selectedGate = null;
	}

	private bool IsOpened()
	{
		if (_info == null)
		{
			return false;
		}
		if (!_info.IsOpened)
		{
			return false;
		}
		return true;
	}

	private bool IsLocked()
	{
		if (GodkingTrialHelper.IsOpenedOther(_data.ChapterIndex))
		{
			return true;
		}
		if (!IsEnoughTicket() && !GodkingTrialHelper.CanRechargeTicket())
		{
			return true;
		}
		return false;
	}

	private bool IsEnoughTicket()
	{
		long godkingTrialKey = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.GodkingTrialKey;
		int num = 0;
		if (godkingTrialKey > num)
		{
			return true;
		}
		return false;
	}

	public void Open(GodkingTrialGroupData data, IListener listener)
	{
		_listener = listener;
		_data = data;
		_info = GlobalAutoBehaviour<AccountManager>.instance.GodkingTrialDungeonInfoManager.Get(data.ChapterIndex);
		UpdateUI();
	}

	public void UpdateUI()
	{
		UpdateGateEnable();
		UpdateGateState();
		UpdateUIInfo();
		UpdateUIRequireClassInfo();
		UpdateButtonState();
	}

	private void UpdateGateEnable()
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(DungeonGates); i < num; i++)
		{
			ContentUIGodkingTrialDungeonGate contentUIGodkingTrialDungeonGate = DungeonGates[i];
			if (!(contentUIGodkingTrialDungeonGate == null))
			{
				if (i == (int)_data.GateType)
				{
					UIHelper.EnableGameObject(contentUIGodkingTrialDungeonGate, enable: true);
					_selectedGate = contentUIGodkingTrialDungeonGate;
				}
				else
				{
					UIHelper.EnableGameObject(contentUIGodkingTrialDungeonGate, enable: false);
				}
			}
		}
	}

	private void UpdateGateState()
	{
		if (_selectedGate == null)
		{
			return;
		}
		if (IsOpened())
		{
			_selectedGate.SetOpened();
			return;
		}
		bool num = _data != null && GlobalAutoBehaviour<AccountManager>.instance.GodkingTrialDungeonInfoManager.ClearedDungeonChapterIndex == _data.ChapterIndex;
		bool flag = IsEnoughTicket() || GodkingTrialHelper.CanRechargeTicket();
		if (num)
		{
			_selectedGate.SetOpened();
			if (flag)
			{
				_selectedGate.CloseGate();
			}
			else
			{
				_selectedGate.SetClosingLock();
			}
			PlayGateSound("UI.GodKing.DungeonClose");
			GlobalAutoBehaviour<AccountManager>.instance.GodkingTrialDungeonInfoManager.ClearedDungeonChapterIndex = 0;
		}
		else if (IsLocked())
		{
			_selectedGate.SetLocked();
		}
		else
		{
			_selectedGate.SetClosed();
		}
	}

	private void UpdateUIInfo()
	{
		if (_data != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(_data.DungeonNamePrefixKey);
			UIHelper.SetLabelText(LabelDungeonName, stringDirect);
		}
	}

	private void UpdateUIRequireClassInfo()
	{
		UIHelper.SetLabelText(LabelClassRequireNotice, LocalizationManager.GetStringDirect("GODKING_TRIAL.CLASS_NOTICE"));
	}

	public void UpdateButtonState()
	{
		bool num = IsOpened();
		bool grayScale = GodkingTrialHelper.IsOpenedOther(_data.ChapterIndex);
		bool enable = num;
		bool num2 = !num;
		UIHelper.EnableGameObject(ButtonEnter, enable);
		if (num2)
		{
			UIHelper.EnableGameObject(ButtonOpen, enable: true);
			UIHelper.SetWidgetGrayScale(ButtonOpen, grayScale);
		}
		else
		{
			UIHelper.EnableGameObject(ButtonOpen, enable: false);
		}
		string stringDirect = LocalizationManager.GetStringDirect(_data.DungeonNamePrefixKey);
		string text = (num2 ? LocalizationManager.GetString(GodkingTrial.DUNGEON_OPEN, stringDirect) : LocalizationManager.GetString(GodkingTrial.DUNGEON_ENTER));
		UIHelper.SetLabelText(LabelOpenButton, text);
		UIHelper.EnableGameObject(LabelOpenButton, enable: false);
		UIHelper.EnableGameObject(LabelOpenButton, enable: true);
	}

	public void LockingGate()
	{
		if (!(_selectedGate == null))
		{
			_selectedGate.LockGate();
		}
	}

	public void UnlockingGate()
	{
		if (!(_selectedGate == null))
		{
			_selectedGate.UnlockGate();
		}
	}

	private void OpenGate()
	{
		if (_selectedGate != null)
		{
			_selectedGate.OpenGate();
			PlayGateSound("UI.GodKing.DungeonOpen");
			if (_listener != null)
			{
				_listener.OnOpening(this);
			}
		}
		UpdateButtonState();
	}

	private void CloseGate()
	{
		if (_selectedGate != null)
		{
			_selectedGate.CloseGate();
			PlayGateSound("UI.GodKing.DungeonClose");
			if (_listener != null)
			{
				_listener.OnClosing(this);
			}
		}
		UpdateButtonState();
	}

	private void ReChargeGate()
	{
		if (_selectedGate != null)
		{
			_selectedGate.UnlockGate();
			if (_listener != null)
			{
				_listener.OnClosing(this);
			}
		}
	}

	private void PlayGateSound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	private void PopupAskOpenGate()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_data.DungeonNamePrefixKey);
		MessagePopup3Button.OpenGlobal(LocalizationManager.GetStringDirect("COMMON_NOTIFICATION"), LocalizationManager.GetString(GodkingTrial.ASK_ENTER_DUNGEON, stringDirect, GodkingTrialHelper.GetRechargeCount()), delegate
		{
			if (IsEnoughTicket())
			{
				SendRqOpenDungeon(_data.ChapterIndex, delegate(GodkingTrialDungeonInfo info)
				{
					if (info != null && info.IsOpened)
					{
						_info = info;
						OpenGate();
					}
				});
			}
			else
			{
				RechargeTicketConfirmPopup();
			}
		}, null);
	}

	public void SendRqOpenDungeon(int chapterIndex, Action<GodkingTrialDungeonInfo> onComplete)
	{
		NShared.OpenGodkingTrialDungeon.Request request = new NShared.OpenGodkingTrialDungeon.Request
		{
			ChapterIndex = chapterIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.OpenGodkingTrialDungeon.Response response)
		{
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else if (response.Result != NShared.OpenGodkingTrialDungeon.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.GodkingTrialDungeonInfoManager.Apply(response.GodkingTrialDungeonInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				Vespa_System_Action_Extension_Method.fire(onComplete, response.GodkingTrialDungeonInfo);
			}
		});
	}

	private void RechargeTicketConfirmPopup()
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Potion);
		List<Tuple<ItemData, int>> staminaPotionList = new List<Tuple<ItemData, int>>();
		foreach (ItemInfo item in itemsByType)
		{
			ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(item.ItemIndex);
			if (data != null && (data.SubData.ActionType == ItemActionType.RecoverStamina || data.SubData.ActionType == ItemActionType.RecoverStaminaPercent) && EnumTools.TryParse<StaminaType>(data.SubData.ActionSubValue, out var o_value) && o_value == StaminaType.GodkingTrialKey)
			{
				staminaPotionList.Add(new Tuple<ItemData, int>(data, item.Count));
			}
		}
		if (staminaPotionList.Count > 0)
		{
			string staminaName = LocalizationManager.GetString(StaminaType.GodkingTrialKey);
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("INVEN_USE_STAMINA", staminaName);
			SelectItemMessagePopup.Open(title, stringDirect, staminaPotionList, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
			{
				UIHelper.ShowItemConfirmPopup(selectItemPopup, staminaPotionList, staminaName, idx, count, 0, delegate
				{
					ReChargeGate();
				});
			}, delegate
			{
				ConsumeCashPopup();
			}, closeOnYes: false);
		}
		else
		{
			ConsumeCashPopup();
		}
	}

	private void ConsumeCashPopup()
	{
		if (!IsEnoughTicket() && !GodkingTrialHelper.CanRechargeTicket())
		{
			MessagePopup.OpenInform("You have already used your weekly Ruby recharge for Trial of the God King. You can recharge again after the weekly reset on Monday (00:00 UTC).");
			return;
		}
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(StaminaType.GodkingTrialKey);
		int num = 0;
		long currencyCount = num;
		if (data != null && Vespa_Generic_IList_Extension_Method.IsValidIndex(data.RechargeGemValue, 0))
		{
			int num2 = int.Parse(data.RechargeGemValue[0]);
			currencyCount = num2;
		}
		Action okAction = delegate
		{
			GodkingTrialHelper.SendRqReset(delegate(NShared.RechargeStamina.ResultType resultType)
			{
				switch (resultType)
				{
				case NShared.RechargeStamina.ResultType.ResetLimitExceeded:
					MessagePopup.OpenInform("You have already used your weekly Ruby recharge for Trial of the God King. You can recharge again after the weekly reset on Monday (00:00 UTC).");
					break;
				case NShared.RechargeStamina.ResultType.MaxStamina:
					MessagePopup.OpenInform("You already have the maximum number of Trial of the God King entries. Use an entry before recharging.");
					break;
				case NShared.RechargeStamina.ResultType.NotEnoughGem:
					MessagePopup.OpenInform("You do not have enough Rubies to recharge a Trial of the God King entry.");
					break;
				default:
					MessagePopup.OpenInform("The Trial of the God King entry could not be recharged. Please try again later.");
					break;
				case NShared.RechargeStamina.ResultType.Success:
					break;
				}
			});
		};
		UIHelper.BuyConfirmPopupOpen(CurrencyType.Gem, currencyCount, delegate
		{
			PopupUIGodkingTrialWarningConsumeCash.OpenGlobal(delegate
			{
				okAction();
			});
		}, okAction);
	}

	public void OnClickDungeonInfo()
	{
		if (_data != null)
		{
			PopupUIGodkingTrialBossInfo.OpenDungeonInfo(_data.ChapterIndex, _data);
		}
	}

	public void OnClickEnter()
	{
		if (_data != null && _info != null)
		{
			SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenGodkingTrialDungeonInfo(_data.ChapterIndex);
		}
	}

	public void OnClickOpen()
	{
		if (_data != null)
		{
			if (GodkingTrialHelper.IsOpenedOther(_data.ChapterIndex))
			{
				CenterMessage.Open(LocalizationManager.GetString(GodkingTrial.OTHER_DUNGEON_OPENED));
			}
			else if (!IsEnoughTicket() && !GodkingTrialHelper.CanRechargeTicket())
			{
				RechargeTicketConfirmPopup();
			}
			else
			{
				PopupAskOpenGate();
			}
		}
	}
}
