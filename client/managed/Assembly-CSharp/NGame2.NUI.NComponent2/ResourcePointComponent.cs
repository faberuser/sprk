using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ResourcePointComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/Guild/HeadQuarter/ResourcePointComponent";

	public static PrefabAuto<ResourcePointComponent> PrefabAuto = new PrefabAuto<ResourcePointComponent>("Prefab2/Component/Guild/HeadQuarter/ResourcePointComponent");

	public UISpriteSelector SpriteSelector_Icon;

	public UILabel Label_Name;

	public UILabel Label_Amount;

	private int _amount;

	public GuildConstructionResource ResourceType { get; private set; }

	public void Open(GuildConstructionResource resourceType, int amount)
	{
		ResourceType = resourceType;
		_amount = amount;
		SetIcon();
		SetName();
		SetAmount();
	}

	private void SetIcon()
	{
		SpriteSelector_Icon.Select(ResourceType.ToString());
	}

	private void SetName()
	{
		UIHelper.SetLabelText(Label_Name, GetName());
	}

	private string GetName()
	{
		string text = string.Empty;
		string result = string.Empty;
		if (ResourceType == GuildConstructionResource.Wood)
		{
			text = "GUILD_MATERIAL_WOOD";
		}
		else if (ResourceType == GuildConstructionResource.Stone)
		{
			text = "GUILD_MATERIAL_STONE";
		}
		else if (ResourceType == GuildConstructionResource.Metal)
		{
			text = "GUILD_MATERIAL_METAL";
		}
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			result = LocalizationManager.GetStringDirect(text);
		}
		return result;
	}

	private void SetAmount()
	{
		UIHelper.SetLabelText(Label_Amount, _amount);
	}

	public void OnShowTooltip()
	{
		TooltipTextInfo tooltipTextInfo = new TooltipTextInfo();
		tooltipTextInfo.Name.Value = ResourceType.ToString();
		tooltipTextInfo.Desc.Value = ResourceType.ToString() + "Desc";
		GuildConstructionResourceTooltip.Open(SpriteSelector_Icon.GetComponent<UIWidget>(), tooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		GuildConstructionResourceTooltip.Hide(SpriteSelector_Icon.GetComponent<UIWidget>());
	}
}
