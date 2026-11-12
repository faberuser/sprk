using NGame2.N3D;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NGame3DWidget;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI;

public class UICreatureWidget : UIWidgetExtraRenderQ
{
	[SerializeField]
	private Transform _pivotTm;

	private bool _loadComplete;

	private RenderQueueVisitor _renderQueueVisitor = new RenderQueueVisitor();

	private ILocalTimer _localTimer;

	public ILocalTimer LocalTimer
	{
		get
		{
			if (_localTimer == null)
			{
				return UnityLocalTimer.Default;
			}
			return _localTimer;
		}
		set
		{
			_localTimer = value;
		}
	}

	public bool LoadComplete
	{
		get
		{
			if (RenderFacade == null)
			{
				return false;
			}
			return RenderFacade.AssetCreatureAsync.ISDONE();
		}
	}

	public Transform PivotTm
	{
		get
		{
			if (!(_pivotTm == null))
			{
				return _pivotTm;
			}
			return base.transform;
		}
	}

	public RenderFacade RenderFacade { get; private set; }

	public int RenderLayer
	{
		get
		{
			if (panel != null)
			{
				return panel.gameObject.layer;
			}
			return base.gameObject.layer;
		}
	}

	public void Open(CreatureData creatureData, int star, int costumeIndex = 0, CreatureStock.ELoadLevel loadLevel = CreatureStock.ELoadLevel.Normal, float alpha = 1f)
	{
		Close();
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(PivotTm, TeamType.Left, creatureData.CodeName, star);
		renderFacadeBuilder.LoadLevel = loadLevel;
		if (costumeIndex != 0)
		{
			renderFacadeBuilder.SetCostume(costumeIndex);
		}
		if (RenderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(RenderFacade);
			SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		}
		RenderFacade = renderFacadeBuilder.Build();
		if (RenderFacade != null)
		{
			RenderFacade.DISABLEGROUNDCLIP();
			RenderFacade.SetTimer(new LocalTimer(LocalTimer));
			RenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			RenderFacade.Action.SetDirection(-VectorXZ.up, blendMotion: false);
			RenderFacade.RenderGroupModifier.SetExtraAlpha(alpha);
			_loadComplete = false;
		}
	}

	public void OpenPet(PetInfo petInfo, CreatureStock.ELoadLevel loadLevel = CreatureStock.ELoadLevel.Normal)
	{
		Close();
		if (RenderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(RenderFacade);
			SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		}
		if (petInfo != null)
		{
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(PivotTm, TeamType.Left, petInfo);
			renderFacadeBuilder.LoadLevel = loadLevel;
			RenderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
			if (RenderFacade != null)
			{
				RenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
				RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
				RenderFacade.RenderGroupModifier.SetExtraAlpha(0f);
				_loadComplete = false;
			}
		}
	}

	public void Open(CachedHeroInfo cachedHeroInfo)
	{
		Close();
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(PivotTm, TeamType.Left, cachedHeroInfo);
		RenderFacade = renderFacadeBuilder.Build();
		if (RenderFacade != null)
		{
			RenderFacade.DISABLEGROUNDCLIP();
			RenderFacade.SetTimer(new LocalTimer(LocalTimer));
			RenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			RenderFacade.Action.SetDirection(-VectorXZ.up, blendMotion: false);
			_loadComplete = false;
		}
	}

	public void OpneMission(CachedHeroInfo cachedHeroInfo)
	{
		Close();
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(PivotTm, TeamType.Left, cachedHeroInfo);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		RenderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		if (RenderFacade != null)
		{
			RenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
		}
	}

	public void SetCustomIdle(DefineCreature.EMotion motionType, string idleName)
	{
		if (!(RenderFacade == null))
		{
			RenderFacade.Action.SetCustomIdle(new AnimationKeyData(motionType, idleName));
		}
	}

	public void SetEmotiAction(string animationKey)
	{
		if (!(RenderFacade == null))
		{
			SetAction(DefineCreature.EMotion.EmotiMotion, animationKey);
		}
	}

	public void SetNamedAction(string animationKey)
	{
		if (!(RenderFacade == null))
		{
			SetAction(DefineCreature.EMotion.NamedAction, animationKey);
		}
	}

	public void SetMotionTypeAction(DefineCreature.EMotion motionType)
	{
		if (!(RenderFacade == null))
		{
			SetAction(motionType, null);
		}
	}

	private void SetAction(DefineCreature.EMotion motionType, string animationKey)
	{
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		stockData.AnimationKey = CreateAnimationKey(motionType, animationKey);
		stockData.MotionEnd = DefineCreature.EMotionEnd.End;
		RenderFacade.Action.Motion(stockData, null);
	}

	private AnimationKeyData CreateAnimationKey(DefineCreature.EMotion motionType, string animationKey)
	{
		return motionType switch
		{
			DefineCreature.EMotion.EmotiMotion => AnimationKeyData.CreateEmotiMotion(animationKey), 
			DefineCreature.EMotion.NamedAction => AnimationKeyData.CreateNamedAction(animationKey), 
			_ => AnimationKeyData.CreateMotionType(motionType), 
		};
	}

	public void Close()
	{
		if (RenderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(RenderFacade);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (RenderFacade != null)
		{
			if (!_loadComplete && RenderFacade.AssetCreatureAsync.ISDONE())
			{
				_loadComplete = true;
				SetDirty();
			}
			RenderFacade.ChainUpdate();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			ReleaseAllContents();
		}
	}

	private void ReleaseAllContents()
	{
		Close();
	}

	public void SetAlpha(float alpha)
	{
		GetComponent<UIWidget>().alpha = alpha;
	}

	public void FadeIn(float duration)
	{
		SetAlpha(0f);
		TweenAlpha.Begin(base.gameObject, duration, 1f);
	}

	public void FadeOut(float duration)
	{
		SetAlpha(1f);
		TweenAlpha.Begin(base.gameObject, duration, 0f);
	}

	public void SetLocalPosition(int[] localPos)
	{
		if (!(PivotTm == null))
		{
			int valueInt = ArgParser.GetValueInt(localPos, 0);
			int valueInt2 = ArgParser.GetValueInt(localPos, 1);
			int valueInt3 = ArgParser.GetValueInt(localPos, 2);
			PivotTm.localPosition = new Vector3(valueInt, valueInt2, valueInt3);
		}
	}

	public void SetLocalRotation(int[] localRotation)
	{
		if (!(PivotTm == null))
		{
			int valueInt = ArgParser.GetValueInt(localRotation, 0);
			int valueInt2 = ArgParser.GetValueInt(localRotation, 1);
			int valueInt3 = ArgParser.GetValueInt(localRotation, 2);
			PivotTm.eulerAngles = new Vector3(valueInt, valueInt2, valueInt3);
		}
	}

	public void SetLocalScale(int scale)
	{
		if (!(PivotTm == null))
		{
			scale = ((scale == 0) ? 1 : scale);
			Vector3 localScale = new Vector3(scale, scale, scale);
			PivotTm.localScale = localScale;
		}
	}

	public override int UpdateExtraRenderQueue(UIPanel panel, int renderQueue, bool isExplicit)
	{
		if (RenderFacade == null)
		{
			return renderQueue;
		}
		if (RenderFacade.AssetCreature == null)
		{
			return renderQueue;
		}
		RenderFacade.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(alpha * panel.alpha);
		RenderFacade.AssetCreature.Visit(_renderQueueVisitor);
		return _renderQueueVisitor.UpdateExtraRenderQueue(renderQueue, isExplicit);
	}
}
