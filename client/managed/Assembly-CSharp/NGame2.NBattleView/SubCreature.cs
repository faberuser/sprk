using NGame2.NNodeTree.NObject;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleView;

public class SubCreature : IRenderGroupModifier
{
	public ICreatureLoad AssetCreatureAsync;

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

	public ExtraAlphaController ExtraAlphaController { get; private set; }

	public SubCreatureContoller SubCreatureAnimationController { get; private set; }

	public ActionSubCreature ActionSubCreature { get; private set; }

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

	public Transform SubCreatureTm { get; private set; }

	public long PivotUid { get; private set; }

	public bool Released { get; private set; }

	public void ChainUpdate()
	{
		SubCreatureUpdate();
	}

	public void TestSubCreature()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F2))
		{
			ReleaseSubCreature();
		}
		else if (Input.GetKeyDown(KeyCode.F3))
		{
			AnimationKeyData animationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, "Dash");
			DefineCreature.EMotionEnd motionEnd = DefineCreature.EMotionEnd.End;
			if (ActionSubCreature != null)
			{
				NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
				stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
				stockData.AnimationKey = animationKey;
				stockData.MotionEnd = motionEnd;
				stockData.TimeLength = 0.5f;
				ActionSubCreature.Motion(stockData, blendMotion: true, null);
			}
		}
		else if (Input.GetKeyDown(KeyCode.F4) && ActionSubCreature != null)
		{
			ActionSubCreature.Die(null);
		}
	}

	private void SubCreatureUpdate()
	{
		if (AssetCreature != null)
		{
			AssetCreature.ChainUpdate();
			if (SubCreatureAnimationController != null)
			{
				SubCreatureAnimationController.ChainUpdate();
			}
			if (ActionSubCreature != null)
			{
				ActionSubCreature.ChainUpdate();
			}
		}
		if (ExtraAlphaController != null)
		{
			ExtraAlphaController.ChainUpdate();
		}
	}

	public void CreateSubCreature(string code, ILocalTimer localTimer, Transform tm, long pivotUid)
	{
		if (AssetCreatureAsync != null)
		{
			VespaDebug.LogError("AssetCreatureAsync exist");
			return;
		}
		if (String_Extension_Method.IsNullOrEmpty(code))
		{
			VespaDebug.LogError("Creature Code is Null or Empty");
			return;
		}
		SubCreatureTm = tm;
		PivotUid = pivotUid;
		CreatureStock stock = new CreatureStock
		{
			Code = code
		};
		AssetCreatureAsync = SingletonAuto<CreatureContainer>.instance.CreateAsync(stock);
		AssetCreatureAsync.Value.SetParent(SubCreatureTm);
		AssetCreatureAsync.Value.SetTimer(localTimer);
		ExtraAlphaController = new ExtraAlphaController(this);
		CreateAnimationController();
		SetIdle();
	}

	private void CreateAnimationController()
	{
		SubCreatureAnimationController = new SubCreatureContoller(AssetCreatureAsync);
		ActionSubCreature = new ActionSubCreature(SubCreatureAnimationController);
	}

	private void ReleaseAnimationController()
	{
		SubCreatureAnimationController = null;
		ActionSubCreature = null;
	}

	private void SetIdle()
	{
		if (ActionSubCreature != null)
		{
			ActionSubCreature.Idle();
		}
	}

	public void ReleaseSubCreature()
	{
		Released = true;
		ReleaseAnimationController();
		if (AssetCreatureAsync != null)
		{
			CreatureContainer.ReleaseCreature(AssetCreatureAsync);
			AssetCreatureAsync = null;
		}
		if (PivotUid != 0L)
		{
			Container.instance.ReleaseObjectUid(PivotUid);
		}
	}
}
