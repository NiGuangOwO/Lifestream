﻿using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Configuration;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.MathHelpers;
using Lifestream.Data;
using Lumina.Excel.GeneratedSheets;
using System.IO;
using Path = System.IO.Path;

namespace Lifestream.Systems;
public sealed class ResidentialAethernet : IDisposable
{
    private const string FileName = "HousingData.json";
    public HousingData HousingData;

    public readonly Dictionary<uint, ResidentialZoneInfo> ZoneInfo = new() {
        [ResidentalAreas.The_Goblet] = new() { SubdivisionModifier = new(-700f, -700f)},
        [ResidentalAreas.Mist] = new() { SubdivisionModifier = new(-700f, -700f) },
        [ResidentalAreas.The_Lavender_Beds] = new() { SubdivisionModifier = new(-700f, -700f) },
        [ResidentalAreas.Empyreum] = new() { SubdivisionModifier = new(-704f, -654f) },
        [ResidentalAreas.Shirogane] = new() { SubdivisionModifier = new(-700f, -700f) },
    };

    public ResidentialAetheryte? ActiveAetheryte = null;

    public bool IsInResidentialZone() => ZoneInfo.ContainsKey(Svc.ClientState.TerritoryType);

    public ResidentialAethernet()
    {
        try
        {
            HousingData = EzConfig.LoadConfiguration<HousingData>(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, FileName), false);
            foreach (var zone in ZoneInfo)
            {
                var values = Svc.Data.GetExcelSheet<HousingAethernet>().Where(a => a.TerritoryType.Row == zone.Key).OrderBy(x => x.Order);
                foreach (var a in values)
                {
                    var aetheryte = new ResidentialAetheryte(a, a.Order >= values.Count() / 2, zone.Value.SubdivisionModifier);
                    zone.Value.Aetherytes.Add(aetheryte);
                }
            }
        }
        catch(Exception e)
        {
            e.Log();
        }
    }

    public void Dispose()
    {
        
    }

    public void Tick()
    {
        if (Svc.ClientState.LocalPlayer != null && ZoneInfo.ContainsKey(Svc.ClientState.TerritoryType))
        {
            UpdateActiveAetheryte();
        }
        else
        {
            ActiveAetheryte = null;
        }
    }

    void UpdateActiveAetheryte()
    {
        var a = Utils.GetValidAetheryte();
        if (a != null)
        {
            var aetheryte = GetFromGameObject(a);
            if(aetheryte != null)
            {
                if (ActiveAetheryte == null)
                {
                    P.Overlay.IsOpen = true;
                }
                ActiveAetheryte = aetheryte;
            }
        }
        else
        {
            ActiveAetheryte = null;
        }
    }

    public ResidentialAetheryte? GetFromGameObject(GameObject obj)
    {
        var pos2 = obj.Position.ToVector2();
        if (ZoneInfo.TryGetValue(Svc.ClientState.TerritoryType, out var result))
        {
            foreach (var l in result.Aetherytes)
            {
                if (Vector2.Distance(l.Position, pos2) < 10)
                {
                    return l;
                }
            }
        }
        return null;
    }
}