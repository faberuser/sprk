namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroPurifyResultInfo
{
	public int HeroIndex { get; set; }

	public short AddValue { get; set; }

	public short NewValue { get; set; }
}
