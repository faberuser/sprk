namespace NShared;

public class BattleCallbackAttribute : JsonSerializableAttribute
{
	public BattleCallbackAttribute()
	{
		base.Serialize = true;
		base.Deserialize = false;
		base.Javascript = false;
	}
}
