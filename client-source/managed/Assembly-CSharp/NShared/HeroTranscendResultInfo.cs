namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroTranscendResultInfo
{
	public int HeroIndex { get; set; }

	public int AddPointValue { get; set; }

	public int NewPointValue { get; set; }
}
