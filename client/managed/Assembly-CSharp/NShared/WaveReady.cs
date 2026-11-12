namespace NShared;

[BattleCallback]
public sealed class WaveReady : BaseBattleCallback
{
	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is WaveReady))
		{
			return false;
		}
		return true;
	}
}
