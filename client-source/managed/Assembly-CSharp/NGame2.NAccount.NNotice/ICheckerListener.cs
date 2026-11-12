namespace NGame2.NAccount.NNotice;

public interface ICheckerListener
{
	void AnnounceToObservers(ICheckerNoticeInfo noticeInfo);
}
