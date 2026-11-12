using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class ExtraAlphaController
{
	public enum EAlphaType
	{
		EventCamera,
		DestroyDisappear,
		NodeTree,
		Dead,
		CutScene
	}

	public class ReleaseAnimation
	{
		public float BeginAlpha;

		public float BeginTime;

		public float Delay;

		public float Duration;
	}

	private IRenderGroupModifier _renderModifier;

	private IncreaseDic<EAlphaType, float> _alphaDic = new IncreaseDic<EAlphaType, float>();

	private IncreaseDic<EAlphaType, ReleaseAnimation> _releaseDic = new IncreaseDic<EAlphaType, ReleaseAnimation>();

	public ExtraAlphaController(IRenderGroupModifier renderModifier)
	{
		_renderModifier = renderModifier;
	}

	public void ChainUpdate()
	{
		if (_releaseDic.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < _releaseDic.Count; i++)
		{
			KeyValuePair<EAlphaType, ReleaseAnimation> keyValuePairByIndex = _releaseDic.GetKeyValuePairByIndex(i);
			EAlphaType key = keyValuePairByIndex.Key;
			ReleaseAnimation value = keyValuePairByIndex.Value;
			if (!_alphaDic.ContainsKey(key))
			{
				_releaseDic.RemoveByIndex(i);
				i--;
				continue;
			}
			float num = MainTimer.time - value.BeginTime;
			if (!(num < value.Delay))
			{
				num -= value.Delay;
				if (num < value.Duration)
				{
					float t = Mathf.Clamp01(num / value.Duration);
					float value2 = Mathf.Lerp(value.BeginAlpha, 1f, t);
					_alphaDic.AddAndOverride(key, value2);
				}
				else
				{
					_alphaDic.RemoveSlow(key);
					_releaseDic.RemoveByIndex(i);
					i--;
				}
			}
		}
		UpdateExtraAlpha();
	}

	public void SetAlpha(EAlphaType alphaType, float alpha)
	{
		if (Math.Approximately(alpha, 1f))
		{
			_alphaDic.RemoveSlow(alphaType);
		}
		else
		{
			_alphaDic.AddAndOverride(alphaType, alpha);
		}
		UpdateExtraAlpha();
		_releaseDic.RemoveSlow(alphaType);
	}

	public void ReleaseAlpha(EAlphaType alphaType, float releaseDelay, float releaseDuration)
	{
		if (_alphaDic.TryGetValue(alphaType, out var o_value))
		{
			if (releaseDuration < 0.01f)
			{
				_alphaDic.RemoveSlow(alphaType);
				UpdateExtraAlpha();
				return;
			}
			ReleaseAnimation releaseAnimation = new ReleaseAnimation();
			releaseAnimation.BeginAlpha = o_value;
			releaseAnimation.BeginTime = MainTimer.time;
			releaseAnimation.Delay = releaseDelay;
			releaseAnimation.Duration = releaseDuration;
			_releaseDic.AddAndOverride(alphaType, releaseAnimation);
		}
	}

	private void UpdateExtraAlpha()
	{
		if (_renderModifier == null)
		{
			return;
		}
		RenderGroupModifier2 renderGroupModifier = _renderModifier.RenderGroupModifier;
		if (renderGroupModifier != null)
		{
			float num = 1f;
			for (int i = 0; i < _alphaDic.Count; i++)
			{
				num = Mathf.Min(num, _alphaDic.GetValueByIndex(i));
			}
			renderGroupModifier.SetExtraAlpha(num);
		}
	}
}
