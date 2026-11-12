using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa;

namespace NGame2.NContents;

public class ContentsStatusManager : SingletonAuto<ContentsStatusManager>
{
	private Dictionary<Tuple<string, string>, ContentsStatusInfo> _dic = new Dictionary<Tuple<string, string>, ContentsStatusInfo>();

	private readonly UserObserverManager<ContentsStatusInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public ContentsStatusManager()
	{
		_observerManager = new UserObserverManager<ContentsStatusInfo>();
	}

	public void Add(ContentsStatusInfo info)
	{
		if (info != null)
		{
			Tuple<string, string> key = new Tuple<string, string>(info.ContentsType, info.ContentsSubType);
			_dic[key] = info;
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Add(ContentsStatusInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Add(infos[i]);
		}
	}

	public bool IsEnable(ContentsStatusType type, ContentsStatusSubType subType = ContentsStatusSubType.None)
	{
		return GetContentsStatus(type, subType) == ContentsStatus.On;
	}

	public ContentsStatus GetContentsStatus(ContentsStatusType type, ContentsStatusSubType subType = ContentsStatusSubType.None)
	{
		if (type == ContentsStatusType.None)
		{
			return ContentsStatus.On;
		}
		ContentsStatusInfo value = null;
		_dic.TryGetValue(new Tuple<string, string>(type.ToString(), subType.ToString()), out value);
		ContentsStatus o_output = ContentsStatus.On;
		if (value != null)
		{
			Vespa_String_Extension_Method.TryToEnum<ContentsStatus>(value.Status, out o_output);
		}
		return o_output;
	}
}
