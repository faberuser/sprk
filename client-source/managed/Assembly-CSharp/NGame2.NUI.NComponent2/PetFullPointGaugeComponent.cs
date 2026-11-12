using System;
using NGame2.NAccount;
using NGame2.NOrdeal;
using NGame2.NUI.NHelper.NPet;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PetFullPointGaugeComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void OnClick();
	}

	private enum EPetGaugeState
	{
		MaxFull,
		Normal,
		Sleep
	}

	public static string PrefabPath = "Prefab2/Component/Pet/PetFullPointGaugeComponent";

	public static PrefabAuto<PetFullPointGaugeComponent> PrefabAuto = new PrefabAuto<PetFullPointGaugeComponent>(PrefabPath);

	private static readonly string PET_FOOD_FULL_ANIM_NAME = "Pet_FoodStat_Full";

	public UILabel Label_Title;

	[Header("[Pet Full Point Gauge]")]
	public UISprite FullPointGauage;

	public Animator MaxFullPointAnimator;

	[Header("[Pet Full Point Transform]")]
	public Transform FullPointRoot;

	public UILabel FullPointLabel;

	public Transform MaxFullPointRoot;

	public UILabel MaxFullPointLabel;

	public Transform SleepRoot;

	public UILabel SleepLabel;

	private IListener _listener;

	private PetInfo _petInfo;

	private int _fullPoint;

	private int _maxFullPoint;

	private bool _isMaxFullPointAnimPlaying;

	private EPetGaugeState _gaugeState = EPetGaugeState.Normal;

	private UILabel CurrentFullPointLabel => _gaugeState switch
	{
		EPetGaugeState.Sleep => SleepLabel, 
		EPetGaugeState.MaxFull => MaxFullPointLabel, 
		EPetGaugeState.Normal => FullPointLabel, 
		_ => null, 
	};

	private TimeSpan? PetGetUpRemainTime
	{
		get
		{
			if (_petInfo == null)
			{
				return null;
			}
			if (String_Extension_Method.IsNullOrEmpty(_petInfo.EndPenaltyTime))
			{
				return null;
			}
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return null;
			}
			return PetHelper.GetPetGetUpRemainTime(_petInfo.EndPenaltyTime);
		}
	}

	private string CurrentFullPointStr
	{
		get
		{
			if (_petInfo != null && _petInfo.Status == PetStatusType.Explore)
			{
				int petExploreRemainSec = PetHelper.GetPetExploreRemainSec(_petInfo.PetIndex);
				if (petExploreRemainSec <= 0)
				{
					return LocalizationManager.GetStringDirect("EXPLORE_END");
				}
				return OrdealHelper.GetRemainTimeText(new TimeSpan(0, 0, petExploreRemainSec));
			}
			if (_fullPoint < _maxFullPoint)
			{
				return $"{_fullPoint}/{_maxFullPoint}";
			}
			return "MAX";
		}
	}

	public bool IsFullFullPoint => _fullPoint >= _maxFullPoint;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void Open(IListener listener, PetInfo petInfo = null)
	{
		Init(listener, petInfo);
	}

	private void Init(IListener listener, PetInfo petInfo)
	{
		_listener = listener;
		UpdatePetInfo(petInfo);
	}

	private void IntFullPoint()
	{
		_fullPoint = 0;
	}

	private void Update()
	{
		UpdatePetState();
	}

	public void UpdatePetInfo(PetInfo petInfo)
	{
		if (petInfo != null)
		{
			_petInfo = petInfo;
			CreatureData creatureData = _petInfo.CreatureData;
			if (creatureData != null)
			{
				_maxFullPoint = creatureData.MaxFood;
			}
			IntFullPoint();
			UpdateFullPoint(_petInfo.FullPoint);
			UpdatePetGuageState();
			UpdatePetState();
		}
	}

	public void UpdateFullPoint(int fullPoint)
	{
		_fullPoint += fullPoint;
		if (_petInfo.IsLinePet)
		{
			_fullPoint = _maxFullPoint;
		}
	}

	private void UpdatePetGuageState()
	{
		if (PetHelper.CheckIsSleep(_petInfo))
		{
			_gaugeState = EPetGaugeState.Sleep;
		}
		else if (_fullPoint >= _maxFullPoint)
		{
			_gaugeState = EPetGaugeState.MaxFull;
		}
		else
		{
			_gaugeState = EPetGaugeState.Normal;
		}
	}

	private void UpdatePetState()
	{
		SetFullPoint();
		TimeSpan? petGetUpRemainTime = PetGetUpRemainTime;
		if (!petGetUpRemainTime.HasValue)
		{
			UIHelper.SetLabelText(CurrentFullPointLabel, CurrentFullPointStr);
		}
		else
		{
			UIHelper.SetLabelText(CurrentFullPointLabel, OrdealHelper.GetRemainTimeText(petGetUpRemainTime.Value));
		}
		if (_petInfo.Status == PetStatusType.Explore)
		{
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect("EXPLORE_DOING2"));
		}
		else
		{
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect("PET_STATUS_HAPPY_GRADE"));
		}
	}

	private void SetFullPoint()
	{
		if (FullPointGauage == null || MaxFullPointRoot == null || FullPointRoot == null || SleepRoot == null)
		{
			return;
		}
		if (_maxFullPoint <= 0)
		{
			FullPointGauage.fillAmount = 0f;
			return;
		}
		switch (_gaugeState)
		{
		case EPetGaugeState.Sleep:
			_isMaxFullPointAnimPlaying = false;
			break;
		case EPetGaugeState.MaxFull:
			if (!_isMaxFullPointAnimPlaying && MaxFullPointAnimator != null)
			{
				MaxFullPointAnimator.Play(PET_FOOD_FULL_ANIM_NAME);
				_isMaxFullPointAnimPlaying = true;
			}
			break;
		case EPetGaugeState.Normal:
			_isMaxFullPointAnimPlaying = false;
			break;
		}
		UIHelper.EnableGameObject(SleepRoot, _gaugeState == EPetGaugeState.Sleep);
		UIHelper.EnableGameObject(MaxFullPointRoot, _gaugeState == EPetGaugeState.MaxFull);
		UIHelper.EnableGameObject(FullPointRoot, _gaugeState == EPetGaugeState.Normal);
		float fillAmount = (float)_fullPoint / (float)_maxFullPoint;
		FullPointGauage.fillAmount = fillAmount;
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick();
		}
	}
}
