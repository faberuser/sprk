using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NBattleField;
using NGame2.NMessage;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NManager.NBattle;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView;

public class CreatureManager : BattleViewGenericSingleton<CreatureManager>, InitOnCreate, ISceneContainer, IInspectorGuiDrawer
{
	private class DisappearEntry
	{
		public GameObject Go;

		public RenderFacade RenderFacade;

		public float ElapsedTime;

		public float ZombieTime;
	}

	public class SingletonVerifier : IVerifier
	{
		bool IVerifier.IsValid()
		{
			return BattleViewGenericSingleton<CreatureManager>.IsValidInstance;
		}
	}

	public static SingletonVerifier Verifier = new SingletonVerifier();

	private IncreaseDic<int, Creature> _creatureDictionary = new IncreaseDic<int, Creature>();

	private List<DisappearEntry> _disappearList = new List<DisappearEntry>();

	private Coroutine _lastKillCo;

	public bool SyncPositionEnabled { get; private set; }

	public bool LastKillAnimating { get; private set; }

	public bool IsOnlineWaveReady { get; set; }

	public static HeroInfo GetHeroInfoFromCreatureUid(int creatureUid)
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return null;
		}
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureUid);
		if (creature == null)
		{
			return null;
		}
		return creature.CreateInfo?.HeroInfo;
	}

	void InitOnCreate.InitializeSingleton()
	{
	}

	ICamera ISceneContainer.GetCamera()
	{
		return Game3DRoot.instance.GameCamera;
	}

	RenderFacade ISceneContainer.GetRenderFacade(int uid)
	{
		Creature creature = GetCreature(uid);
		if (creature != null)
		{
			return creature.RenderFacade;
		}
		return null;
	}

	Creature ISceneContainer.GetBattleCreature(int uid)
	{
		return GetCreature(uid);
	}

	int ISceneContainer.SpawnRenderFacade(StringKey key, RenderFacadeBuilder renderFacadeBuilder)
	{
		return 0;
	}

	void ISceneContainer.ReleaseAssetEffect(string code)
	{
	}

	void ISceneContainer.SpawnAssetEffect(IAsyncLoad<IEffect> effectAsync)
	{
		EffectContainer.ReleaseEffect(effectAsync, detachParent: true, immediatelyRelease: true);
	}

	void ISceneContainer.LoadBattleField(string code, string prefix, Vector3 position, Quaternion rotation, float scale, float backgroundAngle)
	{
	}

	public bool IsDying()
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			if (_creatureDictionary.GetValueByIndex(i).IsDying)
			{
				return true;
			}
		}
		return false;
	}

	public Creature GetCreature(int creatureUid)
	{
		Creature vALUE = _creatureDictionary.GETVALUE(creatureUid, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		return null;
	}

	public int GetCreatureCount()
	{
		return _creatureDictionary.Count;
	}

	public Creature GetCreatureByIndex(int index)
	{
		return _creatureDictionary.GetValueByIndex(index);
	}

	public Creature[] GetCreaturesByTeam(TeamType teamType)
	{
		List<Creature> list = new List<Creature>();
		int creatureCount = GetCreatureCount();
		for (int i = 0; i < creatureCount; i++)
		{
			Creature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex != null && valueByIndex.TeamType == teamType)
			{
				list.Add(valueByIndex);
			}
		}
		return list.ToArray();
	}

	public void ReleaseAllCreatures()
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			Creature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			valueByIndex.Close();
			VespaObject.DestroyGo(valueByIndex);
		}
		_creatureDictionary.Clear();
		for (int j = 0; j < _disappearList.Count; j++)
		{
			DisappearEntry disappearEntry = _disappearList[j];
			RenderFacadeBuilder.ReleaseRenderFacade(disappearEntry.RenderFacade);
			VespaObject.DestroyGo(disappearEntry.Go);
		}
		_disappearList.Clear();
	}

	public void ReleaseExternalHandler()
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			_creatureDictionary.GetValueByIndex(i).NodeTreeController.ReleaseExternalHandler();
		}
	}

	public void EnableSyncPosition(bool enabled)
	{
		SyncPositionEnabled = enabled;
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			Creature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (!enabled && valueByIndex.Action.IsMove())
			{
				valueByIndex.Action.Idle();
			}
		}
	}

	public void ResetTeamViewPosition(TeamType teamType)
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			Creature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (teamType == TeamType.None || valueByIndex.TeamType == teamType)
			{
				Vector3 i_src = ((valueByIndex.TeamType == TeamType.Left) ? Vector3.right : Vector3.left);
				valueByIndex.Action.Idle(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src));
			}
		}
	}

	public void OnEventCamera(EventCamera.EventValue current)
	{
		for (int i = 0; i < GetCreatureCount(); i++)
		{
			Creature creatureByIndex = GetCreatureByIndex(i);
			if (!(creatureByIndex == null) && (current.ApplyTeamType == TeamType.All || current.ApplyTeamType == creatureByIndex.TeamType))
			{
				creatureByIndex.OnEventCamera(current);
			}
		}
	}

	public void AnimateLastKill(int creatureUid)
	{
		MainTimer.EventSet(ETimerEventFlag.LastKill, isSet: true);
		CreatureEventMessage creatureEventMessage = SmallPooling<CreatureEventMessage>.CreateInstance();
		creatureEventMessage.Open(creatureUid, CreatureEventMessage.EEventType.LastKill, null, string.Empty, UnityUnscaledLocalTimer.Default);
		SingletonAuto<GlobalManager>.instance.PostMessage(creatureEventMessage);
		SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
		SceneAutoBehaviour<BattleManagement>.instance.CloseBossStatus();
		SceneAutoBehaviour<BattleManagement>.instance.ClosePatternAlarm();
		if (_lastKillCo != null)
		{
			VespaCoroutine.Stop(this, _lastKillCo);
			_lastKillCo = null;
		}
		_lastKillCo = VespaCoroutine.Start(this, coAnimateLastKill(creatureUid));
	}

	private IEnumerator coAnimateLastKill(int creatureUid)
	{
		float zombieTime = 0f;
		LastKillAnimating = true;
		yield return new WaitForSeconds(0.1f);
		AnimationData.RuntimeData runtime;
		do
		{
			yield return null;
			zombieTime += GlobalTimer.deltaTime;
			if (zombieTime > 10f)
			{
				break;
			}
			Creature creature = GetCreature(creatureUid);
			if (creature == null)
			{
				break;
			}
			runtime = creature.RenderFacade.AnimationData.Runtime;
		}
		while (runtime != null && runtime.Stock.AnimationKey.MotionType == DefineCreature.EMotion.Die && !runtime.IsMotionEnd);
		LastKillAnimating = false;
	}

	public void ChainUpdate()
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		MainTimer.EventSet(ETimerEventFlag.SkillSoloPause, battleCache.IsInSoloTime);
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			_creatureDictionary.GetValueByIndex(i).TimerUpdate();
		}
		for (int j = 0; j < _creatureDictionary.Count; j++)
		{
			_creatureDictionary.GetValueByIndex(j).ChainUpdate();
		}
		float num = 1f;
		for (int k = 0; k < _disappearList.Count; k++)
		{
			DisappearEntry disappearEntry = _disappearList[k];
			disappearEntry.ZombieTime += MainTimer.deltaTime;
			if (disappearEntry.RenderFacade.AnimationData.Runtime != null && disappearEntry.RenderFacade.AnimationData.Runtime.GameAssetStock.Key.MotionType == DefineCreature.EMotion.Die && !disappearEntry.RenderFacade.AnimationData.Runtime.IsMotionEnd && disappearEntry.ZombieTime < 10f)
			{
				disappearEntry.RenderFacade.ChainUpdate();
				continue;
			}
			disappearEntry.ElapsedTime += MainTimer.deltaTime;
			if (disappearEntry.ElapsedTime >= num)
			{
				RenderFacadeBuilder.ReleaseRenderFacade(disappearEntry.RenderFacade);
				VespaObject.DestroyGo(disappearEntry.Go);
				_disappearList.RemoveAt(k);
				k--;
			}
			else
			{
				float alpha = Mathf.InverseLerp(num, 0f, disappearEntry.ElapsedTime);
				disappearEntry.RenderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.DestroyDisappear, alpha);
				disappearEntry.RenderFacade.ChainUpdate();
			}
		}
	}

	public void OnMessageFast(CallbackMessage<SpawnProjectile> message)
	{
		if (message.IsValidMessage())
		{
			SpawnProjectile callback = message.Callback;
			string pathName = $"ExtraSkill/{callback.SkillIndex}";
			if (PackLoad.IsExists(pathName))
			{
				SceneAutoBehaviour<RuntimeGlobalManager>.instance.LoadNodeTree(pathName);
			}
		}
	}

	public void OnMessageFast(CallbackMessage<SpawnCreature> message)
	{
		if (!message.IsValidMessage())
		{
			return;
		}
		SpawnCreature callback = message.Callback;
		if (!_creatureDictionary.ContainsKey(callback.CreatureUid))
		{
			GameObject gameObject = new GameObject($"{callback.CreatureUid}(PId={callback.PlayerId})");
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, base.transform);
			Creature creature = gameObject.AddComponent<Creature>();
			if (!creature.Open(callback))
			{
				creature.Close();
				VespaObject.DestroyGo(gameObject);
			}
			else
			{
				_creatureDictionary.Add(callback.CreatureUid, creature);
			}
		}
	}

	public void OnMessageFast(CallbackMessage<DestroyCreature> message)
	{
		if (message.IsValidMessage())
		{
			DestroyCreature callback = message.Callback;
			Creature vALUE = _creatureDictionary.GETVALUE(callback.CreatureUid, null);
			if (!(vALUE == null))
			{
				_creatureDictionary.RemoveSlow(callback.CreatureUid);
				DisappearEntry disappearEntry = new DisappearEntry();
				disappearEntry.Go = vALUE.gameObject;
				disappearEntry.RenderFacade = vALUE.CloseWithoutRenderFacade();
				disappearEntry.ElapsedTime = 0f;
				_disappearList.Add(disappearEntry);
				VespaObject.DestroyComponent(vALUE);
			}
		}
	}

	public void OnMessageFast(CallbackMessage<EnterCreature> message)
	{
		if (message.IsValidMessage())
		{
			EnterCreature callback = message.Callback;
			Creature vALUE = _creatureDictionary.GETVALUE(callback.CreatureUid, null);
			if (!(vALUE == null))
			{
				vALUE.gameObject.SetActive(value: true);
			}
		}
	}

	public void OnMessageFast(CallbackMessage<LeaveCreature> message)
	{
		if (message.IsValidMessage())
		{
			LeaveCreature callback = message.Callback;
			Creature vALUE = _creatureDictionary.GETVALUE(callback.CreatureUid, null);
			if (!(vALUE == null))
			{
				vALUE.gameObject.SetActive(value: false);
			}
		}
	}

	public void OnMessageFast(CallbackMessage<WaveReady> message)
	{
		if (message.IsValidMessage())
		{
			_ = message.Callback;
			VespaCoroutine.Start(this, coWaitForCreatureLoading(delegate
			{
				IsOnlineWaveReady = true;
			}));
		}
	}

	private IEnumerator coWaitForCreatureLoading(System.Action onLoaded)
	{
		float waitStartTime = Time.time;
		while (true)
		{
			bool flag = true;
			for (int i = 0; i < _creatureDictionary.Count; i++)
			{
				RenderFacade renderFacade = _creatureDictionary.GetValueByIndex(i).RenderFacade;
				if (!(renderFacade == null))
				{
					ICreatureLoad assetCreatureAsync = renderFacade.AssetCreatureAsync;
					if (assetCreatureAsync != null && !assetCreatureAsync.IsDone)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag || Time.time - waitStartTime > 5f)
			{
				break;
			}
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		Vespa_System_Action_Extension_Method.fire(onLoaded);
	}
}
