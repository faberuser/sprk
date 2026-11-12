using System;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ValueChangeComponent : UIScrollItemBehaviour
{
	public enum EValueChange
	{
		Unchanged,
		Good,
		Bad
	}

	[Serializable]
	public class ValueColorData
	{
		public Color FieldColor = Color.black;

		public Color UnchangedColor = Color.black;

		public Color GoodColor = Color.green;

		public Color BadColor = Color.red;
	}

	public const string PrefabPath = "Prefab2/Component/ValueChangeComponent";

	public static PrefabAuto<ValueChangeComponent> PrefabAuto = new PrefabAuto<ValueChangeComponent>("Prefab2/Component/ValueChangeComponent");

	public ValueColorData ValueColor = new ValueColorData();

	public UIWidget RootWidget;

	public UILabel FieldLabel;

	public UILabel ValueFromLabel;

	public UILabel ValueToLabel;

	public void Open(string field, string valueFrom, string valueTo, EValueChange valueChange = EValueChange.Unchanged)
	{
		UIHelper.SetLabelText(FieldLabel, Vespa_String_Extension_Method.NguiColor(field, ValueColor.FieldColor));
		UIHelper.SetLabelText(ValueFromLabel, Vespa_String_Extension_Method.NguiColor(valueFrom, ValueColor.UnchangedColor));
		UIHelper.SetLabelText(text: valueChange switch
		{
			EValueChange.Unchanged => Vespa_String_Extension_Method.NguiColor(valueTo, ValueColor.UnchangedColor), 
			EValueChange.Good => Vespa_String_Extension_Method.NguiColor(valueTo, ValueColor.GoodColor), 
			EValueChange.Bad => Vespa_String_Extension_Method.NguiColor(valueTo, ValueColor.BadColor), 
			_ => Vespa_String_Extension_Method.NguiColor(valueTo, ValueColor.UnchangedColor), 
		}, label: ValueToLabel);
	}

	public void Open(StatType statType, long valueFrom, long valueTo, bool showSign = true)
	{
		EValueChange valueChange = EValueChange.Unchanged;
		if (valueFrom < valueTo)
		{
			valueChange = EValueChange.Good;
		}
		else if (valueFrom > valueTo)
		{
			valueChange = EValueChange.Bad;
		}
		string field = LocalizationManager.GetString(statType);
		string statDisplayValue = StatData.GetStatDisplayValue(statType, valueFrom, showSign);
		string statDisplayValue2 = StatData.GetStatDisplayValue(statType, valueTo, showSign);
		Open(field, statDisplayValue, statDisplayValue2, valueChange);
	}
}
