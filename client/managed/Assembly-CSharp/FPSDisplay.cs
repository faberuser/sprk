using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
	private float _fps;

	private float _elapsedTime;

	private int _prevFrameCount;

	private void Start()
	{
	}

	private void Update()
	{
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime >= 1f)
		{
			int num = Time.renderedFrameCount - _prevFrameCount;
			_fps = (float)num / _elapsedTime;
			_prevFrameCount = Time.renderedFrameCount;
			_elapsedTime = 0f;
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("FPS=" + (int)_fps);
	}
}
