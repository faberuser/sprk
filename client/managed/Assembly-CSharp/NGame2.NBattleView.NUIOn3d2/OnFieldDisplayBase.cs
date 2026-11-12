using System.Collections.Generic;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3d2;

public abstract class OnFieldDisplayBase : LinkFactoryComponent, IInspectorGuiDrawer
{
	public enum EAttachType
	{
		Height,
		Hit,
		Center
	}

	private static Camera _mainCamera;

	public static Vector3 MainCameraRight = Vector3.right;

	public static Vector3 MainCameraUp = Vector3.up;

	public static int WidgetDepthOrder = 1;

	public Transform OffsetTransform;

	public UIWidget Root;

	[HideInInspector]
	public List<UIWidget> WidgetList = new List<UIWidget>();

	public int DepthOffset;

	public EAttachType AttachType;

	public bool FollowPivot;

	public float TimeLength;

	public Vector2 OffsetBegin = Vector2.zero;

	public Vector2 OffsetEnd = Vector2.zero;

	public Vector2 RandomOffset = Vector2.zero;

	public AnimationCurve CurveX = new AnimationCurve();

	public AnimationCurve CurveY = new AnimationCurve();

	public AnimationCurve CurveScale = new AnimationCurve();

	public AnimationCurve CurveAlpha = new AnimationCurve();

	private Transform _pivotTm;

	private Vector3 _pivotOffset;

	private TeamType _teamType;

	private float _elapsedTime;

	public static Camera MainCamera
	{
		get
		{
			return _mainCamera;
		}
		set
		{
			_mainCamera = value;
			if (_mainCamera == null)
			{
				MainCameraRight = Vector3.right;
				MainCameraUp = Vector3.up;
			}
			else
			{
				MainCameraRight = _mainCamera.transform.right;
				MainCameraUp = _mainCamera.transform.up;
			}
		}
	}

	public bool IsEnd => _elapsedTime >= TimeLength;

	public static void ResetStatic(Camera camera)
	{
		MainCamera = camera;
		WidgetDepthOrder = 1;
	}

	public static void ChainOnDestroy()
	{
		MainCamera = null;
	}

	protected bool Open(int creatureUid)
	{
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureUid);
		if (creature == null)
		{
			return false;
		}
		Transform transform = null;
		Vector3 pivotOffset = Vector3.zero;
		if (AttachType == EAttachType.Height)
		{
			transform = creature.RenderFacade.HeightTm;
		}
		else if (AttachType == EAttachType.Hit)
		{
			transform = creature.RenderFacade.AssetCreature.GetPivot(EPivotType.Hit);
		}
		else
		{
			if (AttachType != EAttachType.Center)
			{
				return false;
			}
			transform = creature.RenderFacade.transform;
			pivotOffset = creature.RenderFacade.ShapeBounds.center;
		}
		if (transform == null)
		{
			return false;
		}
		Open(transform, pivotOffset, creature.TeamType);
		base.gameObject.SetActive(value: true);
		return true;
	}

	private void Open(Transform pivotTm, Vector3 pivotOffset, TeamType teamType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(WidgetList); i++)
		{
			WidgetList[i].depth = WidgetDepthOrder + DepthOffset;
			WidgetDepthOrder++;
		}
		Vector2 i_this = new Vector2(RandomOffset.x * Random.Range(-1f, 1f), RandomOffset.y * Random.Range(-1f, 1f));
		_pivotTm = pivotTm;
		_pivotOffset = pivotOffset / Mathf.Max(Vespa_Unity_Vector2_Extension_Method.MaxComponent(Vespa_Unity_Vector3_Extension_Method.XY(base.transform.lossyScale)), 0.0001f) + Vespa_Unity_Vector2_Extension_Method.ToVec3(i_this);
		_teamType = teamType;
		_elapsedTime = 0f;
		base.transform.position = pivotTm.position;
		UpdateAnimation(0f);
	}

	public void ChainUpdate()
	{
		_elapsedTime += GlobalTimer.deltaTime;
		if (FollowPivot && _pivotTm != null)
		{
			base.transform.position = _pivotTm.position;
		}
		UpdateAnimation(Mathf.Clamp01(_elapsedTime / Vespa_Float_Extension_Method.NoneZero(TimeLength)));
	}

	private void UpdateAnimation(float frame)
	{
		Vector3 localPosition = MainCameraRight * Math.LerpOver(OffsetBegin.x, OffsetEnd.x, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveX, frame, frame)) + MainCameraUp * Math.LerpOver(OffsetBegin.y, OffsetEnd.y, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveY, frame, frame)) + _pivotOffset;
		if (_teamType == TeamType.Right)
		{
			localPosition.x = 0f - localPosition.x;
		}
		OffsetTransform.localPosition = localPosition;
		OffsetTransform.localScale = Vector3.one * Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveScale, frame, 1f);
		if (MainCamera != null)
		{
			OffsetTransform.rotation = MainCamera.transform.rotation;
		}
		Root.alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(CurveAlpha, frame, 1f);
	}
}
