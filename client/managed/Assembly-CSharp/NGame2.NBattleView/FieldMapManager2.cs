using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NScene.NPreloader;
using NGame2.NUI;
using NGameAsset;
using NGameAsset.NBattleFieldMap2;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView;

public class FieldMapManager2 : BattleViewGenericSingleton<FieldMapManager2>, IInspectorGuiDrawer
{
	public class Creature
	{
		public int Uid;

		public RenderFacade RenderFacade;

		public Creature(int uid, RenderFacade renderFacade)
		{
			Uid = uid;
			RenderFacade = renderFacade;
		}
	}

	public const int SIDE_N = 0;

	public const int SIDE_L = 1;

	public const int SIDE_R = 2;

	public const int SIDE_U = 3;

	public const int SIDE_D = 4;

	private Builder _mapBuilder;

	private IAsyncLoad<IBattleFieldMap2> _mapAsync;

	private List<GuildFlagFacade> _guildFlagList = new List<GuildFlagFacade>();

	private Coroutine _openCo;

	private int _creatureUid = 1;

	private List<Creature> _creatureContainer = new List<Creature>();

	private SimplePreloader _preloader;

	public string FieldName { get; private set; }

	public GuildFlagData GuildFlagData { get; private set; }

	public IRuntime NodeTreeRuntime { get; set; }

	private void Start()
	{
	}

	public void Open(string fieldName, GuildFlagData guildFlagData, System.Action onComplete)
	{
		Clear();
		FieldName = fieldName;
		GuildFlagData = guildFlagData;
		_openCo = VespaCoroutine.Start(this, coOpen(onComplete));
	}

	public void OnEvent(EBattleFieldEventType eventType, int eventValue)
	{
		if (_mapAsync != null)
		{
			_mapAsync.Value.OnEvent(eventType, eventValue);
		}
	}

	public Vector3 AdjustCameraPos(CameraStat.CameraStock cameraStock, Vector3 posOnCameraSpace)
	{
		if (!cameraStock.FitToField)
		{
			return posOnCameraSpace;
		}
		CameraPlaneBounds cameraPlaneBounds = CalcCameraPlaneBounds();
		if (cameraPlaneBounds == null)
		{
			return posOnCameraSpace;
		}
		Vector3 vector = posOnCameraSpace;
		for (int i = 0; i < cameraPlaneBounds.Count; i++)
		{
			Plane plane = cameraPlaneBounds.Planes[i];
			if (!plane.GetSide(vector))
			{
				Ray ray = new Ray(vector, cameraPlaneBounds.RayDir[i]);
				if (plane.Raycast(ray, out var enter))
				{
					vector = ray.GetPoint(enter);
				}
			}
		}
		return vector;
	}

	private CameraPlaneBounds CalcCameraPlaneBounds()
	{
		if (_mapBuilder == null)
		{
			return null;
		}
		if (!Game3DRoot.IsValidInstance)
		{
			return null;
		}
		CameraPlaneBounds cameraPlaneBounds = new CameraPlaneBounds();
		cameraPlaneBounds.Pivot[1] = CameraStat.Basic.CameraRotationInverse * _mapBuilder.MapBase.LeftPivot + _mapBuilder.MapBase.LeftBillboardOffset;
		cameraPlaneBounds.Pivot[2] = CameraStat.Basic.CameraRotationInverse * _mapBuilder.MapBase.RightPivot + _mapBuilder.MapBase.RightBillboardOffset;
		cameraPlaneBounds.Pivot[3] = CameraStat.Basic.CameraRotationInverse * _mapBuilder.MapBase.TopPivot + _mapBuilder.MapBase.TopBillboardOffset;
		cameraPlaneBounds.Pivot[4] = CameraStat.Basic.CameraRotationInverse * _mapBuilder.MapBase.BottomPivot + _mapBuilder.MapBase.BottomBillboardOffset;
		float fieldOfView = CameraStat.Basic.FieldOfView;
		float fovH = CameraStat.Basic.FovH;
		Vector3 inNormal = Quaternion.AngleAxis((0f - fovH) * 0.5f, Vector3.up) * Vector3.right;
		Vector3 inNormal2 = Quaternion.AngleAxis(fovH * 0.5f, Vector3.up) * Vector3.left;
		Vector3 inNormal3 = Quaternion.AngleAxis((0f - fieldOfView) * 0.5f, Vector3.right) * Vector3.down;
		Vector3 inNormal4 = Quaternion.AngleAxis(fieldOfView * 0.5f, Vector3.right) * Vector3.up;
		cameraPlaneBounds.Planes[1] = new Plane(inNormal, cameraPlaneBounds.Pivot[1]);
		cameraPlaneBounds.Planes[2] = new Plane(inNormal2, cameraPlaneBounds.Pivot[2]);
		cameraPlaneBounds.Planes[3] = new Plane(inNormal3, cameraPlaneBounds.Pivot[3]);
		cameraPlaneBounds.Planes[4] = new Plane(inNormal4, cameraPlaneBounds.Pivot[4]);
		cameraPlaneBounds.CalcNearPivotPlane();
		cameraPlaneBounds.RayDir[0] = Vector3.forward;
		cameraPlaneBounds.RayDir[1] = Vector3.right;
		cameraPlaneBounds.RayDir[2] = Vector3.left;
		cameraPlaneBounds.RayDir[3] = Vector3.down;
		cameraPlaneBounds.RayDir[4] = Vector3.up;
		return cameraPlaneBounds;
	}

