using NGame2.N3D;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NBuilding;

public class BaseBuilding : IBuilding
{
	private int _index;

	private RenderFacade _model;

	private Transform _root;

	private float _render3dAngle;

	private float _alpha;

	public virtual string PrevIdleAnimationKey => "";

	public virtual string IdleAnimationKey => "";

	public virtual string UpgradeAnimationKey => "";

	Vector3 IBuilding.Position
	{
		get
		{
			if (_root == null)
			{
				return Vector3.zero;
			}
			return _root.transform.localPosition;
		}
	}

	public int Index => _index;

	public int Level => GetLevel();

	public bool ReservedUpgrade => GetReservedUpgrade();

	public string ModelName => GetModelName(Level);

	public bool HaveUpgradeAnimationKey => !string.IsNullOrEmpty(UpgradeAnimationKey);

	RenderFacade IBuilding.Model => _model;

	public virtual BaseBuilding GetInstance()
	{
		return null;
	}

	public virtual int GetLevel()
	{
		return 0;
	}

	public virtual string GetModelName(int level)
	{
		return "";
	}

	public virtual bool GetReservedUpgrade()
	{
		return false;
	}

	public virtual void CompletedUpgradeAnim()
	{
	}

	public virtual void UpgradeCancel()
	{
	}

	public virtual void UpgradePrepare()
	{
	}

	public virtual void Upgrade()
	{
	}

	public virtual void UpgradeComplete()
	{
	}

	public virtual void Update()
	{
		if (_model != null)
		{
			_model.ChainUpdate();
		}
	}

	private BaseBuilding ModelBuild(BaseBuilding buildingInstance, int index, Transform parent, float render3dAngle)
	{
		if (buildingInstance == null)
		{
			return null;
		}
		buildingInstance._index = index;
		return ModelBuild(buildingInstance, index, buildingInstance.GetLevel(), parent, render3dAngle);
	}

	private BaseBuilding ModelBuild(BaseBuilding buildingInstance, int index, int level, Transform parent, float render3dAngle)
	{
		if (buildingInstance == null)
		{
			return null;
		}
		buildingInstance._index = index;
		string modelName = buildingInstance.GetModelName(level);
		if (modelName == null)
		{
			return null;
		}
		buildingInstance._model = RenderFacadeBuild(modelName, parent, render3dAngle);
		buildingInstance._root = parent;
		buildingInstance._render3dAngle = render3dAngle;
		buildingInstance.SetIdleAnimation(buildingInstance.IdleAnimationKey);
		buildingInstance._alpha = 1f;
		return buildingInstance;
	}

	public RenderFacade RenderFacadeBulid(string modelName)
	{
		return RenderFacadeBuild(modelName, _root, _render3dAngle);
	}

	private RenderFacade RenderFacadeBuild(string modelName, Transform parent, float render3dAngle)
	{
		RenderFacade renderFacade = new RenderFacadeBuilder(parent, TeamType.Left, modelName, 0)
		{
			LoadLevel = CreatureStock.ELoadLevel.Normal
		}.Build();
		renderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(render3dAngle, depthShift: true), null);
		return renderFacade;
	}

	public void SetExtraAlpha(RenderFacade model, float alpha, bool changeNow = false)
	{
		if (!(model == null))
		{
			model.AssetCreature.GetRenderGroupModifier().SetExtraAlpha(alpha);
			if (changeNow)
			{
				model.ChainUpdate();
			}
		}
	}

	private void SetExtraAlpha(float alpha)
	{
		_alpha = alpha;
		SetExtraAlpha(_model, alpha);
	}

	public void SetIdleAnimation(RenderFacade model, string key)
	{
		if (!(model == null) && !String_Extension_Method.IsNullOrEmpty(key))
		{
			model.Action.SetCustomIdle(new AnimationKeyData(DefineCreature.EMotion.NamedAction, key));
		}
	}

	private void SetIdleAnimation(string key)
	{
		SetIdleAnimation(_model, key);
	}

	public void SetActionAnimation(RenderFacade model, string key)
	{
		if (!(model == null) && !String_Extension_Method.IsNullOrEmpty(key))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, key);
			stockData.MotionEnd = DefineCreature.EMotionEnd.End;
			model.Action.Motion(stockData, blendMotion: false, null);
		}
	}

	private void SetActionAnimation(string key)
	{
		SetActionAnimation(_model, key);
	}

	private void SetUpgradeAnimation(string upgradeKey, string idleKey)
	{
		if (!(_model == null))
		{
			SetActionAnimation(upgradeKey);
			SetIdleAnimation(IdleAnimationKey);
		}
	}

	public void ChangeModel(RenderFacade model)
	{
		if (!(model == null))
		{
			if (_model != null)
			{
				_model.Close();
			}
			_model = model;
			SetExtraAlpha(_alpha);
		}
	}

	public void SwitchModel(RenderFacade model)
	{
		if (!(model == null))
		{
			_model = model;
		}
	}

	public RenderFacade GetModel()
	{
		return _model;
	}

	IBuilding IBuilding.Create(WorldMapBuildData buildData, Transform parent, float render3dAngle)
	{
		return ModelBuild(GetInstance(), buildData.BuildingIndex, parent, render3dAngle);
	}

	void IBuilding.Update()
	{
		Update();
	}

	void IBuilding.Close()
	{
		if (_model != null)
		{
			_model.Close();
		}
		if (_root != null)
		{
			VespaObject.DestroyGo(_root);
		}
	}

	void IBuilding.SetExtraAlpha(float alpha)
	{
		SetExtraAlpha(alpha);
	}

	void IBuilding.CompleteReservedUpgrade()
	{
		CompletedUpgradeAnim();
	}

	void IBuilding.UpgradeCancel()
	{
		UpgradeCancel();
	}

	void IBuilding.UpgradePrepare()
	{
		UpgradePrepare();
	}

	void IBuilding.Upgrade()
	{
		Upgrade();
	}

	void IBuilding.UpgradeComplete()
	{
		UpgradeComplete();
	}
}
