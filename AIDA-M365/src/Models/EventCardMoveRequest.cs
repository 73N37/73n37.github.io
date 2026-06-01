using System;

namespace AIDA.M365.Models;

/// <summary>
/// Data payload carrying parameters to transition an event card to a new stage or date range.
/// </summary>
/// <param name="Card">The target event card to modify.</param>
/// <param name="TargetSectionKey">The destination column key.</param>
/// <param name="StartUtc">New scheduled start date and time in UTC.</param>
/// <param name="EndUtc">New scheduled end date and time in UTC.</param>
public sealed record EventCardMoveRequest(
    GodsEventCard Card,
    string TargetSectionKey,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc);
