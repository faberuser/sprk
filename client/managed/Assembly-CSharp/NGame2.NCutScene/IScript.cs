using LitJson;
using NVespa;

namespace NGame2.NCutScene;

public interface IScript : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	bool IsEnd { get; }

	void Reset();

	void Visit(IVisitor visitor);

	void Begin(ILocalTimer localTimer, bool isSkip);

	void Update();

	void End();

	void FinalizeScript(FinalizeType finalizeType = FinalizeType.Normal);
}
