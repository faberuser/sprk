namespace NShared;

[JsonSerializable(Javascript = true)]
public class FlaskResultInfo
{
	public int HeroIndex { get; set; }

	public int ItemIndex { get; set; }

	public int AddExp { get; set; }

	public int AddExpRatio { get; set; }

	public int NewExp { get; set; }
}
