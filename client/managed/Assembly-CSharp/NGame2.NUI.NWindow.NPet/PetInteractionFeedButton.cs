using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NPet;
using NGame2.NUI.NHelper.NPet;
using NNguiExtension;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetInteractionFeedButton : MonoBehaviour
{
	public interface IListener
	{
		bool CheckStartPetEat { get; }

		void PetEating(int addFullPoint);

		void EndPetEat(int eatCount, Action failAction);
	}

	private enum EEatState
	{
		StartEat,
		Eating,
		EndEat
	}

	private readonly int PetEatFoodCount = 1;

	private readonly float EatDelayTime = 0.1f;

	private readonly PetActionType PetActionType;

	public Transform RootTransform;

	public UISpriteSelector FeedRewardSpriteSelector;

	public UILabel FoodDescLabel;

	public UILabel FoodStateLabel;

	private IListener _iListener;

	private IPetInteractionProvider _iProvider;

	private EEatState _eEatState = EEatState.EndEat;

	private DateTime? _pressedTime;

	private int _eatCount;

	private long _remainPetFoodCount;

	private long _maxPetFoodCount;

	private int _addFullPoint;

	public void Init(IListener iListener, IPetInteractionProvider iProvider)
	{
		_iListener = iListener;
		_iProvider = iProvider;
		SetPetEatInfo();
	}

	private void Update()
	{
		UpdatePetEat();
		UpdatePetFoodState();
	}

	public void SetPetEatInfo()
	{
		_eEatState = EEatState.EndEat;
		_eatCount = 0;
		_remainPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
		_maxPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.MaxPetFoodCount;
		_addFullPoint = GlobalAutoBehaviour<AccountManager>.instance.PetManager.PetFoodValue;
		_pressedTime = null;
		SetButtonState();
	}

	private void SetButtonState()
	{
		if (_iProvider != null)
		{
			EPetInteractionState ePetInteractionState = GetEPetInteractionState();
			if (FeedRewardSpriteSelector != null)
			{
				FeedRewardSpriteSelector.SelectEnum(ePetInteractionState);
			}
			UIHelper.EnableGameObject(FoodStateLabel, ePetInteractionState == EPetInteractionState.Feed);
		}
	}

	private void UpdatePetFoodState()
	{
		if (_eEatState == EEatState.EndEat)
		{
			_remainPetFoodCount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
			PetHelper.UpdatePetFoodState(FoodStateLabel, _remainPetFoodCount, _maxPetFoodCount);
		}
	}

	private void UpdatePetEat()
	{
		if (_eEatState == EEatState.EndEat)
		{
			return;
		}
		if (_iListener == null)
		{
			SetPetEatInfo();
			return;
		}
		if (_iProvider == null)
		{
			SetPetEatInfo();
			return;
		}
		if (_eatCount >= _remainPetFoodCount)
		{
			EndPetEat();
			return;
		}
		if (_iProvider.IsFullFullPoint)
		{
			EndPetEat();
			return;
		}
		if (_eEatState == EEatState.StartEat)
		{
			_eEatState = EEatState.Eating;
			_pressedTime = DateTime.UtcNow;
		}
		else if (!_pressedTime.HasValue || (DateTime.UtcNow - _pressedTime.Value).TotalSeconds < (double)EatDelayTime)
		{
			return;
		}
		_eatCount += PetEatFoodCount;
		UIHelper.SetLabelText(FoodStateLabel, PetHelper.GetRemainPetFoodCountStr(checked(_remainPetFoodCount - _eatCount), _maxPetFoodCount));
		_iListener.PetEating(PetEatFoodCount * _addFullPoint);
		_pressedTime = DateTime.UtcNow;
	}

	private void EndPetEat()
	{
		if (_eEatState == EEatState.EndEat)
		{
			return;
		}
		if (_iListener == null)
		{
			SetPetEatInfo();
		}
		else if (_eatCount <= 0)
		{
			SetPetEatInfo();
		}
		else if (_pressedTime.HasValue)
		{
			_pressedTime = null;
			_iListener.EndPetEat(_eatCount, delegate
			{
				_eEatState = EEatState.EndEat;
			});
		}
	}

	private EPetInteractionState GetEPetInteractionState()
	{
		return EPetInteractionState.Feed;
	}

	public void Enable(bool enable)
	{
		UIHelper.EnableGameObject(RootTransform, enable);
	}

	public void OnReleaseEat()
	{
		EndPetEat();
	}

	public void OnPressEat()
	{
		if (_iListener != null && _iProvider != null && _eEatState == EEatState.EndEat && _iListener.CheckStartPetEat && !_iProvider.IsFullFullPoint && _remainPetFoodCount > 0)
		{
			_eEatState = EEatState.StartEat;
		}
	}
}
