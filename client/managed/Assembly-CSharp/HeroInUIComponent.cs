using System;
using System.Collections.Generic;
using NGame2;
using NGame2.N3D;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI;
using NGame2.NUtil;
using NGame2.NUtil.NSoundFx;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

[Obsolete("Warnning : Don't use")]
public class HeroInUIComponent : MonoBehaviour, ICreatureInUIComponent
{
	public const string PrefabPath = "Prefab2/Component/Battle/HeroInUIComponent";

	public static PrefabAuto<HeroInUIComponent> PrefabAuto = new PrefabAuto<HeroInUIComponent>("Prefab2/Component/Battle/HeroInUIComponent");

	public Transform RenderPivot;

	public UIWidget WidgetSpeakBubble;

	public UILabel Label_Speak;

	public TweenAlpha BubbleTweenAlpha;

	private RenderFacade NpcRenderFacade;

	private int _skipFrame;

	private System.Action OpenComplete;

	private bool _isPlayMotion;

	private CreatureInUIVisualData _visualData;

	private List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	private float _prevScaleAmount;

	public CachedHeroInfo HeroInfo { get; private set; }

	private bool IsPlaySound { get; set; }

	public RenderFacade RenderFacade => NpcRenderFacade;

	public void Open(CachedHeroInfo heroInfo, bool isPlaySound, CreatureInUIVisualData visualData, System.Action Complete = null)
	{
		IsPlaySound = isPlaySound;
		HeroInfo = heroInfo;
		_visualData = visualData;
		OpenComplete = Complete;
		CreateRenderFacade();
		if (BubbleTweenAlpha != null)
		{
			UIHelper.EnableGameObject(BubbleTweenAlpha, enable: false);
		}
		if (NpcRenderFacade != null)
		{
			Vespa_System_Action_Extension_Method.fire(OpenComplete);
		}
	}

	public void Clear()
	{
		_isPlayMotion = false;
		if (BubbleTweenAlpha != null)
		{
			BubbleTweenAlpha.ResetToBeginning();
		}
		if (NpcRenderFacade != null)
		{
			ClearSound(new AnimationKeyData(DefineCreature.EMotion.NamedAction, "Victory"));
			RenderFacadeBuilder.ReleaseRenderFacade(NpcRenderFacade);
			NpcRenderFacade = null;
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
		string creatureCode = (String_Extension_Method.IsNullOrEmpty(HeroInfo.CreatureData.ModelCode) ? HeroInfo.CreatureData.CodeName : HeroInfo.CreatureData.ModelCode);
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
		if (NpcRenderFacade == null)
		{
			return;
		}
		if (_skipFrame > 0)
		{
			_skipFrame--;
			return;
		}
		if (NpcRenderFacade.RenderGroupModifier != null)
		{
			NpcRenderFacade.RenderGroupModifier.SetExtraAlpha(1f);
		}
		if (_isPlayMotion && NpcRenderFacade.Action.IsActionEnd())
		{
			_isPlayMotion = false;
			MotionNpcNormalStand(blendMotion: true);
		}
		NpcRenderFacade.ChainUpdate();
		UpdateHeroCamera();
	}

	private void CreateRenderFacade()
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3D);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(RenderPivot, TeamType.Left, HeroInfo);
		renderFacadeBuilder.LoadLevel = _visualData.LoadLevel;
		NpcRenderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		if (NpcRenderFacade != null)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(NpcRenderFacade.gameObject, layerInfo.Camera.gameObject.layer);
			NpcRenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			NpcRenderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
			NpcRenderFacade.RenderGroupModifier.SetExtraAlpha(0f);
			NpcRenderFacade.ChainUpdate();
			_skipFrame = 1;
		}
		if (WidgetSpeakBubble != null)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(WidgetSpeakBubble.gameObject, layerInfo.Camera.gameObject.layer);
		}
	}

	public void SetSpeech(string speech)
	{
		bool flag = !String_Extension_Method.IsNullOrEmpty(speech);
		UIHelper.EnableGameObject(WidgetSpeakBubble, flag);
		if (!flag)
		{
			return;
		}
		UIHelper.SetLabelText(Label_Speak, speech);
		if (BubbleTweenAlpha != null)
		{
			BubbleTweenAlpha.ResetToBeginning();
			BubbleTweenAlpha.PlayForward();
			BubbleTweenAlpha.SetOnFinished(delegate
			{
				UIHelper.EnableGameObject(WidgetSpeakBubble, enable: false);
			});
		}
	}

	public void SetMotion(string motionName, bool isLoop)
	{
		if (!(NpcRenderFacade == null))
		{
			_isPlayMotion = true;
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Idle;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, motionName);
			stockData.MotionEnd = (isLoop ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			NpcRenderFacade.Action.Motion(stockData, null);
		}
	}

	public void SetNamedMotion(string motionName, bool isLoop)
	{
		if (!(NpcRenderFacade == null))
		{
			_isPlayMotion = true;
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Idle;
			stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, motionName);
			stockData.MotionEnd = (isLoop ? DefineCreature.EMotionEnd.Loop : DefineCreature.EMotionEnd.End);
			NpcRenderFacade.Action.Motion(stockData, null);
		}
	}

	private void MotionNpcNormalStand(bool blendMotion)
	{
		if (!(NpcRenderFacade == null))
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Idle;
			stockData.AnimationKey = _visualData.DefalutIdleKey;
			stockData.MotionEnd = DefineCreature.EMotionEnd.Loop;
			NpcRenderFacade.Action.Motion(stockData, blendMotion, null);
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

	private void UpdateHeroCamera()
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
