using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CraftInstantGemData : IDataKeySelector<int>
{
	[Key(0)]
	public int MinRemainTime { get; set; }

	[Key(1)]
	public int MaxRemainTime { get; set; }

	[Key(2)]
	public int BaseReqGem { get; set; }

	[Key(3)]
	public int ReqSecondPerGem { get; set; }

	int IDataKeySelector<int>.Key => MinRemainTime;

	public override string ToString()
	{
		return "MinRemainTime =" + MinRemainTime + ", MaxRemainTime = " + MaxRemainTime;
	}

	public int GetReqGem(int remainTime)
	{
		if (MinRemainTime > remainTime || MaxRemainTime < remainTime)
		{
			return 0;
		}
		int num = BaseReqGem;
		int num2 = remainTime - MinRemainTime;
		num2 = ((num2 >= 0) ? num2 : 0);
		if (ReqSecondPerGem != 0)
		{
			num += num2 / ReqSecondPerGem;
			num += ((num2 % ReqSecondPerGem > 0) ? 1 : 0);
		}
		return num;
	}
}
