using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class StarBlinkSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private StarBlinkSubComponent _this;

		private bool _isVisible;

		public override bool IsActive()
		{
			return _isVisible;
		}

		public override void Release()
		{
			_isVisible = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(bool isVisible)
		{
			if (_isVisible != isVisible)
			{
				_isVisible = isVisible;
				if (!_isVisible)
				{
					ReleaseComponent();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (_isVisible && Owner.gameObject.activeInHierarchy && _this == null)
			{
				CreateComponent();
			}
			if (_this != null)
			{
				ProxyBase.RearrangeDepth(Owner, _this.WidgetRoot);
			}
		}

		public override void RefreshInfo()
		{
			if (!(_this == null))
			{
				int blinkStar = 0;
				if (Owner != null && Owner.EquipItemInfo != null)
				{
					blinkStar = Owner.EquipItemInfo.Star;
				}
				_this.SetBlinkStar(blinkStar);
			}
		}

		private void CreateComponent()
		{
			_this = PrefabAuto.Factory.Create(Owner.RootWidget.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_this.transform);
			RefreshInfo();
		}

		private void ReleaseComponent()
		{
			FactoryBase.SafeReleaseComponent(ref _this);
		}
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/StarBlinkSubComponent";

	public static PrefabAuto<StarBlinkSubComponent> PrefabAuto = new PrefabAuto<StarBlinkSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public float FullGaugeBlinkTime;

	public AnimationCurve FullGaugeBlinkCurve = new AnimationCurve();

	public UISprite[] StarBlinks;

	private int _blinkStar;

	private void Update()
	{
		if (_blinkStar != 0)
		{
			int num = _blinkStar - 1;
			float num2 = Mathf.Repeat(GlobalTimer.time, FullGaugeBlinkTime) / FullGaugeBlinkTime;
			float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(FullGaugeBlinkCurve, num2, num2);
			if (StarBlinks[num] != null && StarBlinks[num].gameObject.activeSelf)
			{
				StarBlinks[num].alpha = alpha;
			}
		}
	}

	public void SetBlinkStar(int blinkStar)
	{
		_blinkStar = blinkStar;
		for (int i = 0; i < StarBlinks.Length; i++)
		{
			UIHelper.EnableGameObject(StarBlinks[i], i == _blinkStar - 1);
		}
	}
}
