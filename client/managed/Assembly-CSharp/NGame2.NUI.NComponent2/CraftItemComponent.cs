using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CraftItemComponent : UIScrollItemBehaviour
{
	public enum ECraftState
	{
		None,
		CanOpen,
		CanCraft,
		DoCraft,
		Closed
	}

	public enum ECraftFinished
	{
		None,
		NotFinished,
		Finished
	}

	public interface Listener
	{
		void OnAddSlot(int index);

		void OnCraftItem(int index);

		void OnCancelCraft(int index);

		void OnFinishCraft(CraftItemComponent comp);

		void OnGetItem(int index);
	}

	public const string PrefabPath = "Prefab2/Component/Inventory/CraftItemComponent";

	public static PrefabAuto<CraftItemComponent> PrefabAuto = new PrefabAuto<CraftItemComponent>("Prefab2/Component/Inventory/CraftItemComponent");

	public Transform WidgetCanOpen;

	public Transform WidgetCanCraft;

	public Transform WidgetDoCraft;

	public Transform WidgetClosed;

	public Transform CraftRoot;

	public Transform GetRoot;

	public UILabel LabelGemForOpen;

	public UIBundleLoader IconTexture;

	public UILabel LabelResultItemCount;

	public UILabel LabelRemainTime;

	public UILabel LabelItemName;

	public UIClipSprite SpriteRemainTime;

	public UILabel LabelGemCost;

	public UIGame3DWidget EffectGauge;

	public UIGame3DWidget EffectGet;

	public UIWidget WidgetRemainTimeBg;

	public UIWidget WidgetRemainTimeFrame;

	public UIWidget WidgetCompletedBg;

	private bool bBlockUpdate;

	private Listener _listener;

	private ECraftState _state;

	private ECraftFinished _finished;

	private CraftSlotInfo _info;

	public float FillAnimationDuration = 1f;

	public int _index { get; private set; }

	public int _remainTime { get; private set; }

	public void Open(Listener listener, int index)
	{
		_listener = listener;
		_index = index;
		_info = GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Get(index);
		_finished = ECraftFinished.None;
		bBlockUpdate = false;
		SetupState();
		SetupUI();
	}

	private int GetSlotOpenGold()
	{
		string key = "CraftSlotIndexOpen" + _index;
		return GameRoot.Instance.ConstantDataContainer.GetValueInt(key);
	}

	private void SetupUI()
	{
		UIHelper.EnableGameObject(WidgetCanCraft, _state == ECraftState.CanCraft);
		UIHelper.EnableGameObject(WidgetCanOpen, _state == ECraftState.CanOpen);
		UIHelper.EnableGameObject(WidgetDoCraft, _state == ECraftState.DoCraft);
		UIHelper.EnableGameObject(WidgetClosed, _state == ECraftState.Closed);
		if (_state == ECraftState.CanOpen)
		{
			_ = "CraftSlotIndexOpen" + _index;
			int slotOpenGold = GetSlotOpenGold();
			long i_value = slotOpenGold;
			UIHelper.SetLabelText(LabelGemForOpen, Vespa_Int_Extension_Method.ToCommaString(i_value));
		}
		else if (_state == ECraftState.DoCraft)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(_info.ItemIndex);
			if (data == null)
			{
				return;
			}
			if (IconTexture != null)
			{
				IconTexture.AssetPath = UIHelper.GetItemIconPath(_info.ItemIndex);
			}
			UIHelper.SetLabelText(LabelItemName, data.Name);
		}
		if (_state != ECraftState.DoCraft)
		{
			EffectGauge.Stop(immediatelyClear: true);
			EffectGet.Stop(immediatelyClear: true);
		}
	}

	private void SetupState()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("CraftSlotCountMin");
		if (_info != null)
		{
			if (_info.ItemIndex != 0)
			{
				_state = ECraftState.DoCraft;
			}
			else
			{
				_state = ECraftState.CanCraft;
			}
		}
		else if (_index <= valueInt)
		{
			_state = ECraftState.CanCraft;
		}
		else if (_index - 1 == valueInt)
		{
			_state = ECraftState.CanOpen;
		}
		else if (GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Get(_index - 1) != null)
		{
			_state = ECraftState.CanOpen;
		}
		else
		{
			_state = ECraftState.Closed;
		}
	}

	private int GetFinishGem()
	{
		int remainTime = GetRemainTime();
		return GameRoot.Instance.CraftInstantGemDataContainer.GetReqGem(remainTime);
	}

	private int GetRemainTime()
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(_info.CompleteTime, out var o_dateTime))
		{
			return 0;
		}
		return (int)(o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow()).TotalSeconds;
	}

	private bool SetupFinished(out DateTime completeTime)
	{
		completeTime = default(DateTime);
		if (_state != ECraftState.DoCraft || _info == null)
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(_info.CompleteTime, out completeTime))
		{
			return false;
		}
		ECraftFinished eCraftFinished = ECraftFinished.None;
		eCraftFinished = ((completeTime >= NGame2.NUI.NWindow.Common.GetUTCNow()) ? ECraftFinished.NotFinished : ECraftFinished.Finished);
		if (_finished != eCraftFinished)
		{
			_finished = eCraftFinished;
			return true;
		}
		_finished = eCraftFinished;
		return false;
	}

	private IEnumerator coFillAnimation(Action onFinish)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(_info.CompleteTime, out var o_dateTime))
		{
			Vespa_System_Action_Extension_Method.fire(onFinish);
			yield break;
		}
		int remainSec = (int)(o_dateTime - NGame2.NUI.NWindow.Common.GetUTCNow()).TotalSeconds;
		CraftItemData data = GameRoot.Instance.CraftItemDataContainer.GetData(_info.CraftIndex);
		if (data == null)
		{
			Vespa_System_Action_Extension_Method.fire(onFinish);
			yield break;
		}
		int num = ((data.ReqTime == 0) ? 1 : data.ReqTime);
		float startRatio = Mathf.Clamp01((float)(num - remainSec) / (float)num);
		float ElapsedTime = 0f;
		if (FillAnimationDuration == 0f)
		{
			Vespa_System_Action_Extension_Method.fire(onFinish);
			yield break;
		}
		while (ElapsedTime < FillAnimationDuration)
		{
			ElapsedTime += Time.deltaTime;
			float ratio = Mathf.Lerp(startRatio, 1f, ElapsedTime / FillAnimationDuration);
			float num2 = Mathf.Lerp(remainSec, 0f, ElapsedTime / FillAnimationDuration);
			SetDoCraftUI(ratio, new TimeSpan(0, 0, (int)num2));
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onFinish);
	}

	public void ShowFillAnimation()
	{
		if (_state == ECraftState.DoCraft && _info != null && _finished != ECraftFinished.Finished)
		{
			bBlockUpdate = true;
			VespaCoroutine.StartScene(coFillAnimation(delegate
			{
				bBlockUpdate = false;
				_info = GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Get(_index);
				SetupState();
				SetupUI();
			}));
		}
	}

	private void SetDoCraftUI(float ratio, TimeSpan remainSec)
	{
		SpriteRemainTime.ClipFillAmount = ratio;
		string text = "";
		int num = (int)remainSec.TotalHours;
		int remainTime = (int)remainSec.TotalSeconds;
		UIHelper.SetLabelText(text: (num >= 100) ? $"{num:D3}:{remainSec.Minutes:D2}:{remainSec.Seconds:D2}" : $"{num:D2}:{remainSec.Minutes:D2}:{remainSec.Seconds:D2}", label: LabelRemainTime);
		int reqGem = GameRoot.Instance.CraftInstantGemDataContainer.GetReqGem(remainTime);
		UIHelper.SetLabelText(LabelGemCost, reqGem.ToString());
		if (WidgetRemainTimeFrame != null)
		{
			float x = WidgetRemainTimeFrame.worldCorners[0].x;
			float x2 = WidgetRemainTimeFrame.worldCorners[2].x;
			EffectGauge.transform.position = Vespa_Unity_Vector3_Extension_Method.ReplaceX(EffectGauge.transform.position, Mathf.Lerp(x, x2, ratio));
		}
	}

	private bool CheckRefundCraft()
	{
		CraftItemData data = GameRoot.Instance.CraftItemDataContainer.GetData(_info.CraftIndex);
		if (data == null || !data.IsRefund)
		{
			return false;
		}
		ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(_info.ItemIndex);
		if (data2 == null)
		{
			return false;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string content = LocalizationManager.GetString(Inventory.INVENTORY_CRAFT_EVENT_PERIOD_END_MSG, data2.Name);
		string yes = LocalizationManager.GetString(Inventory.INVENTORY_CRAFT_EVENT_PERIOD_END_BUTTON);
		int priceValue = _info.ItemCount * data.ReqGold;
		PreserveMessagePopup2.Open(title, content, yes, PreserveMessagePopup2.EPriceType.Gold, priceValue, delegate
		{
			if (_listener != null)
			{
				_listener.OnCancelCraft(_index);
			}
		});
		return true;
	}

	private void Update()
	{
		if (bBlockUpdate || _state != ECraftState.DoCraft || _info == null)
		{
			return;
		}
		DateTime completeTime;
		bool num = SetupFinished(out completeTime);
		UIHelper.EnableGameObject(CraftRoot, _finished == ECraftFinished.NotFinished);
		UIHelper.EnableGameObject(GetRoot, _finished == ECraftFinished.Finished);
		CraftItemData data = GameRoot.Instance.CraftItemDataContainer.GetData(_info.CraftIndex);
		if (data != null)
		{
			UIHelper.SetLabelText(text: (data.ResultItemCount * ((_info.ItemCount == 0) ? 1 : _info.ItemCount)).ToString(), label: LabelResultItemCount);
		}
		if (_finished == ECraftFinished.Finished)
		{
			UIHelper.EnableGameObject(WidgetRemainTimeBg, enable: false);
			UIHelper.EnableGameObject(WidgetRemainTimeFrame, enable: false);
			UIHelper.EnableGameObject(WidgetCompletedBg, enable: true);
		}
		else
		{
			TimeSpan remainSec = completeTime - NGame2.NUI.NWindow.Common.GetUTCNow();
			int num2 = (int)remainSec.TotalSeconds;
			int num3 = ((data.ReqTime == 0) ? 1 : data.ReqTime);
			float ratio = Mathf.Clamp01((float)(num3 - num2) / (float)num3);
			UIHelper.EnableGameObject(WidgetRemainTimeBg, enable: true);
			UIHelper.EnableGameObject(WidgetRemainTimeFrame, enable: true);
			UIHelper.EnableGameObject(WidgetCompletedBg, enable: false);
			SetDoCraftUI(ratio, remainSec);
		}
		if (num)
		{
			if (_finished == ECraftFinished.NotFinished)
			{
				EffectGauge.Play(null);
				EffectGet.Stop(immediatelyClear: true);
			}
			else if (_finished == ECraftFinished.Finished)
			{
				EffectGauge.Stop(immediatelyClear: true);
				EffectGet.Play(null);
			}
		}
	}

	public void OnChanged(CraftSlotInfo slotInfo)
	{
		if (slotInfo != null && !bBlockUpdate)
		{
			_info = GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Get(_index);
			SetupState();
			SetupUI();
		}
	}

	public void OnClickAddSlot()
	{
		if (bBlockUpdate)
		{
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string content = LocalizationManager.GetString(Inventory.INVENTORY_ADD_CRAFT_SLOT_MSG);
		string yes = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		PreserveMessagePopup2.Open(title, content, yes, PreserveMessagePopup2.EPriceType.Gold, GetSlotOpenGold(), delegate
		{
			if (_listener != null)
			{
				_listener.OnAddSlot(_index);
			}
		});
	}

	public void OnClickCraftItem()
	{
		if (!bBlockUpdate && _listener != null)
		{
			_listener.OnCraftItem(_index);
		}
	}

	public void OnClickCancelCraft()
	{
		if (bBlockUpdate || CheckRefundCraft())
		{
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Inventory.INVENTORY_CANCEL_CRAFT_MSG);
		MessagePopup.Open(title, message, delegate
		{
			if (_listener != null)
			{
				_listener.OnCancelCraft(_index);
			}
		}, null);
	}

	public void OnClickFinishCraft()
	{
		if (bBlockUpdate || CheckRefundCraft())
		{
			return;
		}
		LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		LocalizationManager.GetString(Inventory.INVENTORY_FINISH_CRAFT_MSG);
		LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		CraftInstantItemMessagePopup.Open(_info, delegate
		{
			if (_listener != null)
			{
				_remainTime = GetRemainTime();
				_listener.OnFinishCraft(this);
			}
		});
	}

	public void OnClickGetItem()
	{
		if (!bBlockUpdate && !CheckRefundCraft() && _listener != null)
		{
			_listener.OnGetItem(_index);
		}
	}

	public bool CanCraftState()
	{
		return _state == ECraftState.CanCraft;
	}
}
