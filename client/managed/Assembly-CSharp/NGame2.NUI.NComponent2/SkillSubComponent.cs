using NShared;

namespace NGame2.NUI.NComponent2;

public interface SkillSubComponent
{
	HeroInfo HeroInfo { get; set; }

	int SkillSlotIndex { get; set; }

	void PlayLevelUpEffect();

	void SetSkillLevel(int level);

	void SetSkillExtend(int skillExtend);

	void Refresh();

	void ShowSkillLevel(bool isShow);

	void ActiveForceSkillGroup();
}
