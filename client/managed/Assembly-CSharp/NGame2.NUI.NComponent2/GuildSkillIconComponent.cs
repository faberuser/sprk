using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildSkillIconComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/Guild/GuildSkillIconComponent";

	public static PrefabAuto<GuildSkillIconComponent> PrefabAuto = new PrefabAuto<GuildSkillIconComponent>("Prefab2/Component/Guild/GuildSkillIconComponent");

	public UILabel Label_Level;

	public UISpriteSelector IconSelector;

	public int SkillIndex { get; private set; }

	public void Open(int index, int level, GuildSkillType skillType)
	{
		SkillIndex = index;
		bool flag = level > 0;
		UIHelper.EnableGameObject(Label_Level, flag);
		if (flag)
		{
			UIHelper.SetLabelText(Label_Level, "Lv." + level);
		}
		IconSelector.SelectEnum(skillType);
	}

	public void RefreshLevel(int level)
	{
		if (UIHelper.IsEnabledGameObjet(Label_Level))
		{
			UIHelper.SetLabelText(Label_Level, "Lv." + level);
		}
	}
}
