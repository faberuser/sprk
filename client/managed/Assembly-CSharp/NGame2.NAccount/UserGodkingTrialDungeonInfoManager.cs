using NShared;

namespace NGame2.NAccount;

public class UserGodkingTrialDungeonInfoManager : NShared.UserGodkingTrialDungeonInfoManager
{
	public int ClearedDungeonChapterIndex { get; set; }

	public void SetClearedDungeonChapterIndex(GodkingTrialDungeonInfo info)
	{
		if (info != null)
		{
			ClearedDungeonChapterIndex = info.ChapterIndex;
		}
	}

	public void Apply(GodkingTrialDungeonInfo info)
	{
		Add(info);
	}

	public void Apply(GodkingTrialDungeonInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
