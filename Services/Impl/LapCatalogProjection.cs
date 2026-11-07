using System.Globalization;
using System.Text.RegularExpressions;
using Entities.Class;
using Entities.Dtos;
using Services.Models;

namespace Services.Impl;

internal static class LapCatalogProjection
{
    internal static CatalogLapDto Describe(LapListDto lap, LapCatalogEnrichment enrichment)
    {
        CatalogLapDto result = new()
        {
            DriverNumber = lap.DriverNumber,
            LapNumber = lap.LapNumber,
            Duration = lap.LapDuration,
            IsPitOutLap = lap.IsPitOutLap,
            UnavailableData = enrichment.UnavailableSources.ToList()
        };

        List<Stint> matchingStints = (enrichment.Stints ?? [])
            .Where(stint => stint.DriverNumber == lap.DriverNumber &&
                (!stint.LapStart.HasValue || stint.LapStart <= lap.LapNumber) &&
                (!stint.LapEnd.HasValue || stint.LapEnd >= lap.LapNumber))
            .ToList();
        if (matchingStints.Count == 1 && matchingStints[0].LapStart > 0 &&
            matchingStints[0].LapEnd >= matchingStints[0].LapStart && !string.IsNullOrWhiteSpace(matchingStints[0].Compound) &&
            !string.Equals(matchingStints[0].Compound, "UNKNOWN", StringComparison.OrdinalIgnoreCase))
        {
            result.Compound = matchingStints[0].Compound;
        }

        if (lap.IsPitOutLap == true)
        {
            AddAlert(result, "pit_out", "laps", "OpenF1 identifies a pit out lap.");
        }
        if (enrichment.Pits?.Any(pit => pit.DriverNumber == lap.DriverNumber && pit.LapNumber == lap.LapNumber) == true)
        {
            AddAlert(result, "pit_in", "pit", "OpenF1 records a pit lane visit on this lap.");
        }

        AddRaceControlAlerts(result, lap, enrichment.RaceControls ?? []);
        if (lap.LapDuration == null)
        {
            result.UnavailableData.Add("lap_duration");
        }
        if (result.Compound == null)
        {
            result.UnavailableData.Add("compound_not_identified");
        }
        if (lap.IsPitOutLap == null)
        {
            result.UnavailableData.Add("pit_out_status");
        }
        if (lap.DateStart == null || lap.DateStart == default(DateTime))
        {
            result.UnavailableData.Add("lap_start_time");
        }
        if (lap.DurationSector1 == null)
        {
            result.UnavailableData.Add("duration_sector_1");
        }
        if (lap.DurationSector2 == null)
        {
            result.UnavailableData.Add("duration_sector_2");
        }
        if (lap.DurationSector3 == null)
        {
            result.UnavailableData.Add("duration_sector_3");
        }

        result.UnavailableData.Add("official_lap_validity_not_provided");
        result.UnavailableData = result.UnavailableData.Distinct().ToList();
        result.SuggestedEligible = lap.LapDuration > 0 &&
            !result.Alerts.Any(alert => alert.Code is "pit_in" or "pit_out" or "time_deleted");
        return result;
    }

    private static void AddRaceControlAlerts(CatalogLapDto result, LapListDto lap, List<RaceControl> controls)
    {
        bool deleted = false;
        foreach (RaceControl control in controls.OrderBy(control => control.Date))
        {
            string message = control.Message ?? string.Empty;
            bool targeted = control.DriverNumber == lap.DriverNumber;
            if (control.DriverNumber == null)
            {
                Match car = Regex.Match(message, @"\bCAR\s+(?<number>\d+)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                targeted = car.Success && int.TryParse(car.Groups["number"].Value, out int driver) && driver == lap.DriverNumber;
                if (car.Success && !targeted)
                {
                    continue;
                }
            }
            else if (!targeted)
            {
                continue;
            }

            bool sessionWide = control.DriverNumber == null &&
                (string.Equals(control.Scope, "Track", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(control.Scope, "Sector", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(control.Category, "SessionStatus", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(control.Category, "SafetyCar", StringComparison.OrdinalIgnoreCase));

            bool timeDecision = message.Contains("TIME", StringComparison.OrdinalIgnoreCase) &&
                (message.Contains("DELETED", StringComparison.OrdinalIgnoreCase) || message.Contains("REINSTATED", StringComparison.OrdinalIgnoreCase));
            if (timeDecision)
            {
                // race_control.lap_number is the race's current lap, not necessarily the deleted lap.
                Match explicitLap = Regex.Match(message, @"\bLAP\s+(?<number>\d+)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (targeted && explicitLap.Success && int.TryParse(explicitLap.Groups["number"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int affectedLap))
                {
                    if (affectedLap == lap.LapNumber)
                    {
                        deleted = message.Contains("DELETED", StringComparison.OrdinalIgnoreCase);
                        AddAlert(result, "race_control", "race_control", message);
                    }
                }
                else
                {
                    result.UnavailableData.Add("time_deletion_lap_not_identified");
                }
                continue;
            }

            if (!targeted && !sessionWide)
            {
                result.UnavailableData.Add("race_control_driver_or_scope_not_identified");
                continue;
            }

            bool directLap = targeted && control.LapNumber == lap.LapNumber;
            bool duringLap = lap.DateStart.HasValue && lap.DateStart.Value != default && lap.LapDuration > 0 &&
                control.Date >= lap.DateStart.Value && (control.Date - lap.DateStart.Value).TotalSeconds < lap.LapDuration;
            if (directLap || duringLap)
            {
                AddAlert(result, "race_control", "race_control", message);
            }
            else if (control.LapNumber <= 0 && (!lap.DateStart.HasValue || lap.LapDuration is null or <= 0))
            {
                result.UnavailableData.Add("race_control_lap_association");
            }
        }

        if (deleted)
        {
            AddAlert(result, "time_deleted", "race_control", "A lap-specific time deletion was identified and not subsequently reinstated.");
        }
    }

    private static void AddAlert(CatalogLapDto lap, string code, string source, string message)
    {
        lap.Alerts.Add(new LapAlertDto { Code = code, Source = source, Message = message });
    }
}
