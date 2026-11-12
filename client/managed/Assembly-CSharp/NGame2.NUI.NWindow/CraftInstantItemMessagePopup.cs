using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CraftInstantItemMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/CraftInstantItemMessagePopup";

	public UIButton Button_Yes;

	public UIBundleLoader IconTexture;

	public UILabel LabelResultItemCount;

	public UILabel LabelItemName;

	public UILabel LabelItemDesc;

	public UILabel LabelRemainTime;

	public UIClipSprite SpriteRemainTime;

	public UILabel LabelGemCost;

	public UIGame3DWidget EffectGauge;

	public UIWidget WidgetRemainTimeFrame;

	private CraftSlotInfo _info;

	private Action _onYes;

	private Action _onCancel;

	private bool isFinished;

	private int finishGem;

	public static void Open(CraftSlotInfo slotInfo, Action onYes, Action onCancel = null)
	{
		WindowPrefab<CraftInstantItemMessagePopup> windowPrefab = new WindowPrefab<CraftInstantItemMessagePopup>("Resources/Prefab2/Window/MessagePopup/CraftInstantItemMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(slotInfo, onYes, onCancel);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(CraftSlotInfo slotInfo, Action onYes, Action onCancel)
	{
		base.Open();
		_onYes = onYes;
		_onCancel = onCancel;
		_info = slotInfo;
		isFinished = false;
		finishGem = 0;
		ClearMaterial();
		SetupUI();
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		SetupFinished(out var completeTime);
		DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
		TimeSpan remainSec = completeTime - uTCNow;
		int num = (int)remainSec.TotalSeconds;
		CraftItemData data = GameRoot.Instance.CraftItemDataContainer.GetData(_info.CraftIndex);
		if (data != null)
		{
			UIHelper.SetLabelText(LabelResultItemCount, data.ResultItemCount.ToString());
			int num2 = ((data.ReqTime == 0) ? 1 : data.ReqTime);
			float ratio = Mathf.Clamp01((float)(num2 - num) / (float)num2);
			SetDoCraftUI(ratio, remainSec);
		}
	}

	public void OnClickYes()
	{
		isFinished = true;
		finishGem = GetFinishGem();
		Action onYes = _onYes;
		Close();
		Vespa_System_Action_Extension_Method.fire(onYes);
	}

	public void OnClickCancel()
	{
		Action onCancel = _onCancel;
		Close();
		Vespa_System_Action_Extension_Method.fire(onCancel);
	}

	private void ClearMaterial()
	{
		UIHelper.SetLabelText(LabelRemainTime, string.Empty);
		UIHelper.SetLabelText(LabelResultItemCount, string.Empty);
		UIHelper.SetLabelText(LabelItemName, string.Empty);
		UIHelper.SetLabelText(LabelItemDesc, string.Empty);
		UIHelper.SetLabelText(LabelGemCost, string.Empty);
	}

	private void SetupUI()
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(_info.ItemIndex);
		if (data != null)
		{
			if (IconTexture != null)
			{
				IconTexture.AssetPath = UIHelper.GetItemIconPath(_info.ItemIndex);
			}
			UIHelper.SetLabelText(LabelItemName, data.Name);
			UIHelper.SetLabelText(LabelItemDesc, data.Description2);
			EffectGauge.Play(null);
		}
	}

	private void SetDoCraftUI(float ratio, TimeSpan remainSec)
	{
		string text = "";
		int num = (int)remainSec.TotalHours;
		_ = remainSec.TotalSeconds;
		UIHelper.SetLabelText(text: (num >= 100) ? $"{num:D3}:{remainSec.Minutes:D2}:{remainSec.Seconds:D2}" : $"{num:D2}:{remainSec.Minutes:D2}:{remainSec.Seconds:D2}", label: LabelRemainTime);
		SpriteRemainTime.ClipFillAmount = ratio;
		if (WidgetRemainTimeFrame != null)
		{
			float x = WidgetRemainTimeFrame.worldCorners[0].x;
			float x2 = WidgetRemainTimeFrame.worldCorners[2].x;
			EffectGauge.transform.position = Vespa_Unity_Vector3_Extension_Method.ReplaceX(EffectGauge.transform.position, Mathf.Lerp(x, x2, ratio));
		}
		if (!isFinished)
		{
			int num2 = GetFinishGem();
			UIHelper.SetLabelText(LabelGemCost, num2.ToString());
		}
		else
		{
			UIHelper.SetLabelText(LabelGemCost, finishGem.ToString());
		}
	}

	public bool SetupFinished(out DateTime completeTime)
	{
		completeTime = default(DateTime);
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(_info.CompleteTime, out completeTime))
		{
			return false;
		}
		return true;
	}

	private int GetFinishGem()
	{
		SetupFinished(out var completeTime);
		DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
		int remainTime = (int)(completeTime - uTCNow).TotalSeconds;
		return GameRoot.Instance.CraftInstantGemDataContainer.GetReqGem(remainTime);
	}
}
