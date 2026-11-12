using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NCostume;

public class MySetCostumeComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(MySetCostumeComponent component);
	}

	public enum ComponentType
	{
		None,
		CurrentCostume,
		Empty,
		Storage
	}

	public struct OpenData
	{
		public static OpenData NullData = new OpenData
		{
			ComponentType = ComponentType.None,
			SlotIndex = 0,
			CostumeStorage = null
		};

		public int SlotIndex;

		public ComponentType ComponentType;

		public CostumeStorageSlotInfo CostumeStorage;

		public static OpenData CreateEmptySlot(int slotIndex)
		{
			return new OpenData
			{
				SlotIndex = slotIndex,
				ComponentType = ComponentType.Empty
			};
		}

		public static OpenData CreateCurrentCostume()
		{
			return new OpenData
			{
				ComponentType = ComponentType.CurrentCostume
			};
		}

		public static OpenData CreateStorageSlot(CostumeStorageSlotInfo info)
		{
			return new OpenData
			{
				ComponentType = ComponentType.Storage,
				CostumeStorage = info,
				SlotIndex = info.CostumeStorageSlotIndex
			};
		}

		public bool IsEqual(OpenData rhs)
		{
			if (ComponentType != rhs.ComponentType)
			{
				return false;
			}
			if (SlotIndex != rhs.SlotIndex)
			{
				return false;
			}
			return Vespa_Common_Class_Extension_Method.EQUALS(CostumeStorage, rhs.CostumeStorage);
		}

		public bool IsNull()
		{
			return IsEqual(NullData);
		}
	}

	public const string PrefabPath = "Prefab2/Component/Costume/MySetCostumeComponent";

	public static PrefabAuto<MySetCostumeComponent> PrefabAuto = new PrefabAuto<MySetCostumeComponent>("Prefab2/Component/Costume/MySetCostumeComponent");

	public const string DefaultTexturePath = "Customize/Product/Portrait/Customize_Myset_Default";

	public const string EmptyTexturePath = "Customize/Product/Portrait/Customize_Empty_Slot";

	public UILabel NameLabel;

	public UILabel EmptyLabel;

	public UIBundleLoader CostumeBundleTex;

	public UISprite Sprite_Selected;

	private Listener _listener;

	private const string LocaleSlotNameKey = "CONTENT_CUSTOMIZE_LIST_MYSET_SLOT_NAME";

	public OpenData ComponentOpenData { get; private set; }

	public bool Open(OpenData openData, Listener listener)
	{
		_listener = listener;
		ComponentOpenData = openData;
		InitCostumeImage();
		SetName();
		SetSelected(Selected: false);
		return true;
	}

	private void SetName()
	{
		if (ComponentOpenData.ComponentType == ComponentType.CurrentCostume)
		{
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("CONTENTS_CUSTOMIZE_MY_SET_SHOW_NOW"));
			return;
		}
		UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_LIST_MYSET_SLOT_NAME", ComponentOpenData.SlotIndex));
	}

	private void InitCostumeImage()
	{
		UIHelper.EnableGameObject(EmptyLabel, ComponentOpenData.ComponentType == ComponentType.Empty);
		UIHelper.EnableGameObject(CostumeBundleTex, ComponentOpenData.ComponentType == ComponentType.Storage || ComponentOpenData.ComponentType == ComponentType.CurrentCostume || ComponentOpenData.ComponentType == ComponentType.Empty);
		if (ComponentOpenData.ComponentType == ComponentType.CurrentCostume)
		{
			UIHelper.SetBundleTexture(CostumeBundleTex, "Customize/Product/Portrait/Customize_Myset_Default");
		}
		else if (ComponentOpenData.ComponentType == ComponentType.Empty)
		{
			UIHelper.SetBundleTexture(CostumeBundleTex, "Customize/Product/Portrait/Customize_Empty_Slot");
		}
		else
		{
			if (ComponentOpenData.ComponentType != ComponentType.Storage)
			{
				return;
			}
			CostumeStorageSlotInfo costumeStorage = ComponentOpenData.CostumeStorage;
			if (costumeStorage == null)
			{
				UIHelper.EnableGameObject(CostumeBundleTex, enable: false);
				return;
			}
			CostumeData costumeData = GameRoot.Instance.CostumeDataContainer.GetData(costumeStorage.CostumeIndex);
			if (costumeData == null || costumeStorage.CostumeIndex == 0)
			{
				CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(costumeStorage.HeroIndex);
				costumeData = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(costumeStorage.HeroIndex, cachedHeroInfo.Star);
			}
			string battlePortraitPath = costumeData.BattlePortraitPath;
			UIHelper.SetBundleTexture(CostumeBundleTex, battlePortraitPath);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_listener = null;
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void SetSelected(bool Selected)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Selected);
	}
}
