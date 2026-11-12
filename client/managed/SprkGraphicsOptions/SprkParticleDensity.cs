using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SprkParticleDensity : MonoBehaviour
{
	private struct Emitter
	{
		public ParticleSystem system;

		public ParticleSystemRenderer renderer;

		public float rate;

		public float coverage;
	}

	private readonly List<Emitter> emitters = new List<Emitter>();

	private readonly List<ParticleSystem> systems = new List<ParticleSystem>();

	private static readonly Dictionary<ParticleSystem, SprkParticleDensity> owners = new Dictionary<ParticleSystem, SprkParticleDensity>();

	private static readonly string[] protectedNames = new string[10] { "warning", "telegraph", "indicator", "target", "range", "circle", "decal", "marker", "area", "aoe" };

	public float Demand => DemandForMask(-1);

	private static bool Protected(Transform node)
	{
		Transform transform = node;
		while (transform != null)
		{
			string text = transform.name;
			string[] array = protectedNames;
			foreach (string value in array)
			{
				if (text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
			transform = transform.parent;
		}
		return false;
	}

	public void Capture()
	{
		Clear();
		systems.Clear();
		GetComponentsInChildren(includeInactive: true, systems);
		foreach (ParticleSystem system in systems)
		{
			if (!owners.TryGetValue(system, out var value) || !(value != null) || !(value != this))
			{
				ParticleSystem.EmissionModule emission = system.emission;
				ParticleSystem.MainModule main = system.main;
				ParticleSystemRenderer component = system.GetComponent<ParticleSystemRenderer>();
				if (!(component == null) && component.renderMode == ParticleSystemRenderMode.Billboard && emission.enabled && emission.burstCount == 0 && main.maxParticles >= 128 && emission.rateOverTime.mode == ParticleSystemCurveMode.Constant && !(emission.rateOverTimeMultiplier < 32f) && !system.trails.enabled && !system.collision.enabled && !system.trigger.enabled && !system.subEmitters.enabled && !system.lights.enabled && !Protected(system.transform))
				{
					emitters.Add(new Emitter
					{
						system = system,
						renderer = component,
						rate = emission.rateOverTimeMultiplier,
						coverage = component.maxParticleSize
					});
					owners[system] = this;
				}
			}
		}
		systems.Clear();
	}

	public float DemandForMask(int mask)
	{
		float uncapped;
		return DemandForMask(mask, 1f, out uncapped);
	}

	public float DemandForMask(int mask, float density, out float uncapped)
	{
		uncapped = 0f;
		if (!base.gameObject.activeInHierarchy)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Emitter emitter in emitters)
		{
			if (emitter.system != null && (mask & (1 << emitter.system.gameObject.layer)) != 0 && emitter.system.gameObject.activeInHierarchy && emitter.system.emission.enabled && emitter.renderer != null && emitter.renderer.enabled)
			{
				ParticleSystem.MainModule main = emitter.system.main;
				float num2 = emitter.rate * density * Mathf.Max(0.1f, main.startLifetime.constantMax);
				uncapped += num2;
				num += Mathf.Min(main.maxParticles, num2);
			}
		}
		return num;
	}

	public void Apply(float density)
	{
		Apply(density, 0);
	}

	public void Apply(float density, int coverage)
	{
		Apply(density, coverage, 1f, -1);
	}

	public void Apply(float density, int coverage, float battleFactor, int battleMask)
	{
		foreach (Emitter emitter in emitters)
		{
			if (emitter.system != null)
			{
				ParticleSystem.EmissionModule emission = emitter.system.emission;
				bool flag = (battleMask & (1 << emitter.system.gameObject.layer)) != 0;
				emission.rateOverTimeMultiplier = emitter.rate * (base.gameObject.activeInHierarchy ? (density * (flag ? battleFactor : 1f)) : 1f);
				if (emitter.renderer != null)
				{
					emitter.renderer.maxParticleSize = ((!flag || coverage == 0 || !base.gameObject.activeInHierarchy) ? emitter.coverage : Mathf.Min(emitter.coverage, (coverage == 1) ? 0.15f : 0.08f));
				}
			}
		}
	}

	public void Clear()
	{
		Apply(1f);
		foreach (Emitter emitter in emitters)
		{
			owners.Remove(emitter.system);
		}
		emitters.Clear();
	}

	private void OnDisable()
	{
		Apply(1f);
	}

	private void OnDestroy()
	{
		Clear();
	}

	private void OnEnable()
	{
		SprkGraphicsOptions.ApplyParticleBudget(this);
	}
}
