using System.Collections.Generic;
using NShared;
using NVespa;

namespace NGame2.NHonoraryChamber;

public class HonoraryChamberManager : SingletonAuto<HonoraryChamberManager>
{
	private Dictionary<RecordsOfHonorContentType, RecordsOfHonorTopInfo> _dicContentsRanker = new Dictionary<RecordsOfHonorContentType, RecordsOfHonorTopInfo>();

	public void Set(RecordsOfHonorTopInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Set(infos[i]);
		}
	}

	public void Set(RecordsOfHonorTopInfo info)
	{
		if (info == null)
		{
			VespaDebug.LogError("RecordsOfHonorTopInfo is null");
		}
		else
		{
			_dicContentsRanker[info.ContentType] = info;
		}
	}

	public RecordsOfHonorTopInfo Get(RecordsOfHonorContentType contentsType)
	{
		if (!_dicContentsRanker.ContainsKey(contentsType))
		{
			return null;
		}
		return _dicContentsRanker[contentsType];
	}

	public Dictionary<RecordsOfHonorContentType, RecordsOfHonorTopInfo>.ValueCollection GetValues()
	{
		return _dicContentsRanker.Values;
	}
}
