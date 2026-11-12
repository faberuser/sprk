using UnityEngine;

namespace NVespa.NNGUIExtension;

[RequireComponent(typeof(UIWidget))]
public class UIWidgetScaleChanger : MonoBehaviour
{
	public enum CoordType
	{
		X,
		Y
	}

	public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public CoordType Coord;

	public float Duration = 1f;

	public float Delay;

	public bool ManualStart;

	private UIWidget _thisWidget;

	private Vector3 _finalSize;

	private bool _started;

	private float _startedTime;

	private void Awake()
	{
		_thisWidget = GetComponent<UIWidget>();
		_finalSize = _thisWidget.localSize;
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
		Vector3 vector = _finalSize * ratio;
		float num = ((Coord == CoordType.X) ? vector.x : _finalSize.x);
		float num2 = ((Coord == CoordType.Y) ? vector.y : _finalSize.y);
		_thisWidget.width = (int)num;
		_thisWidget.height = (int)num2;
	}
}
