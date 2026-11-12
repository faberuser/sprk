namespace NShared;

public static class SkillTriggerUtil
{
	public static int TableValue(int[] tableValues, int index)
	{
		if (tableValues == null || index >= tableValues.Length)
		{
			return 0;
		}
		return tableValues[index];
	}

	public static bool HasTableValue(int[] tableValues, int index)
	{
		return !NoTableValue(tableValues, index);
	}

	public static bool NoTableValue(int[] tableValues, int index)
	{
		if (tableValues != null)
		{
			return index >= tableValues.Length;
		}
		return true;
	}

	public static bool EqualToTableValue(int[] tableValues, int index, int checkValue)
	{
		return checkValue == TableValue(tableValues, index);
	}

	public static bool LargerThanTableValue(int[] tableValues, int index, int checkValue)
	{
		return checkValue > TableValue(tableValues, index);
	}

	public static bool LargerOrEqualToTableValue(int[] tableValues, int index, int checkValue)
	{
		return checkValue >= TableValue(tableValues, index);
	}

	public static bool SmallerThanTableValue(int[] tableValues, int index, int checkValue)
	{
		return checkValue < TableValue(tableValues, index);
	}

	public static bool SmallerOrEqualToTableValue(int[] tableValues, int index, int checkValue)
	{
		return checkValue <= TableValue(tableValues, index);
	}
}
