using System.Collections;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NUIEffect;

public class MoveFromTo : BaseUIEffect
{
	public UIGame3DWidget Effect;

	public AnimationCurve PositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool WaitEffectEnd;

	public override void Play(string[] values)
	{
		if (!(Effect == null))
		{
			VespaCoroutine.Start(this, coMove(values));
		}
	}

	private IEnumerator coMove(string[] values)
	{
		string valueString = ArgParser.GetValueString(values, 0);
		string valueString2 = ArgParser.GetValueString(values, 1);
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, valueString, 0, includeInactive: true);
		Transform transform2 = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, valueString2, 0, includeInactive: true);
		if (transform2 == null)
		{
			VespaObject.DestroyGo(base.gameObject);
			yield break;
		}
		if ((bool)transform)
		{
			base.transform.position = transform.position;
		}
		Effect.Play(null);
		Vector3 startPosition = base.transform.position;
		Vector3 endPosition = transform2.position;
		Keyframe keyframe = PositionCurve.keys[PositionCurve.keys.Length - 1];
		float lastTime = keyframe.time;
		float currentTime = 0f;
		while (currentTime < lastTime && Effect.IsPlaying)
		{
			float t = PositionCurve.Evaluate(currentTime);
			Vector3 vector = Vector3.Lerp(startPosition, endPosition, t);
			base.transform.position = new Vector3(vector.x, vector.y, startPosition.z);
			currentTime += Time.deltaTime;
			yield return null;
		}
		base.transform.position = new Vector3(endPosition.x, endPosition.y, startPosition.z);
		while (Effect.IsPlaying && WaitEffectEnd)
		{
			yield return null;
		}
		Effect.Stop(immediatelyClear: true);
		VespaObject.DestroyGo(base.gameObject);
	}
}
