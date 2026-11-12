using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class TextBallon : IInteractiveAction
{
	private AiInteractionComponent _aiInteractionComponent;

	private int textIndex;

	private float _lastCheckedTimeForTextBallon;

	private string[] _textArray;

	void IInteractiveAction.Init(PcAvatarData data, AiInteractionComponent aiInteractionComponent)
	{
		_aiInteractionComponent = aiInteractionComponent;
		_textArray = data.TouchText;
	}

	bool IInteractiveAction.CanAction()
	{
		return true;
	}

	void IInteractiveAction.DoAction()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_textArray[textIndex++ % __Extension_Method_ICollection__.COUNT(_textArray)]);
		_aiInteractionComponent.SetBallonText(stringDirect);
		_lastCheckedTimeForTextBallon = Time.time;
	}

	void IInteractiveAction.Update()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("AvatarTextDisappearDelay", 5);
		if (Time.time - _lastCheckedTimeForTextBallon > (float)valueInt)
		{
			_aiInteractionComponent.SetBallonText(string.Empty);
		}
	}
}
