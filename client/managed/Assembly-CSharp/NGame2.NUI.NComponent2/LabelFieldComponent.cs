using System;
using System.Collections.Generic;
using NShared;
using NShared.NJit;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class LabelFieldComponent : UIScrollItemBehaviour
{
	public class FieldButtonInfo
	{
		public Action Action { get; private set; }

		public string Localize { get; private set; }

		public bool ButtonState { get; private set; }

		public FieldButtonInfo(Action action, string localize, bool buttonState)
		{
			Action = action;
			Localize = localize;
			ButtonState = buttonState;
		}
	}

	public const string PrefabPath = "Prefab2/Component/LabelFieldComponent";

	public const string PrefabPathInspect = "Prefab2/Component/LabelFieldComponent2";

	public const string PrefabPath3 = "Prefab2/Component/LabelFieldComponent3";

	public const string PrefabPath4 = "Prefab2/Component/LabelFieldComponent4";

	public const string PerfabPathEx = "Prefab2/Component/LabelFieldComponentEx";

	public const string PrefabPathPet = "Prefab2/Component/PetLabelFieldComponent";

	public static PrefabAuto<LabelFieldComponent> PrefabAuto = new PrefabAuto<LabelFieldComponent>("Prefab2/Component/LabelFieldComponent");

	public static PrefabAuto<LabelFieldComponent> PrefabAutoInspect = new PrefabAuto<LabelFieldComponent>("Prefab2/Component/LabelFieldComponent2");

	public static PrefabAuto<LabelFieldComponent> PrefabAuto3 = new PrefabAuto<LabelFieldComponent>("Prefab2/Component/LabelFieldComponent3");

	public static PrefabAuto<LabelFieldComponent> PrefabAuto4 = new PrefabAuto<LabelFieldComponent>("Prefab2/Component/LabelFieldComponent4");

	public static PrefabAuto<LabelFieldComponent> PrefabAutoEx = new PrefabAuto<LabelFieldComponent>("Prefab2/Component/LabelFieldComponentEx");

	public static PrefabAuto<LabelFieldComponent> PrefabAutoPet = new PrefabAuto<LabelFieldComponent>("Prefab2/Component/PetLabelFieldComponent");

	public Color ValueNormalColor = Color.black;

	public Color ValueEnhancedColor = Color.black;

	public Color ValuePlusColor = Color.black;

	public UIWidget RootWidget;

	public UILabel FieldLabel;

	public UILabel ValueLabel;

	public UILabel ExtraLabel;

	public UIButton FieldButton;

	public UILabel FieldButtonNameLabel;

	private FieldButtonInfo _fieldButtonInfo;

	public void Open(string field, string value, string extra = null, FieldButtonInfo fieldButtonInfo = null)
	{
		_fieldButtonInfo = fieldButtonInfo;
		Open(field, value, ValueNormalColor, extra);
	}

	public void Open(string field, string value, Color color, string extra = null)
	{
		UIHelper.SetLabelText(FieldLabel, field);
		UIHelper.SetLabelText(ValueLabel, Vespa_String_Extension_Method.NguiColor(value, color));
		if (!string.IsNullOrEmpty(extra))
		{
			UIHelper.EnableGameObject(ExtraLabel, enable: true);
			UIHelper.SetLabelText(ExtraLabel, extra);
		}
		else
		{
			UIHelper.EnableGameObject(ExtraLabel, enable: false);
		}
		SetFieldButtonInfo();
	}

	public void Open(StatType statType, long totalValue, long addValue)
	{
		Open(statType, totalValue, (addValue == 0L) ? null : new long[1] { addValue }, GameRoot.Instance.StatAttenuationDataContainer);
	}

	public void OpenMultedStat(StatType statType, long totalValue, long addValue)
	{
		Open(statType, totalValue, (addValue == 0L) ? null : new long[1] { addValue }, GameRoot.Instance.MultedStatAttenuationDataContainer);
	}

	public void OpenAddedStat(StatType statType, long totalValue, long addValue)
	{
		Open(statType, totalValue, (addValue == 0L) ? null : new long[1] { addValue }, GameRoot.Instance.AddedStatAttenuationDataContainer);
	}

	public void Open(StatType statType, long value)
	{
		string field = LocalizationManager.GetString(statType);
		string statDisplayValue = StatData.GetStatDisplayValue(statType, value, showSign: false);
		StatAttenuationData data = GameRoot.Instance.StatAttenuationDataContainer.GetData(statType);
		if (data != null)
		{
			long optionValue = data.Attenuate(value);
			string statDisplayValue2 = StatData.GetStatDisplayValue(isPercentStat: true, optionValue, showSign: false);
			Open(field, statDisplayValue, statDisplayValue2);
		}
		else
		{
			Open(field, statDisplayValue);
		}
	}

	public void Open(StatType statType, long totalValue, IList<long> addValues, JitDataContainerDefault<StatType, StatAttenuationData> statAttenuationDataContainer)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(addValues))
		{
			Open(statType, totalValue);
			return;
		}
		string empty = string.Empty;
		long num = 0L;
		for (int i = 0; i < addValues.Count; i++)
		{
			num += addValues[i];
		}
		long optionValue = totalValue - num;
		string text = string.Empty;
		for (int j = 0; j < addValues.Count; j++)
		{
			string statDisplayValue = StatData.GetStatDisplayValue(statType, addValues[j]);
			text += statDisplayValue;
		}
		string statDisplayValue2 = StatData.GetStatDisplayValue(statType, totalValue, showSign: false);
		string statDisplayValue3 = StatData.GetStatDisplayValue(statType, optionValue, showSign: false);
		empty = $"{Vespa_String_Extension_Method.NguiColor(statDisplayValue2, ValueEnhancedColor)} ({statDisplayValue3}{Vespa_String_Extension_Method.NguiColor(text, ValuePlusColor)})";
		StatAttenuationData statAttenuationData = statAttenuationDataContainer?.GetData(statType);
		if (statAttenuationData != null)
		{
			long optionValue2 = statAttenuationData.Attenuate(totalValue);
			string statDisplayValue4 = StatData.GetStatDisplayValue(isPercentStat: true, optionValue2, showSign: false);
			Open(LocalizationManager.GetString(statType), empty, statDisplayValue4);
		}
		else
		{
			Open(LocalizationManager.GetString(statType), empty);
		}
		SetFieldButtonInfo();
	}

	protected void SetFieldButtonInfo()
	{
		UIHelper.EnableGameObject(FieldButton, _fieldButtonInfo != null);
		if (_fieldButtonInfo != null)
		{
			UIHelper.EnableButton(FieldButton, _fieldButtonInfo.ButtonState);
			UIHelper.SetLabelText(FieldButtonNameLabel, _fieldButtonInfo.Localize);
		}
	}

	public void OnClick()
	{
		if (_fieldButtonInfo != null && _fieldButtonInfo.Action != null)
		{
			Vespa_System_Action_Extension_Method.fire(_fieldButtonInfo.Action);
		}
	}
}
