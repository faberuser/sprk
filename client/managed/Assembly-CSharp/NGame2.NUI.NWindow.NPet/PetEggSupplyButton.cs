using System;
using System.Linq;
using NGame2.NUI.NHelper.NPet;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetEggSupplyButton : MonoBehaviour
{
	public interface IListener
	{
		void RequestEggReward();
	}

	private enum EEggState
	{
		None,
		Waiting,
		Egg
	}

	private static readonly string EGG_GAUGE_LOOP_ANIM_NAME = "Anim_PetEggGauge_Loop";

	private static readonly string EGG_GAUGE_MAX_ANIM_NAME = "Anim_PetEggGauge_Max";

	private static readonly string EGG_ANIM_TRIGGER_GAUGE_MAX = "MaxGauge";

	public UISprite EggGauage;

	public UILabel RemindTimeLabel;

	public UIButton EggButton;

	public Animator EggAnimator;

	[Header("Particle Root")]
	public Transform EggLoopParticleRoot;

	public Transform EggMaxParticleRoot;

	private IListener _iListener;

	private EEggState _eEggState;

	private EEggState _ePrevEggState;

	private TimeSpan? _remainTimeEggSupply;

	private int _eggSupplyCoolTime;

	private bool IsRunMaxEggAnimation
	{
		get
		{
			if (EggAnimator == null)
			{
				return false;
			}
			AnimatorStateInfo currentAnimatorStateInfo = EggAnimator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.IsName(EGG_GAUGE_MAX_ANIM_NAME))
			{
				return currentAnimatorStateInfo.normalizedTime < 1f;
			}
			return false;
		}
	}

	private bool IsCompeleteMaxEggAnimation
	{
		get
		{
			if (EggAnimator == null)
			{
				return true;
			}
			AnimatorStateInfo currentAnimatorStateInfo = EggAnimator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.IsName(EGG_GAUGE_MAX_ANIM_NAME))
			{
				return currentAnimatorStateInfo.normalizedTime >= 1f;
			}
			return false;
		}
	}

	public void Open(IListener listener)
	{
		_iListener = listener;
		Init();
	}

	private void Init()
	{
		NewEquipGachaData newEquipGachaData = (from v in GameRoot.Instance.NewEquipGachaDataContainer.GetAllData()
			where v.Type == EquipGachaType.Pet
			where v.FreeCoolTime > 0
			select v).LastOrDefault();
		if (newEquipGachaData != null)
		{
			_eggSupplyCoolTime = newEquipGachaData.FreeCoolTime;
		}
		_eEggState = EEggState.None;
		_ePrevEggState = EEggState.None;
		UpdateEgg();
	}

	private void Update()
	{
		UpdateEgg();
	}

	private void UpdateEgg()
	{
		if (!(RemindTimeLabel == null))
		{
			_remainTimeEggSupply = PetHelper.UpdateRemainTimeEggSupply(RemindTimeLabel);
			UpdateEggState();
			UpdateEggGauage();
			UpdateAnimation();
			_ePrevEggState = _eEggState;
		}
	}

	private void UpdateEggGauage()
	{
		if (!(EggGauage == null))
		{
			if (_eEggState != EEggState.Waiting || IsRunMaxEggAnimation)
			{
				EggGauage.fillAmount = 1f;
				return;
			}
			float num = (float)_remainTimeEggSupply.Value.TotalSeconds;
			EggGauage.fillAmount = 1f - num / (float)_eggSupplyCoolTime;
		}
	}

	private void UpdateEggState()
	{
		if (!_remainTimeEggSupply.HasValue || !_remainTimeEggSupply.HasValue)
		{
			_eEggState = EEggState.Egg;
		}
		else
		{
			_eEggState = EEggState.Waiting;
		}
	}

	private void UpdateAnimation()
	{
		if (EggAnimator == null)
		{
			return;
		}
		if (_ePrevEggState != EEggState.Egg && _eEggState == EEggState.Egg)
		{
			EnableButton();
			if (_ePrevEggState == EEggState.Waiting)
			{
				EggAnimator.SetBool(EGG_ANIM_TRIGGER_GAUGE_MAX, value: true);
			}
		}
		if (_ePrevEggState != EEggState.Waiting && _eEggState == EEggState.Waiting)
		{
			DisableButton();
		}
		if (IsCompeleteMaxEggAnimation)
		{
			EggAnimator.SetBool(EGG_ANIM_TRIGGER_GAUGE_MAX, value: false);
			EggAnimator.Play(EGG_GAUGE_LOOP_ANIM_NAME, -1, 0f);
		}
	}

	private void EnableButton()
	{
		if (EggButton != null)
		{
			UIHelper.EnableButton(EggButton, enable: true);
		}
	}

	private void DisableButton()
	{
		if (EggButton != null)
		{
			UIHelper.EnableButton(EggButton, enable: false);
		}
	}

	public void OnClickEggButton()
	{
		if (_iListener != null)
		{
			_iListener.RequestEggReward();
		}
	}
}
