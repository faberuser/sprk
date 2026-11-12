namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerKingsPassExp : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => SeasonIndex;

	public int SeasonIndex { get; set; }

	public int Exp { get; set; }

	public int Level { get; set; }

	public string UpdatedTime { get; set; }
}
