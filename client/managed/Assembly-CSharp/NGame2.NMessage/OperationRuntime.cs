using System.Collections.Generic;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NMessage;

public class OperationRuntime : SmallPooling<OperationRuntime>, ISmallPoolingItem
{
	public class SoloTimerV2 : ILocalTimer
	{
		public int deltaTick => GlobalTimer.deltaTick;

		public int playbackSpeed
		{
			get
			{
				if (!pause)
				{
					return 1000;
				}
				return 0;
			}
		}

		public bool pause => GlobalTimer.pause;

		public ETimerEventFlag TotalEventFlag => (ETimerEventFlag)0;

		public EAudioMixerFlag TotalAudioMixerFlag => EAudioMixerFlag.None;

		public int GetPlaybackSpeedRecursively(int depth)
		{
			if (!pause)
			{
				return 1000;
			}
			return 0;
		}

		public bool GetPauseRecursively(int depth)
		{
			return GlobalTimer.pause;
		}
	}

	private Dictionary<int, BeginOperation> _beginOperationDic;

	public bool IsUsed => ReserveCount > 0;

	public bool IsEnd
	{
		get
		{
			if (BeginCount > 0 && BeginCount > EndCount)
			{
				return false;
			}
			if (EndCount + CancelCount + ResistCount < ReserveCount)
			{
				return false;
			}
			return true;
		}
	}

	public int ReserveCount { get; private set; }

	public int FlyingCount { get; private set; }

	public int BeginCount { get; private set; }

	public int EndCount { get; private set; }

	public int CancelCount { get; private set; }

	public int ResistCount { get; private set; }

	public int FrameNumber { get; private set; }

	public FireSkill.OperationInfo OperationInfo { get; private set; }

	public FireSkill FireSkillCallback { get; private set; }

	public SkillData SkillData { get; private set; }

	public SkillData FiredSkillData { get; private set; }

	public OperationData OperationData { get; private set; }

	public bool IsSoloTimer { get; private set; }

	public ILocalTimer OperationTimer { get; private set; }

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		ReserveCount = 0;
		FlyingCount = 0;
		BeginCount = 0;
		EndCount = 0;
		CancelCount = 0;
		ResistCount = 0;
		FrameNumber = 0;
		OperationInfo = null;
		FireSkillCallback = null;
		OperationData = null;
		OperationTimer = null;
		if (_beginOperationDic != null)
		{
			_beginOperationDic.Clear();
		}
	}

	public void Open(FireSkill.OperationInfo operationInfo, FireSkill packet)
	{
		OperationInfo = operationInfo;
		FireSkillCallback = packet;
		SkillData = GameRoot.Instance.SkillDataContainer.GetData(FireSkillCallback.SkillId);
		if (SkillData != null)
		{
			OperationData = SkillData.GetOperationData(OperationInfo.OperationNo);
			FiredSkillData = GameRoot.Instance.SkillDataContainer.GetData(FireSkillCallback.FiredSkillId);
			if (FiredSkillData == null)
			{
				FiredSkillData = SkillData;
			}
			SetupOperationTimer();
		}
	}

	public void OnReserve()
	{
		ReserveCount++;
		FrameNumber = Time.frameCount;
	}

	public void OnFlying()
	{
		FlyingCount++;
		FrameNumber = Time.frameCount;
	}

	public void OnBegin(BeginOperation callback)
	{
		BeginCount++;
		FrameNumber = Time.frameCount;
		if (_beginOperationDic == null)
		{
			_beginOperationDic = new Dictionary<int, BeginOperation>();
		}
		_beginOperationDic.Remove(callback.CreatureUid);
		_beginOperationDic.Add(callback.CreatureUid, callback);
	}

	public void OnEnd()
	{
		EndCount++;
		FrameNumber = Time.frameCount;
	}

	public void OnCancel()
	{
		CancelCount++;
		FrameNumber = Time.frameCount;
	}

	public void OnResist()
	{
		ResistCount++;
		FrameNumber = Time.frameCount;
	}

	public BeginOperation GetBeginOperation(int creatureUid)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_beginOperationDic, creatureUid, null);
	}

	private void SetupOperationTimer()
	{
		if (BattleConstant.StopAllOperationInSoloTime)
		{
			IsSoloTimer = false;
			OperationTimer = MainTimer.DefaultLocal;
		}
		else if ((FiredSkillData.GetSoloTimeMs(OperationInfo.IsIgnoreSoloTime) > 0 || SkillData.GetSoloTimeMs(OperationInfo.IsIgnoreSoloTime) > 0) && !OperationData.HasFlag(OperationFlag.PauseOnSoloTime))
		{
			IsSoloTimer = true;
			OperationTimer = new SoloTimerV2();
		}
		else
		{
			IsSoloTimer = false;
			OperationTimer = MainTimer.DefaultLocal;
		}
	}
}
