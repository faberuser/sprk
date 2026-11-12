using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NRender;

public class RenderFacade : MonoBehaviour, IInspectorGuiDrawer, IRenderGroupModifier
{
	public Transform HeightTm;

	public Transform DirectionReal;

	public Transform DirectionView;

	public Transform FlipTM;

	public ICreatureLoad AssetCreatureAsync;

	private IncreaseDic<string, float> _runtimeScaleDic = new IncreaseDic<string, float>();

	private IncreaseDic<string, Vector3> _runtimeOffsetDic = new IncreaseDic<string, Vector3>();

	private float _runtimeFinalScale = 1f;

	private Bounds _baseShapeBounds;

	private bool _ignoreFlip;

	public TeamType TeamType { get; private set; }

	public bool IsFlip
	{
		get
		{
			if (!_ignoreFlip)
			{
				return TeamType == TeamType.Right;
			}
			return false;
		}
	}

	public LocalTimer LocalTimer { get; private set; }

	public float DeltaTime
	{
		get
		{
			if (LocalTimer != null)
			{
				return LocalTimer.deltaTime;
			}
			return MainTimer.deltaTime;
		}
	}

	public AnimationData AnimationData { get; private set; }

	public Action Action { get; private set; }

	public ExtraAlphaController ExtraAlphaController { get; private set; }

	public ExtraScaleController ExtraScaleController { get; private set; }

	public ExtraOffsetController ExtraOffsetController { get; private set; }

	public ColorEffect ColorEffect { get; private set; }

	public ShakeEffect ShakeEffect { get; private set; }

	public PivotData Pivot { get; private set; }

	public RenderGroupModifier2 RenderGroupModifier
	{
		get
		{
			if (AssetCreatureAsync == null || AssetCreatureAsync.Value == null)
			{
				return null;
			}
			return AssetCreatureAsync.Value.GetRenderGroupModifier();
		}
	}

	public ICreature AssetCreature
	{
		get
		{
			if (AssetCreatureAsync == null)
			{
				return null;
			}
			return AssetCreatureAsync.Value;
		}
	}

	public Bounds ShapeBounds { get; private set; }

	public bool Opened { get; private set; }

	public bool Open(TeamType teamType, ICreatureLoad assetCreatureAsync, Bounds baseShapeBounds, bool ignoreFlip)
	{
		if (Opened)
		{
			return false;
		}
		Opened = true;
		_ignoreFlip = ignoreFlip;
		TeamType = teamType;
		AssetCreatureAsync = assetCreatureAsync;
		_baseShapeBounds = baseShapeBounds;
		ShapeBounds = _baseShapeBounds;
		CreateTransform();
		RefreshFlipTm();
		DirectionReal.localRotation = Quaternion.identity;
		DirectionView.localRotation = Quaternion.identity;
		AnimationData = new AnimationData(this);
		Action = new Action(this);
		ExtraAlphaController = new ExtraAlphaController(this);
		ExtraScaleController = new ExtraScaleController(this);
		ExtraOffsetController = new ExtraOffsetController(this);
		ColorEffect = new ColorEffect(this);
		ShakeEffect = new ShakeEffect(this);
		Pivot = new PivotData(this);
		VectorXZ direction = ((TeamType == TeamType.Left) ? VectorXZ.right : (-VectorXZ.right));
		Action.SetDirection(direction, blendMotion: false);
		Action.Idle();
		return true;
	}

