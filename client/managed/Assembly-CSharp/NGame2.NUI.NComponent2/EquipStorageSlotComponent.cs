using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class EquipStorageSlotComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public new interface IListener
	{
		void OnClickSelect(int equipStorageIndex);

		void OnClickUnlock();
	}

	public const string PrefabPath = "Prefab2/Component/Hero/EquipStorageSlotComponent";

	public static PrefabAuto<EquipStorageSlotComponent> PrefabAuto = new PrefabAuto<EquipStorageSlotComponent>("Prefab2/Component/Hero/EquipStorageSlotComponent");

	public UILabel Label_SlotIndex;

	public UIBundleLoader BundleLoader_HeroTexture;

	public UILabel Label_Name;

	public UISprite Sprite_LockImage;

	public UISprite Sprite_SelectImage;

	public UIWidget DispatchRoot;

	private IListener Listener;

	public EquipPresetInfo EquipPresetInfo { get; private set; }

	public EquipStorageSlotInfo StorageInfo { get; private set; }

	public int Index { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Reset();
	}

	private void Reset()
	{
		UIHelper.EnableGameObject(Sprite_LockImage, enable: false);
		UIHelper.EnableGameObject(Label_Name, enable: false);
		UIHelper.EnableGameObject(BundleLoader_HeroTexture, enable: false);
		UIHelper.EnableGameObject(Label_SlotIndex, enable: false);
		UIHelper.EnableGameObject(Sprite_SelectImage, enable: false);
		UIHelper.EnableGameObject(DispatchRoot, enable: false);
	}

	public void Open(EquipPresetInfo presetInfo, int index, IListener listener)
	{
		EquipPresetInfo = presetInfo;
		Listener = listener;
		Index = index;
		PresetInit(presetInfo, index);
	}

	public void PresetInit(EquipPresetInfo presetInfo, int index)
	{
		Reset();
		if (presetInfo == null)
		{
			UIHelper.EnableGameObject(Sprite_LockImage, enable: true);
			UIHelper.EnableGameObject(Label_SlotIndex, enable: true);
			UIHelper.SetLabelText(Label_SlotIndex, index);
		}
		else
		{
			UIHelper.EnableGameObject(Label_SlotIndex, enable: true);
			UIHelper.SetLabelText(Label_SlotIndex, index);
			UIHelper.EnableGameObject(Label_Name, enable: true);
			SetName(presetInfo.Name);
		}
	}

	public void SetName(string name)
	{
		UIHelper.SetLabelText(Label_Name, name);
	}

	public void SetPortraitTexture(int heroIndex)
	{
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo != null)
		{
			UIHelper.EnableGameObject(BundleLoader_HeroTexture, enable: true);
			string heroIconPath = UIHelper.GetHeroIconPath(heroInfo);
			UIHelper.SetBundleTexture(BundleLoader_HeroTexture, heroIconPath);
		}
	}

	public void UpdatePortraitTexture(int selectedHeroIndex)
	{
		SprkGearPresetPortraits.Update(this, selectedHeroIndex);
	}

	public void ShowSelectImage(bool isShow)
	{
		UIHelper.EnableGameObject(Sprite_SelectImage, isShow);
	}

	public void OnClickComponent()
	{
		if (Listener != null)
		{
			if (EquipPresetInfo == null)
			{
				Listener.OnClickUnlock();
			}
			else
			{
				Listener.OnClickSelect(EquipPresetInfo.EquipStorageSlotIndex);
			}
		}
	}
}