	public void OnEventCamera(EventCamera.EventValue current)
	{
		if (_mapAsync != null)
		{
			_mapAsync.Value.GetRenderGroupModifier().ColorMix(Vespa_Unity_Color_Extension_Method.Lerp(Color.black, Colors.zero, current.BattleFieldAlpha));
		}
	}

	public void NodeTreeStart(TreeData nodeTree)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			NodeTreeStop();
			BuildChildRootRuntime buildChildRootRuntime = default(BuildChildRootRuntime);
			buildChildRootRuntime.Open(nodeTree);
			NodeTreeRuntime = buildChildRootRuntime.Build(BattleViewGenericSingleton<CreatureManager>.instance, GlobalTimer.DefaultLocal, null, 0);
			if (NodeTreeRuntime != null)
			{
				SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(NodeTreeRuntime.CaptureSelf(), immediatelyStart: true);
			}
		}
	}

	public void NodeTreeStop()
	{
		if (NodeTreeRuntime != null)
		{
			NodeTreeRuntime.RequestStop(ERequestStop.Normal);
			NodeTreeRuntime.ReleaseSelf();
			NodeTreeRuntime = null;
		}
	}

	public bool NodeTreePlaying()
	{
		if (NodeTreeRuntime == null)
		{
			return false;
		}
		return NodeTreeRuntime.IsPlaying;
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			Clear();
		}
	}

	private void Clear()
	{
		if (_openCo != null)
		{
			VespaCoroutine.Stop(this, _openCo);
			_openCo = null;
		}
		if (_mapAsync != null)
		{
			Factory.ReleaseMap(_mapAsync);
			_mapAsync = null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_guildFlagList); i++)
		{
			_guildFlagList[i].ReleaseSelf();
		}
		_guildFlagList.Clear();
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_creatureContainer); j++)
		{
			_creatureContainer[j].RenderFacade.Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_creatureContainer);
		NodeTreeStop();
		if (_preloader != null)
		{
			_preloader.ReleaseAsyncLoadAll();
		}
	}

	private IEnumerator coOpen(System.Action onComplete)
	{
		_mapBuilder = SingletonAuto<BuilderContainer>.instance.Get(FieldName);
		if (_mapBuilder == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		if (_mapBuilder.UseCustomFieldCamera)
		{
			CameraStat.ResetCameraStock(_mapBuilder.CustomFieldCamera);
		}
		else
		{
			CameraStat.ResetCameraStock();
		}
		Quaternion.AngleAxis(CameraStat.Basic.LookAngle, Vector3.right);
		BattleFieldMapStock stock = new BattleFieldMapStock(FieldName, string.Empty);
		CalcCameraPlaneBounds();
		_mapAsync = SingletonAuto<Factory>.instance.CreateAsync(stock);
		if (_mapAsync == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		_mapAsync.Value.SetParent(base.transform, Game3DRoot.instance.GameCamera.MainCamera);
		while (!_mapAsync.IsDone)
		{
			yield return null;
		}
		if (!_mapAsync.IsValid)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		CreateFieldMapCreature();
		CreateGuildFlag();
		StartNodeTree();
		Vespa_System_Action_Extension_Method.fire(onComplete);
		_openCo = null;
	}

	private void StartNodeTree()
	{
		if (_preloader == null)
		{
			_preloader = new SimplePreloader(null);
		}
		VespaCoroutine.Start(this, CreateRuntime());
	}

	private IEnumerator CreateRuntime()
	{
		if (_mapBuilder == null || _mapBuilder.MapBase.FieldMapNodeTree == null || !_mapBuilder.MapBase.FieldMapNodeTree.IsValid())
		{
			yield break;
		}
		string packPath = _mapBuilder.MapBase.FieldMapNodeTree.PackPath;
		string nodeTreeCode = _mapBuilder.MapBase.FieldMapNodeTree.TreeCode;
		PackLoad packLoad = SingletonAuto<PackContainer>.instance.GetPack(packPath);
		while (!packLoad.ISDONE())
		{
			yield return null;
		}
		TreeData nodeTree = null;
		if (packLoad.ISVALID())
		{
			nodeTree = packLoad.Value.GetNodeTree(nodeTreeCode);
		}
		if (nodeTree != null)
		{
			nodeTree.ReserveCache(_preloader);
			while (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
			{
				yield return null;
			}
			NodeTreeStart(nodeTree);
		}
	}

	private void CreateFieldMapCreature()
	{
		if (_mapBuilder == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_mapBuilder.MapBase.FieldMapCreatureDataList); i++)
		{
			MapBaseData.FieldMapCreatureData fieldMapCreatureData = _mapBuilder.MapBase.FieldMapCreatureDataList[i];
			RenderFacade renderFacade = CreateRenderFacade(fieldMapCreatureData);
			if (!(renderFacade == null))
			{
				AnimationData.StockData stockData = new AnimationData.StockData();
				stockData.State = AnimationData.EState.Action;
				stockData.AnimationKey = fieldMapCreatureData.AnimationKey;
				stockData.MotionEnd = fieldMapCreatureData.MotionEnd;
				renderFacade.Action.Motion(stockData, null);
				_creatureContainer.Add(new Creature(_creatureUid++, renderFacade));
			}
		}
	}

	private RenderFacade CreateRenderFacade(MapBaseData.FieldMapCreatureData fieldMapCreatureData)
	{
		if (fieldMapCreatureData == null || String_Extension_Method.IsNullOrEmpty(fieldMapCreatureData.Code))
		{
			return null;
		}
		string code = fieldMapCreatureData.Code;
		int star = fieldMapCreatureData.Star;
		RenderFacade renderFacade = new RenderFacadeBuilder(base.transform, TeamType.Left, code, star)
		{
			LoadLevel = CreatureStock.ELoadLevel.IncludeFacial
		}.Build();
		renderFacade.transform.localPosition = fieldMapCreatureData.Position;
		renderFacade.transform.localRotation = fieldMapCreatureData.Rotation;
		renderFacade.transform.localScale = fieldMapCreatureData.Scale;
		return renderFacade;
	}

	private bool CreatureLoaded()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_creatureContainer); i++)
		{
			if (!_creatureContainer[i].RenderFacade.AssetCreatureAsync.IsDone)
			{
				return false;
			}
		}
		return true;
	}

	public List<Creature> GetCreatureContainer()
	{
		return _creatureContainer;
	}

	public RenderFacade GetRenderFacade(long uid)
	{
		return _creatureContainer.Find((Creature creature) => creature.Uid == uid)?.RenderFacade;
	}

	private List<MapBaseData.GuildFlagPivot> GetGuildFlagPivotList(TeamType teamType)
	{
		if (_mapBuilder == null)
		{
			return null;
		}
		if (teamType == TeamType.Left)
		{
			return _mapBuilder.MapBase.GuildFlagForLeftList;
		}
		return _mapBuilder.MapBase.GuildFlagForRightList;
	}

	private MapBaseData.GuildFlagPivot GetGuildFlagPivot(TeamType teamType, int index)
	{
		if (_mapBuilder == null)
		{
			return null;
		}
		if (teamType == TeamType.Left)
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_mapBuilder.MapBase.GuildFlagForLeftList, index))
			{
				return null;
			}
			return _mapBuilder.MapBase.GuildFlagForLeftList[index];
		}
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_mapBuilder.MapBase.GuildFlagForRightList, index))
		{
			return null;
		}
		return _mapBuilder.MapBase.GuildFlagForRightList[index];
	}

	private void CreateGuildFlag()
	{
		if (GuildFlagData == null || GuildFlagData.EGuildFlagDisplayType == EGuildFlagDisplayType.None)
		{
			return;
		}
		string flagCode = Vespa_Generic_IList_Extension_Method.GetLast(GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetAllData()).FlagCode;
		GuildArenaSeasonRewardData data = GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetData(GuildFlagData.LeftGuildFlag);
		if (data != null)
		{
			flagCode = data.FlagCode;
		}
		string guildLogo3dBgIconPath = UIHelper.GetGuildLogo3dBgIconPath(GuildFlagData.LeftGuildFlagBG);
		string guildLogoIconPath = UIHelper.GetGuildLogoIconPath(GuildFlagData.LeftGuildLogo);
		List<MapBaseData.GuildFlagPivot> guildFlagPivotList = GetGuildFlagPivotList(TeamType.Left);
		if (GuildFlagData.ExistLeftGuild && !__Extension_Method_ICollection__.IsNullOrEmpty(guildFlagPivotList) && !String_Extension_Method.IsNullOrEmpty(guildLogoIconPath))
		{
			foreach (MapBaseData.GuildFlagPivot item in guildFlagPivotList)
			{
				if (item != null)
				{
					GuildFlagFacade guildFlagFacade = GuildFlagFacade.CreateFlag(flagCode, guildLogoIconPath, guildLogo3dBgIconPath, base.gameObject.layer);
					guildFlagFacade.ApplyGuildFlag(item);
					_guildFlagList.Add(guildFlagFacade);
				}
			}
		}
		if (GuildFlagData.EGuildFlagDisplayType == EGuildFlagDisplayType.Mine)
		{
			return;
		}
		string flagCode2 = Vespa_Generic_IList_Extension_Method.GetLast(GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetAllData()).FlagCode;
		data = null;
		data = GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetData(GuildFlagData.RightGuildFlag);
		if (data != null)
		{
			flagCode2 = data.FlagCode;
		}
		string guildLogo3dBgIconPath2 = UIHelper.GetGuildLogo3dBgIconPath(GuildFlagData.RightGuildFlagBG);
		string guildLogoIconPath2 = UIHelper.GetGuildLogoIconPath(GuildFlagData.RightGuildLogo);
		List<MapBaseData.GuildFlagPivot> guildFlagPivotList2 = GetGuildFlagPivotList(TeamType.Right);
		if (!GuildFlagData.ExistRightGuild || __Extension_Method_ICollection__.IsNullOrEmpty(guildFlagPivotList2) || String_Extension_Method.IsNullOrEmpty(guildLogoIconPath2))
		{
			return;
		}
		foreach (MapBaseData.GuildFlagPivot item2 in guildFlagPivotList2)
		{
			if (item2 != null)
			{
				GuildFlagFacade guildFlagFacade2 = GuildFlagFacade.CreateFlag(flagCode2, guildLogoIconPath2, guildLogo3dBgIconPath2, base.gameObject.layer);
				guildFlagFacade2.ApplyGuildFlag(item2);
				_guildFlagList.Add(guildFlagFacade2);
			}
		}
	}

	public void PlayGuildFlag()
	{
		foreach (GuildFlagFacade guildFlag in _guildFlagList)
		{
			if (guildFlag != null)
			{
				guildFlag.Play();
			}
		}
	}

	public void ChainUpdate()
	{
		if (_mapAsync != null)
		{
			_mapAsync.Value.ChainUpdate(MainTimer.DefaultLocal, Game3DRoot.instance.GameCamera.MainCamera);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_creatureContainer); i++)
			{
				_creatureContainer[i].RenderFacade.ChainUpdate();
			}
		}
	}
}
