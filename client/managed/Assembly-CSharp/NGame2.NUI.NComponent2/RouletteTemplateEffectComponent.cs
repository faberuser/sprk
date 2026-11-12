using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RouletteTemplateEffectComponent : LinkFactoryComponent, InitOnCreate, IInspectorGuiDrawer
{
	[Serializable]
	public class EffectComponent
	{
		public GameObject obj;

		public ParticleSystem particleSys;
	}

	public const string PrefabPath = "Prefab2/Component/MiniGame/RouletteTemplateEffectComponent";

	public static PrefabAuto<RouletteTemplateEffectComponent> PrefabAuto = new PrefabAuto<RouletteTemplateEffectComponent>("Prefab2/Component/MiniGame/RouletteTemplateEffectComponent");

	[HideInInspector]
	public List<EffectComponent> EffectList = new List<EffectComponent>();

	public void Open(Vector3 eluerAngle)
	{
		Set(eluerAngle);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(base.transform);
	}

	void InitOnCreate.OnCreate()
	{
		SetEnable(isEnable: false);
	}

	void InitOnCreate.OnRelease()
	{
		SetEnable(isEnable: false);
	}

	public void Set(Vector3 eluerAngle)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(EffectList); i++)
		{
			Set(eluerAngle, EffectList[i]);
		}
	}

	public void Set(Vector3 eluerAngle, EffectComponent effectComponent)
	{
		if (effectComponent != null && !(effectComponent.obj == null) && !(effectComponent.particleSys == null))
		{
			float num = eluerAngle.z % 90f;
			ParticleSystem.MainModule main = effectComponent.particleSys.main;
			main.startRotation = MathF.PI / 180f * (90f - num);
		}
	}

	public void SetEnable(bool isEnable)
	{
		Set(base.transform.rotation.eulerAngles);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(EffectList); i++)
		{
			SetEnable(EffectList[i], isEnable);
		}
	}

	private void SetEnable(EffectComponent effectComponent, bool isEanble)
	{
		if (effectComponent != null)
		{
			UIHelper.EnableGameObject(effectComponent.obj, enable: false);
			if (isEanble)
			{
				UIHelper.EnableGameObject(effectComponent.obj, enable: true);
				effectComponent.particleSys.Play();
			}
		}
	}
}
