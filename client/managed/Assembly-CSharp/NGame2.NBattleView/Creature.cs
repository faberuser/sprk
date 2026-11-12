using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager;
using NGame2.NBattleView.NCreature;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NBattleView.NUIOn3D;
using NGame2.NBattleView.NUIOn3d2;
using NGame2.NEventTrigger;
using NGame2.NMessage;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NComponent2.NBattle.NSub;
using NGame2.NUI.NManager.NBattle;
using NGameAsset;
using NGameAsset.NSound;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView;

public class Creature : MonoBehaviour, HUD.IValue, BattleChattingEmoticonOn3D.IValue, IInspectorGuiDrawer
{
	private HUD _hud;

	private BattleChattingEmoticonOn3D _emoticon;

	private Transform _cachedTransform;

	private float _deadElapsedTime;

	private HashSet<int> _triggeredEventSet = new HashSet<int>();

	private HudOffset _hudOffset;

	private SoundStock _deadSoundStock;

	private SoundStock _damageSoundStock;

	private SoundStock _downSoundStock;

	public int Uid { get; private set; }

	public BattleFacade.CreatureCacheData CreatureCache => SingletonAuto<BattleFacade>.instance.GetCreatureCache(Uid);

	public int CreateWaveIndex { get; private set; }

	public BattleCreatureCreateInfo CreateInfo { get; private set; }

	public CreatureData CreatureTableData { get; private set; }

	public CreatureVisualData VisualData { get; private set; }

	public Action Action => RenderFacade.Action;

	public LocalTimer BaseLocalTimer { get; private set; }

	public LocalTimer ActionLocalTimer { get; private set; }

	public LocalTimer MoveLocalTimer { get; private set; }

	public RenderFacade RenderFacade { get; private set; }

	public Bounds ShapeBounds => RenderFacade.ShapeBounds;

	public NGame2.NBattleField.NManager.DropItemManager.CreatureDrop CreatureDrop { get; private set; }

	public DrawTargetController DrawTargetController { get; private set; }

	public PackLoad NodeTreePackLoad { get; private set; }

	public CreatureVariable Variable { get; private set; }

	public NodeTreeController NodeTreeController { get; private set; }

	public CastingComponent.ICastingValue CastingValue { get; set; }

	public Dictionary<string, int> KeyStackDic { get; private set; }

	public TeamType TeamType => CreatureCache.TeamType;

	public bool IsLeftTeam => TeamType == TeamType.Left;

	public bool IsDead => CreatureCache.IsDead;

	public bool IsResting => CreatureCache.IsResting;

	public bool IsDying
	{
		get
		{
			AnimationData.RuntimeData runtime = RenderFacade.AnimationData.Runtime;
			if (runtime.Stock.State == AnimationData.EState.Die && !runtime.IsMotionEnd)
			{
				return true;
			}
			return false;
		}
	}

	public int HeroTableIndex => CreateInfo.HeroInfo.HeroIndex;

	public string HeroCodeName => CreatureTableData.CodeName;

	public Transform cachedTransform => _cachedTransform;

	public Vector3 Position => _cachedTransform.transform.position;

	public int GroupIndex => CreateInfo.GroupIndex;

	bool HUD.IValue.IsDie => IsDead;

	float HUD.IValue.HpRatio => (float)CreatureCache.HpRatioM * 0.001f;

	float HUD.IValue.HudOffset
	{
		get
		{
			if (_hudOffset == null)
			{
				return ShapeBounds.size.y + 0.1f;
			}
			return _hudOffset.GetY();
		}
	}

	long HUD.IValue.Hp => CreatureCache.Hp;

	long HUD.IValue.MaxHp => CreatureCache.MaxHp;

	int HUD.IValue.Mp => CreatureCache.Mp;

	long HUD.IValue.MaxMp => CreatureCache.MaxMp;

	public bool OnDynamicHUD
	{
		set
		{
			if (_hudOffset != null)
			{
				_hudOffset.SetDynamicHub(value);
			}
		}
	}

	public float HUDOffetY
	{
		set
		{
			if (_hudOffset != null)
			{
				_hudOffset.SetHudOffsetY(value);
			}
		}
	}

	bool BattleChattingEmoticonOn3D.IValue.IsDie => IsDead;

