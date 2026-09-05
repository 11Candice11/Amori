using Amori.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Data.Context;

public sealed class AmoriDbContext(DbContextOptions<AmoriDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<RelationshipMember> RelationshipMembers => Set<RelationshipMember>();

    // Incident Management
    public DbSet<RelationshipIncident> RelationshipIncidents => Set<RelationshipIncident>();
    public DbSet<RelationshipIncidentNote> RelationshipIncidentNotes => Set<RelationshipIncidentNote>();
    public DbSet<RelationshipIncidentResponse> RelationshipIncidentResponses => Set<RelationshipIncidentResponse>();
    public DbSet<RelationshipIncidentHistory> RelationshipIncidentHistory => Set<RelationshipIncidentHistory>();
    public DbSet<RelationshipIncidentReview> RelationshipIncidentReviews => Set<RelationshipIncidentReview>();
    public DbSet<RelationshipIncidentLesson> RelationshipIncidentLessons => Set<RelationshipIncidentLesson>();

    // Mood
    public DbSet<MoodCheckIn> MoodCheckIns => Set<MoodCheckIn>();

    // Reminders
    public DbSet<Reminder> Reminders => Set<Reminder>();

    // Messages
    public DbSet<Message> Messages => Set<Message>();

    // Voice Notes
    public DbSet<VoiceNote> VoiceNotes => Set<VoiceNote>();

    // Emergency
    public DbSet<EmergencyRequest> EmergencyRequests => Set<EmergencyRequest>();

    // Splitting
    public DbSet<SplittingSession> SplittingSessions => Set<SplittingSession>();

    // Hugs
    public DbSet<Hug> Hugs => Set<Hug>();

    // Memories
    public DbSet<Memory> Memories => Set<Memory>();
    public DbSet<MemoryMedia> MemoryMedia => Set<MemoryMedia>();

    // Timeline
    public DbSet<TimelineEvent> TimelineEvents => Set<TimelineEvent>();

    // Surprises
    public DbSet<Surprise> Surprises => Set<Surprise>();

    // Relationship Tickets
    public DbSet<RelationshipTicket> RelationshipTickets => Set<RelationshipTicket>();
    public DbSet<TicketResponse> TicketResponses => Set<TicketResponse>();

    // Calendar
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    // Date Planner
    public DbSet<DateIdea> DateIdeas => Set<DateIdea>();

    // Wishlist
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Bucket List
    public DbSet<BucketListItem> BucketListItems => Set<BucketListItem>();

    // Important Dates
    public DbSet<ImportantDate> ImportantDates => Set<ImportantDate>();

    // Notifications
    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();

    // Games (Distraction Centre)
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes found in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AmoriDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
