using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class MailInfoContainer : SingletonAuto<MailInfoContainer>
{
	private List<NShared.MailInfo> _mailList = new List<NShared.MailInfo>();

	private List<NShared.MailInfo> _globalMailList = new List<NShared.MailInfo>();

	private List<int> _openedMailIndices = new List<int>();

	private const int NewMailCountPerRead = 50;

	public void Open()
	{
		UpdateMail();
	}

	public void UpdateMail()
	{
		ClearMailList();
		ClearGlobalMailList();
		RequestMailList(0L, isNewGlobal: true);
	}

	private void RequestMailList(long lastIndex, bool isNewGlobal)
	{
		if (isNewGlobal)
		{
			ClearGlobalMailList();
			SceneAutoBehaviour<MailManagement>.instance.RequestGlobalMailList(delegate(NShared.MailInfo[] globalMails)
			{
				if (__Extension_Method_ICollection__.COUNT(globalMails) > 0)
				{
					foreach (NShared.MailInfo mailInfo in globalMails)
					{
						NShared.MailInfo mailInfo2 = _globalMailList.Find((NShared.MailInfo e) => e.MailIndex == mailInfo.MailIndex);
						if (mailInfo2 != null)
						{
							_globalMailList.Remove(mailInfo2);
						}
						if (mailInfo.OpenRemainTime > 0)
						{
							DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
							mailInfo.OpenTime = ((mailInfo.OpenRemainTime > 0) ? Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow.AddSeconds(mailInfo.OpenRemainTime)) : Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow));
						}
						_globalMailList.Add(mailInfo);
					}
				}
				SceneAutoBehaviour<MailManagement>.instance.RequestMailList(lastIndex, 50, ResponseMailList, OnError);
			}, OnError);
		}
		else
		{
			SceneAutoBehaviour<MailManagement>.instance.RequestMailList(lastIndex, 50, ResponseMailList, OnError);
		}
	}

	private void ResponseMailList(NShared.MailInfo[] mailInfos, int[] openedMailIndices, int totalCount)
	{
		AddOpenedMailIndices(openedMailIndices);
		new List<NShared.MailInfo>();
		if (__Extension_Method_ICollection__.COUNT(mailInfos) == 0)
		{
			UpdateMailListPopup();
			return;
		}
		foreach (NShared.MailInfo mailInfo in mailInfos)
		{
			NShared.MailInfo mailInfo2 = _mailList.Find((NShared.MailInfo e) => e.MailIndex == mailInfo.MailIndex);
			if (mailInfo2 != null)
			{
				_mailList.Remove(mailInfo2);
			}
			if (mailInfo.OpenRemainTime > 0)
			{
				DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
				mailInfo.OpenTime = ((mailInfo.OpenRemainTime > 0) ? Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow.AddSeconds(mailInfo.OpenRemainTime)) : Vespa_DateTime_Extension_Method.ToNetworkString2(uTCNow));
			}
			_mailList.Add(mailInfo);
		}
		UpdateMailListPopup();
	}

	private void UpdateMailListPopup()
	{
		List<NShared.MailInfo> uIMailList = GetUIMailList();
		int totalCount = GetTotalCount();
		SceneAutoBehaviour<MailManagement>.instance.GetInstance<MailListPopup>().UpdateUIMailList(uIMailList, totalCount, 0L);
	}

	public void OnDeleteMail(long mailIndex)
	{
		NShared.MailInfo mailInfo = _mailList.Find((NShared.MailInfo e) => e.MailIndex == mailIndex);
		if (mailInfo != null)
		{
			_mailList.Remove(mailInfo);
			int totalCount = GetTotalCount();
			SceneAutoBehaviour<MailManagement>.instance.GetInstance<MailListPopup>().DeleteMail(mailIndex, totalCount);
		}
		else if (_globalMailList.RemoveAll((NShared.MailInfo e) => e.MailIndex == mailIndex) > 0)
		{
			int totalCount2 = GetTotalCount();
			SceneAutoBehaviour<MailManagement>.instance.GetInstance<MailListPopup>().DeleteMail(mailIndex, totalCount2);
		}
	}

	public void OnReceiveAll()
	{
		UpdateMail();
	}

	private void OnError()
	{
	}

	public List<NShared.MailInfo> GetUIMailList()
	{
		List<NShared.MailInfo> list = new List<NShared.MailInfo>();
		foreach (NShared.MailInfo mail in _mailList)
		{
			if (!ExpireTimeElapsed(mail.ExpireTime) && !OpenTimeElapsed(mail.OpenTime) && (mail.OpenIndex == 0 || _openedMailIndices.Find((int e) => e == mail.OpenIndex) == mail.OpenIndex))
			{
				list.Add(mail);
			}
		}
		foreach (NShared.MailInfo globalMail in _globalMailList)
		{
			if (!OpenTimeElapsed(globalMail.OpenTime))
			{
				list.Add(globalMail);
			}
		}
		return list;
	}

	public void AddOpenedMailIndices(int[] openedMailIndices)
	{
		if (__Extension_Method_ICollection__.COUNT(openedMailIndices) > 0)
		{
			_openedMailIndices.Clear();
			_openedMailIndices.AddRange(openedMailIndices);
		}
	}

	public int GetTotalCount()
	{
		int num = 0;
		foreach (NShared.MailInfo mail in _mailList)
		{
			if (!ExpireTimeElapsed(mail.ExpireTime) && !OpenTimeElapsed(mail.OpenTime) && (mail.OpenIndex == 0 || _openedMailIndices.Find((int e) => e == mail.OpenIndex) == mail.OpenIndex))
			{
				num++;
			}
		}
		foreach (NShared.MailInfo globalMail in _globalMailList)
		{
			if (!OpenTimeElapsed(globalMail.OpenTime))
			{
				num++;
			}
		}
		MailManagement.MailCount = num;
		return num;
	}

	public void ClearMailList()
	{
		List<NShared.MailInfo> list = new List<NShared.MailInfo>();
		foreach (NShared.MailInfo mail in _mailList)
		{
			if (!OpenTimeElapsed(mail.OpenTime) && (mail.OpenIndex == 0 || _openedMailIndices.Find((int e) => e == mail.OpenIndex) == mail.OpenIndex))
			{
				list.Add(mail);
			}
		}
		foreach (NShared.MailInfo item in list)
		{
			_mailList.Remove(item);
		}
	}

	public void ClearGlobalMailList()
	{
		List<NShared.MailInfo> list = new List<NShared.MailInfo>();
		foreach (NShared.MailInfo globalMail in _globalMailList)
		{
			if (!OpenTimeElapsed(globalMail.OpenTime))
			{
				list.Add(globalMail);
			}
		}
		foreach (NShared.MailInfo item in list)
		{
			_globalMailList.Remove(item);
		}
	}

	private bool ExpireTimeElapsed(string ExpireTime)
	{
		if (String_Extension_Method.IsNullOrEmpty(ExpireTime))
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(ExpireTime, out var o_dateTime))
		{
			return true;
		}
		if ((o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds >= 0.0)
		{
			return false;
		}
		return true;
	}

	private bool OpenTimeElapsed(string OpenTime)
	{
		if (String_Extension_Method.IsNullOrEmpty(OpenTime))
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(OpenTime, out var o_dateTime))
		{
			return true;
		}
		return DateTime.UtcNow.CompareTo(o_dateTime) < 0;
	}
}
