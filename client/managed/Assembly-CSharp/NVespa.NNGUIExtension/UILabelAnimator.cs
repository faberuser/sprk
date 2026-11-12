using System;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[RequireComponent(typeof(UILabel))]
public class UILabelAnimator : MonoBehaviour
{
	public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float Duration = 1f;

	private UILabel _thisLabel;

	private int _from;

	private int _to;

	private Func<int, string> _displayer;

	private bool _started;

	private float _startedTime;

	private void Awake()
	{
		_thisLabel = GetComponent<UILabel>();
		_started = false;
	}

	public void Animate(int from, int to, Func<int, string> displayer = null)
	{
		_from = from;
		_to = to;
		_displayer = displayer;
		_started = true;
		_startedTime = Time.time;
	}

	private void Update()
	{
		if (_started)
		{
			float num = Time.time - _startedTime;
			if (num >= Duration)
			{
				_thisLabel.text = ((_displayer != null) ? _displayer(_to) : _to.ToString());
				_started = false;
			}
			else
			{
				float t = Curve.Evaluate(num / Duration);
				float num2 = Mathf.Lerp(_from, _to, t);
				_thisLabel.text = ((_displayer != null) ? _displayer((int)num2) : ((int)num2).ToString());
			}
		}
	}
}
