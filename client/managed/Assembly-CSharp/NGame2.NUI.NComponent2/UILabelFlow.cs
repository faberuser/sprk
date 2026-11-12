using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class UILabelFlow : MonoBehaviour
{
	public UIPanel Panel_FlowText;

	public UILabel Label_FlowText;

	public float Speed = 50f;

	private bool _isPlay;

	private Vector3 _defaultPos;

	private Vector3 _startPlayPos;

	private Vector3 _endPlayPos;

	private float _currentPosX;

	public bool IsPlay => _isPlay;

	private void Awake()
	{
		if (Label_FlowText != null)
		{
			_defaultPos = Label_FlowText.gameObject.transform.localPosition;
		}
	}

	private void OnEnable()
	{
		CheckLabelPlay();
	}

	private void OnDisable()
	{
		Stop();
	}

	public void SetLabelText(string text, bool forceStop = false)
	{
		if (!(Label_FlowText.text == text))
		{
			UIHelper.SetLabelText(Label_FlowText, text);
			CheckLabelPlay(forceStop);
		}
	}

	public void CheckLabelPlay(bool forceStop = false)
	{
		if (CheckOverflow() && !forceStop)
		{
			Play();
		}
		else
		{
			Stop();
		}
	}

	private bool CheckOverflow()
	{
		if (Label_FlowText == null)
		{
			return false;
		}
		return (float)Label_FlowText.width - Panel_FlowText.width > 0f;
	}

	private void Play()
	{
		_isPlay = true;
		SetPlayPos();
	}

	private void Stop()
	{
		_isPlay = false;
		Label_FlowText.gameObject.transform.localPosition = _defaultPos;
		_startPlayPos = _defaultPos;
		_endPlayPos = _defaultPos;
		_currentPosX = _defaultPos.x;
	}

	private void SetPlayPos()
	{
		float num = 0f;
		float num2 = 0f;
		switch (Label_FlowText.pivot)
		{
		case UIWidget.Pivot.TopLeft:
		case UIWidget.Pivot.Left:
		case UIWidget.Pivot.BottomLeft:
			num = Label_FlowText.width;
			num2 = Panel_FlowText.width;
			_startPlayPos = new Vector3(_defaultPos.x + num2, _defaultPos.y, _defaultPos.z);
			_endPlayPos = new Vector3(_defaultPos.x - num, _defaultPos.y, _defaultPos.z);
			_currentPosX = _startPlayPos.x;
			Label_FlowText.gameObject.transform.localPosition = _startPlayPos;
			break;
		case UIWidget.Pivot.Top:
		case UIWidget.Pivot.Center:
		case UIWidget.Pivot.Bottom:
			num = Panel_FlowText.width * 0.5f + (float)Label_FlowText.width * 0.5f;
			num2 = Panel_FlowText.width * 0.5f + (float)Label_FlowText.width * 0.5f;
			_startPlayPos = new Vector3(_defaultPos.x + num2, _defaultPos.y, _defaultPos.z);
			_endPlayPos = new Vector3(_defaultPos.x - num, _defaultPos.y, _defaultPos.z);
			_currentPosX = _startPlayPos.x;
			Label_FlowText.gameObject.transform.localPosition = _startPlayPos;
			break;
		default:
			Stop();
			break;
		}
	}

	private void Update()
	{
		if (_isPlay && !UpdateLabelPos())
		{
			Play();
		}
	}

	private bool UpdateLabelPos()
	{
		_currentPosX -= Speed * Time.deltaTime;
		if (_currentPosX < _endPlayPos.x)
		{
			return false;
		}
		Label_FlowText.gameObject.transform.localPosition = new Vector3(_currentPosX, _defaultPos.y, _defaultPos.z);
		return true;
	}
}