	Vector2 BattleChattingEmoticonOn3D.IValue.EmoticonOffset => new Vector2(IsLeftTeam ? (0f - (ShapeBounds.size.x * 0.5f + 0.2f)) : (ShapeBounds.size.x * 0.5f + 0.2f), ShapeBounds.size.y + 0.5f);

	CastingComponent.ICastingValue HUD.IValue.Casting => CastingValue;

	public SubCreature PendingDestroySubCreature { get; private set; }

	public SubCreature SubCreature { get; private set; }

	private int CalcPlaybackSpeed()
	{
		BattleFacade.CreatureCacheData creatureCache = CreatureCache;
		if (creatureCache.Uid == 0)
		{
			return 1000;
		}
		return creatureCache.AttackSpeed;
	}

	public bool Open(SpawnCreature spawnCreature)
	{
		bool isPlayer = spawnCreature.CreateInfo.IsPlayer;
		_ = spawnCreature.CreateInfo.IsBoss;
		_cachedTransform = base.transform;
		Uid = spawnCreature.CreatureUid;
		BaseLocalTimer = new LocalTimer(null);
		MoveLocalTimer = new LocalTimer(BaseLocalTimer);
		ActionLocalTimer = new LocalTimer(BaseLocalTimer);
		ActionLocalTimer.playbackSpeedFunc = CalcPlaybackSpeed;
		CreateWaveIndex = SingletonAuto<BattleFacade>.instance.GetBattleCache().WaveIndex;
		CreateInfo = spawnCreature.CreateInfo;
		CreatureTableData = GameRoot.Instance.CreatureDataContainer.GetData(HeroTableIndex);
		if (CreatureTableData == null)
		{
			return false;
		}
		if (CreatureTableData.Race == CreatureRaceType.Darkness)
		{
			BaseLocalTimer.AudioMixerFlag |= EAudioMixerFlag.DarknessCreature;
		}
		else if (isPlayer)
		{
			BaseLocalTimer.AudioMixerFlag |= EAudioMixerFlag.Player;
		}
		string pathNameFromCode = PackHelper.GetPathNameFromCode(HeroCodeName);
		NodeTreePackLoad = SceneAutoBehaviour<RuntimeGlobalManager>.instance.LoadNodeTree(pathNameFromCode);
		LoadExtraSkill(CreatureTableData.ExtraSkill);
		LoadExtraSkill(spawnCreature.CreateInfo.AddExtraSkill);
		NodeTreeController = new NodeTreeController(this);
		Variable = SmallPooling<CreatureVariable>.CreateInstance();
		KeyStackDic = new Dictionary<string, int>();
		string text = string.Empty;
		string modelPrefix = string.Empty;
		bool ignoreFlip = false;
		if (!String_Extension_Method.IsNullOrEmpty(spawnCreature.CreateInfo.VisualInfo))
		{
			VisualData = GameRoot.Instance.CreatureVisualDataContainer.GetData(spawnCreature.CreateInfo.VisualInfo);
			if (VisualData == null)
			{
				text = spawnCreature.CreateInfo.VisualInfo;
			}
			else
			{
				text = VisualData.OnSpawn;
				modelPrefix = VisualData.ModelPrefix;
				ignoreFlip = VisualData.IgnoreFlip;
			}
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(cachedTransform, TeamType, spawnCreature.CreateInfo.HeroInfo);
		renderFacadeBuilder.IgnoreFlip = ignoreFlip;
		renderFacadeBuilder.SetModelPrefix(modelPrefix);
		RenderFacade = renderFacadeBuilder.Build();
		if (RenderFacade == null)
		{
			VespaDebug.LogError("RendererFacade is null");
			return false;
		}
		RenderFacade.SetTimer(ActionLocalTimer);
		if (VisualData != null && !String_Extension_Method.IsNullOrEmpty(VisualData.DamageColor))
		{
			RenderFacade.ColorEffect.SetDamageColorName(VisualData.DamageColor);
		}
		if (spawnCreature.CreateInfo.CreatureScaleR != 0)
		{
			float scale = (float)spawnCreature.CreateInfo.CreatureScaleR * 0.001f;
			RenderFacade.SetRuntimeScale("SpawnCreature.CreatureScaleR", scale);
		}
		if (spawnCreature.CreateInfo.CreatureHeightMm != 0)
		{
			float num = (float)spawnCreature.CreateInfo.CreatureHeightMm * 0.001f;
			RenderFacade.SetRuntimeOffset("SpawnCreature.CreatureOffsetY", Vector3.up * num);
		}
		Vector3 localPosition = RenderFacade.transform.localPosition;
		bool flag = false;
		int offsetMx = CreatureTableData.OffsetMx;
		if (offsetMx != 0)
		{
			localPosition.x += (float)offsetMx / 1000f;
			flag = true;
		}
		int offsetMz = CreatureTableData.OffsetMz;
		if (offsetMz != 0)
		{
			localPosition.z += (float)offsetMz / 1000f;
			flag = true;
		}
		if (flag)
		{
			RenderFacade.transform.localPosition = localPosition;
		}
		DrawTargetController = new DrawTargetController(this);
		long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		HUD.EHudType hudType = ((SingletonAuto<BattleFacade>.instance.GetPlayerCache(id).TeamType != TeamType) ? HUD.EHudType.Enemy : ((spawnCreature.PlayerId != id) ? HUD.EHudType.Ally : HUD.EHudType.Me));
		EBattleMenu battleMenu = SceneAutoBehaviour<BattleManagement>.instance.BattleMenu;
		if ((battleMenu & EBattleMenu.Hud) != EBattleMenu.None)
		{
			bool flag2 = true;
			BattleDefineData battleDefine = SingletonAuto<BattleContext>.instance.DataProvider.GetBattleDefine();
			CreatureRaceType race = spawnCreature.CreateInfo.HeroInfo.CreatureData.Race;
			bool flag3 = VisualData != null && VisualData.HideHpBar;
			if ((race == CreatureRaceType.Boss && battleDefine.DisableBossHUD) || flag3)
			{
				flag2 = false;
			}
			if (flag2)
			{
				_hud = HUD.CreateInstance(cachedTransform);
				if (_hud != null)
				{
					string nickname = GameSetting.data.Test.GetNickname(TeamType, HeroTableIndex);
					_hud.Open(hudType, spawnCreature, this, nickname);
				}
				else
				{
					VespaDebug.LogError("HUD is null");
				}
				_hudOffset = new HudOffset(ShapeBounds, RenderFacade);
			}
		}
		if ((battleMenu & EBattleMenu.EmoticonPopup) != EBattleMenu.None && Uid != 0)
		{
			_emoticon = BattleChattingEmoticonOn3D.PrefabAuto.Factory.Create(cachedTransform, Game3DRoot.BattleUILayer);
			if (_emoticon != null)
			{
				_emoticon.Open(this, TeamType);
			}
		}
		SetBattlePos(spawnCreature.SpawnPosition);
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			SendCreatureEventMessage(CreatureEventMessage.EEventType.Spawn, text);
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		CreatureDrop = SceneAutoBehaviour<NGame2.NBattleField.NManager.DropItemManager>.instance.GetDrop(spawnCreature.CreateInfo.IsBoss, battleCache.WaveIndex, spawnCreature.CreateInfo.WaveSlotIndex);
		SetSoundStock();
		return true;
	}

	private void LoadExtraSkill(ExtraSkillInfo[] extraSkills)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(extraSkills); i++)
		{
			LoadExtraSkill(extraSkills[i].SkillIndex);
		}
	}

	private void LoadExtraSkill(int[] extraSkills)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(extraSkills); i++)
		{
			LoadExtraSkill(extraSkills[i]);
		}
	}

	private void LoadExtraSkill(int skillIndex)
	{
		string pathName = $"ExtraSkill/{skillIndex}";
		if (PackLoad.IsExists(pathName))
		{
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.LoadNodeTree(pathName);
		}
	}

	public void SendCreatureEventMessage(CreatureEventMessage.EEventType eventType, string keyword)
	{
		CreatureEventMessage creatureEventMessage = CreateCreatureEventMessage(eventType, keyword);
		if (creatureEventMessage != null)
		{
			SingletonAuto<GlobalManager>.instance.PostMessage(creatureEventMessage);
		}
	}

	public CreatureEventMessage CreateCreatureEventMessage(CreatureEventMessage.EEventType eventType, string keyword)
	{
		if (!NodeTreePackLoad.ISDONE() || !NodeTreePackLoad.ISVALID())
		{
			return null;
		}
		CreatureEventMessage creatureEventMessage = SmallPooling<CreatureEventMessage>.CreateInstance();
		creatureEventMessage.Open(Uid, eventType, NodeTreePackLoad.Value, keyword);
		return creatureEventMessage;
	}

	public void Close()
	{
		RenderFacade renderFacade = CloseWithoutRenderFacade();
		if (renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(renderFacade);
			renderFacade = null;
		}
		if (PendingDestroySubCreature != null)
		{
			PendingDestroySubCreature.ReleaseSubCreature();
		}
		if (SubCreature != null)
		{
			SubCreature.ReleaseSubCreature();
		}
		_triggeredEventSet.Clear();
	}

	public RenderFacade CloseWithoutRenderFacade()
	{
		BaseLocalTimer.EventSet(ETimerEventFlag.SkillSoloPlaying, isSet: false);
		if (DrawTargetController != null)
		{
			DrawTargetController.Close();
			DrawTargetController = null;
		}
		if (NodeTreeController != null)
		{
			NodeTreeController.Close();
			NodeTreeController = null;
		}
		if (Variable != null)
		{
			SmallPooling<CreatureVariable>.ReleaseInstance(Variable);
			Variable = null;
		}
		if (KeyStackDic != null)
		{
			KeyStackDic = null;
		}
		if (_hud != null)
		{
			_hud.Close();
			HUD.ReleaseInstance(_hud);
			_hud = null;
		}
		if (_emoticon != null)
		{
			BaseBattleChattingEmoticon.ReleaseInstance(_emoticon);
			_emoticon = null;
		}
		RenderFacade renderFacade = RenderFacade;
		RenderFacade = null;
		return renderFacade;
	}

	public void SetBattlePos(Vector3 position, bool validateBattleArea = false)
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (validateBattleArea && battleCache.IsBattleEnabled && battleCache.ElapsedBattleTimeMs > 3000)
		{
			cachedTransform.position = SingletonAuto<BattleFacade>.instance.ValidateBattlePos(CreateInfo.GroupIndex, position);
		}
		else
		{
			cachedTransform.position = position;
		}
	}

	public void TimerUpdate()
	{
		BaseLocalTimer.EventSet(ETimerEventFlag.SkillSoloPlaying, CreatureCache.IsInSoloSkill);
	}

	public void ChainUpdate()
	{
		if (IsDead)
		{
			UpdateDeadExtraAlpha();
		}
		if (!IsDead && Action.IsDead)
		{
			Action.Revive();
			RenderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.Dead, 1f);
		}
		NodeTreeController.ChainUpdate();
		RenderFacade.ChainUpdate();
		DrawTargetController.ChainUpdate();
		if (_hud != null)
		{
			_hud.ChainUpdate();
		}
		if (_emoticon != null)
		{
			_emoticon.ChainUpdate();
		}
		if (!TryUpdateSubCreature(PendingDestroySubCreature))
		{
			PendingDestroySubCreature = null;
		}
		if (!TryUpdateSubCreature(SubCreature))
		{
			SubCreature = null;
		}
	}

	private bool TryUpdateSubCreature(SubCreature subCreature)
	{
		if (subCreature == null)
		{
			return false;
		}
		if (subCreature.Released)
		{
			return false;
		}
		subCreature.ChainUpdate();
		return true;
	}

	private void UpdateDeadExtraAlpha()
	{
		AnimationData.RuntimeData runtime = RenderFacade.AnimationData.Runtime;
		if (runtime != null && runtime.Stock.State == AnimationData.EState.Die && runtime.IsMotionEnd)
		{
			_deadElapsedTime += MainTimer.deltaTime;
			if (!(_deadElapsedTime < GameSetting.data.BattleView.Creature_DeadDisappearDelay))
			{
				float num = _deadElapsedTime - GameSetting.data.BattleView.Creature_DeadDisappearDelay;
				float alpha = Mathf.Clamp01(1f - num);
				RenderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.Dead, alpha);
			}
		}
	}

	public void OnEventCamera(EventCamera.EventValue current)
	{
		if (_hud != null)
		{
			_hud.EventAlpha = current.UiOn3dAlpha;
		}
		if (_emoticon != null)
		{
			_emoticon.EventAlpha = current.UiOn3dAlpha;
		}
	}

	private void CreateDropItems()
	{
		if (CreatureDrop != null)
		{
			BattleViewGenericSingleton<DropItemManager>.instance.CreateDrop(this, CreatureDrop);
			CreatureDrop = null;
		}
	}

	public void SetCreateWaveIndex(int waveIndex)
	{
		CreateWaveIndex = waveIndex;
	}

	public void OnMessageFast(EmoticonData data)
	{
		if (_emoticon != null)
		{
			_emoticon.Show(data);
		}
	}

	public void OnMessageFast(CreatureEventMessage message)
	{
		if (message.IsValidMessage() && message.CreatureUid == Uid)
		{
			if (message.EventType == CreatureEventMessage.EEventType.BossNameTag)
			{
				SceneAutoBehaviour<BattleManagement>.instance.OpenBossNameTag(this, isBossTag: true);
			}
			else if (message.EventType == CreatureEventMessage.EEventType.EliteNameTag)
			{
				SceneAutoBehaviour<BattleManagement>.instance.OpenBossNameTag(this, isBossTag: false);
			}
			else if (message.EventType == CreatureEventMessage.EEventType.BossStatus_Show)
			{
				SceneAutoBehaviour<BattleManagement>.instance.OpenBossStatus(this);
				SceneAutoBehaviour<BattleManagement>.instance.OpenPatternAlarm(this);
			}
		}
	}

	public void OnMessageFast(OperationMessage<ResistOperation> message)
	{
		if (message.IsValidMessage() && message.Callback.CreatureUid == Uid && SprkArenaPractice.DamageText() != ERenderDamageText.HideAll)
		{
			Manager.instance.OnResist(message.TargetCreatureUid, message.Callback.StateType);
		}
	}

	public void OnMessageFast(CallbackMessage<NShared.BeginMove> message)
	{
		if (message.IsValidMessage() && message.Callback.CreatureUid == Uid && message.Callback.OperationUid == 0L)
		{
			NodeTreeController.SetExternalHandler(new NGame2.NBattleView.NCreature.NHandler.BeginMove(this, message.Callback));
		}
	}

	public void OnMessageFast(CallbackMessage<NShared.SetPosition> message)
	{
		if (message.IsValidMessage() && (message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.None || message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.Spawn) && NodeTreeController != null && NodeTreeController.Main == null)
		{
			NodeTreeController.SetExternalHandler(new NGame2.NBattleView.NCreature.NHandler.SetPosition(this, message.Callback));
		}
	}

	public void OnMessageFast(CallbackMessage<NShared.Damage> message)
	{
		if (!message.IsValidMessage() || message.Callback.CreatureUid != Uid)
		{
			return;
		}
		if (message.Callback.IsDead)
		{
			if (!SingletonAuto<BattleContext>.instance.DataProvider.GetBattleDefine().IgnoreDropItem)
			{
				CreateDropItems();
			}
			SendDeadCreatureMessage();
			NotifyOnKill(message.Callback);
		}
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(message.Callback.OperationUid);
		if (operationRuntime == null)
		{
			return;
		}
		bool num = (message.Callback.DamageFlag & DamageFlag.Dodged) != 0;
		bool flag = (message.Callback.DamageFlag & DamageFlag.Immune) != 0;
		bool flag2 = (message.Callback.DamageFlag & DamageFlag.Dot) != 0;
		CreateDisplayInstance(message, operationRuntime);
		if (num || flag)
		{
			return;
		}
		if (CreateInfo != null && CreateInfo.IsBoss && message.Callback.Amount != 0L)
		{
			BossHpPercentage triggerEvent = new BossHpPercentage
			{
				ChapterIndex = SingletonAuto<BattleFacade>.instance.GetCampaignCache().ChapterIndex,
				DungeonIndex = SingletonAuto<BattleFacade>.instance.GetCampaignCache().DungeonIndex,
				Percentage = message.Callback.FinalPercentage
			};
			if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, _triggeredEventSet, out var triggeredData) && triggeredData != null)
			{
				_triggeredEventSet.Add(triggeredData.Index);
			}
		}
		if (message.Callback.IsDead)
		{
			_deadElapsedTime = 0f;
			NodeTreeController.SetExternalHandler(null);
			Action.Die(operationRuntime.OperationTimer);
			_deadSoundStock.LocalTimer = operationRuntime.OperationTimer;
			_deadSoundStock.MixerFlag = BaseLocalTimer.AUDIOMIXERFLAG();
			bool enable = !SingletonAuto<SoundContainer>.instance.TryPlayOnce(_deadSoundStock, 10f);
			RenderFacade.EnableAnimationSound(enable);
			BattleFacade.BattleConditionBase battleCondition = SingletonAuto<BattleFacade>.instance.BattleCondition;
			if (battleCondition != null && battleCondition.IsLastKill())
			{
				BattleDefineData battleDefine = SingletonAuto<BattleContext>.instance.DataProvider.GetBattleDefine();
				if (battleDefine != null && !battleDefine.DisableLastKill)
				{
					BattleViewGenericSingleton<CreatureManager>.instance.AnimateLastKill(Uid);
				}
				else
				{
					RenderFacade.ColorEffect.Damage();
				}
			}
			else
			{
				RenderFacade.ColorEffect.Damage();
			}
		}
		else
		{
			VectorXZ direction = Tools.GetDirection(message.Callback.CreatureUid, operationRuntime.FireSkillCallback.CreatureUid);
			RenderFacade.ColorEffect.Damage();
			if (!flag2 && !CreatureTableData.IsStaticPosition())
			{
				RenderFacade.ShakeEffect.Damage(-direction);
			}
		}
	}

	private void CreateDisplayInstance(CallbackMessage<NShared.Damage> message, OperationRuntime operation)
	{
		if (SprkArenaPractice.DamageText() != ERenderDamageText.HideAll)
		{
			bool num = (message.Callback.DamageFlag & DamageFlag.Dodged) != 0;
			bool flag = (message.Callback.DamageFlag & DamageFlag.Immune) != 0;
			bool flag2 = (message.Callback.DamageFlag & DamageFlag.Critical) != 0;
			bool flag3 = (message.Callback.DamageFlag & DamageFlag.Blocked) != 0;
			bool flag4 = (message.Callback.DamageFlag & DamageFlag.Dot) != 0;
			bool flag5 = SingletonAuto<BattleFacade>.instance.IsActiveSkill(operation.FireSkillCallback);
			bool flag6 = SprkArenaPractice.DamageText() == ERenderDamageText.ShowAll;
			bool flag7 = SprkArenaPractice.DamageText() == ERenderDamageText.ShowAbnormal;
			bool num2 = num && (flag6 || flag7);
			bool flag8 = flag && (flag6 || flag7);
			bool flag9 = flag2 && (flag6 || flag7);
			bool flag10 = flag3 && (flag6 || flag7);
			bool flag11 = flag4 && flag6;
			bool flag12 = flag5 && flag6;
			bool flag13 = flag6;
			if (num2)
			{
				Manager.instance.OnMiss(message.Callback.CreatureUid);
			}
			else if (flag8)
			{
				bool isMagical = (message.Callback.DamageFlag & DamageFlag.Magical) != 0;
				Manager.instance.OnImmune(message.Callback.CreatureUid, isMagical);
			}
			else if (flag9)
			{
				Manager.instance.OnDamageCritical(message.Callback.CreatureUid, message.Callback.Amount);
			}
			else if (flag10)
			{
				Manager.instance.OnBlock(message.Callback.CreatureUid, message.Callback.Amount);
			}
			else if (flag11)
			{
				Manager.instance.OnDamageDot(message.Callback.CreatureUid, message.Callback.Amount, message.Callback.DamageStateType);
			}
			else if (flag12)
			{
				Manager.instance.OnDamageSkill(message.Callback.CreatureUid, message.Callback.Amount);
			}
			else if (flag13)
			{
				Manager.instance.OnDamageNormal(message.Callback.CreatureUid, message.Callback.Amount);
			}
		}
	}

	private void SendDeadCreatureMessage()
	{
		string visualInfo = CreateInfo.VisualInfo;
		if (!String_Extension_Method.IsNullOrEmpty(visualInfo))
		{
			string text = visualInfo;
			CreatureVisualData data = GameRoot.Instance.CreatureVisualDataContainer.GetData(visualInfo);
			if (data != null)
			{
				text = data.OnDead;
			}
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				SendCreatureEventMessage(CreatureEventMessage.EEventType.Dead, text);
			}
		}
	}

	private void NotifyOnKill(NShared.Damage callback)
	{
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(callback.OperationUid);
		if (operationRuntime != null)
		{
			Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(operationRuntime.FireSkillCallback.CreatureUid);
			if (!(creature == null))
			{
				SceneAutoBehaviour<BattleManagement>.instance.OnKillCreature(creature, this);
			}
		}
	}

	public bool OnMessageFast(CallbackMessage<Heal> message)
	{
		if (!message.IsValidMessage())
		{
			return false;
		}
		if (message.Callback.CreatureUid != Uid)
		{
			return false;
		}
		if (SprkArenaPractice.DamageText() == ERenderDamageText.ShowAll && (message.Callback.HealFlag & HealFlag.NoUiDisplay) == 0)
		{
			Manager.instance.OnHeal(message.Callback.CreatureUid, message.Callback.Amount);
		}
		if (CreateInfo != null && CreateInfo.IsBoss && message.Callback.Amount != 0L)
		{
			BossHpPercentage triggerEvent = new BossHpPercentage
			{
				ChapterIndex = SingletonAuto<BattleFacade>.instance.GetCampaignCache().ChapterIndex,
				DungeonIndex = SingletonAuto<BattleFacade>.instance.GetCampaignCache().DungeonIndex,
				Percentage = message.Callback.FinalPercentage
			};
			if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, _triggeredEventSet, out var triggeredData) && triggeredData != null)
			{
				_triggeredEventSet.Add(triggeredData.Index);
			}
		}
		return true;
	}

	public bool OnMessageFast(CallbackMessage<ChangeState> message)
	{
		if (!message.IsValidMessage())
		{
			return false;
		}
		if (message.Callback.CreatureUid != Uid)
		{
			return false;
		}
		if (message.IsUsed)
		{
			return false;
		}
		StateData data = GameRoot.Instance.StateDataContainer.GetData(message.Callback.ChangedStateType);
		if (data != null && data.CcMotionType == CcMotionType.Knockback && message.Callback.StateAdded)
		{
			_damageSoundStock.LocalTimer = BaseLocalTimer;
			bool enable = !SingletonAuto<SoundContainer>.instance.TryPlayOnce(_damageSoundStock, 10f);
			RenderFacade.EnableAnimationSound(enable);
		}
		if (CrowdControl.IsCrowdControl(message.Callback))
		{
			NodeTreeController.SetExternalHandler(new CrowdControl(this, message.Callback));
		}
		return true;
	}

	public void CreateSubCreature(string code, Transform tm, long pivotUid)
	{
		if (SubCreature != null)
		{
			ReleaseSubCreature(null, null);
		}
		SubCreature = new SubCreature();
		SubCreature.CreateSubCreature(code, ActionLocalTimer, tm, pivotUid);
	}

	public void ReleaseSubCreature(LinkNodeTree2 linkNodeTree, RuntimeInstance runtime)
	{
		if (!PendingReleaseSubCreature(linkNodeTree, runtime) && SubCreature != null)
		{
			SubCreature.ReleaseSubCreature();
			SubCreature = null;
		}
	}

	private bool PendingReleaseSubCreature(LinkNodeTree2 linkNodeTree, RuntimeInstance runtime)
	{
		if (linkNodeTree == null || runtime == null)
		{
			return false;
		}
		LinkNodeTree2.EGetResult oResult;
		TreeData tree = linkNodeTree.GetTree(out oResult);
		if (tree == null || oResult != LinkNodeTree2.EGetResult.Ok)
		{
			return false;
		}
		BuildChildRootRuntime buildChildRootRuntime = default(BuildChildRootRuntime);
		if (!buildChildRootRuntime.Open(tree))
		{
			return false;
		}
		buildChildRootRuntime.AddValue(VariableKeyword.Myself, (object)SubCreature);
		RuntimeInstance runtimeInstance = buildChildRootRuntime.Build(runtime.SceneContainer, runtime.LocalTimer, runtime.Observer, runtime.Layer);
		if (runtimeInstance == null)
		{
			return false;
		}
		if (!SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtimeInstance, immediatelyStart: false))
		{
			return false;
		}
		PendingDestroySubCreature = SubCreature;
		SubCreature = null;
		return true;
	}

	private void SetSoundStock()
	{
		string prefix = "Dead";
		string prefix2 = "Damage";
		string prefix3 = "Down";
		string heroCodeName = HeroCodeName;
		string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(CreateInfo.HeroInfo);
		_deadSoundStock.Code = SoundHelper.GetSoundCode(prefix, heroCodeName, costumeVoicePostfix);
		_damageSoundStock.Code = SoundHelper.GetSoundCode(prefix2, heroCodeName, costumeVoicePostfix);
		_downSoundStock.Code = SoundHelper.GetSoundCode(prefix3, heroCodeName, costumeVoicePostfix);
	}
}
