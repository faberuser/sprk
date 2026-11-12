using NGame2.NBattleView;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaitForGimmick : BaseWaitForNext<WaitForGimmick>
{
	private class CheckGimmick : IJob
	{
		bool IJob.Complete(WaitForGimmick _this)
		{
			return ReadyToPlayGimmick(_this);
		}

		private bool ReadyToPlayGimmick(WaitForGimmick _this)
		{
			if (_this != null && _this._listener != null && _this._listener.WaveWinner() == TeamType.Left && _this.HasWaveGimmick())
			{
				string text = _this.ChangeMapName();
				string text2 = _this.ChagneFieldCameraName();
				_this._jobList.Add(new CreateSceneChange());
				if (!String_Extension_Method.IsNullOrEmpty(text))
				{
					_this._jobList.Add(new ChangeBattleField(text));
				}
				if (!String_Extension_Method.IsNullOrEmpty(text2))
				{
					_this._jobList.Add(new ChangeFieldCamera(text2));
				}
				_this._jobList.Add(new WaveMove());
				_this._jobList.Add(new CloseSceneChange());
				_this._jobList.Add(new SendWaveResume());
				_this._jobList.Add(new WaitWaveMove());
			}
			return true;
		}
	}

	private class ChangeBattleField : IJob
	{
		public string fieldName;

		public ChangeBattleField(string fieldName)
		{
			this.fieldName = fieldName;
		}

		bool IJob.Complete(WaitForGimmick _this)
		{
			HashString hashString = new HashString();
			hashString._value = fieldName;
			SingletonAuto<BattleFacade>.instance.ChangeBattleField(hashString);
			return true;
		}
	}

	private class ChangeFieldCamera : IJob
	{
		private string _cameraName = string.Empty;

		public ChangeFieldCamera(string cameraName)
		{
			_cameraName = cameraName;
		}

		bool IJob.Complete(WaitForGimmick This)
		{
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Freeze();
			Game3DRoot.instance.GameCamera.ChangeFieldCameraData(_cameraName);
			return true;
		}
	}

	private PunishmentRaidData _punishmentRaidData;

	private bool IsPunishmentRaidBoss
	{
		get
		{
			if (_listener == null)
			{
				return false;
			}
			RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(_listener.GetChapterIndex(), _listener.GetDungeonIndex());
			if (raidDataByDungeon == null)
			{
				return false;
			}
			return GameRoot.Instance.PunishmentRaidDataContainer.GetData(raidDataByDungeon.Index, raidDataByDungeon.Level)?.IsBoss ?? false;
		}
	}

	private bool HasWaveGimmick()
	{
		if (_punishmentRaidData == null)
		{
			return false;
		}
		if (!_punishmentRaidData.IsBoss || __Extension_Method_ICollection__.IsNullOrEmpty(_punishmentRaidData.WaveGimmickValues))
		{
			return false;
		}
		return true;
	}

	public string ChangeMapName()
	{
		if (_punishmentRaidData == null)
		{
			return null;
		}
		return ArgParser.GetValueString(_punishmentRaidData.WaveGimmickValues, 1);
	}

	public string ChagneFieldCameraName()
	{
		if (_punishmentRaidData == null)
		{
			return null;
		}
		return ArgParser.GetValueString(_punishmentRaidData.WaveGimmickValues, 2);
	}

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		_punishmentRaidData = GetPunishmentRaidData();
		_jobList.Add(new CheckGimmick());
	}

	private PunishmentRaidData GetPunishmentRaidData()
	{
		if (_listener == null)
		{
			return null;
		}
		int chapterIndex = _listener.GetChapterIndex();
		int dungeonIndex = _listener.GetDungeonIndex();
		RaidData raidDataByDungeon = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(chapterIndex, dungeonIndex);
		if (raidDataByDungeon == null)
		{
			return null;
		}
		PunishmentRaidData data = GameRoot.Instance.PunishmentRaidDataContainer.GetData(raidDataByDungeon.Index, raidDataByDungeon.Level);
		if (data == null)
		{
			return null;
		}
		return data;
	}

	public override void ChainUpdate()
	{
		while (!__Extension_Method_ICollection__.IsNullOrEmpty(_jobList))
		{
			if (!_jobList[0].Complete(this))
			{
				return;
			}
			DebugLog($"[4-4.WaitForGimmick] Remove Job: {_jobList[0].GetType().Name}");
			_jobList.RemoveAt(0);
		}
		if (HasWaveGimmick())
		{
			Next(new WaveOpening(_handlerListWaveReady));
		}
		else
		{
			Next(new WaveEnding());
		}
	}
}
