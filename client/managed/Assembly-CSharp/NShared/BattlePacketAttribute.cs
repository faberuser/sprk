namespace NShared;

public class BattlePacketAttribute : JsonSerializableAttribute
{
	public BattlePacketAttribute()
	{
		base.Serialize = true;
		base.Deserialize = true;
		base.Javascript = true;
	}
}
