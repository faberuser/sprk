using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[SerializeField]
public class HeroInnViewButtonGroup : MonoBehaviour
{
	public interface Listener
	{
		int GetCostValue(FriendlyActionType friendlyAction);

		string GetCostKey(FriendlyActionType friendlyAction);

		bool CheckCoolTime(FriendlyActionType friendlyAction);

		bool CheckCost(FriendlyActionType friendlyAction);

		bool CheckClosenessPoint();
	}

	[SerializeField]
	public HeroInnViewButton[] Buttons;

	public bool IsForwardStatus;

	private Listener _listener;

	public void SetListener(Listener listener)
	{
		_listener = listener;
	}

	public void SetButtonLabel()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(Buttons) && _listener != null)
		{
			for (int i = 0; i < Buttons.Length; i++)
			{
				HeroInnViewButton heroInnViewButton = Buttons[i];
				int costValue = _listener.GetCostValue(heroInnViewButton.ActionType);
				string costKey = _listener.GetCostKey(heroInnViewButton.ActionType);
				heroInnViewButton.SetCostLabel(costValue, costKey);
			}
		}
	}

	public void UpdateButtons()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(Buttons) && _listener != null)
		{
			for (int i = 0; i < Buttons.Length; i++)
			{
				HeroInnViewButton heroInnViewButton = Buttons[i];
				bool isEnableCool = _listener.CheckCoolTime(heroInnViewButton.ActionType);
				bool isEnableCost = _listener.CheckCost(heroInnViewButton.ActionType);
				bool isMaxClosenessPoint = _listener.CheckClosenessPoint();
				heroInnViewButton.Set(isEnableCool, isEnableCost, isMaxClosenessPoint);
			}
			SetButtonLabel();
		}
	}

	public void Reset()
	{
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].Reset();
		}
	}
}
