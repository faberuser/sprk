namespace NShared.SprkSetSkillRules;

/// <summary>
/// SPRK: stores auto-battle skill rules on the server ({"heroIndex": rules}, 0 clears a hero) so
/// the server's battle worker can apply them in online battles. Replace = the complete set.
/// </summary>
[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/set_skill_rules";

	public string SkillRules { get; set; }

	public bool Replace { get; set; }
}
