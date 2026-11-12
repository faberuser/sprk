using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NUtil;

public class AnimationTester : MonoBehaviour, IInspectorGuiDrawer
{
	public enum ECameraTestType
	{
		Off,
		Time,
		FirstActionFrame
	}

	[Serializable]
	public class ActionInfo
	{
		public string StateName;

		public float TimeLength;
	}

	[HideInInspector]
	public ECameraTestType CameraTestType;

	[HideInInspector]
	public CameraTester CameraTester;

	[HideInInspector]
	public List<ActionInfo> ActionList = new List<ActionInfo>();

	public Action<float> onUpdate;

	private bool _isPlaying;

	private bool _isLoop;

	private int _actionIndex;

	private float _elapsedTime;

	private void Start()
	{
	}

	private void Update()
	{
		Animation component = base.gameObject.GetComponent<Animation>();
		if (component == null || !_isPlaying || __Extension_Method_ICollection__.COUNT(ActionList) == 0)
		{
			return;
		}
		float num = _elapsedTime;
		_actionIndex = -1;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ActionList); i++)
		{
			if (num >= ActionList[i].TimeLength)
			{
				num -= ActionList[i].TimeLength;
				continue;
			}
			_actionIndex = i;
			break;
		}
		if (_actionIndex < 0)
		{
			if (_isLoop)
			{
				_actionIndex = 0;
				_elapsedTime = 0f;
				num = 0f;
			}
			else
			{
				_actionIndex = __Extension_Method_ICollection__.COUNT(ActionList) - 1;
				num = 999f;
			}
		}
		ActionInfo actionInfo = ActionList[_actionIndex];
		foreach (AnimationState item in component)
		{
			if (item.name == actionInfo.StateName)
			{
				item.enabled = true;
				item.weight = 1f;
				item.normalizedSpeed = 0f;
				item.normalizedTime = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(actionInfo.TimeLength));
			}
			else
			{
				item.enabled = false;
			}
		}
		if (CameraTester != null && CameraTestType != ECameraTestType.Off)
		{
			if (CameraTestType == ECameraTestType.Time)
			{
				CameraTester.SetAnimationTime(_elapsedTime);
			}
			else if (CameraTestType == ECameraTestType.FirstActionFrame && !__Extension_Method_ICollection__.IsNullOrEmpty(ActionList))
			{
				ActionInfo actionInfo2 = ActionList[0];
				float animationFrame = Mathf.Clamp01(_elapsedTime / Vespa_Float_Extension_Method.NoneZero(actionInfo2.TimeLength));
				CameraTester.SetAnimationFrame(animationFrame);
			}
		}
		Vespa_System_Action_Extension_Method.fire(onUpdate, _elapsedTime);
		_elapsedTime += Time.deltaTime;
	}
}
