using System;
using System.Collections.Generic;
using System.Text;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUtil.NStringProxy;

public class EquipItemDataBuilder : IBuilder
{
	public enum EMethod
	{
		ReqLevelAndPartType,
		Desc1
	}

	private ItemData _itemData;

	private EquipItemData _equipItemData;

	private EquipItemInfo _equipItemInfo;

	private CachedHeroInfo _cachedHeroInfo;

	private EMethod _method;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public EquipItemDataBuilder(ItemData itemData, EquipItemData equipItemData, EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo, EMethod method, bool appendLine)
	{
		_itemData = itemData;
		_equipItemData = equipItemData;
		_equipItemInfo = equipItemInfo;
		_method = method;
		_appendLine = appendLine;
		_cachedHeroInfo = cachedHeroInfo;
	}

	public string Build()
	{
		if (_equipItemData == null)
		{
			return string.Empty;
		}
		if (_method == EMethod.ReqLevelAndPartType)
		{
			return Build_ReqLevelAndPartType();
		}
		if (_method == EMethod.Desc1)
		{
			return Build_Desc1();
		}
		VespaDebug.LogError("Not Implemented Yet: " + Vespa_String_Extension_Method.GetEnumString(_method));
		return string.Empty;
	}

	private string Build_ReqLevelAndPartType()
	{
		string text = ((_equipItemData.ReqLevel != 0) ? (LocalizationManager.GetString(Equip.EQUIP_INFO_LEVELLIMIT) + " " + _equipItemData.ReqLevel) : (LocalizationManager.GetString(Equip.EQUIP_INFO_LEVELLIMIT) + " " + LocalizationManager.GetString(Common.COMMON_NOT_EXIST)));
		return text + " " + LocalizationManager.GetString(_equipItemData.PartType);
	}

	private void AddMainStat(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		if (_equipItemInfo == null)
		{
			if (GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(_equipItemData.EquipCode) != null)
			{
				equipItemStatData = new StatData();
				equipItemStatData.AddEquipItemStat(_equipItemData, 0, _equipItemData.DetailIndex, 0);
			}
			else
			{
				equipItemStatData = _equipItemData;
			}
			equipPartType = _equipItemData.PartType;
		}
		else
		{
			if (_equipItemData != null)
			{
				equipPartType = _equipItemData.PartType;
			}
			else
			{
				ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(_equipItemInfo.ItemIndex);
				equipPartType = data.SubData.PartType;
			}
			equipItemStatData = StatCalculator.GetEquipItemStatData(_equipItemInfo);
		}
		long statValue = 0L;
		if (equipItemStatData.GetMainStat(equipPartType, out var statType, out statValue) && sb != null)
		{
			bool isShowSign = StatData.IsPercentStat(statType);
			sb.AppendLine(StatBuilder.GetString(statType, statValue, isShowSign));
		}
	}

	private void AddRuneStat(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		if (_equipItemInfo == null)
		{
			return;
		}
		foreach (int runeItemIndex in _equipItemInfo.GetRuneItemIndexList())
		{
			ItemData<RuneItemData> data = GameRoot.Instance.ItemDataContainer.GetData<RuneItemData>(runeItemIndex);
			if (data == null)
			{
				continue;
			}
			foreach (KeyValuePair<Enum, long> item in data.SubData.GetStatTypeAndValue())
			{
				string text = LocalizationManager.GetString(item.Key);
				string statDisplayValue = StatData.GetStatDisplayValue(item.Key, item.Value);
				if (sb != null)
				{
					sb.AppendLine(text + " " + statDisplayValue);
				}
			}
		}
	}

	private void AddFixedStat(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		int num = 1;
		while (true)
		{
			int fixedOptionIndex = _equipItemData.GetFixedOptionIndex(num);
			if (fixedOptionIndex != 0)
			{
				EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(fixedOptionIndex);
				if (data != null)
				{
					OperationType optionType = data.OptionType;
					int optionValue = data.GetOptionValue((_equipItemInfo != null) ? _equipItemInfo.Star : 0, 0);
					sb.AppendLine(StatData.GetOptionDisplay(optionType, optionValue));
				}
				num++;
				continue;
			}
			break;
		}
	}

