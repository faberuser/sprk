using System;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

[Serializable]
public class PCTrailRendererData
{
	public Material TrailMaterial;

	public float Lifetime = 1f;

	public bool TimeBaseTexMapping;

	public AnimationCurve TexRemapping = new AnimationCurve();

	public float TrailSize = 1f;

	public AnimationCurve SizeOverLife = new AnimationCurve();

	public Gradient ColorOverLife;

	public bool StretchSizeToFit;

	public bool StretchColorToFit;

	public float MaterialTileLength;

	public bool UseForwardOverride;

	public Vector3 ForwardOverride;

	public bool ForwardOverrideRelative;

	public Color GetColor(float frame)
	{
		return Vespa_Unity_Gradient_Extension_Method.EVALUATE(ColorOverLife, frame, Color.white);
	}

	public float GetTrailSize(float frame)
	{
		return Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(SizeOverLife, frame, 1f) * TrailSize;
	}
}
