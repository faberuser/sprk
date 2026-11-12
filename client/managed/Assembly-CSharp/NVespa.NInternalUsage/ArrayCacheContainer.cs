using System.Collections.Generic;
using System.Text;

namespace NVespa.NInternalUsage;

public class ArrayCacheContainer : SingletonAuto<ArrayCacheContainer>
{
	private List<IGenericArrayCache> _cacheList = new List<IGenericArrayCache>();

	protected override bool ThreadSafe => true;

	public void Add(IGenericArrayCache arrayCache)
	{
		lock (_cacheList)
		{
			_cacheList.Add(arrayCache);
		}
	}

	public void OnSceneChanged()
	{
		lock (_cacheList)
		{
			for (int i = 0; i < _cacheList.Count; i++)
			{
				_cacheList[i].OnSceneChanged();
			}
		}
	}

	public void DumpSummary(StringBuilder sb)
	{
	}
}
