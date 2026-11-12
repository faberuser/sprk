using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class ExtraScaleController
{
	public class ScaleAnimation
	{
		public float From;

		public float To;

		public float TimeLength;

		public AnimationCurve BlendCurve;

		public float ElapsedTime;
	}

	private RenderFacade _renderFacade;

	private IncreaseDic<string, ScaleAnimation> _animationDic = new IncreaseDic<string, ScaleAnimation>();

	public ExtraScaleController(RenderFacade renderFacade)
	{
		_renderFacade = renderFacade;
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < _animationDic.Count; i++)
		{
			KeyValuePair<string, ScaleAnimation> keyValuePairByIndex = _animationDic.GetKeyValuePairByIndex(i);
			string key = keyValuePairByIndex.Key;
			ScaleAnimation value = keyValuePairByIndex.Value;
			value.ElapsedTime += _renderFacade.DeltaTime;
			float t = Mathf.Clamp01(value.ElapsedTime / value.TimeLength);
			float scale = Mathf.Lerp(value.From, value.To, t);
			_renderFacade.SetRuntimeScale(key, scale);
			if (value.ElapsedTime >= value.TimeLength)
			{
				_animationDic.RemoveByIndex(i);
				i--;
			}
		}
	}

	public void SetScale(string keyword, float from, float to, float timeLength, AnimationCurve blendCurve)
	{
		ScaleAnimation scaleAnimation = _animationDic.GETVALUE(keyword, null);
		if (scaleAnimation == null)
		{
			scaleAnimation = new ScaleAnimation();
			_animationDic.Add(keyword, scaleAnimation);
		}
		scaleAnimation.From = from;
		scaleAnimation.To = to;
		scaleAnimation.TimeLength = Vespa_Float_Extension_Method.NoneZero(timeLength);
		scaleAnimation.BlendCurve = blendCurve;
		scaleAnimation.ElapsedTime = 0f;
	}
}
