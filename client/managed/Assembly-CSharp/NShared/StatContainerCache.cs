using System.Collections.Generic;

namespace NShared;

public class StatContainerCache
{
	private readonly Dictionary<long, List<StatContainer>> _statContainersByCheckSum = new Dictionary<long, List<StatContainer>>();

	public int CreatedCount { get; private set; }

	public int SkippedCount { get; private set; }

	public StatContainer GetCached(StatContainer statContainer)
	{
		long checkSum = statContainer.GetCheckSum();
		List<StatContainer> value = null;
		if (_statContainersByCheckSum.TryGetValue(checkSum, out value) && value != null)
		{
			for (int i = 0; i < value.Count; i++)
			{
				StatContainer statContainer2 = value[i];
				if (statContainer.EqualStat(statContainer2))
				{
					SkippedCount++;
					return statContainer2;
				}
			}
			value.Add(statContainer);
			CreatedCount++;
			return statContainer;
		}
		value = new List<StatContainer>();
		value.Add(statContainer);
		_statContainersByCheckSum[checkSum] = value;
		CreatedCount++;
		return statContainer;
	}

	public void Clear()
	{
		_statContainersByCheckSum.Clear();
		CreatedCount = 0;
		SkippedCount = 0;
	}
}
