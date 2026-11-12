using NGame2.NUI;

namespace NGame2.NUtil.NStringProxy;

public class TimeRemainedBuilder : IBuilder
{
	protected int _remainedSec;

	protected bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public TimeRemainedBuilder(int remainedSec, bool appendLine)
	{
		_remainedSec = remainedSec;
		_appendLine = appendLine;
	}

	public virtual string Build()
	{
		return UIHelper.GetRemainTimeText(_remainedSec);
	}
}
