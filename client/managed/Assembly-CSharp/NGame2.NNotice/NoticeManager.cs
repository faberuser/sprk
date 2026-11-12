using NGame2.NUI.NWindow.NNotice;
using NShared;
using NVespa;

namespace NGame2.NNotice;

public class NoticeManager : SingletonAuto<NoticeManager>
{
	public void AddNotice(Notice notice)
	{
		NoticeWindow noticeWindow = NoticeWindow.instance;
		if (!noticeWindow.IsOpen())
		{
			noticeWindow.Open();
		}
		noticeWindow.AddNotice(notice);
	}

	public void RemoveNotice(int noticeId)
	{
		NoticeWindow noticeWindow = NoticeWindow.instance;
		if (noticeWindow.IsOpen())
		{
			noticeWindow.RemoveNotice(noticeId);
		}
	}
}
