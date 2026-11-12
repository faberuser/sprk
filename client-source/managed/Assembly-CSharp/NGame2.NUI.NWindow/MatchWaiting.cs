using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchWaiting : BaseWindow
{
	public interface Listener
	{
		void OnMatchCancel();

		void OnForceMatchCancel();
	}

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

	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchWaiting";

	public PlayerInfo Player1;

	public PlayerInfo Player2;

	public Transform Player2Root;

	public UIButton CancelButton;

	public Transform Waiting;

	private float startTime;

	private float limitWaitingTime;

	private bool isTimeOut;

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		UIHelper.EnableGameObject(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>(), enable: false);
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().gameObject.SetActive(value: false);
		UIHelper.EnableGameObject(Player2Root, enable: false);
		string nick = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick;
		int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
		int avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex;
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetValues().ToList();
		SetupPlayer(Player1, nick, teamLevel, avatarHeroIndex, heroInfoList);
		GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
		Player1.P1.Reset();
		Player1.P2.Reset();
		Player2.P1.Reset();
		Player2.P2.Reset();
		startTime = Time.realtimeSinceStartup;
		limitWaitingTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickWaitingLimit_Client");
	}

	public override void Close()
	{
		base.Close();
		UIHelper.EnableGameObject(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>(), enable: true);
	}

	public override void Reset()
	{
		base.Reset();
		if (Player1.renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(Player1.renderFacade);
			Player1.renderFacade = null;
		}
		if (Player2.renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(Player2.renderFacade);
			Player2.renderFacade = null;
		}
		UIHelper.EnableGameObject(CancelButton, enable: true);
		UIHelper.EnableButton(CancelButton, enable: true, grayscaleIfDisable: true);
		UIHelper.EnableGameObject(Waiting, enable: true);
		ResetTween();
		isTimeOut = false;
		Player1.P1.Reset();
		Player1.P2.Reset();
		Player2.P1.Reset();
		Player2.P2.Reset();
	}

	private void ResetTween()
	{
		if (!(Waiting == null))
		{
			UITweener[] componentsInChildren = Waiting.GetComponentsInChildren<UITweener>();
			foreach (UITweener obj in componentsInChildren)
			{
				obj.ResetToBeginning();
				obj.PlayForward();
			}
		}
	}

	private void Update()
	{
		if (Player1.renderFacade != null)
		{
			Player1.renderFacade.RenderGroupModifier.SetExtraAlpha(1f);
			Player1.renderFacade.ChainUpdate();
		}
		if (Player2.renderFacade != null)
		{
			Player2.renderFacade.RenderGroupModifier.SetExtraAlpha(1f);
			Player2.renderFacade.ChainUpdate();
		}
		if (!isTimeOut && Time.realtimeSinceStartup - startTime > limitWaitingTime)
		{
			isTimeOut = true;
			if (_listener != null)
			{
				UIHelper.EnableButton(CancelButton, enable: false, grayscaleIfDisable: true);
				_listener.OnForceMatchCancel();
			}
		}
	}

	private void SetupPlayer(PlayerInfo player, string nick, int teamLevel, int avatarIndex, List<CachedHeroInfo> heroInfoList)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3D);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(player.NickRoot.gameObject, layerInfo.Camera.gameObject.layer);
		UIHelper.SetLabelText(player.Nick, nick);
		UIHelper.SetLabelText(player.TeamLevel, teamLevel);
		UIAvatarHelper.AvatarInfo avatarInfo = new UIAvatarHelper.AvatarInfo(avatarIndex);
		string codeName = avatarInfo.GetCreatureData().CodeName;
		int star = avatarInfo.Star;
		int costumeIndex = avatarInfo.CostumeIndex;
		CachedHeroInfo heroInfo = heroInfoList.Find((CachedHeroInfo e) => e.HeroIndex == avatarInfo.HeroIndex);
		CreateRenderFacade(player, codeName, star, heroInfo, costumeIndex);
		player.Flag.Play(null);
	}

	private void CreateRenderFacade(PlayerInfo player, string creatureCode, int star, CachedHeroInfo heroInfo, int costumeIndex)
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3D);
		AnimationKeyData animationKey = AnimationKeyData.CreateNamedAction("Idle");
		ClientCreatureData data = GameRoot.Instance.ClientCreatureDataContainer.GetData(creatureCode, costumeIndex);
		float num = data?.MatchPickBanScale ?? 300f;
		float[] array = ((data != null) ? data.MatchPickBanOffset : new float[3] { 0f, -700f, 0f });
		player.RenderPivot.localPosition = new Vector3(array[0], array[1], array[2]);
		player.RenderPivot.localScale = new Vector3(num, num, num);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(player.RenderPivot, TeamType.Left, creatureCode, star);
		renderFacadeBuilder.SetCostume(costumeIndex);
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

	public void OnClickCancel()
	{
		if (_listener != null)
		{
			_listener.OnMatchCancel();
			UIHelper.EnableButton(CancelButton, enable: false, grayscaleIfDisable: true);
		}
	}

	public void SetOpponent(MatchedAccountInfo[] accountInfos)
	{
		UIHelper.EnableGameObject(CancelButton, enable: false);
		UIHelper.EnableGameObject(Waiting, enable: false);
		UIHelper.EnableGameObject(Player2Root, enable: true);
		MatchedAccountInfo matchedAccountInfo = accountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		MatchedAccountInfo matchedAccountInfo2 = accountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		if (matchedAccountInfo == null || matchedAccountInfo2 == null)
		{
			VespaDebug.LogError("Error : info null");
			return;
		}
		SetupPlayer(Player2, matchedAccountInfo2.Nick, matchedAccountInfo2.UserInfo.TeamLevel, matchedAccountInfo2.AvatarHeroIndex, matchedAccountInfo2.HeroInfos.Values.ToList());
		if (matchedAccountInfo.TeamType == TeamType.Left)
		{
			Player1.P1.Play();
			Player2.P2.Play();
		}
		else
		{
			Player1.P2.Play();
			Player2.P1.Play();
		}
	}

	public IEnumerator PlayFlagEffect_Co()
	{
		yield return new WaitForSeconds(1f);
		Player1.Flag.Play(null);
		Player2.Flag.Play(null);
	}
}
