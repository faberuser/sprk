using NShared.NUtil;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class BattleVersionInfo
{
	public string Checksum { get; set; }

	public int Length { get; set; }

	public int Count { get; set; }

	public static BattleVersionInfo Generate(string path)
	{
		Sha512.GetDirectoryInfo(path, out var count, out var length, out var checksum);
		return new BattleVersionInfo
		{
			Checksum = checksum,
			Length = length,
			Count = count
		};
	}

	public override string ToString()
	{
		return Checksum + "_" + Length + "_" + Count;
	}
}
