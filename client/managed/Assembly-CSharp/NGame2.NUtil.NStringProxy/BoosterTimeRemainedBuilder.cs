using NGame2.NUI.NManager;

namespace NGame2.NUtil.NStringProxy;

public class BoosterTimeRemainedBuilder : TimeRemainedBuilder
{
	public BoosterTimeRemainedBuilder(int remainedSec, bool appendLine)
		: base(remainedSec, appendLine)
	{
	}

	public override string Build()
	{
		return BoosterManager.GetBoosterRemainTime(_remainedSec);
	}
}
