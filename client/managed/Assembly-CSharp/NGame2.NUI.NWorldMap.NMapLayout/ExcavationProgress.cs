using System;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class ExcavationProgress : LinkFactoryComponent
{
	public static PrefabAuto<ExcavationProgress> PrefabAuto = new PrefabAuto<ExcavationProgress>("Prefab2/WorldMap/ExcavationProgress");

	public UILabel DotLabel;

	public UISprite GaugeSprite;

	private float _startedTime;

	private float _duration;

	private Action _onEnd;

	private bool _ended;

	private float _lastDotTime;

	private int _dotCount;

	public void Open(float duration, Action onEnd)
	{
		_startedTime = Time.time;
		_duration = duration;
		_onEnd = onEnd;
		_ended = false;
		_lastDotTime = Time.time;
		_dotCount = 1;
	}

	private void Update()
	{
		if (_duration <= 0f || _ended)
		{
			return;
		}
		float num = (Time.time - _startedTime) / _duration;
		if (num >= 1f)
		{
			UIHelper.SetSpriteFillAmount(GaugeSprite, 1f);
			_ended = true;
			Vespa_System_Action_Extension_Method.fire(_onEnd);
			return;
		}
		UIHelper.SetSpriteFillAmount(GaugeSprite, num);
		if (Time.time - _lastDotTime > 0.3f)
		{
			_lastDotTime = Time.time;
			_dotCount = _dotCount % 3 + 1;
			UIHelper.SetLabelText(DotLabel, new string('.', _dotCount));
		}
	}
}
