namespace NGame2.NScene.NLobbyScenePrerequisites;

internal interface IRequestWrapper
{
	bool IsDoneOrTimeout { get; }

	void Init();
}
