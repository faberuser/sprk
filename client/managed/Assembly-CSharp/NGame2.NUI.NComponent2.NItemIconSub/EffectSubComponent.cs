using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class EffectSubComponent : LinkFactoryComponent, InitOnCreate, IInspectorGuiDrawer
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private EffectSubComponent _this;

		public override bool IsActive()
		{
			return true;
		}

		public override void Release()
		{
			ReleaseComponent();
		}

		public override void OnDisable()
		{
		}

		public UIGame3DWidget EnableAndGet(EffectWidgetType effectType)
		{
			if (_this == null)
			{
				CreateComponent();
			}
			if (_this == null)
			{
				return null;
			}
			return _this.EnableAndGet(effectType);
		}

		public override void ChainUpdate()
		{
			if (_this != null)
			{
				ProxyBase.RearrangeDepth(Owner, _this.WidgetRoot);
			}
		}

		public override void RefreshInfo()
		{
		}

		private void CreateComponent()
		{
			_this = PrefabAuto.Factory.Create(Owner.WidgetThis.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_this.transform);
			RefreshInfo();
		}

		private void ReleaseComponent()
		{
			FactoryBase.SafeReleaseComponent(ref _this);
		}
	}

	[Serializable]
	private class EffectUiData
	{
		public EffectWidgetType EffectType;

		public UIGame3DWidget EffectWidget;
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/EffectSubComponent";

	public static PrefabAuto<EffectSubComponent> PrefabAuto = new PrefabAuto<EffectSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	[HideInInspector]
	[SerializeField]
	private List<EffectUiData> _effectList = new List<EffectUiData>();

	void InitOnCreate.OnCreate()
	{
		for (int i = 0; i < _effectList.Count; i++)
		{
			UIHelper.EnableGameObject(_effectList[i].EffectWidget, enable: false);
		}
	}

	void InitOnCreate.OnRelease()
	{
		for (int i = 0; i < _effectList.Count; i++)
		{
			EffectUiData effectUiData = _effectList[i];
			if (!(effectUiData.EffectWidget == null))
			{
				if (effectUiData.EffectWidget.IsPlaying)
				{
					effectUiData.EffectWidget.Stop(immediatelyClear: true);
				}
				UIHelper.EnableGameObject(effectUiData.EffectWidget, enable: false);
			}
		}
	}

	public UIGame3DWidget EnableAndGet(EffectWidgetType effectType)
	{
		for (int i = 0; i < _effectList.Count; i++)
		{
			EffectUiData effectUiData = _effectList[i];
			if (effectUiData.EffectType == effectType)
			{
				UIHelper.EnableGameObject(effectUiData.EffectWidget, enable: true);
				return effectUiData.EffectWidget;
			}
		}
		return null;
	}
}
