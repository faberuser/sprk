using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NSound;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroRecruitView : FrameControlWindow, WorldMapManagement.IAvatarHider
{
	public UILabel HeroChatLabel;

	public Transform HeroTransform;

	protected RenderFacade _renderFacade;

	protected ISound _heroSound;

	protected List<int> _areadyOpenHeroIndexList = new List<int>();

	public System.Action InitAction;

	public HeroInfo HeroInfo { get; private set; }

	public CreatureData CreatureData { get; private set; }

	[HideInInspector]
	public int HeroIndex { get; set; }

	protected IEnumerator<TaskCurrentData> OpenAsync(HeroInfo heroInfo, bool isSelectHero, string npcCode = "", bool isInitModel = true)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return TaskCurrentData.RUNNING;
		}
		bool completeOpen = false;
		Open(delegate
		{
			completeOpen = true;
		});
		while (!completeOpen)
		{
			yield return TaskCurrentData.RUNNING;
		}
		HeroInfo = heroInfo;
		CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		HeroIndex = HeroInfo.HeroIndex;
		yield return TaskCurrentData.RUNNING;
		UpdateUI(isSelectHero);
		yield return TaskCurrentData.RUNNING;
		if (isInitModel)
		{
			InitHeroModel(npcCode);
		}
		sceneChanger.Close();
		AddOpenHeroIndex(HeroIndex);
		Vespa_System_Action_Extension_Method.fire(InitAction);
		InitAction = null;
		yield return TaskCurrentData.SUCCESS;
	}

	public IEnumerator<TaskCurrentData> ReloadAsync(HeroInfo heroInfo, string npcCode = "")
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		Reset();
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return TaskCurrentData.RUNNING;
		}
		bool completeOpen = false;
		Open(delegate
		{
			completeOpen = true;
		});
		while (!completeOpen)
		{
			yield return TaskCurrentData.RUNNING;
		}
		HeroInfo = heroInfo;
		CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		HeroIndex = HeroInfo.HeroIndex;
		yield return TaskCurrentData.RUNNING;
		UpdateUI(bSelectHero: true);
		yield return TaskCurrentData.RUNNING;
		InitHeroModel(npcCode);
		sceneChanger.Close();
		Vespa_System_Action_Extension_Method.fire(InitAction);
		InitAction = null;
		yield return TaskCurrentData.SUCCESS;
	}

	public virtual void UpdateUI(bool bSelectHero, bool BaseSpeakAction = true)
	{
	}

	protected override void OnShowRoot(bool isShow)
	{
		base.OnShowRoot(isShow);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_renderFacade != null)
		{
			_renderFacade.ChainUpdate();
		}
		if (IsOpen())
		{
			SceneAutoBehaviour<WorldMapManagement>.instance.AddAvatarHider(this);
		}
	}

	protected virtual void InitHeroModel(string npcCode = "")
	{
		InitHeroModel(ref _renderFacade, HeroInfo, HeroTransform, npcCode);
	}

	protected void InitHeroModel(ref RenderFacade renderFacade, HeroInfo heroInfo, Transform transformRoot, string npcCode = "")
	{
		VectorXZ direction = VectorXZ.zero;
		if (renderFacade != null)
		{
			direction = renderFacade.Action.GetDirection();
			_ = renderFacade.Action.IsBattleMode;
			RenderFacadeBuilder.ReleaseRenderFacade(renderFacade);
			renderFacade = null;
		}
		if (String_Extension_Method.IsNullOrEmpty(npcCode))
		{
			renderFacade = CreateHeroModel(heroInfo, transformRoot);
		}
		else
		{
			renderFacade = CreateHeroModel(heroInfo, npcCode);
		}
		renderFacade.Action.SetDirection(direction, blendMotion: false);
	}

	protected virtual int CheckReplaceBasicCostume(int costumeIndex)
	{
		return -1;
	}

	protected RenderFacade CreateHeroModel(HeroInfo heroInfo, Transform rootTransform)
	{
		ClientCreatureData clientCreatureDataByIndex = DataHelper.GetClientCreatureDataByIndex(heroInfo.HeroIndex, heroInfo.CostumeIndex);
		float num = clientCreatureDataByIndex?.HeroInnScale ?? 100f;
		float[] array = ((clientCreatureDataByIndex != null) ? clientCreatureDataByIndex.HeroInnOffset : new float[3] { 1070f, -540f, -540f });
		string text = ((clientCreatureDataByIndex != null) ? clientCreatureDataByIndex.HeroInnMotion : "Normal");
		rootTransform.localScale = new Vector3(num, num, num);
		rootTransform.localPosition = new Vector3(array[0], array[1], array[2]);
		RenderFacadeBuilder renderFacadeBuilder = null;
		int num2 = heroInfo.CostumeIndex;
		int num3 = CheckReplaceBasicCostume(num2);
		if (num3 != -1)
		{
			num2 = num3;
			HeroInfo heroInfo2 = JsonMarshaler.Clone(heroInfo);
			heroInfo2.CostumeIndex = num2;
			renderFacadeBuilder = new RenderFacadeBuilder(rootTransform, TeamType.Left, heroInfo2);
		}
		else
		{
			renderFacadeBuilder = new RenderFacadeBuilder(rootTransform, TeamType.Left, heroInfo);
		}
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		renderFacadeBuilder.SetCostume(num2);
		RenderFacade renderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		renderFacade.Action.SetCustomIdle(new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, text));
		return renderFacade;
	}

	protected RenderFacade CreateHeroModel(HeroInfo heroInfo, string npcCode)
	{
		int angle;
		int scale;
		int x;
		int y;
		bool stepRewardViewHero = GameRoot.Instance.NPCDataContainer.GetData(npcCode).GetStepRewardViewHero(out angle, out scale, out x, out y);
		float y2 = (stepRewardViewHero ? ((float)angle) : 74f);
		float num = (stepRewardViewHero ? ((float)scale) : 140f);
		float[] array = ((!stepRewardViewHero) ? new float[2] { -80f, -130f } : new float[2] { x, y });
		string text = "Normal";
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D).Camera.transform, TeamType.Left, heroInfo);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		if (HeroInfo.CostumeIndex != 0)
		{
			renderFacadeBuilder.SetCostume(HeroInfo.CostumeIndex);
		}
		RenderFacade renderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		renderFacade.Action.SetCustomIdle(new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, text));
		renderFacade.transform.localRotation = Quaternion.Euler(0f, y2, 0f);
		renderFacade.transform.localScale = new Vector3(num, num, num);
		renderFacade.transform.localPosition = new Vector3(array[0], array[1], 0f);
		return renderFacade;
	}

	private void Close3D()
	{
		if ((bool)_renderFacade)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
			_renderFacade = null;
		}
	}

	public override void Reset()
	{
		base.Reset();
		StopHeroSound();
		Close3D();
	}

	private void StopHeroSound()
	{
		StopHeroSound(ref _heroSound);
	}

	protected void StopHeroSound(ref ISound sound)
	{
		if (sound != null)
		{
			SoundContainer.ReleaseSound(sound);
			sound = null;
		}
	}

	protected void ResponseAction(RenderFacade renderFacade, ref ISound sound, string motionName, string soundName, bool bBlendAction = true)
	{
		if (renderFacade != null)
		{
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			if (string.Compare(motionName, "Stun") == 0)
			{
				stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Stun);
			}
			else if (string.Compare(motionName, "Down") == 0)
			{
				stockData.AnimationKey = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Down);
			}
			else
			{
				stockData.AnimationKey = new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, motionName);
			}
			stockData.MotionEnd = DefineCreature.EMotionEnd.End;
			renderFacade.Action.Motion(stockData, bBlendAction, null);
		}
		bool enable = !TryPlaySound(soundName);
		renderFacade.EnableAnimationSound(enable);
	}

	protected void ResponseAction(string motionName, string soundName, bool bBlendAction = true)
	{
		ResponseAction(_renderFacade, ref _heroSound, motionName, soundName, bBlendAction);
	}

	public void UpdateResponse(FriendlyActionType actionType)
	{
		HeroFriendlyActionData data = GameRoot.Instance.HeroFriendlyActionDataContainer.GetData(HeroIndex);
		if (data != null)
		{
			string speech = string.Empty;
			string motion = string.Empty;
			string sound = string.Empty;
			string[] array = data.GetResponse(actionType);
			if (array == null)
			{
				array = data.ActionResponse1;
			}
			EntryMenuManagement.GetActionValues(array, out speech, out sound, out motion);
			string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(HeroInfo);
			string soundCode = SoundHelper.GetSoundCode(sound, costumeVoicePostfix);
			UIHelper.SetLabelText(HeroChatLabel, LocalizationManager.GetStringDirect(speech));
			ResponseAction(motion, soundCode);
			UpdateUI(bSelectHero: false, BaseSpeakAction: false);
		}
	}

	public bool IsAreadyOpenHero(int heroIndex)
	{
		return _areadyOpenHeroIndexList.Contains(HeroIndex);
	}

	public void AddOpenHeroIndex(int heroIndex)
	{
		if (!_areadyOpenHeroIndexList.Contains(heroIndex))
		{
			_areadyOpenHeroIndexList.Add(heroIndex);
		}
	}

	public void RemoveAllOpenHeroIndex()
	{
		_areadyOpenHeroIndexList.Clear();
	}

	public override void Close()
	{
		base.Close();
		RemoveAllOpenHeroIndex();
	}

	public override void OnSystemBackButton()
	{
		VespaCoroutine.Start(coClose(this));
	}

	public void OnClickClose()
	{
		VespaCoroutine.Start(coClose(this));
	}

	private bool TryPlaySound(string motionName)
	{
		StopHeroSound();
		SoundStock stock = new SoundStock
		{
			Code = motionName,
			LocalTimer = GlobalTimer.DefaultLocal
		};
		_heroSound = SingletonAuto<SoundContainer>.instance.CreateSoundExact(stock);
		if (_heroSound == null)
		{
			return false;
		}
		SingletonAuto<SoundContainer>.instance.PlayOnce(_heroSound, 5f);
		return true;
	}

	protected static IEnumerator coClose(HeroRecruitView This)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(10f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (This != null && This.IsOpen())
		{
			This.Close();
		}
		sceneChanger.Close();
	}

	bool WorldMapManagement.IAvatarHider.IsOpen()
	{
		return base.IsOpen();
	}

	WorldMapManagement.AvatarHideFlag WorldMapManagement.IAvatarHider.GetAvatarHideFlag()
	{
		return WorldMapManagement.AvatarHideFlag.HideAll;
	}
}
