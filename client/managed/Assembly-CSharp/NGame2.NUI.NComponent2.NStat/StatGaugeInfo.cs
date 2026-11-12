using System;
using System.Collections;
using NShared;
using NVespa.NGlobal;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class StatGaugeInfo : MonoBehaviour
{
	public enum StateType
	{
		None,
		Selected,
		Disabled
	}

	[Serializable]
	public class StateRootInfo
	{
		public StateType Type;

		public Transform Root;
	}

	public UISprite GaugeSprite;

	public UILabel NameLabel;

	public UILabel CurrentLabel;

	public UILabel MaxLabel;

	public UILabel CenterLabel;

	public StateRootInfo[] StateRoot;

	public UILabel Label_Splitter;

	public UILabel SingleNameLabel;

	public Color NameColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color CurrentColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color GaugeColor = new Color32(120, 100, 80, byte.MaxValue);

	public Color AnimatingGaugeColor = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public Color MaxColor = new Color32(byte.MaxValue, 165, 0, byte.MaxValue);

	public Color MaxCenterColor = new Color32(0, byte.MaxValue, 0, byte.MaxValue);

	private Action _onClickSelect;

	public Enum StatType { get; private set; }

	public int Current { get; private set; }

	public int Max { get; private set; }

	public int CurRatio { get; private set; }

	public int MaxRatio { get; private set; }

	public void Open(Enum statType, int current, int max)
	{
		Open(statType, current, max, current, max, null);
	}

	public void Open(Enum statType, int current, int max, int curRatio, int maxRatio, Action onClickSelect)
	{
		StatType = statType;
		Current = current;
		Max = max;
		CurRatio = curRatio;
		MaxRatio = maxRatio;
		_onClickSelect = onClickSelect;
		Refresh();
		RestoredPortal.WorkshopOptionDrag(this);
	}

	public void OpenEmpty(string empty)
	{
		UIHelper.EnableGameObject(NameLabel, enable: true);
		UIHelper.SetLabelText(NameLabel, empty);
		UIHelper.SetWidgetColor(NameLabel, NameColor);
		UIHelper.EnableGameObject(CurrentLabel, enable: false);
		UIHelper.EnableGameObject(MaxLabel, enable: false);
		UIHelper.EnableGameObject(GaugeSprite, enable: false);
		UIHelper.EnableGameObject(Label_Splitter, enable: false);
		UIHelper.EnableGameObject(CenterLabel, enable: false);
		UIHelper.EnableGameObject(SingleNameLabel, enable: false);
	}

	public void OpenExtraSkill(string text, Action onClickSelect = null)
	{
		_onClickSelect = onClickSelect;
		UIHelper.EnableGameObject(SingleNameLabel, enable: true);
		UIHelper.SetLabelText(SingleNameLabel, text);
		UIHelper.SetWidgetColor(SingleNameLabel, NameColor);
		UIHelper.EnableGameObject(CurrentLabel, enable: false);
		UIHelper.EnableGameObject(MaxLabel, enable: false);
		UIHelper.EnableGameObject(GaugeSprite, enable: false);
		UIHelper.EnableGameObject(Label_Splitter, enable: false);
		UIHelper.EnableGameObject(CenterLabel, enable: false);
		UIHelper.EnableGameObject(NameLabel, enable: false);
		RestoredPortal.WorkshopOptionDrag(this);
	}

	public void SetSplitterLabel(string splitter)
	{
		UIHelper.SetLabelText(Label_Splitter, splitter);
	}

	public void OnClickSelect()
	{
		Vespa_System_Action_Extension_Method.fire(_onClickSelect);
	}

	public void SelectRoot(StateType stateType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StateRoot); i++)
		{
			UIHelper.EnableGameObject(StateRoot[i].Root, StateRoot[i].Type == stateType);
		}
	}

	private void Refresh()
	{
		string text = LocalizationManager.GetString(StatType);
		string statDisplayValue = StatData.GetStatDisplayValue(StatType, Current);
		string statDisplayValue2 = StatData.GetStatDisplayValue(StatType, Max);
		bool num = CurRatio >= MaxRatio;
		UIHelper.SetSpriteFillAmount(fillAmount: (MaxRatio > 0) ? ((float)CurRatio / (float)MaxRatio) : 0f, sprite: GaugeSprite);
		UIHelper.SetWidgetColor(GaugeSprite, GaugeColor);
		UIHelper.EnableGameObject(NameLabel, enable: true);
		UIHelper.SetLabelText(NameLabel, text);
		UIHelper.SetWidgetColor(NameLabel, NameColor);
		UIHelper.SetLabelText(CurrentLabel, statDisplayValue);
		UIHelper.SetWidgetColor(NameLabel, CurrentColor);
		UIHelper.SetLabelText(MaxLabel, statDisplayValue2);
		UIHelper.SetWidgetColor(NameLabel, MaxColor);
		if (num)
		{
			UIHelper.SetLabelText(CenterLabel, "MAX");
			UIHelper.SetWidgetColor(CenterLabel, MaxCenterColor);
		}
	}

	public ITask CreateGaugeAnimation(int newCurrent, int newRatio)
	{
		if (newRatio == CurRatio)
		{
			return null;
		}
		return new DelegateTask(coAnimateGaugeTo(newCurrent, newRatio));
	}

	private IEnumerator coAnimateGaugeTo(int newCurrent, int newRatio)
	{
		float startTime = Time.time;
		float duration = 0.5f;
		float initGaugeRatio = (float)CurRatio / (float)MaxRatio;
		float finalGaugeRatio = (float)newRatio / (float)MaxRatio;
		int num = newCurrent - Current;
		string statDisplayValue = StatData.GetStatDisplayValue(StatType, num);
		UIHelper.SetLabelText(CenterLabel, statDisplayValue);
		UIHelper.SetWidgetColor(CenterLabel, CurrentColor);
		float num2;
		do
		{
			yield return null;
			num2 = Time.time - startTime;
			float t = Mathf.Clamp01(num2 / duration);
			float fillAmount = Mathf.Lerp(initGaugeRatio, finalGaugeRatio, t);
			Color color = Color.Lerp(AnimatingGaugeColor, GaugeColor, t);
			UIHelper.SetSpriteFillAmount(GaugeSprite, fillAmount);
			UIHelper.SetWidgetColor(GaugeSprite, color);
		}
		while (!(num2 > duration));
		Current = newCurrent;
		CurRatio = newRatio;
		Refresh();
		if (newRatio < MaxRatio)
		{
			float postStartTime = Time.time;
			float postDuration = 0.5f;
			while (Time.time <= postStartTime + postDuration)
			{
				yield return null;
				float t2 = Mathf.Clamp01((Time.time - postStartTime) / postDuration);
				Color color2 = Color.Lerp(CurrentColor, new Color(0f, 0f, 0f, 0f), t2);
				UIHelper.SetWidgetColor(CenterLabel, color2);
			}
		}
	}
}
