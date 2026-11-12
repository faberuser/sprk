using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class PetInteractionSpecialButton : MonoBehaviour
{
	public interface IListener
	{
		void PetPlay();
	}

	public Transform RootTransform;

	public UIButton Button;

	private IListener _iListener;

	public void Init(IListener iListener)
	{
		_iListener = iListener;
	}

	public void Enable(bool enable)
	{
		UIHelper.EnableGameObject(RootTransform, enable);
	}

	public void OnClick()
	{
		if (_iListener != null)
		{
			_iListener.PetPlay();
		}
	}
}
