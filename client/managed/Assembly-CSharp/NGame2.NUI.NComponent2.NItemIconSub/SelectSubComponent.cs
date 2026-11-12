using System;
using System.Collections.Generic;
using NNguiExtension;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class SelectSubComponent : LinkFactoryComponent, IInspectorGuiDrawer
{
	public enum ESelectType
	{
		Select_Checked,
		Select_BorderBook,
		Select_BorderEquip
	}

	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private SelectSubComponent _this;

		public bool IsSelected { get; private set; }

		public ESelectType SelectType { get; private set; }

		public bool IsSoulWeapon { get; private set; }

		public override bool IsActive()
		{
			return IsSelected;
		}

		public override void Release()
		{
			IsSelected = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(bool isSelected, ESelectType selectType = ESelectType.Select_Checked, bool isSoulWeapon = false)
		{
			if (IsSelected == isSelected && SelectType == selectType)
			{
				return;
			}
			IsSelected = isSelected;
			SelectType = selectType;
			IsSoulWeapon = isSoulWeapon;
			if (_this != null)
			{
				if (!IsSelected)
				{
					ReleaseComponent();
				}
				else
				{
					RefreshInfo();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (IsSelected && Owner.gameObject.activeInHierarchy && _this == null)
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
				_this.SetSelect(SelectType, IsSoulWeapon);
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
	public class SelectUiData
	{
		public ESelectType SelectType;

		public UIWidget Root;
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/SelectSubComponent";

	public static PrefabAuto<SelectSubComponent> PrefabAuto = new PrefabAuto<SelectSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISpriteSelector SelectSpriteSelector;

	[HideInInspector]
	public List<SelectUiData> SelectUiList = new List<SelectUiData>();

	public void SetSelect(ESelectType selectType, bool isSoulWeapon)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SelectUiList); i++)
		{
			SelectUiData selectUiData = SelectUiList[i];
			UIHelper.EnableGameObject(selectUiData.Root, selectUiData.SelectType == selectType);
		}
		SetSelectSpriteSelector(isSoulWeapon);
	}

	private void SetSelectSpriteSelector(bool isSoulWeapon)
	{
		if (SelectSpriteSelector != null)
		{
			SelectSpriteSelector.Select(isSoulWeapon ? "SoulWeapon" : "Normal");
		}
	}
}
