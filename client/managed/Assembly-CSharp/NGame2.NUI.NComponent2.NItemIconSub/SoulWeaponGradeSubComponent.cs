using NNguiExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class SoulWeaponGradeSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private SoulWeaponGradeSubComponent _this;

		private int _grade = -1;

		private int _level;

		public void Set(int grade, int level)
		{
			if (_grade == grade && _level == level)
			{
				return;
			}
			_grade = grade;
			_level = level;
			if (_this != null)
			{
				if (!IsActive())
				{
					ReleaseComponent();
				}
				else
				{
					RefreshInfo();
				}
			}
		}

		public override bool IsActive()
		{
			return _grade != -1;
		}

		public override void Release()
		{
			_grade = -1;
			_level = 0;
		}

		public override void ChainUpdate()
		{
			if (IsActive() && Owner.gameObject.activeInHierarchy && _this == null)
			{
				CreateComponent();
			}
			if (!IsActive() && _this != null)
			{
				ReleaseComponent();
			}
			if (_this != null)
			{
				ProxyBase.RearrangeDepth(Owner, _this.WidgetRoot);
			}
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public override void RefreshInfo()
		{
			if (_this != null)
			{
				_this.Set(_grade, _level);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/SoulWeaponGradeSubComponent";

	public static PrefabAuto<SoulWeaponGradeSubComponent> PrefabAuto = new PrefabAuto<SoulWeaponGradeSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISpriteSelector UISpriteSelector;

	public UILabel LabelLevel;

	public void Set(int grade, int level)
	{
		SetGrade(grade);
		SetLevel(level);
	}

	public void SetGrade(int grade)
	{
		if (UISpriteSelector != null)
		{
			UISpriteSelector.SelectInt(grade);
		}
	}

	public void SetLevel(int level)
	{
		string text = ((level == 0) ? "" : level.ToString());
		UIHelper.SetLabelText(LabelLevel, text);
	}
}
