using System;
using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class SingleStatComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/Stat/SingleStatComponent";

	public static PrefabAuto<SingleStatComponent> PrefabAuto = new PrefabAuto<SingleStatComponent>("Prefab2/Component/Stat/SingleStatComponent");

	public UIWidget RootWidget;

	public Color StatTypeColor = Color.yellow;

	public Color OptionTypeColor = Color.white;

	public Color ValueNormalColor = Color.white;

	public Color ValueEnhancedColor = Color.cyan;

	public Color ValuePlusColor = Color.green;

	public Color NotAppliedEquipOptionColor;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _valueLabel;

	private long _totalValue;

	public void Open(Enum statType, long totalValue, long equipValue = 0L)
	{
		Open(statType, totalValue, (equipValue == 0L) ? null : new long[1] { equipValue });
	}

	public void Open(Enum statType, long totalValue, IList<long> equipValues = null)
	{
		_totalValue = totalValue;
		_iconSprite.spriteName = "StatIcon_" + statType.ToString();
		UIHelper.SetLabelText(_nameLabel, LocalizationManager.GetString(statType));
		string empty = string.Empty;
		if (__Extension_Method_ICollection__.COUNT(equipValues) > 0)
		{
			long num = 0L;
			foreach (long equipValue in equipValues)
			{
				num += equipValue;
			}
			long optionValue = totalValue - num;
			string text = string.Empty;
			foreach (long equipValue2 in equipValues)
			{
				string statDisplayValue = StatData.GetStatDisplayValue(statType, equipValue2);
				text += statDisplayValue;
			}
			string statDisplayValue2 = StatData.GetStatDisplayValue(statType, totalValue, showSign: false);
			string statDisplayValue3 = StatData.GetStatDisplayValue(statType, optionValue, showSign: false);
			string text2 = NGUIText.EncodeColor(ValueEnhancedColor);
			string text3 = NGUIText.EncodeColor(ValueNormalColor);
			string text4 = NGUIText.EncodeColor(ValuePlusColor);
			empty = string.Format("[" + text2 + "]{0}[-] ([" + text3 + "]{1}[-][" + text4 + "]{2}[-])", statDisplayValue2, statDisplayValue3, text);
		}
		else
		{
			string statDisplayValue4 = StatData.GetStatDisplayValue(statType, totalValue, showSign: false);
			string text5 = NGUIText.EncodeColor(ValueNormalColor);
			empty = string.Format("[" + text5 + "]{0}[-]", statDisplayValue4);
		}
		UIHelper.SetLabelText(_valueLabel, empty);
		InitColor(statType);
	}

	public void Open(string stat_name, long value, string value_desc)
	{
		_totalValue = value;
		SetStatColor(StatTypeColor);
		UIHelper.SetLabelText(_nameLabel, stat_name);
		UIHelper.SetLabelText(_valueLabel, value_desc);
	}

	public void OpenNotAppliedEquipOption(string desc)
	{
		_totalValue = 0L;
		SetStatColor(NotAppliedEquipOptionColor);
		UIHelper.SetLabelText(_nameLabel, desc);
		UIHelper.SetLabelText(_valueLabel, string.Empty);
	}

	private void InitColor(Enum statType)
	{
		if (statType.GetType() == typeof(StatType))
		{
			SetStatColor(StatTypeColor);
		}
		else if (statType.GetType() == typeof(OperationType))
		{
			SetStatColor(OptionTypeColor);
		}
	}

	public void SetStatColor(Color color)
	{
		_nameLabel.color = color;
	}

	public void SetEquipItemFormat()
	{
		UIHelper.SetLabelText(_valueLabel, $"+{_totalValue}");
	}
}
