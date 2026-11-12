using NGame2.NContentsInfo;
using NShared;

namespace NGame2.NAccount;

public class UserAttendanceManager : NShared.UserAttendanceManager
{
	private readonly UserObserverManager<AttendanceInfo> _observerManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserAttendanceManager()
	{
		_observerManager = new UserObserverManager<AttendanceInfo>();
	}

	public void Apply(AttendanceInfo info)
	{
		Add(info);
		if (info != null)
		{
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void Apply(AttendanceInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public AttendanceInfo GetInfoByKeyword(EAttendanceKeyword keyword)
	{
		int indexByKeyword = AttendanceKeywordGroup.Helper.GetIndexByKeyword(keyword);
		return Get(indexByKeyword);
	}
}
