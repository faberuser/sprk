namespace NGame2.NAccount.NNotice;

public class DebugNotice : IUserObserver<SoulWeaponNoticeInfo>, IUserObserver
{
	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void IUserObserver<SoulWeaponNoticeInfo>.OnChanged(SoulWeaponNoticeInfo changedInfo)
	{
	}
}
