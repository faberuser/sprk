using System;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchWaitingBase : BaseWindow
{
	[Serializable]
	public class PlayerInfo
	{
		public UILabel Nick;

		public UILabel TeamLevel;

		public Transform RenderPivot;

		public UIWidget NickRoot;

		public PlayerEffect P1;

		public PlayerEffect P2;

		public UIGame3DWidget Flag;

		[HideInInspector]
		public RenderFacade renderFacade;

		[HideInInspector]
		public int _skipFrame;
	}

	[Serializable]
	public class PlayerEffect
	{
		public Transform Root;

		public TweenAlpha TweenAlpha_Text;

		public TweenScale TweenScale_Text;

		public TweenAlpha TweenAlpha_Glow;

		public TweenScale TweenScale_Glow;

		public TweenAlpha TweenAlpha_TextureGlow01;

		public TweenScale TweenScale_TextureGlow01;

		public TweenAlpha TweenAlpha_TextureGlow02;

		public TweenScale TweenScale_TextureGlow02;

		public TweenRotation TweenRotation_TextureGlow02;

		public void Reset()
		{
			UIHelper.EnableGameObject(Root, enable: false);
			TweenAlpha_Text.ResetToBeginning();
			TweenScale_Text.ResetToBeginning();
			TweenAlpha_Glow.ResetToBeginning();
			TweenScale_Glow.ResetToBeginning();
			TweenAlpha_TextureGlow01.ResetToBeginning();
			TweenScale_TextureGlow01.ResetToBeginning();
			TweenAlpha_TextureGlow02.ResetToBeginning();
			TweenScale_TextureGlow02.ResetToBeginning();
			TweenRotation_TextureGlow02.ResetToBeginning();
		}

		public void Play()
		{
			UIHelper.EnableGameObject(Root, enable: true);
			TweenAlpha_Text.PlayForwardReset();
			TweenScale_Text.PlayForwardReset();
			TweenAlpha_Glow.PlayForwardReset();
			TweenScale_Glow.PlayForwardReset();
			TweenAlpha_TextureGlow01.PlayForwardReset();
			TweenScale_TextureGlow01.PlayForwardReset();
			TweenAlpha_TextureGlow02.PlayForwardReset();
			TweenScale_TextureGlow02.PlayForwardReset();
			TweenRotation_TextureGlow02.PlayForwardReset();
		}
	}

	protected void SetupPlayer(PlayerInfo player, string nick, int teamLevel, int avatarIndex)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3D);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(player.NickRoot.gameObject, layerInfo.Camera.gameObject.layer);
		UIHelper.SetLabelText(player.Nick, nick);
		UIHelper.SetLabelText(player.TeamLevel, teamLevel);
		UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(avatarIndex);
		CreatureData creatureData = avatarInfo.GetCreatureData();
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(avatarInfo.HeroIndex);
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo != null)
		{
			cachedHeroInfo = cachedHeroInfo.CreatePlayerAvatarHeroInfo(GlobalAutoBehaviour<AccountManager>.instance.UserManager.PlayerAvatarHeroInfo);
		}
		CreateRenderFacade(player, creatureData, cachedHeroInfo);
		player.Flag.Play(null);
	}

	protected void CreateRenderFacade(PlayerInfo player, CreatureData creatureData, CachedHeroInfo heroInfo)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3D);
		AnimationKeyData animationKey = AnimationKeyData.CreateNamedAction("Idle");
		ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(creatureData.CodeName, heroInfo.CostumeIndex);
		float num = data?.MatchPickBanScale ?? 300f;
		float[] array = ((data != null) ? data.MatchPickBanOffset : new float[3] { 0f, -700f, 0f });
		player.RenderPivot.localPosition = new Vector3(array[0], array[1], array[2]);
		player.RenderPivot.localScale = new Vector3(num, num, num);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(player.RenderPivot, TeamType.Left, heroInfo);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.Normal;
		renderFacadeBuilder.AddPreload(animationKey);
		player.renderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		if (player.renderFacade != null)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(player.renderFacade.gameObject, layerInfo.Camera.gameObject.layer);
			player.renderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			player.renderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
			player.renderFacade.RenderGroupModifier.SetExtraAlpha(0f);
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Idle);
			stockData.MotionEnd = DefineCreature.EMotionEnd.End;
			stockData.IsPlaySound = true;
			player.renderFacade.Action.Motion(stockData, blendMotion: false, null);
			player.renderFacade.ChainUpdate();
			player._skipFrame = 1;
		}
	}
}
