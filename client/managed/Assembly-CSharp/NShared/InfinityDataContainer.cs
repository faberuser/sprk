using NShared.NJit;

namespace NShared;

public class InfinityDataContainer : JitDataContainerDefault<int, InfinityData>
{
	public const string TABLENAME = "InfinityTable";

	public override TablePathDefine PathDefine => new TablePathDefine("InfinityTable");

	public int GetSumValue(int index, int step)
	{
		InfinityData data = GetData(index);
		if (data == null || step <= 0)
		{
			return 0;
		}
		int num = data.StartValue;
		int num2 = step - 1;
		int num3 = ((data.Multiplier == 0) ? 1 : data.Multiplier);
		if (num2 == 0)
		{
			return num * num3;
		}
		int[] increaseValues = data.IncreaseValues;
		int num4 = ((increaseValues != null) ? increaseValues.Length : 0);
		int num5 = num2 - num4;
		int num6 = 0;
		for (int i = 0; i < num4; i++)
		{
			int num7 = increaseValues[i];
			if (i < num2)
			{
				num += num7;
				num6 = num7;
				continue;
			}
			return num * num3;
		}
		num += num6 * num5;
		return num * num3;
	}
}
