using UnityEngine;

namespace NVespa;

public class ParticleInitialData
{
	public Quaternion TransformLocalRotation { get; set; }

	public Vector3 TransformLocalScale { get; set; }

	public Vector3 StartSizeMultiplier { get; set; }

	public float StartSpeedMultiplier { get; set; }

	public float GravityModifierMultiplier { get; set; }

	public void Init(ParticleSystem particleSystem, Transform particleTm)
	{
		TransformLocalRotation = particleTm.localRotation;
		TransformLocalScale = particleTm.localScale;
		ParticleSystem.MainModule main = particleSystem.main;
		StartSpeedMultiplier = main.startSpeedMultiplier;
		GravityModifierMultiplier = main.gravityModifierMultiplier;
		if (main.startSize3D)
		{
			StartSizeMultiplier = new Vector3(main.startSizeXMultiplier, main.startSizeYMultiplier, main.startSizeZMultiplier);
		}
		else
		{
			StartSizeMultiplier = Vector3.one * main.startSizeMultiplier;
		}
	}

	public void ApplyToParticleSystem(ParticleSystem particleSystem, float scale)
	{
		ParticleSystem.MainModule main = particleSystem.main;
		main.startSpeedMultiplier = StartSpeedMultiplier * scale;
		main.gravityModifierMultiplier = GravityModifierMultiplier * scale;
		if (main.startSize3D)
		{
			main.startSizeXMultiplier = StartSizeMultiplier.x * scale;
			main.startSizeYMultiplier = StartSizeMultiplier.y * scale;
			main.startSizeZMultiplier = StartSizeMultiplier.z * scale;
		}
		else
		{
			main.startSizeMultiplier = StartSizeMultiplier.x * scale;
		}
	}
}
