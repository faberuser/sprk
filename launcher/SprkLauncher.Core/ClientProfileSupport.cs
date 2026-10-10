using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace SprkLauncher.Core;

public static class ClientProfileSupport
{
    public static void EnsureInstalled(string root)
    {
        var managed = Path.Combine(root, "King's Raid_Data", "Managed");
        using var gameStream = File.OpenRead(Path.Combine(managed, "Assembly-CSharp.dll"));
        using var game = new PEReader(gameStream);
        var metadata = game.GetMetadataReader();
        var hooked = metadata.MemberReferences.Any(handle => {
            var member = metadata.GetMemberReference(handle);
            if (metadata.GetString(member.Name) != "ResolveHostUrl" || member.Parent.Kind != HandleKind.TypeReference) return false;
            return metadata.GetString(metadata.GetTypeReference((TypeReferenceHandle)member.Parent).Name) == "SprkAccounts";
        });
        using var helperStream = File.OpenRead(Path.Combine(managed, "SprkAccounts.dll"));
        using var helper = new PEReader(helperStream);
        var helperMetadata = helper.GetMetadataReader();
        var supported = helperMetadata.MethodDefinitions.Any(handle =>
            helperMetadata.GetString(helperMetadata.GetMethodDefinition(handle).Name) == "ResolveHostUrl");
        if (!hooked || !supported)
            throw new InvalidDataException("This server's client release does not support server profiles yet. Publish the updated client release to that update server, then retry.");
    }
}
