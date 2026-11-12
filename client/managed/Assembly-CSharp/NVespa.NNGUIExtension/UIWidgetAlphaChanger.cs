using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIRect))]
public class UIWidgetAlphaChanger : MonoBehaviour
{
	public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float Duration = 1f;

	public float Delay;

	public bool ManualStart;

	private UIRect _thisWidget;

	private float _finalAlpha;

	private bool _started;

	private float _startedTime;

	private void Awake()
	{
		_thisWidget = GetComponent<UIRect>();
		_finalAlpha = _thisWidget.alpha;
	}

	private void OnEnable()
	{
		_started = !ManualStart;
		_startedTime = Time.time;
	}

	public void Animate(float from, float to)
	{
		Curve = AnimationCurve.EaseInOut(0f, from, 1f, to);
		_started = true;
		_startedTime = Time.time;
	}

	private void Update()
	{
		if (_started)
		{
			float num = Time.time - _startedTime - Delay;
			if (num >= Duration)
			{
				Apply(1f);
				_started = false;
			}
			else
			{
				float ratio = Curve.Evaluate(num / Duration);
				Apply(ratio);
			}
		}
	}

	private void Apply(float ratio)
	{
		float alpha = _finalAlpha * ratio;
		_thisWidget.alpha = alpha;
	}
}
