using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle.NSub;

public class CastingComponent : LinkFactoryComponent, IInspectorGuiDrawer
{
	public enum ECastingType
	{
		Increase
	}

	public interface ICastingValue
	{
		ECastingType CastingType { get; }

		bool IsEnd { get; }

		float Ratio { get; }
	}

	[Serializable]
	public class GaugeData
	{
		public ECastingType CastingType;

		public UIWidget Root;

		public UISprite Gauge;
	}

	public const string PrefabPath = "Prefab2/Component/Battle/Sub/CastingComponent";

	public static PrefabAuto<CastingComponent> PrefabAuto = new PrefabAuto<CastingComponent>("Prefab2/Component/Battle/Sub/CastingComponent");

	public UIWidget RootWidget;

	[HideInInspector]
	public List<GaugeData> GaugeList = new List<GaugeData>();

	private int _creatureUid;

	public bool IsEnabled => UIHelper.IsEnabledGameObjet(RootWidget);

	public void Open(int creatureUid)
	{
		_creatureUid = creatureUid;
		UIHelper.EnableGameObject(RootWidget, enable: false);
	}

	public void Reset()
	{
		_creatureUid = 0;
	}

	public void ChainUpdate(ICastingValue casting)
	{
		if (!(RootWidget == null))
		{
			if (casting == null || casting.IsEnd)
			{
				EndCasting();
			}
			else
			{
				PlayCasting(casting);
			}
		}
	}

	private void PlayCasting(ICastingValue casting)
	{
		UIHelper.EnableGameObject(RootWidget, enable: true);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GaugeList); i++)
		{
			GaugeData gaugeData = GaugeList[i];
			UIHelper.EnableGameObject(gaugeData.Root, gaugeData.CastingType == casting.CastingType);
			if (gaugeData.CastingType == casting.CastingType)
			{
				UIHelper.SetSpriteFillAmount(gaugeData.Gauge, casting.Ratio);
			}
		}
	}

	private void EndCasting()
	{
		UIHelper.EnableGameObject(RootWidget, enable: false);
	}
}