	private void AddExtraStat(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		if (_equipItemInfo == null || !_equipItemInfo.IsIdentified)
		{
			return;
		}
		int num = 1;
		while (true)
		{
			int extraOptionIndex = _equipItemInfo.GetExtraOptionIndex(num);
			if (extraOptionIndex != 0)
			{
				EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(extraOptionIndex);
				if (data == null)
				{
					sb.AppendLine($"Not Found EquipOptionData. (ExtraOptionIndex:{extraOptionIndex})");
					break;
				}
				OperationType optionType = data.OptionType;
				int optionValue = data.GetOptionValue(_equipItemInfo.Star, 0);
				sb.AppendLine(StatData.GetExtraOptionDisplay(optionType, optionValue));
				num++;
				continue;
			}
			break;
		}
	}

	private void AddToolTipBuilder(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		if (_equipItemInfo != null)
		{
			return;
		}
		int num = GameRoot.Instance.EquipItemDetailDataContainer.GetData(_equipItemData.DetailIndex)?.OptionCount ?? 0;
		if (_equipItemData.PartType != EquipPartType.EnhanceStone)
		{
			for (int i = 1; i <= num; i++)
			{
				GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionLevel(i);
				EnumBuilder enumBuilder = new EnumBuilder().Key(Equip.EQUIP_OPTION_STAT_TEXT).NguiTag("[808080]");
				sb.AppendLine(enumBuilder.Build());
			}
		}
		if (_equipItemData.WorkPlace == EquipItemWorkPlace.Valance)
		{
			EnumBuilder enumBuilder2 = new EnumBuilder().DirectKey("EQUIP_OPTION_ADDITIONALSKILL").NguiTag("[808080]");
			sb.AppendLine(enumBuilder2.Build());
		}
	}

	private void AddRandomStat(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		if (_equipItemInfo == null)
		{
			return;
		}
		int num = 0;
		foreach (EquipItemOptionInfo optionInfo in _equipItemInfo.GetOptionInfoList())
		{
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
			if (data != null)
			{
				int optionOpenLevel = _equipItemInfo.GetOptionOpenLevel(optionInfo.SlotIndex);
				int optionStep = optionInfo.OptionStep;
				bool isOptionOpened = optionInfo.IsOptionOpened;
				OperationType optionType = data.OptionType;
				int optionValue = data.GetOptionValue(_equipItemInfo.Star, optionStep);
				sb.AppendLine(StatData.GetOptionDisplay(optionType, optionValue, isOptionOpened, optionOpenLevel, appendColor: true, isFixedOption: false));
				num++;
			}
		}
		if (num == 0 && _equipItemData != null && _equipItemData.EquipType == EquipItemEquipType.TechnoMagic)
		{
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_TECHNOMAGIC_UNIDENTIFIED");
			string enumString = GameRoot.LocalizeProxy.GetEnumString(Equip.EQUIP_OPEN_OPTION_ALL);
			sb.AppendLine(StatData.GetUnIdentifiedOptionDisplay(enumString, stringDirect));
		}
	}

	private void AddEnchantOptionStat(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		if (_equipItemInfo == null)
		{
			return;
		}
		int num = 1;
		while (true)
		{
			int enchantOptionIndex = _equipItemInfo.GetEnchantOptionIndex(num);
			if (enchantOptionIndex != 0)
			{
				EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(enchantOptionIndex);
				if (data != null)
				{
					OperationType optionType = data.OptionType;
					int enchantOptionStep = _equipItemInfo.GetEnchantOptionStep(num);
					int optionValue = data.GetOptionValue(0, enchantOptionStep);
					sb.AppendLine(StatData.GetEnchantOptionDisplay(optionType, optionValue, optionValue, string.Empty));
				}
				num++;
				continue;
			}
			break;
		}
	}

