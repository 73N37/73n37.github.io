using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

public sealed class DemoDynamicsCrmService : IDynamicsCrmService
{
    private readonly ConcurrentDictionary<(string, string), DynamicsCrmProfile> _profiles = new();

    public DemoDynamicsCrmService()
    {
        // Seed some realistic Estate Venue leads and opportunities
        SeedProfile(new DynamicsCrmProfile
        {
            EntityId = "lead-1",
            EntityLogicalName = "lead",
            ClientName = "Eleanor Kensington",
            Email = "eleanor.kensington@royal-society.org",
            Phone = "+44 20 7946 0192",
            Topic = "Kensington-Windsor Grand Wedding 2026",
            Status = "Qualified (Ready to Book)",
            EstimatedBudget = 85000.00m,
            PreferredVenue = "Grand Ballroom & South Gardens",
            Notes = "Prefers organic floral arrangements, high-security requirements, and fine dining for 150 guests. Linking to estate wedding slot.",
            CreatedOnUtc = DateTimeOffset.UtcNow.AddDays(-12)
        });

        SeedProfile(new DynamicsCrmProfile
        {
            EntityId = "opp-2",
            EntityLogicalName = "opportunity",
            ClientName = "Lord Marcus Sterling",
            Email = "marcus.sterling@sterling-holdings.co.uk",
            Phone = "+44 7700 900077",
            Topic = "Lord Sterling's Golden Jubilee (50th Birthday)",
            Status = "In Proposal & Negotiations",
            EstimatedBudget = 42500.00m,
            PreferredVenue = "Lakeside Pavilion",
            Notes = "Wants open-air fire pits, premium acoustic audio setup, and customized champagne bar. Guest count estimated at 80.",
            CreatedOnUtc = DateTimeOffset.UtcNow.AddDays(-6)
        });

        SeedProfile(new DynamicsCrmProfile
        {
            EntityId = "lead-3",
            EntityLogicalName = "lead",
            ClientName = "Victoria Duchess of York",
            Email = "victoria.y@duchess-estates.com",
            Phone = "+44 20 7946 0883",
            Topic = "Summer Solstice Wedding Feast",
            Status = "Inquiry Received",
            EstimatedBudget = 120000.00m,
            PreferredVenue = "Rose Gardens & Conservatory",
            Notes = "High-end luxury styling, classical musicians, and specific strict catering requirements. Anticipates 200 high-profile guests.",
            CreatedOnUtc = DateTimeOffset.UtcNow.AddDays(-2)
        });
    }

    private void SeedProfile(DynamicsCrmProfile profile)
    {
        _profiles[(profile.EntityLogicalName.ToLowerInvariant(), profile.EntityId.ToLowerInvariant())] = profile;
    }

    public Task<DynamicsCrmProfile?> GetProfileAsync(
        string entityLogicalName,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var key = (entityLogicalName.ToLowerInvariant(), entityId.ToLowerInvariant());
        _profiles.TryGetValue(key, out var profile);
        return Task.FromResult(profile);
    }
}
