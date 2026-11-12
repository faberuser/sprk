using System;
using UnityEngine;

namespace NNguiExtension;

[RequireComponent(typeof(UITexture))]
public class UITextureUVAnimation : MonoBehaviour
{
	public Vector2 AnimationSpd = new Vector2(1f, 0f);

	private bool _play;

	private UITexture _tex;

	private Func<float> _deltaTimer;

	private void Start()
	{
		_tex = GetComponent<UITexture>();
	}

	public void StartAnimation()
	{
		_play = true;
	}

	public void StopAnimation()
	{
		_play = false;
	}

	private void Update()
	{
		if (_play && !(_tex == null))
		{
			Rect uvRect = _tex.uvRect;
			float deltaTime = GetDeltaTime();
			uvRect.x += AnimationSpd.x * deltaTime;
			uvRect.y += AnimationSpd.y * deltaTime;
			uvRect.x = Repeat(uvRect.x, _tex.width);
			uvRect.y = Repeat(uvRect.y, _tex.height);
			_tex.uvRect = uvRect;
		}
	}

	private float Repeat(float val, float max)
	{
		float num = Mathf.Sign(val);
		return Mathf.Repeat(Mathf.Abs(val), max) * num;
	}

	public void SetTimer(Func<float> deltaTimer)
	{
		_deltaTimer = deltaTimer;
	}

	private float GetDeltaTime()
	{
		if (_deltaTimer != null)
		{
			return Vespa_System_Action_Extension_Method.fire(_deltaTimer);
		}
		return Time.deltaTime;
	}
}
