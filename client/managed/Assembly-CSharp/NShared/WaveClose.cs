namespace NShared;

[BattleCallback]
public sealed class WaveClose : BaseBattleCallback
{
	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is WaveClose))
		{
			return false;
		}
		return true;
	}
}
