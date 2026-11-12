using System;
using System.Collections;
using NGame2.NBattleView.NUIOn3D;
using NNguiExtension;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class AiInteractionComponent : MonoBehaviour
{
	public interface Listener
	{
		void OnClickReward();
	}

	public enum InteractionType
	{
		Guild,
		Lobby
	}

	public GameObject ParticleObj;

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/AiinteractionComponent";

	public static PrefabAuto<AiInteractionComponent> PrefabAuto = new PrefabAuto<AiInteractionComponent>("Resources/Prefab2/Window/Guild/AiinteractionComponent");

	public bool IsRewardOn;

	[NonSerialized]
	public float EventAlpha = 1f;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private UIWidget _reward;

	public UISprite BallonSprite;

	public UIWidget TextBallonRoot;

	public UILabel TextBallonLabel;

	public UISpriteSelector InterationTypeSpriteSelector;

	private bool _hideHUD;

	private Listener _listener;

	public static AiInteractionComponent CreateInstance(Transform parent)
	{
		return PrefabAuto.Factory.Create(parent);
	}

	public static void ReleaseInstance(AiInteractionComponent instance)
	{
		FactoryBase.ReleaseComponent(instance);
	}

	public void Open(WorldMapHud.IValue value, Listener listener)
	{
		_listener = listener;
		Reset();
		ShowReward(isShow: false);
		SetBallonText(string.Empty);
		SetLocalPosition(value);
		SetInterationType();
		UIHelper.EnableGameObject(ParticleObj, enable: false);
	}

	public void SetExtraAlpha(float alpha)
	{
		if (_panel != null)
		{
			_panel.alpha = alpha;
		}
	}

	public void SetLocalPosition(WorldMapHud.IValue value)
	{
		base.transform.localPosition = Vector3.up * ((value.HudOffset > 1f) ? value.HudOffset : 2f);
	}

	public void SetInterationType(InteractionType type = InteractionType.Guild)
	{
		if (InterationTypeSpriteSelector != null)
		{
			InterationTypeSpriteSelector.SelectEnum(type);
		}
	}

	public void SetName(string Name)
	{
		UIHelper.SetLabelText(_nameLabel, Name);
	}

	private void Reset()
	{
		EventAlpha = 1f;
		SetExtraAlpha(1f);
	}

	public void ChainUpdate()
	{
		Camera cameraByLayer = GlobalAutoBehaviour<GlobalCameraManager>.instance.GetCameraByLayer(base.gameObject.layer);
		if (!(cameraByLayer == null))
		{
			base.transform.rotation = cameraByLayer.transform.rotation;
		}
	}

	public void ShowName(bool isShow)
	{
		UIHelper.EnableGameObject(_nameLabel, isShow);
	}

	public void SetBallonText(string text)
	{
		bool enable = !String_Extension_Method.IsNullOrEmpty(text) && !_reward.gameObject.activeSelf;
		UIHelper.EnableGameObject(TextBallonRoot, enable);
		UIHelper.SetLabelText(TextBallonLabel, text);
		if (!(BallonSprite == null) && !(TextBallonLabel == null))
		{
			BallonSprite.height = TextBallonLabel.height * 2;
			TextBallonLabel.transform.localPosition = new Vector3(TextBallonLabel.transform.localPosition.x, TextBallonLabel.height, TextBallonLabel.transform.localPosition.z);
		}
	}

	public void ShowReward(bool isShow)
	{
		IsRewardOn = isShow;
		UIHelper.EnableGameObject(_reward, isShow);
		if (isShow)
		{
			SetBallonText(string.Empty);
		}
	}

	public void OnclickReward(GameObject obj)
	{
		if (_listener != null)
		{
			VespaCoroutine.Start(RewardIntenal());
		}
	}

	public IEnumerator RewardIntenal()
	{
		UIHelper.EnableGameObject(ParticleObj, enable: true);
		yield return new WaitForSeconds(0.5f);
		UIHelper.EnableGameObject(ParticleObj, enable: false);
		if (_listener != null)
		{
			_listener.OnClickReward();
		}
	}
}
