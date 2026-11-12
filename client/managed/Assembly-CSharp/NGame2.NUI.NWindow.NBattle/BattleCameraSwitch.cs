using System;
using NGame2.NAccount;
using NGame2.NBattleField;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleCameraSwitch : BaseWindow
{
	public interface IListener
	{
		void SwichingCamera();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BattleCameraSwitch";

	private readonly string[] AnimatorTriggerNames = new string[2] { "ToDark", "ToLight" };

	public Animator Anim;

	public UIButton ButtonSwitch;

	public UISprite SpriteLight;

	public UISprite SpriteDark;

	[SerializeField]
	public ParticleSystem[] ParticleSystems;

	private float _switchInterval = 1f;

	private IListener _listener;

	private TeamType _myTeamType;

	private int _curGroupIndex;

	private DateTime _lastClickSwichTime;

	private bool _initParticle;

	public void Open(IListener listener)
	{
		_myTeamType = GetMyTeam();
		_listener = listener;
		SetSwitchInterval();
		Open(delegate
		{
			_myTeamType = GetMyTeam();
			_listener = listener;
			SetSwitchInterval();
		}, null);
	}

	private void SetParticleSystem()
	{
		for (int i = 0; i < ParticleSystems.Length; i++)
		{
			ParticleSystem particleSystem = ParticleSystems[i];
			if (!(particleSystem == null))
			{
				ParticleSystem.MainModule main = particleSystem.main;
				main.playOnAwake = true;
			}
		}
	}

	private void SetSwitchInterval()
	{
		_switchInterval = GameRoot.Instance.ConstantDataContainer.GetValueInt("ShakmehCamSwitchInterval", 1000);
		_switchInterval /= 1000f;
	}

	public override void Close()
	{
		_curGroupIndex = 0;
		_myTeamType = TeamType.Left;
		_listener = null;
		UIHelper.EnableButton(ButtonSwitch, enable: true, grayscaleIfDisable: true);
		_initParticle = false;
		base.Close();
	}

	public int GetAliveCreatureCount(int groupIndex)
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return 0;
		}
		return SingletonAuto<BattleFacade>.instance.GetAliveCreatureCountOfTeamAndGroup(_myTeamType, groupIndex);
	}

	private TeamType GetMyTeam()
	{
		long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		return SingletonAuto<BattleFacade>.instance.GetPlayerCache(id).TeamType;
	}

	private bool IsMainGroupCamera()
	{
		return _curGroupIndex == 0;
	}

	private int GetNextGroupIndex()
	{
		if (!IsMainGroupCamera())
		{
			return 0;
		}
		return 1;
	}

	private void SetNextGroupIndex()
	{
		_curGroupIndex = (IsMainGroupCamera() ? 1 : 0);
	}

	public string GetAnimatorTriggerName(int groupIndex)
	{
		return AnimatorTriggerNames[groupIndex];
	}

	private bool IsBattleEnable()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return false;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache == null)
		{
			return false;
		}
		if (battleCache.IsBattleStarted)
		{
			return !battleCache.IsBattleEnded;
		}
		return false;
	}

	private bool CanSwitchingTime()
	{
		return (DateTime.Now - _lastClickSwichTime).TotalSeconds > (double)_switchInterval;
	}

	private bool CanSwitchingCreature()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return false;
		}
		int nextGroupIndex = GetNextGroupIndex();
		return GetAliveCreatureCount(nextGroupIndex) != 0;
	}

	public bool EnableCurrent()
	{
		if (GetAliveCreatureCount(_curGroupIndex) == 0)
		{
			int nextGroupIndex = GetNextGroupIndex();
			if (GetAliveCreatureCount(nextGroupIndex) != 0)
			{
				return false;
			}
		}
		return true;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_myTeamType != TeamType.Left)
		{
			_myTeamType = GetMyTeam();
		}
		if (!EnableCurrent())
		{
			SwitchingCamera();
		}
		UpdateButtonState();
	}

	private void UpdateButtonState()
	{
		if (!IsBattleEnable())
		{
			UIHelper.EnableButton(ButtonSwitch, enable: false, grayscaleIfDisable: true);
			UIHelper.SetGrayscaleSprite(SpriteLight, isGray: true);
			UIHelper.SetGrayscaleSprite(SpriteDark, isGray: true);
			return;
		}
		bool flag = CanSwitchingCreature();
		if (flag)
		{
			flag = CanSwitchingTime();
		}
		UIHelper.EnableButton(ButtonSwitch, flag, grayscaleIfDisable: true);
		UIHelper.SetGrayscaleSprite(SpriteLight, !flag);
		UIHelper.SetGrayscaleSprite(SpriteDark, !flag);
	}

	private void SwitchingCamera()
	{
		if (_listener != null)
		{
			if (!_initParticle)
			{
				SetParticleSystem();
				_initParticle = true;
			}
			_lastClickSwichTime = DateTime.Now;
			PlayAnimator(_curGroupIndex);
			_listener.SwichingCamera();
			SetNextGroupIndex();
		}
	}

	public void OnClickSwitch()
	{
		if (CanSwitchingCreature() && CanSwitchingTime())
		{
			SwitchingCamera();
		}
	}

	public void PlayAnimator(int groupIndex)
	{
		if (Anim != null)
		{
			ResetAllTrigger();
			string animatorTriggerName = GetAnimatorTriggerName(groupIndex);
			Anim.SetTrigger(animatorTriggerName);
		}
	}

	private void ResetAllTrigger()
	{
		if (!(Anim == null))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AnimatorTriggerNames); i++)
			{
				string text = AnimatorTriggerNames[i];
				Anim.ResetTrigger(text);
			}
		}
	}
}
