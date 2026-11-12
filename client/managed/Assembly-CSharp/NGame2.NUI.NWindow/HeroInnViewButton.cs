using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroInnViewButton : MonoBehaviour
{
	private enum ButtonStatus
	{
		Enable,
		Disable_None,
		Disable_Cool
	}

	public FriendlyActionType ActionType;

	public UIButton Button;

	public UILabel Label_Desc;

	public UILabel Label_Disable;

	public UILabel Label_Cost;

	public UILabel Label_MaxCloseness;

	public GameObject Newmark;

	private bool newMarkInit;

	private bool prevIsEnableCost;

	public void Set(bool isEnableCool, bool isEnableCost, bool isMaxClosenessPoint)
	{
		switch (ActionType)
		{
		case FriendlyActionType.Greeting:
		case FriendlyActionType.Conversation:
		case FriendlyActionType.Gift:
			FriendlyButtonSet(isEnableCool, isEnableCost);
			break;
		case FriendlyActionType.Closeness_Greeting:
		case FriendlyActionType.Closeness_Conversation:
		case FriendlyActionType.Closeness_Gift:
			ClosenessButtonSet(isEnableCool, isEnableCost, isMaxClosenessPoint);
			break;
		default:
			ConnectTalkButtonSet(isEnableCost, isMaxClosenessPoint);
			break;
		}
	}

	public void FriendlyButtonSet(bool isEnableCool, bool isEnableCost)
	{
		SetButton(isEnableCool && isEnableCost);
		SetLabel(Label_Disable, !isEnableCool);
		SetLabel(Label_Cost, isEnableCool);
	}

	public void ClosenessButtonSet(bool isEnableCool, bool isEnableCost, bool isMaxCloseness)
	{
		SetButton(isEnableCool && isEnableCost);
		SetLabel(Label_Disable, !isEnableCool && !isMaxCloseness);
		SetLabel(Label_MaxCloseness, isMaxCloseness);
		SetLabel(Label_Cost, isEnableCool && !isMaxCloseness);
	}

	public void ConnectTalkButtonSet(bool isEnableCost, bool isMaxCloseness)
	{
		SetButton(isEnableCost);
		SetLabel(Label_Disable, isEnable: false);
		SetLabel(Label_Cost, !isEnableCost);
		SetNewMark(isEnableCost);
	}

	public void SetNewMark(bool isEnableCost)
	{
		if (!newMarkInit)
		{
			prevIsEnableCost = isEnableCost;
			newMarkInit = true;
		}
		if (isEnableCost != prevIsEnableCost)
		{
			prevIsEnableCost = isEnableCost;
			if (isEnableCost)
			{
				UIHelper.EnableGameObject(Newmark, enable: true);
			}
		}
	}

	public void Reset()
	{
		SetButton(isEnable: true);
		UIHelper.EnableGameObject(Newmark, enable: false);
		prevIsEnableCost = false;
		newMarkInit = false;
	}

	private void SetButton(bool isEnable)
	{
		UIHelper.EnableButton(Button, isEnable, grayscaleIfDisable: true);
	}

	private void SetLabel(UILabel label, bool isEnable)
	{
		UIHelper.EnableGameObject(label, isEnable);
	}

	public void SetCostLabel(int cost, string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key, cost);
		UIHelper.SetLabelText(Label_Cost, stringDirect);
	}
}
