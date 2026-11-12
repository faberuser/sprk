using System.Collections.Generic;
using NGame2.NUI.NWorldMap;
using NGame2.NUI.NWorldMap.NMapLayout;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class CampaignNodeNpcAvatarManager : GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>
{
	public class NpcUpdateInfo
	{
		public int ChapterIndex;

		public int DungeonIndex;

		public CampaignNodeNPCData.NPCAvatarState NpcState;
	}

	public List<NpcUpdateInfo> NpcUpdateInfoList = new List<NpcUpdateInfo>();

	public NpcUpdateInfo GetUpdateInfo(int dungeonIndex)
	{
		return NpcUpdateInfoList.Find((NpcUpdateInfo e) => e.DungeonIndex == dungeonIndex);
	}

	private NpcAvatarManager GetNpcAvaterManager()
	{
		if (!SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			return null;
		}
		MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		if (mapLayout == null)
		{
			return null;
		}
		return mapLayout.EventManager.NpcAvatarManager;
	}

	public void UpdateNpcState(CampaignNodeNPCData.NPCAvatarState updateState, bool clearList = false)
	{
		GetNpcAvaterManager()?.UpdateNPCState(NpcUpdateInfoList, updateState);
		if (clearList)
		{
			NpcUpdateInfoList.Clear();
		}
	}

	public void AddToNPCInfoList(int chapterIndex, int dungeonIndex, CampaignNodeNPCData.NPCAvatarState state)
	{
		NpcUpdateInfo item = new NpcUpdateInfo
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			NpcState = state
		};
		NpcUpdateInfoList.Add(item);
	}
}