	private void OnEnable()
	{
		if (AssetCreatureAsync != null && AssetCreature != null)
		{
			AssetCreature.Visit(OnEnableVisitor.Default);
		}
	}

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit && AssetCreatureAsync != null)
		{
			CreatureContainer.ReleaseCreature(AssetCreatureAsync);
			AssetCreatureAsync = null;
		}
	}

	public void SetTeamType(TeamType teamType)
	{
		if (TeamType != teamType)
		{
			TeamType = teamType;
			RefreshFlipTm();
		}
	}

	public void SetRuntimeOffset(string offsetName, Vector3 offset)
	{
		if (!String_Extension_Method.IsNullOrEmpty(offsetName))
		{
			_runtimeOffsetDic.AddAndOverride(offsetName, offset);
		}
	}

	public Vector3 GetRuntimeOffset(string offsetName)
	{
		if (String_Extension_Method.IsNullOrEmpty(offsetName))
		{
			return Vector3.zero;
		}
		return _runtimeOffsetDic.GETVALUE(offsetName, Vector3.zero);
	}

	public void SetRuntimeScale(string scaleName, float scale)
	{
		if (!String_Extension_Method.IsNullOrEmpty(scaleName))
		{
			_runtimeScaleDic.AddAndOverride(scaleName, scale);
			RefreshScale();
		}
	}

	public float GetRuntimeScale(string scaleName)
	{
		if (String_Extension_Method.IsNullOrEmpty(scaleName))
		{
			return 1f;
		}
		return _runtimeScaleDic.GETVALUE(scaleName, 1f);
	}

	private void RefreshScale()
	{
		float num = 1f;
		for (int i = 0; i < _runtimeScaleDic.Count; i++)
		{
			num *= _runtimeScaleDic.GetValueByIndex(i);
		}
		num = Vespa_Float_Extension_Method.NoneZero(num);
		if (!Mathf.Approximately(_runtimeFinalScale, num))
		{
			_runtimeFinalScale = num;
			float x = _baseShapeBounds.size.x * _runtimeFinalScale;
			float z = _baseShapeBounds.size.z * _runtimeFinalScale;
			float num2 = _baseShapeBounds.size.y * _runtimeFinalScale;
			ShapeBounds = new Bounds(Vector3.up * num2 * 0.5f, new Vector3(x, num2, z));
			HeightTm.localPosition = Vector3.up * num2;
			RefreshFlipTm();
		}
	}

	private void RefreshFlipTm()
	{
		if (!IsFlip)
		{
			FlipTM.localRotation = Quaternion.identity;
			FlipTM.localScale = Vector3.one * _runtimeFinalScale;
		}
		else
		{
			FlipTM.localRotation = Quaternion.AngleAxis(180f, Vector3.right);
			FlipTM.localScale = -Vector3.one * _runtimeFinalScale;
		}
	}

	public void SetTimer(LocalTimer localTimer)
	{
		LocalTimer = localTimer;
		if (AssetCreatureAsync != null)
		{
			AssetCreatureAsync.Value.SetTimer(LocalTimer);
		}
	}

	private void CreateTransform()
	{
		GameObject gameObject = new GameObject("Height");
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, base.transform);
		HeightTm = gameObject.transform;
		HeightTm.localPosition = Vector3.up * ShapeBounds.size.y;
		GameObject gameObject2 = new GameObject("DirectionReal");
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject2.transform, base.transform);
		DirectionReal = gameObject2.transform;
		GameObject gameObject3 = new GameObject("DirectionView");
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject3.transform, base.transform);
		DirectionView = gameObject3.transform;
		GameObject gameObject4 = new GameObject("FlipTM");
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject4.transform, base.transform);
		FlipTM = gameObject4.transform;
		AssetCreatureAsync.Value.SetParent(FlipTM);
	}

	public void Close()
	{
		if (Opened)
		{
			Opened = false;
			if (AssetCreatureAsync != null)
			{
				CreatureContainer.ReleaseCreature(AssetCreatureAsync);
				AssetCreatureAsync = null;
			}
			_runtimeScaleDic.Clear();
			_runtimeOffsetDic.Clear();
			VespaObject.DestroyGo(FlipTM);
			VespaObject.DestroyGo(DirectionView);
			VespaObject.DestroyGo(DirectionReal);
			VespaObject.DestroyGo(HeightTm);
		}
	}

	public void EnableAnimationSound(bool enable)
	{
		if (AnimationData != null)
		{
			AnimationData.SetSFXEnable(enable);
		}
	}

	public void EnableKeyword(string keyword)
	{
		RenderGroupModifier.SetMaterialKeyword(keyword, isEnable: true);
	}

	public void DisableKeyword(string keyword)
	{
		RenderGroupModifier.SetMaterialKeyword(keyword, isEnable: false);
	}

	public void ChainUpdate()
	{
		if (Action != null)
		{
			Action.ChainUpdate();
		}
		if (AnimationData != null)
		{
			AnimationData.ChainUpdate();
		}
		if (ExtraAlphaController != null)
		{
			ExtraAlphaController.ChainUpdate();
		}
		if (ExtraScaleController != null)
		{
			ExtraScaleController.ChainUpdate();
		}
		if (ExtraOffsetController != null)
		{
			ExtraOffsetController.ChainUpdate();
		}
		if (ColorEffect != null)
		{
			ColorEffect.ChainUpdate();
		}
		if (ShakeEffect != null)
		{
			ShakeEffect.ChainUpdate();
			Vector3 finalValue = ShakeEffect.FinalValue;
			for (int i = 0; i < _runtimeOffsetDic.Count; i++)
			{
				finalValue += _runtimeOffsetDic.GetValueByIndex(i);
			}
			if (FlipTM != null)
			{
				FlipTM.localPosition = finalValue;
			}
		}
		if (AssetCreature != null)
		{
			SprkGraphicsOptions.UpdatePreviewCreature(AssetCreature, this);
		}
	}
}
