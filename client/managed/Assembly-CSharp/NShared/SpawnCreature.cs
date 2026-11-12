using NShared.NUtil;

namespace NShared;

[BattleCallback]
public sealed class SpawnCreature : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public long PlayerId { get; set; }

	public BattleCreatureCreateInfo CreateInfo { get; set; }

	public Vector3 SpawnPosition { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is SpawnCreature o))
		{
			return false;
		}
		string text = JsonMarshaler.EncodeJsonString(this);
		string text2 = JsonMarshaler.EncodeJsonString(o);
		if (text != text2)
		{
			return false;
		}
		return true;
	}
}
