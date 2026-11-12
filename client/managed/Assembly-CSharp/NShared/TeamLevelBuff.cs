namespace NShared;

[JsonSerializable(Javascript = true)]
public class TeamLevelBuff : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => OpenBuffLevel;

	public int OpenBuffLevel { get; set; }

	public int BuffReinforce { get; set; }

	public string UpdatedTime { get; set; }
}
