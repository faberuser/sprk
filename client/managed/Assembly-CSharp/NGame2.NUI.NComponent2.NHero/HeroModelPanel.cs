using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NRender;
using NGame2.NUtil;
using NGame2.NUtil.NSoundFx;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHero;

public class HeroModelPanel : MonoBehaviour, ICreatureInUIComponent
{
	public Transform RenderPivot;

	public UICreatureWidget CreatureWidget;

	private Action OpenComplete;

	private bool _isPlayMotion;

	private string _appearMotionKey = string.Empty;

	private CreatureInUIVisualData _visualData;

	private List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	private float _prevScaleAmount;

	public CreatureData HeroData { get; private set; }

	private RenderFacade RenderFacade
	{
		get
		{
			if (CreatureWidget == null)
			{
				return null;
			}
			return CreatureWidget.RenderFacade;
		}
	}

	public void Open(CreatureData heroData, int heroStar, int coustumeIndex, CreatureInUIVisualData visualData, Action Complete = null)
	{
		HeroData = heroData;
		_visualData = visualData;
		if (CreatureWidget != null)
		{
			CreatureWidget.Open(heroData, heroStar, coustumeIndex);
		}
		if (RenderFacade != null)
		{
			RenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
			ClientCreatureData clientCreatureDataByIndex = DataHelper.GetClientCreatureDataByIndex(heroData.Index, coustumeIndex);
			if (clientCreatureDataByIndex != null)
			{
				VectorXZ direction = new VectorXZ(0f, -1f);
				direction.x = clientCreatureDataByIndex.LookRotationX;
				direction.z = clientCreatureDataByIndex.LookRotationZ;
				RenderFacade.Action.SetDirection(direction, blendMotion: false);
				if (clientCreatureDataByIndex.PayShopScale > 1f)
				{
					RenderPivot.localScale = Vector3.one * clientCreatureDataByIndex.PayShopScale;
				}
			}
			_appearMotionKey = clientCreatureDataByIndex.PayShopAnimationKey;
			Vespa_System_Action_Extension_Method.fire(OpenComplete);
		}
		Vespa_System_Action_Extension_Method.fire(Complete);
	}

	public void Clear()
	{
		_isPlayMotion = false;
		if (RenderFacade != null)
		{
			ClearSound(new AnimationKeyData(DefineCreature.EMotion.NamedAction, "Victory"));
			RenderFacadeBuilder.ReleaseRenderFacade(RenderFacade);
			CreatureWidget.Close();
		}
		if (OpenComplete != null)
		{
			OpenComplete = null;
		}
		CloseTouch();
	}

	private void Update()
	{
		if (!(RenderFacade == null))
		{
			if (CreatureWidget != null)
			{
				CreatureWidget.SetAlpha(1f);
			}
			if (_isPlayMotion && RenderFacade.Action.IsActionEnd())
			{
				_isPlayMotion = false;
				MotionHeroNormalStand();
			}
			UpdateCamera();
		}
	}

	public void SetSpecialMotion()
	{
		if (!(CreatureWidget == null) && (!_isPlayMotion || RenderFacade.Action.IsActionEnd()))
		{
			_isPlayMotion = true;
			CreatureWidget.SetNamedAction("Victory");
		}
	}

	private void MotionHeroNormalStand()
	{
		if (!(CreatureWidget == null))
		{
			CreatureWidget.SetMotionTypeAction(DefineCreature.EMotion.Idle);
		}
	}

	public float GetNPCActionTime(string name)
	{
		if (!(RenderFacade.AssetCreature is CreatureInstance creatureInstance))
		{
			return 1f;
		}
		foreach (KeyValuePair<AnimationKeyData, IAnimationData> item in (IEnumerable<KeyValuePair<AnimationKeyData, IAnimationData>>)creatureInstance.AnimationDic)
		{
			if (item.Key.Name == name)
			{
				return item.Value.TimeLength;
			}
		}
		return 1f;
	}

	private void CloseTouch()
	{
		_touchList.Clear();
		_prevScaleAmount = 0f;
	}

	private void UpdateCamera()
	{
		if (_touchList.Count != 0 && _touchList.Count == 1 && RenderFacade != null)
		{
			UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
			VectorXZ direction = RenderFacade.Action.GetDirection();
			direction = NVespa.Math.Rotate(direction, mouseOrTouch.delta.x * 1f);
			RenderFacade.Action.SetDirection(direction, blendMotion: false);
		}
	}

	public void OnDragStart()
	{
		if (!_touchList.Contains(UICamera.currentTouch))
		{
			_touchList.Add(UICamera.currentTouch);
			_prevScaleAmount = 0f;
		}
	}

	public void OnDragEnd()
	{
		if (_touchList.Remove(UICamera.currentTouch))
		{
			_prevScaleAmount = 0f;
		}
	}

	private void ClearSound(AnimationKeyData animationKey)
	{
		if (SingletonAuto<SoundContainer>.instance.IsEmptySoundList())
		{
			return;
		}
		string creatureCode = (String_Extension_Method.IsNullOrEmpty(HeroData.ModelCode) ? HeroData.CodeName : HeroData.ModelCode);
		List<FrameSoundData> animationSoundFx = SingletonAuto<SoundFxManager>.instance.GetAnimationSoundFx(creatureCode, animationKey);
		string text = "";
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(animationSoundFx); i++)
		{
			text = animationSoundFx[i].SoundCode;
			ISound sound = SingletonUpdater<AssetUpdateManager>.instance.GetSound(text);
			if (sound != null)
			{
				SoundContainer.ReleaseSound(sound);
			}
		}
	}
}
