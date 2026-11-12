using System;
using System.Collections.Generic;

public static class SprkDispatchPacing
{
	public static void Pump<T>(IList<T> runs, Func<T, bool> tick, Func<bool> hasBudget, int first)
	{
		for (int i = 0; i < 256; i++)
		{
			bool flag = false;
			for (int j = 0; j < runs.Count; j++)
			{
				if (!hasBudget())
				{
					return;
				}
				if (tick(runs[(first + j) % runs.Count]))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
		}
	}
}
