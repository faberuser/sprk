using System;
using MessagePack;
using NShared.NContentsDefine;
using NVespa;

namespace NShared;

[MessagePackObject(false)]
public class EquipOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ShowOptionIndex { get; set; }

	[Key(2)]
	public OperationType OptionType { get; set; }

	[Key(3)]
	public int OptionValueMin0 { get; set; }

	[Key(4)]
	public int OptionValueMax0 { get; set; }

	[Key(5)]
	public int OptionValueMin1 { get; set; }

	[Key(6)]
	public int OptionValueMax1 { get; set; }

	[Key(7)]
	public int OptionValueMin2 { get; set; }

	[Key(8)]
	public int OptionValueMax2 { get; set; }

	[Key(9)]
	public int OptionValueMin3 { get; set; }

	[Key(10)]
	public int OptionValueMax3 { get; set; }

	[Key(11)]
	public int OptionValueMin4 { get; set; }

	[Key(12)]
	public int OptionValueMax4 { get; set; }

	[Key(13)]
	public int OptionValueMin5 { get; set; }

	[Key(14)]
	public int OptionValueMax5 { get; set; }

	[Key(15)]
	public int OptionValueStep { get; set; }

	[Key(16)]
	public string Name { get; set; }

	[Key(17)]
	public string Description { get; set; }

	[Key(18)]
	public int Ratio { get; set; }

	[Key(19)]
	public byte Enchant { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int GetOptionValueMin(int star)
	{
		star = NShared.NContentsDefine.SoulWeaponLimitBreak.ExceptionalMaxStar(star);
		switch (star)
		{
		case 0:
			return OptionValueMin0;
		case 1:
			return OptionValueMin1;
		case 2:
			return OptionValueMin2;
		case 3:
			return OptionValueMin3;
		case 4:
			return OptionValueMin4;
		case 5:
			return OptionValueMin5;
		default:
			VespaDebug.LogError("No option of star " + star);
			return 0;
		}
	}

	public int GetOptionValueMax(int star)
	{
		star = NShared.NContentsDefine.SoulWeaponLimitBreak.ExceptionalMaxStar(star);
		switch (star)
		{
		case 0:
			return OptionValueMax0;
		case 1:
			return OptionValueMax1;
		case 2:
			return OptionValueMax2;
		case 3:
			return OptionValueMax3;
		case 4:
			return OptionValueMax4;
		case 5:
			return OptionValueMax5;
		default:
			VespaDebug.LogError("No option of star " + star);
			return 0;
		}
	}

	public int GetOptionValue(int star, int currentStep)
	{
		int optionValueMin = GetOptionValueMin(star);
		int optionValueMax = GetOptionValueMax(star);
		if (OptionValueStep <= 0)
		{
			return optionValueMin;
		}
		currentStep = System.Math.Max(currentStep, 0);
		currentStep = System.Math.Min(currentStep, OptionValueStep);
		return optionValueMin + (optionValueMax - optionValueMin) * currentStep / OptionValueStep;
	}

	public override string ToString()
	{
		return "Index=" + Index + " OptionType=" + OptionType;
	}
}
