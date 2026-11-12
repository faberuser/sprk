using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StatAttenuationData : IDataKeySelector<StatType>
{
	[Key(0)]
	public StatType StatType { get; set; }

	[Key(1)]
	public int MaxK { get; set; }

	[Key(2)]
	public int X1 { get; set; }

	[Key(3)]
	public int A1 { get; set; }

	[Key(4)]
	public int B1 { get; set; }

	[Key(5)]
	public int X2 { get; set; }

	[Key(6)]
	public int A2 { get; set; }

	[Key(7)]
	public int B2 { get; set; }

	[Key(8)]
	public int MinK { get; set; }

	[Key(9)]
	public int X3 { get; set; }

	[Key(10)]
	public int A3 { get; set; }

	[Key(11)]
	public int B3 { get; set; }

	[Key(12)]
	public int X4 { get; set; }

	[Key(13)]
	public int A4 { get; set; }

	[Key(14)]
	public int B4 { get; set; }

	StatType IDataKeySelector<StatType>.Key => StatType;

	public long Attenuate(long x)
	{
		if (x == 0L)
		{
			return 0L;
		}
		if (x > X1)
		{
			return AttenuateInv(x, MaxK, A1, B1);
		}
		if (x > X2)
		{
			return A2 * x / 1000 + B2;
		}
		if (x < X3)
		{
			return AttenuateInv(x, MinK, A3, B3);
		}
		if (x < X4)
		{
			return Attenuate(x, MinK, A4, B4);
		}
		return x;
	}

	public static long Attenuate(long x, int k, int a, int b)
	{
		long num = a * x * x + b * x + 1000000;
		return (long)k * 1000000L / num;
	}

	public static long AttenuateInv(long x, int k, int a, int b)
	{
		long num = a * x * x + b * x + 1000000;
		return k - (long)k * 1000000L / num;
	}
}
