namespace NShared;

[JsonSerializable(Javascript = true)]
public class GachaTimeResultInfo
{
	public int GachaIndex { get; set; }

	public int NextFreeRemainTime { get; set; }

	public int ResetRemainTime { get; set; }
}
