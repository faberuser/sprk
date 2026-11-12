using NGame2.N3D;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class BasePcAvatar : Avatar, AiInteractionComponent.Listener
{
	public int Index;

	public new bool Hide;

	private NpcTouchBehaviour _touchBehavior;

	public void Open(PcAvatarData avatarData, BasePcMediator mediator, RenderFacadeBuilder renderFacadeBuilder)
	{
		Open(new AvatarInfo
		{
			AvatarController = mediator.AvatarController,
			MapComponent = mediator.MapComponent,
			RenderFacadeBuilder = renderFacadeBuilder
		});
		Index = avatarData.Index;
		AddObserver();
		SetAvatarBehavior(mediator, avatarData);
		SetSpeedFactor(mediator.AvatarController, avatarData);
		_touchBehavior = CreateTouchBehaviour();
		_touchBehavior.Open(base.RenderFacade, OnClickAvatar);
	}

	private NpcTouchBehaviour CreateTouchBehaviour()
	{
		Transform parent = base.RenderFacade.transform.parent;
		if (parent == null)
		{
			return null;
		}
		parent.Find("TouchRoot");
		Transform transform = parent.Find("TouchRoot");
		GameObject gameObject = ((transform != null) ? transform.gameObject : new GameObject("TouchRoot"));
		if (transform == null)
		{
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, parent);
		}
		Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<UIWidget>(gameObject);
		gameObject.layer = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.BottomUI).Layer;
		return Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<NpcTouchBehaviour>(gameObject);
	}

	protected virtual void AddObserver()
	{
	}

	protected virtual void RemoveObserver()
	{
	}

	public virtual void SetPcAvatarInfo(long uid, string name)
	{
	}

	protected virtual void SetAvatarBehavior(BasePcMediator mediator, PcAvatarData pcAvatarData)
	{
	}

	protected virtual void SetSpeedFactor(AvatarController controller, PcAvatarData pcAvatarData)
	{
		if (pcAvatarData != null && !(pcAvatarData.MoveSpeedRate < 0f))
		{
			controller.SpeedFactor = pcAvatarData.MoveSpeedRate;
		}
	}

	public virtual void SetInteractionComponent(PcAvatarData data)
	{
		if (base.AiInteractionComponent == null)
		{
			CreateInteractionComponent(this);
		}
	}

	protected virtual void OnClickReward()
	{
	}

	protected virtual void OnClickAvatar()
	{
	}

	public void SetMotion(string[] motions)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(motions))
		{
			int num = Random.Range(0, motions.Length);
			string text = motions[num];
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
				stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
				stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, text);
				stockData.MotionEnd = DefineCreature.EMotionEnd.End;
				SetMotion(stockData);
			}
		}
	}

	public void Closed()
	{
		Close();
		RemoveObserver();
		if (base.AiInteractionComponent != null)
		{
			AiInteractionComponent.ReleaseInstance(base.AiInteractionComponent);
		}
	}

	public void AvatarShowOrHide(bool isShow)
	{
		Hide = !isShow;
		OnOffAvatarBoxCollider(isShow);
		OnOffInteractionComponent(isShow);
	}

	private void OnOffAvatarBoxCollider(bool enable)
	{
		_touchBehavior.OnOffBoxCollider(enable);
	}

	private void OnOffInteractionComponent(bool isOn)
	{
		UIHelper.EnableGameObject(base.AiInteractionComponent, isOn);
	}

	public void SetInteractionComponentName(string name)
	{
		if (base.AiInteractionComponent == null)
		{
			CreateInteractionComponent(this);
		}
		base.AiInteractionComponent.SetName(name);
	}

	void AiInteractionComponent.Listener.OnClickReward()
	{
		OnClickReward();
	}
}
