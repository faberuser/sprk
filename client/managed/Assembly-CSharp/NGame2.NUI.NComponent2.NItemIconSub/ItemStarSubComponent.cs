using System;
using NNguiExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ItemStarSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private ItemStarSubComponent _this;

		private int _star;

		private int _transcend;

		private bool _valance;

		public int Star => _star;

		public override bool IsActive()
		{
			return _star > 0;
		}

		public override void Release()
		{
			_star = 0;
			_transcend = 0;
			_valance = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(int star, int transcend = 0, bool valance = false)
		{
			if (_star == star && _transcend == transcend)
			{
				return;
			}
			_star = star;
			_transcend = transcend;
			_valance = valance;
			if (_this != null)
			{
				if (IsActive())
				{
					RefreshInfo();
				}
				else
				{
					ReleaseComponent();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (IsActive() && Owner.gameObject.activeInHierarchy && _this == null)
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
			if (_this != null)
			{
				_this.SetStar(_star, _transcend, _valance);
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

	[Serializable]
	public class LimitBreakUi
	{
		public UIWidget Group_Root;

		public UILabel Label_Star;

		public void InitUi(int star)
		{
			UIHelper.EnableGameObject(Group_Root, enable: true);
			UIHelper.SetLabelText(Label_Star, star.ToString());
		}

		public void ResetUi()
		{
			UIHelper.EnableGameObject(Group_Root, enable: false);
		}
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/ItemStarSubComponent";

	public static PrefabAuto<ItemStarSubComponent> PrefabAuto = new PrefabAuto<ItemStarSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISpriteSelector StarSelector;

	public UISpriteSelector HseoTranscendStarSelector;

	public UISpriteSelector TechnoMagicStarSelector;

	[Header("[한계 돌파 별 표시]")]
	public LimitBreakUi LimitBreak;

	public void SetStar(int star, int transcend = 0, bool valance = false)
	{
		LimitBreak.ResetUi();
		UIHelper.EnableGameObject(StarSelector, enable: false);
		UIHelper.EnableGameObject(TechnoMagicStarSelector, enable: false);
		UIHelper.EnableGameObject(HseoTranscendStarSelector, enable: false);
		if (transcend > 0)
		{
			UIHelper.EnableGameObject(HseoTranscendStarSelector, enable: true);
			HseoTranscendStarSelector.SelectInt(transcend);
		}
		else if (valance)
		{
			UIHelper.EnableGameObject(TechnoMagicStarSelector, enable: true);
			TechnoMagicStarSelector.SelectInt(star);
		}
		else if (star > 5)
		{
			LimitBreak.InitUi(star);
		}
		else
		{
			UIHelper.EnableGameObject(StarSelector, enable: true);
			StarSelector.SelectInt(star);
		}
	}
}
