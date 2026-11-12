using System;
using NGame2.NOrdeal;
using NGame2.NUI.NHelper.NPet;
using NShared;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class IncubatorSlotStatePanel : MonoBehaviour, IIncubatorManagePanel
{
	public interface IListener
	{
		void OnClickRewardInfo();

		void OnClickChangeIncubator();
	}

	public UIButton Button_ChangeIncubator;

	public UIButton Button_Reward;

	[Header("[Incubator Manual Group]")]
	public UIWidget Widget_ManualRoot;

	[Header("[Before Incubating Group]")]
	public UIWidget Widget_BeforeIncubatingRoot;

	public UILabel Label_SelectEggName;

	public UIWidget Widget_BasicGaugeRoot;

	public UILabel Label_BasicTime;

	public UIWidget Widget_CompareGaugeRoot;

	public UILabel Label_BeforeTime;

	public UILabel Label_TotalTime;

	[Header("[After Incubating Group]")]
	public UIWidget Widget_AfterIncubatingRoot;

	public UILabel Label_IncubatingEggName;

	public UIWidget Widget_IncubatingRoot;

	public UIClipSprite GaugeFillAmount;

	public UILabel Label_RemainTime;

	public UIWidget Widget_CompleteRoot;

	private IIncubatorDataProvider _dataProvider;

	private IListener _listener;

	public void Open(IIncubatorDataProvider dataProvider, IListener listener)
	{
		_dataProvider = dataProvider;
		_listener = listener;
		SetIncubatorStateUI();
	}

	public void SetIncubatorStateUI()
	{
		if (_dataProvider != null)
		{
			EIncubatingState incubatingState = _dataProvider.GetIncubatingState();
			bool valueBool = GameRoot.Instance.ConstantDataContainer.GetValueBool("PetIncubatorReplacementBtn");
			UIHelper.EnableGameObject(Button_ChangeIncubator, valueBool);
			UIHelper.EnableGameObject(Button_Reward, incubatingState != EIncubatingState.EmptyEgg);
			switch (incubatingState)
			{
			case EIncubatingState.EmptyEgg:
				UIHelper.EnableGameObject(Widget_ManualRoot, enable: true);
				UIHelper.EnableGameObject(Widget_BeforeIncubatingRoot, enable: false);
				UIHelper.EnableGameObject(Widget_AfterIncubatingRoot, enable: false);
				break;
			case EIncubatingState.SelectEgg:
				UIHelper.EnableGameObject(Widget_ManualRoot, enable: false);
				UIHelper.EnableGameObject(Widget_BeforeIncubatingRoot, enable: true);
				UIHelper.EnableGameObject(Widget_AfterIncubatingRoot, enable: false);
				SetSelectEggState();
				break;
			case EIncubatingState.Incubating:
				UIHelper.EnableGameObject(Widget_ManualRoot, enable: false);
				UIHelper.EnableGameObject(Widget_BeforeIncubatingRoot, enable: false);
				UIHelper.EnableGameObject(Widget_AfterIncubatingRoot, enable: true);
				UIHelper.EnableGameObject(Widget_IncubatingRoot, enable: true);
				UIHelper.EnableGameObject(Widget_CompleteRoot, enable: false);
				SetIncubatingState();
				break;
			case EIncubatingState.Complete:
				UIHelper.EnableGameObject(Widget_ManualRoot, enable: false);
				UIHelper.EnableGameObject(Widget_BeforeIncubatingRoot, enable: false);
				UIHelper.EnableGameObject(Widget_AfterIncubatingRoot, enable: true);
				UIHelper.EnableGameObject(Widget_IncubatingRoot, enable: false);
				UIHelper.EnableGameObject(Widget_CompleteRoot, enable: true);
				SetCompleteState();
				break;
			}
		}
	}

	private void SetSelectEggState()
	{
		PetIncubatorSlotInfo selectedSlotInfo = _dataProvider.GetSelectedSlotInfo();
		int selectedEggIndex = _dataProvider.GetSelectedEggIndex();
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(selectedEggIndex);
		UIHelper.SetLabelText(text: GetItemColorName(data.Name, data.Grade), label: Label_SelectEggName);
		int eggBasicReqTimeSec = PetHelper.GetEggBasicReqTimeSec(selectedEggIndex);
		TimeSpan ts = new TimeSpan(0, 0, eggBasicReqTimeSec);
		int eggTotalReqTimeSec = PetHelper.GetEggTotalReqTimeSec(selectedSlotInfo, selectedEggIndex);
		TimeSpan ts2 = new TimeSpan(0, 0, eggTotalReqTimeSec);
		bool flag = eggBasicReqTimeSec == eggTotalReqTimeSec;
		UIHelper.EnableGameObject(Widget_BasicGaugeRoot, flag);
		UIHelper.EnableGameObject(Widget_CompareGaugeRoot, !flag);
		if (flag)
		{
			UIHelper.SetLabelText(Label_BasicTime, OrdealHelper.GetRemainTimeText(ts));
			return;
		}
		UIHelper.SetLabelText(Label_BeforeTime, OrdealHelper.GetRemainTimeText(ts));
		UIHelper.SetLabelText(Label_TotalTime, OrdealHelper.GetRemainTimeText(ts2));
	}

	private void SetIncubatingState()
	{
		PetIncubatorSlotInfo selectedSlotInfo = _dataProvider.GetSelectedSlotInfo();
		SetIncubatingEggLabel();
		TimeSpan? incubatorRemainTime = PetHelper.GetIncubatorRemainTime(selectedSlotInfo);
		UIHelper.SetLabelText(Label_RemainTime, OrdealHelper.GetRemainTimeText(incubatorRemainTime.Value));
		float incubatorRemainTimePercent = PetHelper.GetIncubatorRemainTimePercent(selectedSlotInfo);
		UIHelper.SetSpriteFillAmount(GaugeFillAmount, incubatorRemainTimePercent);
	}

	private void SetCompleteState()
	{
		SetIncubatingEggLabel();
	}

	private void SetIncubatingEggLabel()
	{
		int setItemIndex = _dataProvider.GetSelectedSlotInfo().SetItemIndex;
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(setItemIndex);
		string itemColorName = GetItemColorName(data.Name, data.Grade);
		UIHelper.SetLabelText(Label_IncubatingEggName, itemColorName);
	}

	private string GetItemColorName(string itemName, int grade)
	{
		ItemGradeData data = GameRoot.Instance.ItemGradeDataContainer.GetData(grade);
		string text = "FFFFFF";
		if (data != null)
		{
			text = data.NameColor;
		}
		if (!string.IsNullOrEmpty(text))
		{
			itemName = $"[{text}]{itemName}";
		}
		return itemName;
	}

	public void Close()
	{
		_dataProvider = null;
		_listener = null;
	}

	public void OnClickReward()
	{
		if (_listener != null)
		{
			_listener.OnClickRewardInfo();
		}
	}

	public void OnClickChangeIncubator()
	{
		if (_listener != null)
		{
			_listener.OnClickChangeIncubator();
		}
	}
}
