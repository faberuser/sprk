using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class BuildingComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnSelectBuilding(BuildingComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/HeadQuarter/BuildingComponent";

	public static PrefabAuto<BuildingComponent> PrefabAuto = new PrefabAuto<BuildingComponent>("Prefab2/Component/Guild/HeadQuarter/BuildingComponent");

	public UISprite Sprite_Icon;

	public UITexture Texture_Icon;

	public UISprite Sprite_Select;

	public UILabel Label_Name;

	public UILabel Label_Level;

	public UILabel Label_Desc;

	private Listener _listener;

	private bool _selected;

	public GuildBuildingData BuildingData { get; private set; }

	public void Open(Listener listener, GuildBuildingData buildingData)
	{
		if (buildingData == null)
		{
			VespaDebug.LogError("BuildingData is null");
			return;
		}
		_listener = listener;
		BuildingData = buildingData;
		SetIcon();
		SetInfo();
		SetSelect(select: false);
	}

	private void SetIcon()
	{
		if (!String_Extension_Method.IsNullOrEmpty(BuildingData.IconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(BuildingData.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Icon, plainTexture);
		}
	}

	private void SetInfo()
	{
		UIHelper.SetLabelText(Label_Name, GetLocaleDirect(BuildingData.NameKey));
		UIHelper.SetLabelText(Label_Level, "Lv " + BuildingData.Level);
		UIHelper.SetLabelText(Label_Desc, GetLocaleDirect(BuildingData.DescKey));
	}

	private string GetLocaleDirect(string key)
	{
		return LocalizationManager.GetStringDirect(key);
	}

	public void SetSelect(bool select)
	{
		_selected = select;
		UIHelper.EnableGameObject(Sprite_Select, _selected);
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnSelectBuilding(this);
		}
	}
}
