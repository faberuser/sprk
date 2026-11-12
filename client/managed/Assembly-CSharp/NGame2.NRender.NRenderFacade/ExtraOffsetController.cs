using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class ExtraOffsetController
{
	public class OffsetAnimation
	{
		public Vector3 From;

		public Vector3 To;

		public float TimeLength;

		public AnimationCurve BlendCurve;

		public float ElapsedTime;
	}

	private RenderFacade _renderFacade;

	private IncreaseDic<string, OffsetAnimation> _animationDic = new IncreaseDic<string, OffsetAnimation>();

	public ExtraOffsetController(RenderFacade renderFacade)
	{
		_renderFacade = renderFacade;
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < _animationDic.Count; i++)
		{
			KeyValuePair<string, OffsetAnimation> keyValuePairByIndex = _animationDic.GetKeyValuePairByIndex(i);
			string key = keyValuePairByIndex.Key;
			OffsetAnimation value = keyValuePairByIndex.Value;
			value.ElapsedTime += _renderFacade.DeltaTime;
			float t = Mathf.Clamp01(value.ElapsedTime / value.TimeLength);
			Vector3 offset = Vector3.Lerp(value.From, value.To, t);
			_renderFacade.SetRuntimeOffset(key, offset);
			if (value.ElapsedTime >= value.TimeLength)
			{
				_animationDic.RemoveByIndex(i);
				i--;
			}
		}
	}

	public void SetOffset(string keyword, Vector3 from, Vector3 to, float timeLength, AnimationCurve blendCurve)
	{
		OffsetAnimation offsetAnimation = _animationDic.GETVALUE(keyword, null);
		if (offsetAnimation == null)
		{
			offsetAnimation = new OffsetAnimation();
			_animationDic.Add(keyword, offsetAnimation);
		}
		offsetAnimation.From = from;
		offsetAnimation.To = to;
		offsetAnimation.TimeLength = Vespa_Float_Extension_Method.NoneZero(timeLength);
		offsetAnimation.BlendCurve = blendCurve;
		offsetAnimation.ElapsedTime = 0f;
	}
}
