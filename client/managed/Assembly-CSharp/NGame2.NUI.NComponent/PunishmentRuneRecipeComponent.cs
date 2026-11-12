using NGame2.NUI.NComponent2;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent;

public class PunishmentRuneRecipeComponent : LinkFactoryComponent, InitOnCreate, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OnClick(PunishmentRuneRecipeComponent component);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/PunishmentRuneCraft/PunishmentRuneRecipeComponent";

	public static PrefabAuto<PunishmentRuneRecipeComponent> PrefabAuto = new PrefabAuto<PunishmentRuneRecipeComponent>("Resources/Prefab2/Component/PunishmentRuneCraft/PunishmentRuneRecipeComponent");

	public Transform RootPunishmentRune;

	public Transform RootBlood;

	public Transform RootVoidRune;

	public Transform RootLegendRune;

	public UILabel LabelName;

	public UISprite SpriteSelected;

	private ItemIconComponent _compPunishmentRune;

	private ItemIconComponent _compBlood;

	private ItemIconComponent _compVoidRune;

	private ItemIconComponent _compLegendRune;

	private IListener _listener;

	public PunishmentRuneData PunishmentRuneData { get; set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(PunishmentRuneData data, IListener listener)
	{
		_listener = listener;
		PunishmentRuneData = data;
		ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(data.PunishmentRuneIndex);
		UIHelper.SetLabelText(LabelName, ItemNameBuilder.GetItemName(data2));
		_compPunishmentRune = ItemIconComponent.PrefabAuto.Factory.Create(RootPunishmentRune);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_compPunishmentRune.transform);
		_compPunishmentRune.Open(data2, this);
		data2 = GameRoot.Instance.ItemDataContainer.GetData(data.ApostleBloodIndex);
		_compBlood = ItemIconComponent.PrefabAuto.Factory.Create(RootBlood);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_compBlood.transform);
		_compBlood.Open(data2, this);
		data2 = GameRoot.Instance.ItemDataContainer.GetData(data.VoidRuneIndex);
		_compVoidRune = ItemIconComponent.PrefabAuto.Factory.Create(RootVoidRune);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_compVoidRune.transform);
		_compVoidRune.Open(data2, this);
		data2 = GameRoot.Instance.ItemDataContainer.GetData(data.LegendRuneIndex);
		_compLegendRune = ItemIconComponent.PrefabAuto.Factory.Create(RootLegendRune);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_compLegendRune.transform);
		_compLegendRune.Open(data2, this);
		_compLegendRune.SetLockSubComponent(locked: false);
		SetSelected(select: false);
	}

	public void SetSelected(bool select)
	{
		UIHelper.EnableGameObject(SpriteSelected, select);
	}

	public void Close()
	{
		FactoryBase.ReleaseComponent(_compPunishmentRune);
		_compPunishmentRune = null;
		FactoryBase.ReleaseComponent(_compBlood);
		_compBlood = null;
		FactoryBase.ReleaseComponent(_compVoidRune);
		_compVoidRune = null;
		FactoryBase.ReleaseComponent(_compLegendRune);
		_compLegendRune = null;
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
