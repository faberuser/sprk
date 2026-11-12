using NShared;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public interface IInteractiveAction
{
	void Init(PcAvatarData data, AiInteractionComponent aiInteractionComponent);

	void DoAction();

	bool CanAction();

	void Update();
}
