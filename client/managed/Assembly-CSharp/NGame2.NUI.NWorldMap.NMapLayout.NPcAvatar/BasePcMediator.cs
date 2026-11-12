using NGame2.NAccount;
using NGameAsset.NWorldMap;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class BasePcMediator : AvatarController.IListener
{
	public int MapWidth = 4096;

	public int MapHeight = 4096;

	private int _curDungeonIndex;

	public MapComponent MapComponent { get; set; }

	public Avatar Avatar { get; set; }

	public AvatarController AvatarController { get; set; }

	public MapLayout Layout => MapComponent.Layout;

	public NodeGraph NodeGraph => MapComponent.NodeGraph;

	public MapViewer Viewer => MapComponent.Viewer;

	public void OnUnloadMap()
	{
	}

	public void OnLoadMap()
	{
	}

	public void Close()
	{
	}

	public void SetDungeonIndex(int dungeonIndex)
	{
		_curDungeonIndex = dungeonIndex;
	}

	int AvatarController.IListener.GetChapterIndex()
	{
		return UserCampaignManager.ChapterIndex;
	}

	int AvatarController.IListener.GetDungeonIndex()
	{
		return _curDungeonIndex;
	}

	void AvatarController.IListener.SetDungeonIndex(int dungeonIndex)
	{
		SetDungeonIndex(dungeonIndex);
	}

	Avatar AvatarController.IListener.GetAvatar()
	{
		return Avatar;
	}

	NodeGraph AvatarController.IListener.GetNodeGraph()
	{
		return NodeGraph;
	}

	ScaleData AvatarController.IListener.GetScaleData()
	{
		return MapComponent.ScaleData;
	}

	void AvatarController.IListener.OnMoveBegin(int chapterIndex, int[] dungeonIndices)
	{
	}

	void AvatarController.IListener.OnMoveEnd()
	{
	}

	void AvatarController.IListener.OnVisitNode(int chapterIndex, int dungeonIndex)
	{
	}
}
