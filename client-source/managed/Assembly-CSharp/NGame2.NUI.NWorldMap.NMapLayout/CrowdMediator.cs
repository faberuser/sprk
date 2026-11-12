using System.Collections;
using NGameAsset.NWorldMap;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class CrowdMediator : AvatarController.IListener
{
	public class CrowdMediatorInfo
	{
		public int ChapterIndex { get; set; }

		public int DungeonIndex { get; set; }

		public Avatar Avatar { get; set; }

		public MapComponent MapComponent { get; set; }

		public AvatarController AvatarController { get; set; }

		public CrowdType CrowdType { get; set; }
	}

	private DummyNodeIndicator _moveTargetIndicator;

	private CrowdMediatorInfo _crowdMediatorInfo;

	public int OrginChapterIndex { get; set; }

	public int OrginDungeonIndex { get; set; }

	public Avatar Avatar
	{
		get
		{
			if (_crowdMediatorInfo == null)
			{
				return null;
			}
			return _crowdMediatorInfo.Avatar;
		}
	}

	public AvatarController AvatarController
	{
		get
		{
			if (_crowdMediatorInfo == null)
			{
				return null;
			}
			return _crowdMediatorInfo.AvatarController;
		}
	}

	public MapComponent MapComponent
	{
		get
		{
			if (_crowdMediatorInfo == null)
			{
				return null;
			}
			return _crowdMediatorInfo.MapComponent;
		}
	}

	public MapLayout Layout
	{
		get
		{
			if (MapComponent == null)
			{
				return null;
			}
			return MapComponent.Layout;
		}
	}

	public NodeGraph NodeGraph
	{
		get
		{
			if (MapComponent == null)
			{
				return null;
			}
			return MapComponent.NodeGraph;
		}
	}

	public MapViewer Viewer
	{
		get
		{
			if (MapComponent == null)
			{
				return null;
			}
			return MapComponent.Viewer;
		}
	}

	public bool IsCollided { get; set; }

	public void Init(CrowdMediatorInfo info)
	{
		_crowdMediatorInfo = info;
	}

	public void Close()
	{
		if (_crowdMediatorInfo != null)
		{
			if (_crowdMediatorInfo.Avatar != null)
			{
				_crowdMediatorInfo.Avatar.Close();
				_crowdMediatorInfo.Avatar = null;
			}
			_crowdMediatorInfo.MapComponent = null;
			_crowdMediatorInfo.AvatarController = null;
			_crowdMediatorInfo = null;
			ClearMoveTargetIndicator();
		}
	}

	private void ClearMoveTargetIndicator()
	{
		if (_moveTargetIndicator != null)
		{
			_moveTargetIndicator.CloseImmediately();
			_moveTargetIndicator = null;
		}
	}

	public void SetDungeonIndex(int dungeonIndex)
	{
		if (_crowdMediatorInfo != null)
		{
			_crowdMediatorInfo.DungeonIndex = dungeonIndex;
		}
	}

	private IEnumerator BeginMove(int chapterIndex, int[] dungeonIndices)
	{
		yield return new WaitForSeconds(0.2f);
		AvatarController.AddMovePath(dungeonIndices);
		Viewer.ChasePlayer();
		ClearMoveTargetIndicator();
		yield return new WaitForSeconds(3f);
		yield return null;
	}

	int AvatarController.IListener.GetChapterIndex()
	{
		if (_crowdMediatorInfo == null)
		{
			return -1;
		}
		return _crowdMediatorInfo.ChapterIndex;
	}

	int AvatarController.IListener.GetDungeonIndex()
	{
		if (_crowdMediatorInfo == null)
		{
			return -1;
		}
		return _crowdMediatorInfo.DungeonIndex;
	}

	void AvatarController.IListener.SetDungeonIndex(int dungeonIndex)
	{
		SetDungeonIndex(dungeonIndex);
	}

	Avatar AvatarController.IListener.GetAvatar()
	{
		if (_crowdMediatorInfo == null)
		{
			return null;
		}
		return _crowdMediatorInfo.Avatar;
	}

	NodeGraph AvatarController.IListener.GetNodeGraph()
	{
		return NodeGraph;
	}

	ScaleData AvatarController.IListener.GetScaleData()
	{
		if (_crowdMediatorInfo == null || _crowdMediatorInfo.MapComponent == null)
		{
			return null;
		}
		return _crowdMediatorInfo.MapComponent.ScaleData;
	}

	void AvatarController.IListener.OnMoveBegin(int chapterIndex, int[] dungeonIndices)
	{
		if (AvatarController != null)
		{
			VespaCoroutine.Start(BeginMove(chapterIndex, dungeonIndices));
		}
	}

	void AvatarController.IListener.OnMoveEnd()
	{
		ClearMoveTargetIndicator();
	}

	void AvatarController.IListener.OnVisitNode(int chapterIndex, int dungeonIndex)
	{
	}
}