	private void AddSetOptionStat(ref System.Text.StringBuilder sb, ref StatData equipItemStatData, ref EquipPartType equipPartType)
	{
		if (_equipItemInfo != null)
		{
			List<EquipOptionData> openedOptionDataList = null;
			List<EquipOptionData> closedOptionDataList = null;
			if (_cachedHeroInfo != null)
			{
				_cachedHeroInfo.GetSelectedSetOptionList(_equipItemInfo, out openedOptionDataList, out closedOptionDataList);
			}
			else
			{
				EquipSetBonusData equipSetBonusData = _equipItemInfo.GetEquipSetBonusData();
				if (equipSetBonusData == null)
				{
					return;
				}
				equipSetBonusData.GetOptionIndexList(0, out var _, out var closedOptionIndexList);
				if (closedOptionIndexList != null)
				{
					for (int i = 0; i < closedOptionIndexList.Count; i++)
					{
						int key = closedOptionIndexList[i];
						EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(key);
						if (data != null)
						{
							closedOptionDataList = closedOptionDataList ?? new List<EquipOptionData>();
							closedOptionDataList.Add(data);
						}
					}
				}
			}
			int num = 1;
			sb.AppendLine();
			if (openedOptionDataList != null)
			{
				foreach (EquipOptionData item in openedOptionDataList)
				{
					EquipSetBonusData equipSetBonusData2 = _equipItemInfo.GetEquipSetBonusData();
					if (equipSetBonusData2 != null)
					{
						int reqSetEquipCount = equipSetBonusData2.GetReqSetEquipCount(num++);
						EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(item.Index);
						if (data2 == null)
						{
							VespaDebug.LogError("Option data not found : " + item.Index);
							sb.Clear();
							return;
						}
						OperationType optionType = data2.OptionType;
						int optionValue = data2.GetOptionValue(_equipItemInfo.Star, 0);
						sb.AppendLine(StatData.GetSetOptionDisplay(optionType, optionValue, opened: true, reqSetEquipCount));
					}
				}
			}
			if (closedOptionDataList == null)
			{
				return;
			}
			{
				foreach (EquipOptionData item2 in closedOptionDataList)
				{
					EquipSetBonusData equipSetBonusData3 = _equipItemInfo.GetEquipSetBonusData();
					if (equipSetBonusData3 != null)
					{
						int reqSetEquipCount2 = equipSetBonusData3.GetReqSetEquipCount(num++);
						EquipOptionData data3 = GameRoot.Instance.EquipOptionDataContainer.GetData(item2.Index);
						if (data3 == null)
						{
							VespaDebug.LogError("Option data not found : " + item2.Index);
							sb.Clear();
							break;
						}
						OperationType optionType2 = data3.OptionType;
						int optionValue2 = data3.GetOptionValue(_equipItemInfo.Star, 0);
						sb.AppendLine(StatData.GetSetOptionDisplay(optionType2, optionValue2, opened: false, reqSetEquipCount2));
					}
				}
				return;
			}
		}
		EquipSetBonusData data4 = GameRoot.Instance.EquipSetBonusDataContainer.GetData(_equipItemData.SetIndex);
		if (data4 == null)
		{
			return;
		}
		List<EquipOptionData> list = null;
		data4.GetOptionIndexList(0, out var _, out var closedOptionIndexList2);
		if (closedOptionIndexList2 != null)
		{
			for (int j = 0; j < closedOptionIndexList2.Count; j++)
			{
				int key2 = closedOptionIndexList2[j];
				EquipOptionData data5 = GameRoot.Instance.EquipOptionDataContainer.GetData(key2);
				if (data5 != null)
				{
					list = list ?? new List<EquipOptionData>();
					list.Add(data5);
				}
			}
		}
		int num2 = 1;
		sb.AppendLine();
		if (list == null)
		{
			return;
		}
		foreach (EquipOptionData item3 in list)
		{
			int reqSetEquipCount3 = data4.GetReqSetEquipCount(num2++);
			EquipOptionData data6 = GameRoot.Instance.EquipOptionDataContainer.GetData(item3.Index);
			if (data6 == null)
			{
				VespaDebug.LogError("Option data not found : " + item3.Index);
				sb.Clear();
				break;
			}
			OperationType optionType3 = data6.OptionType;
			int optionValue3 = data6.GetOptionValue(0, 0);
			sb.AppendLine(StatData.GetSetOptionDisplay(optionType3, optionValue3, opened: false, reqSetEquipCount3));
		}
	}

	private string Build_Desc1()
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		StatData equipItemStatData = null;
		EquipPartType equipPartType = EquipPartType.None;
		AddMainStat(ref sb, ref equipItemStatData, ref equipPartType);
		AddRuneStat(ref sb, ref equipItemStatData, ref equipPartType);
		AddToolTipBuilder(ref sb, ref equipItemStatData, ref equipPartType);
		AddFixedStat(ref sb, ref equipItemStatData, ref equipPartType);
		AddRandomStat(ref sb, ref equipItemStatData, ref equipPartType);
		AddExtraStat(ref sb, ref equipItemStatData, ref equipPartType);
		AddEnchantOptionStat(ref sb, ref equipItemStatData, ref equipPartType);
		AddSetOptionStat(ref sb, ref equipItemStatData, ref equipPartType);
		return sb.ToString();
	}
}
