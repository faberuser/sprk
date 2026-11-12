using NGame2.N3D;
using NGame2.NRender;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

internal class RenderFacadePropDecorator : IPropDecorator
{
	public class RenderFacadeCreateStock
	{
		public RenderFacadeBuilder RenderFacadeBuilder;

		public float Degree;

		public RenderFacadeCreateStock(RenderFacadeBuilder builder, float degree)
		{
			RenderFacadeBuilder = builder;
			Degree = degree;
		}
	}

	protected PropComponent _propComponent;

	protected RenderFacade _renderFacade;

	protected float _extraAlpha;

	protected Mediator _mapMediator;

	public virtual void SetExtraAlpha(float alpha)
	{
		_extraAlpha = alpha;
		if (_renderFacade != null && _renderFacade.AssetCreature != null)
		{
			_renderFacade.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(alpha);
		}
	}

	public virtual void SetPropComponent(PropComponent prop, string[] propActionArgs, Mediator mapMediator)
	{
		_mapMediator = mapMediator;
		_propComponent = prop;
	}

	protected RenderFacadeCreateStock CreateStock(CreatureData creatureData, int heroScale, int degree)
	{
		if (creatureData == null)
		{
			return null;
		}
		Transform facadeTransform = GetFacadeTransform();
		facadeTransform.localScale = new Vector3(heroScale, heroScale, heroScale);
		return new RenderFacadeCreateStock(new RenderFacadeBuilder(facadeTransform, TeamType.Left, creatureData.CodeName, 1), degree);
	}

	protected void PauseMotion()
	{
		if (!(_renderFacade == null))
		{
			_renderFacade.Action.PauseMotion();
		}
	}

	protected Transform GetFacadeTransform()
	{
		Transform transform = _propComponent.transform.Find("FacadeGo");
		if (transform == null)
		{
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(new GameObject("FacadeGo").transform, _propComponent.transform);
		}
		return transform;
	}

	private CreatureData GetCreatureDataForPrefix(int heroIndex, string prefix)
	{
		string codeName = GameRoot.Instance.CreatureDataContainer.GetCode(heroIndex);
		if (String_Extension_Method.IsNullOrEmpty(codeName))
		{
			VespaDebug.LogError($"heroIndex[{heroIndex}] CodeName is Null or Empty ");
			return null;
		}
		return GameRoot.Instance.CreatureDataContainer.GetDataOf((CreatureData data) => data.ModelCode == codeName && data.ModelPrefix == prefix);
	}

	public void CreateRenderFacade(RenderFacadeCreateStock stock)
	{
		if (stock == null)
		{
			VespaDebug.LogError("stock is null");
		}
		RenderFacadeBuilder renderFacadeBuilder = stock.RenderFacadeBuilder;
		float degree = stock.Degree;
		if (_renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
			_renderFacade = null;
		}
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.Minimum;
		_renderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		if (!(_renderFacade == null))
		{
			VectorXZ direction = Vespa_Unity_Vector2_Extension_Method.ToVecXZ(Math.Rotate(degree));
			_renderFacade.Action.SetDirection(direction, blendMotion: false);
			_renderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			_renderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(_mapMediator.MapComponent.Layout.Render3dAngle, depthShift: true), null);
		}
	}

	public virtual void ChainUpdate()
	{
		if (!(_renderFacade == null))
		{
			_renderFacade.ChainUpdate();
		}
	}

	public virtual void Release()
	{
		if (_renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
			_renderFacade = null;
		}
	}

	public virtual void Refresh()
	{
	}
}
