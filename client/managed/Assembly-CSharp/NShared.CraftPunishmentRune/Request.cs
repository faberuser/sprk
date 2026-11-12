namespace NShared.CraftPunishmentRune;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "punishment_rune/craft_punishment_rune";

	public int PunishmentRuneIndex { get; set; }

	public int[] IngredientRuneIndices { get; set; }
}
