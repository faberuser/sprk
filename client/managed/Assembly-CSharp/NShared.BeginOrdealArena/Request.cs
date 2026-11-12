namespace NShared.BeginOrdealArena;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "ordeal_arena/begin_ordeal_arena";

	public int NodeIndex { get; set; }

	public int[] HeroIndices { get; set; }
}
