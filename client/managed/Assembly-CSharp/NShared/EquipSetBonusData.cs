using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipSetBonusData : IDataKeySelector<int>
{
	public const int MaxOptionCount = 6;

	[Key(0)]
	public int SetIndex { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public int OptionIndex1 { get; set; }

	[Key(3)]
	public int OptionIndex2 { get; set; }

	[Key(4)]
	public int OptionIndex3 { get; set; }

	[Key(5)]
	public int OptionIndex4 { get; set; }

	[Key(6)]
	public int OptionIndex5 { get; set; }

	[Key(7)]
	public int OptionIndex6 { get; set; }

	int IDataKeySelector<int>.Key => SetIndex;

	public override string ToString()
	{
		return "SetIndex=" + SetIndex;
	}

	public int GetNewOptionIndexOfSetCount(int setCount)
	{
		return setCount switch
		{
			1 => OptionIndex1, 
			2 => OptionIndex2, 
			3 => OptionIndex3, 
			4 => OptionIndex4, 
			5 => OptionIndex5, 
			6 => OptionIndex6, 
			_ => 0, 
		};
	}

	public int GetReqSetEquipCount(int option)
	{
		for (int i = 1; i <= 6; i++)
		{
			if (GetNewOptionIndexOfSetCount(i) != 0)
			{
				option--;
				if (option <= 0)
				{
					return i;
				}
			}
		}
		return 6;
	}

	public void GetOptionIndexList(int setCount, out List<int> openedOptionIndexList, out List<int> closedOptionIndexList)
	{
		openedOptionIndexList = null;
		closedOptionIndexList = null;
		for (int i = 1; i <= 6; i++)
		{
			int newOptionIndexOfSetCount = GetNewOptionIndexOfSetCount(i);
			if (newOptionIndexOfSetCount != 0)
			{
				if (i <= setCount)
				{
					openedOptionIndexList = openedOptionIndexList ?? new List<int>();
					openedOptionIndexList.Add(newOptionIndexOfSetCount);
				}
				else
				{
					closedOptionIndexList = closedOptionIndexList ?? new List<int>();
					closedOptionIndexList.Add(newOptionIndexOfSetCount);
				}
			}
		}
	}
}
