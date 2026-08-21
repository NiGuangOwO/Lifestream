using Dalamud.Utility.Signatures;
using ECommons.EzHookManager;

namespace Lifestream.Game;

public unsafe class Memory : IDisposable
{
    [Signature("4C 8D 0D ?? ?? ?? ?? 44 0F B7 41", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Fallible)]
    internal int* MaxInstances;

    internal delegate byte OpenPartyFinderInfoDelegate(void* agentLfg, ulong contentId);
    [EzHook("40 53 48 83 EC 20 48 8B D9 E8 ?? ?? ?? ?? 84 C0 74 07 C6 83 ?? ?? ?? ?? ?? 48 83 C4 20 5B C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 40 53", false)]
    internal EzHook<OpenPartyFinderInfoDelegate> OpenPartyFinderInfoHook;

    internal byte OpenPartyFinderInfoDetour(void* agentLfg, ulong contentId)
    {
        PluginLog.Information($"{((nint)agentLfg):X16}, {contentId:X16}");
        return OpenPartyFinderInfoHook.Original(agentLfg, contentId);
    }

    internal Memory()
    {
        SignatureHelper.Initialise(this);
        EzSignatureHelper.Initialize(this);
    }

    public void Dispose()
    {
    }
}
