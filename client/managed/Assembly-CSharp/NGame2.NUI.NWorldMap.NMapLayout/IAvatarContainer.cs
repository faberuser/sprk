using System;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public interface IAvatarContainer
{
	void ForEach(Action<Avatar> action);
}
