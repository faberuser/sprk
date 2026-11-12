using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NRender;
using NGame2.NUI;
using NGame2.NUtil;
using NGame2.NUtil.NSoundFx;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using UnityEngine;

public class PetModelPanel : MonoBehaviour, ICreatureInUIComponent
{
	public Transform RenderPivot;

	public UICreatureWidget CreatureWidget;

	private Action OpenComplete;

	private bool _isPlayMotion;

	private CreatureInUIVisualData _visualData;

	private List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	private float _prevScaleAmount;

	private const string PET_TOUCH_MOTION = "Eat";

	public PetInfo PetInfo { get; private set; }

	private bool IsPlaySound { get; set; }

	private RenderFacade NpcRenderFacade
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

	public void Open(PetInfo petInfo, bool isPlaySound, CreatureInUIVisualData visualData, Action Complete = null)
	{
		IsPlaySound = isPlaySound;
		PetInfo = petInfo;
		if (CreatureWidget != null)
		{
			CreatureWidget.OpenPet(petInfo, visualData.LoadLevel);
		}
		_visualData = visualData;
		if (!(NpcRenderFacade != null))
		{
			return;
		}
		NpcRenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
		ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(petInfo.CreatureData.CodeName);
		if (data != null)
		{
			if (__Extension_Method_ICollection__.COUNT(data.PetListOffset) >= 2)
			{
				Vector3 localPosition = new Vector3(data.PetListOffset[0], data.PetListOffset[1]);
				if (localPosition.magnitude > 1f)
				{
					RenderPivot.localPosition = localPosition;
				}
			}
			if (data.PetListScale > 1f)
			{
				RenderPivot.localScale = Vector3.one * data.PetListScale;
			}
		}
		Vespa_System_Action_Extension_Method.fire(OpenComplete);
	}

	public void Clear()
	{
		_isPlayMotion = false;
		if (NpcRenderFacade != null)
		{
			ClearSound(new AnimationKeyData(DefineCreature.EMotion.NamedAction, "Victory"));
			RenderFacadeBuilder.ReleaseRenderFacade(NpcRenderFacade);
			CreatureWidget.Close();
		}
		if (OpenComplete != null)
		{
			OpenComplete = null;
		}
		CloseTouch();
	}

	private void ClearSound(AnimationKeyData animationKey)
	{
		if (SingletonAuto<SoundContainer>.instance.IsEmptySoundList())
		{
			return;
		}
		string creatureCode = (String_Extension_Method.IsNullOrEmpty(PetInfo.CreatureData.ModelCode) ? PetInfo.CreatureData.CodeName : PetInfo.CreatureData.ModelCode);
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

	private void Update()
	{
		if (!(NpcRenderFacade == null))
		{
			if (CreatureWidget != null)
			{
				CreatureWidget.SetAlpha(1f);
			}
			if (_isPlayMotion && NpcRenderFacade.Action.IsActionEnd())
			{
				_isPlayMotion = false;
				MotionPetNormalStand();
			}
			UpdateCamera();
		}
	}

	public void ClosePetEffect()
	{
		if (!(NpcRenderFacade == null) && NpcRenderFacade.AssetCreature is CreatureInstance creatureInstance)
		{
			creatureInstance.CloseAllNonPermanentEffects();
		}
	}

	public void SetSpecialMotion()
	{
		if (!(CreatureWidget == null))
		{
			_isPlayMotion = true;
			CreatureWidget.SetEmotiAction("Eat");
		}
	}

	private void MotionPetNormalStand()
	{
		if (!(CreatureWidget == null))
		{
			CreatureWidget.SetMotionTypeAction(DefineCreature.EMotion.Idle);
		}
	}

	public float GetNPCActionTime(string name)
	{
		if (!(NpcRenderFacade.AssetCreature is CreatureInstance creatureInstance))
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
		if (_touchList.Count != 0 && _touchList.Count == 1 && NpcRenderFacade != null)
		{
			UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
			VectorXZ direction = NpcRenderFacade.Action.GetDirection();
			direction = NVespa.Math.Rotate(direction, mouseOrTouch.delta.x * 1f);
			NpcRenderFacade.Action.SetDirection(direction, blendMotion: false);
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
}
