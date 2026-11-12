using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NSubStory;

public class SubStoryCostumeOpenChecker : ICostumeOpenChecker
{
	private CostumeOpenChecker _costumeOpenChecker;

	private Dictionary<SubStoryChapterDungeonKey, bool> _dicsUnlockCostumeChecker = new Dictionary<SubStoryChapterDungeonKey, bool>();

	public SubStoryCostumeOpenChecker(CostumeOpenChecker costumeOpenChecker)
	{
		_costumeOpenChecker = costumeOpenChecker;
		SetUnlockCostumeChecker();
	}

	private void SetUnlockCostumeChecker()
	{
		foreach (ChapterDungeonKey2 item in GameRoot.Instance.SubStoryDataContainer.GetDungeons_for_UnlockCostume())
		{
			int chapterIndex = item.ChapterIndex;
			int dungeonIndex = item.DungeonIndex;
			bool value = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
			_dicsUnlockCostumeChecker[new SubStoryChapterDungeonKey(chapterIndex, dungeonIndex)] = value;
		}
	}

	private bool ExistOpenCostume(int chapterIndex, int dungeonIndex)
	{
		SubStoryChapterDungeonKey key = new SubStoryChapterDungeonKey(chapterIndex, dungeonIndex);
		if (!_dicsUnlockCostumeChecker.ContainsKey(key))
		{
			return false;
		}
		bool num = _dicsUnlockCostumeChecker[key];
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
		_dicsUnlockCostumeChecker[key] = flag;
		if (!num && flag)
		{
			return true;
		}
		return false;
	}

	private void OnChanged(int chapterIndex, int dungeonIndex)
	{
		if (ExistOpenCostume(chapterIndex, dungeonIndex))
		{
			SubStoryData data = GameRoot.Instance.SubStoryDataContainer.GetData(chapterIndex, dungeonIndex);
			_costumeOpenChecker.Enqueue(GameRoot.Instance.CostumeDataContainer.GetData(data.UnlockCostumeIndex));
		}
	}

	public void OnChanged(ChapterDungeonInfo changedInfo)
	{
		if (changedInfo != null)
		{
			OnChanged(changedInfo.ChapterIndex, changedInfo.DungeonIndex);
		}
	}
}
