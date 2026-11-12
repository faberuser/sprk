namespace NGame2.NAccount.NNotice;

public interface ISoulWeaponChecker : IChecker
{
	bool IsNoticeType(ESoulWeaponNoticeType noticeType);

	bool GetNoticeTargetState(int index);

	bool GetNoticeTypeState(ESoulWeaponNoticeType noticeType);

	bool AnyStateOn();
}
