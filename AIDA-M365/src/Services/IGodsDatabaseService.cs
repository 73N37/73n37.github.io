using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

/// <summary>
/// Provides unified database synchronization and persistence operations for Engestofte Gods booking events.
/// Handles syncing state changes between the local database and memory cache.
/// </summary>
public interface IGodsDatabaseService
{
    /// <summary>
    /// Fetches all stored gods events from the database, falling back to local memory if offline.
    /// </summary>
    Task<List<GodsEventCard>> FetchAllEventsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves or updates a gods event card in the database, with automatic duplicate merge constraints.
    /// </summary>
    Task SaveEventAsync(GodsEventCard card, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a gods event card from the database by its Microsoft Graph identifier.
    /// </summary>
    Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Synchronizes a card's workflow position and scheduling timestamps after a drag-and-drop action.
    /// </summary>
    Task SyncCardPositionAsync(string graphEventId, string sectionKey, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Seeds initial high-fidelity demo booking events into the database if the active table is empty.
    /// </summary>
    Task SeedDemoDataToDbAsync(CancellationToken cancellationToken = default);
}
