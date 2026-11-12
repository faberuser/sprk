using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class OrdealArenaBuffIconComponent : LinkFactoryComponent
{
	public static string PrefabPath = "Prefab2/Component/OrdealArena/OrdealArenaBuffIconComponent";

	public static PrefabAuto<OrdealArenaBuffIconComponent> PrefabAuto = new PrefabAuto<OrdealArenaBuffIconComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UIBundleLoader BundleTex_ItemIcon;

	public UISpriteSelector SpriteSelect_Grade;

	public UILabel Label_ItemCount;

	private OrdealArenaBuffTooltipInfo _tooltipInfo = new OrdealArenaBuffTooltipInfo();

	private OrdealArenaBuffData _ordealArenaBuffData;

	public void SetInfo(OrdealArenaBuffData buffData, int itemCount = 1)
	{
		Init();
		_ordealArenaBuffData = buffData;
		SetIcon(buffData.IconPath);
		SetGrade(buffData.Grade);
		SetCount(itemCount);
		SetTooltipDesc(buffData.OptionType, buffData.OptionValue);
	}

	private void Init()
	{
		UIHelper.EnableGameObject(BundleTex_ItemIcon, enable: false);
		UIHelper.EnableGameObject(Label_ItemCount, enable: false);
		UIHelper.EnableGameObject(SpriteSelect_Grade, enable: false);
	}

	private void SetIcon(string iconPath)
	{
		UIHelper.SetBundleTexture(BundleTex_ItemIcon, iconPath);
		UIHelper.EnableGameObject(BundleTex_ItemIcon, enable: true);
	}

	private void SetGrade(int grade)
	{
		if (!(SpriteSelect_Grade == null))
		{
			SpriteSelect_Grade.SelectInt(grade);
			UIHelper.EnableGameObject(SpriteSelect_Grade, enable: true);
		}
	}

	private void SetCount(int count)
	{
		if (!(Label_ItemCount == null))
		{
			UIHelper.EnableGameObject(Label_ItemCount, count > 1);
			UIHelper.SetLabelText(Label_ItemCount, count);
		}
	}

	private void SetTooltipDesc(OperationType optionType, int optionValue)
	{
		if (_tooltipInfo == null)
		{
			_tooltipInfo = new OrdealArenaBuffTooltipInfo();
		}
		_tooltipInfo.Reset();
		string empty = string.Empty;
		if (optionType == OperationType.UseSkill)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(optionValue);
			if (data == null)
			{
				return;
			}
			empty = data.Description;
		}
		else
		{
			empty = StatData.GetOptionDisplay(optionType, optionValue, opened: true, 0, appendColor: false);
		}
		_tooltipInfo.Name.Value = empty;
	}

	public void OnShowOrdealBuffTooltip()
	{
		if (_tooltipInfo != null)
		{
			OrdealArenaBuffTooltip.Open(WidgetRoot, _tooltipInfo);
		}
	}

	public void OnHideOrdealBuffTooltip()
	{
		OrdealArenaBuffTooltip.Hide(WidgetRoot);
	}
}
