using UnityEngine;

namespace NGame2.NUI.NWindow.NEclipse;

public class EclipseRewardAnim : MonoBehaviour
{
	public interface IListener
	{
		void StartShowTitle();

		void FinishAppear();
	}

	private IListener _iListener;

	public void Open(IListener iListener)
	{
		_iListener = iListener;
	}

	public void StartShowTitle()
	{
		if (_iListener != null)
		{
			_iListener.StartShowTitle();
		}
	}

	public void FinishAppear()
	{
		if (_iListener != null)
		{
			_iListener.FinishAppear();
		}
	}
}
