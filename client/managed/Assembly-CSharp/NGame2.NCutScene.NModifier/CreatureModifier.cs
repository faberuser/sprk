using System;
using NGame2.N3D;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class CreatureModifier
{
	public class CreatureColor : ColorEffect.IValue
	{
		public bool IsEnd;

		public Color CurColor;

		public float ElapsedTime;

		public float TimeLength;

		public Color FromColor;

		public Color ToColor;

		bool ColorEffect.IValue.IsEnd()
		{
			return IsEnd;
		}

		Color ColorEffect.IValue.GetColor()
		{
			return CurColor;
		}

		public void Update(float deltaTime)
		{
			ElapsedTime += deltaTime;
			float i_t = Mathf.Clamp01(ElapsedTime / Vespa_Float_Extension_Method.NoneZero(TimeLength));
			CurColor = Vespa_Unity_Color_Extension_Method.Lerp(FromColor, ToColor, i_t);
		}
	}

	private GameObject _creatureGo;

	private GameObject _shakeGo;

	private Vector3 _position;

	private float _scale;

	private float _angle;

	private Vector3 _rotation;

	private CreatureColor _creatureColor;

	public RenderFacade RenderFacade { get; private set; }

	public Vector3 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
			_creatureGo.transform.localPosition = value;
		}
	}

	public float Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = value;
			RenderFacade.SetRuntimeScale("CutScene", _scale);
		}
	}

	public float Angle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = value;
			RenderFacade.Action.SetDirection(new VectorXZ(Mathf.Sin(_angle * (MathF.PI / 180f)), 0f - Mathf.Cos(_angle * (MathF.PI / 180f))), blendMotion: false);
		}
	}

	public Vector3 Rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
			_creatureGo.transform.rotation = Quaternion.Euler(value);
		}
	}

	public string Code
	{
		get
		{
			if (_creatureGo == null)
			{
				return null;
			}
			return _creatureGo.name;
		}
	}

	public Transform ShakeTransform => _shakeGo.transform;

	public bool Open(RenderFacadeBuilder renderFacadeBuilder, Transform parentTm, ILocalTimer timer)
	{
		if (renderFacadeBuilder == null)
		{
			return false;
		}
		int layer = parentTm.gameObject.layer;
		_creatureGo = new GameObject(renderFacadeBuilder.GetStock().Code);
		_creatureGo.layer = layer;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_creatureGo.transform, parentTm);
		_shakeGo = new GameObject("CreatureShake");
		_shakeGo.layer = layer;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_shakeGo.transform, _creatureGo.transform);
		renderFacadeBuilder.ParentTm = _shakeGo.transform;
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
		RenderFacade = renderFacadeBuilder.Build();
		if (RenderFacade == null)
		{
			return false;
		}
		RenderFacade.AnimationData.SetBaseTimer(timer);
		RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(0f, depthShift: false), RenderFacade.transform);
		RenderFacade.DISABLEGROUNDCLIP();
		RenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
		RenderFacade.Action.Idle();
		return true;
	}

	public void Close()
	{
		if (RenderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(RenderFacade);
			RenderFacade = null;
		}
		if (_shakeGo != null)
		{
			VespaObject.DestroyGo(_shakeGo);
			_shakeGo = null;
		}
		if (_creatureGo != null)
		{
			VespaObject.DestroyGo(_creatureGo);
			_creatureGo = null;
		}
		if (_creatureColor != null)
		{
			_creatureColor = null;
		}
	}

	public void SetColorMix(Color color, float blendTimeLength)
	{
		if (!(RenderFacade == null))
		{
			RenderFacade.AssetCreature.GetRenderGroupModifier();
			Color fromColor = Colors.zero;
			if (_creatureColor != null)
			{
				fromColor = _creatureColor.CurColor;
				_creatureColor.IsEnd = true;
			}
			_creatureColor = new CreatureColor();
			_creatureColor.FromColor = fromColor;
			_creatureColor.ToColor = color;
			_creatureColor.CurColor = _creatureColor.FromColor;
			_creatureColor.ElapsedTime = 0f;
			_creatureColor.TimeLength = blendTimeLength;
			_creatureColor.IsEnd = false;
			RenderFacade.ColorEffect.AddValue(_creatureColor);
		}
	}

	public void ChainUpdate(ILocalTimer localTimer)
	{
		if (!(RenderFacade == null))
		{
			if (_creatureColor != null)
			{
				_creatureColor.Update((float)localTimer.deltaTick * 0.001f);
			}
			RenderFacade.ChainUpdate();
		}
	}

	public void Show(bool show)
	{
		if ((bool)_creatureGo)
		{
			_creatureGo.SetActive(show);
		}
	}
}
