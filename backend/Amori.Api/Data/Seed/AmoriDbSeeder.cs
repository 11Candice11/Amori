using Amori.Api.Data.Context;
using Amori.Api.Domain.Entities;
using Amori.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Amori.Api.Data.Seed;

/// <summary>
/// Idempotent database seeder.
/// Seeds exactly one mock record per table into a freshly migrated database.
/// Only runs when the tables are empty — safe to call on every startup.
/// Requires at least one User and one Relationship to already exist;
/// if neither exist the seeder bootstraps one of each first.
/// </summary>
public static class AmoriDbSeeder
{
    public static async Task SeedAsync(AmoriDbContext db)
    {
        // ── Ensure we have a User and a Relationship to reference ──────────────
        var userId = await EnsureUserAsync(db);
        var relationshipId = await EnsureRelationshipAsync(db, userId);
        var partnerId = await EnsurePartnerAsync(db, relationshipId, userId);

        await db.SaveChangesAsync();

        // ── Seed every feature table (each guard is idempotent) ─────────────────
        await SeedIncidentsAsync(db, relationshipId, userId, partnerId);
        await SeedMoodCheckInsAsync(db, userId, relationshipId);
        await SeedSplittingAsync(db, userId, relationshipId);
        await SeedEmergencyAsync(db, relationshipId, userId, partnerId);
        await SeedRemindersAsync(db, userId);
        await SeedNotificationsAsync(db, userId);
        await SeedDeviceRegistrationsAsync(db, userId);
        await SeedVoiceNotesAsync(db, userId, relationshipId);
        await SeedMemoriesAsync(db, relationshipId, userId);
        await SeedTimelineAsync(db, relationshipId, userId);
        await SeedSurprisesAsync(db, relationshipId, userId, partnerId);
        await SeedHugsAsync(db, relationshipId, userId, partnerId);
        await SeedCalendarAsync(db, relationshipId, userId);
        await SeedDateIdeasAsync(db, relationshipId, userId);
        await SeedWishlistAsync(db, relationshipId, userId);
        await SeedBucketListAsync(db, relationshipId, userId);
        await SeedImportantDatesAsync(db, relationshipId, userId);
        await SeedGamesAsync(db, relationshipId, userId);
        await SeedQuizAsync(db, relationshipId, userId);
        await SeedWouldYouRatherAsync(db, relationshipId, userId);
        await SeedMessagesAsync(db, relationshipId, userId, partnerId);
        await SeedUserSettingsAsync(db, userId);
        await SeedRefreshTokenAsync(db, userId);
        await SeedRelationshipTicketsAsync(db, relationshipId, userId, partnerId);
        await SeedRelationshipInvitationAsync(db, relationshipId, userId);
    }

    // ── Bootstrap helpers ────────────────────────────────────────────────────

    private static async Task<Guid> EnsureUserAsync(AmoriDbContext db)
    {
        var existing = await db.Users.FirstOrDefaultAsync();
        if (existing is not null) return existing.Id;

        var user = new User
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Email = "alex@amori.app",
            PasswordHash = "$2a$12$seed.hash.placeholder.do.not.use.in.production",
            DisplayName = "Alex",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    private static async Task<Guid> EnsureRelationshipAsync(AmoriDbContext db, Guid userId)
    {
        var existing = await db.Relationships.FirstOrDefaultAsync();
        if (existing is not null) return existing.Id;

        var relationship = new Relationship
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            NickName = "The Amori Couple",
            Status = RelationshipStatus.Active,
            AnniversaryDate = new DateOnly(2023, 2, 14),
            CreatedAt = DateTime.UtcNow
        };
        db.Relationships.Add(relationship);

        var member = new RelationshipMember
        {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            RelationshipId = relationship.Id,
            UserId = userId,
            Role = RelationshipRole.Admin,
            InviteStatus = MemberInviteStatus.Accepted,
            CreatedAt = DateTime.UtcNow
        };
        db.RelationshipMembers.Add(member);

        await db.SaveChangesAsync();
        return relationship.Id;
    }

    private static async Task<Guid> EnsurePartnerAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        // Return a second member if one exists, otherwise create a partner user
        var partner = await db.Users
            .Where(u => u.Id != userId)
            .FirstOrDefaultAsync();

        if (partner is not null)
        {
            // Ensure they are a member of the relationship
            var alreadyMember = await db.RelationshipMembers
                .AnyAsync(m => m.RelationshipId == relationshipId && m.UserId == partner.Id);
            if (!alreadyMember)
            {
                db.RelationshipMembers.Add(new RelationshipMember
                {
                    Id = Guid.NewGuid(),
                    RelationshipId = relationshipId,
                    UserId = partner.Id,
                    Role = RelationshipRole.Member,
                    InviteStatus = MemberInviteStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            return partner.Id;
        }

        var newPartner = new User
        {
            Id = new Guid("44444444-4444-4444-4444-444444444444"),
            Email = "jamie@amori.app",
            PasswordHash = "$2a$12$seed.hash.placeholder.do.not.use.in.production",
            DisplayName = "Jamie",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(newPartner);

        db.RelationshipMembers.Add(new RelationshipMember
        {
            Id = new Guid("55555555-5555-5555-5555-555555555555"),
            RelationshipId = relationshipId,
            UserId = newPartner.Id,
            Role = RelationshipRole.Member,
            InviteStatus = MemberInviteStatus.Accepted,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return newPartner.Id;
    }

    // ── Feature seeders ──────────────────────────────────────────────────────

    private static async Task SeedIncidentsAsync(AmoriDbContext db, Guid relationshipId, Guid userId, Guid partnerId)
    {
        if (await db.RelationshipIncidents.AnyAsync()) return;

        var incident = new RelationshipIncident
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            ReportedByUserId = userId,
            AssignedToUserId = partnerId,
            Title = "Missed anniversary dinner reservation",
            Description = "The dinner reservation was forgotten and had to be cancelled last minute.",
            Category = IncidentCategory.Communication,
            SubCategory = "Planning",
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Low,
            Priority = IncidentPriority.Medium,
            Status = IncidentStatus.Open,
            DueAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        db.RelationshipIncidents.Add(incident);
        await db.SaveChangesAsync();

        if (!await db.RelationshipIncidentNotes.AnyAsync())
        {
            db.RelationshipIncidentNotes.Add(new RelationshipIncidentNote
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                AuthorUserId = userId,
                Content = "I felt hurt and disappointed. I had been looking forward to this for weeks.",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await db.RelationshipIncidentResponses.AnyAsync())
        {
            db.RelationshipIncidentResponses.Add(new RelationshipIncidentResponse
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                AuthorUserId = partnerId,
                Message = "I am truly sorry. I will set multiple reminders going forward.",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await db.RelationshipIncidentHistory.AnyAsync())
        {
            db.RelationshipIncidentHistory.Add(new RelationshipIncidentHistory
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                ActorUserId = userId,
                Action = IncidentHistoryAction.Created,
                OldValue = null,
                NewValue = IncidentStatus.Open.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await db.RelationshipIncidentReviews.AnyAsync())
        {
            db.RelationshipIncidentReviews.Add(new RelationshipIncidentReview
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                CreatedByUserId = userId,
                WhatWentWell = "We talked it through calmly.",
                WhatCouldImprove = "Need a shared calendar for important dates.",
                FutureAction = "Create shared calendar event for every anniversary.",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await db.RelationshipIncidentLessons.AnyAsync())
        {
            db.RelationshipIncidentLessons.Add(new RelationshipIncidentLesson
            {
                Id = Guid.NewGuid(),
                IncidentId = incident.Id,
                CreatedByUserId = userId,
                Lesson = "Always confirm bookings 48 hours in advance and put them in our shared calendar.",
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedMoodCheckInsAsync(AmoriDbContext db, Guid userId, Guid relationshipId)
    {
        if (await db.MoodCheckIns.AnyAsync()) return;

        db.MoodCheckIns.Add(new MoodCheckIn
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RelationshipId = relationshipId,
            CheckInType = CheckInType.Morning,
            Mood = MoodType.Happy,
            Intensity = 8,
            WhatHappened = "Had a lovely breakfast together.",
            Feelings = "Grateful and warm.",
            PerceivedCause = "Quality time with my partner.",
            WhatINeed = "More mornings like this.",
            IsSharedWithPartner = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedSplittingAsync(AmoriDbContext db, Guid userId, Guid relationshipId)
    {
        if (await db.SplittingSessions.AnyAsync()) return;

        var session = new SplittingSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RelationshipId = relationshipId,
            FeelingsSelected = ["anxious", "overwhelmed"],
            Trigger = "Argument about household responsibilities",
            Description = "Felt unheard and dismissed during conversation.",
            WhatINeed = "Some quiet time and then a hug.",
            RecommendedSupportType = SplittingAction.Grounding,
            ActionsTaken = [SplittingAction.Grounding],
            InitialMood = MoodType.Anxious,
            FinalMood = MoodType.Calm,
            Status = SplittingSessionStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        db.SplittingSessions.Add(session);
        await db.SaveChangesAsync();

        if (!await db.SplittingQuestions.AnyAsync())
        {
            var question = new SplittingQuestion
            {
                Id = Guid.NewGuid(),
                Question = "What is happening for you right now?",
                QuestionType = "reflection",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.SplittingQuestions.Add(question);
            await db.SaveChangesAsync();

            if (!await db.SplittingAnswers.AnyAsync())
            {
                db.SplittingAnswers.Add(new SplittingAnswer
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    QuestionId = question.Id,
                    Answer = "I am feeling really overwhelmed and just need a moment to breathe.",
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedEmergencyAsync(AmoriDbContext db, Guid relationshipId, Guid userId, Guid partnerId)
    {
        if (await db.EmergencyRequests.AnyAsync()) return;

        db.EmergencyRequests.Add(new EmergencyRequest
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            SenderId = userId,
            RecipientId = partnerId,
            Type = EmergencyRequestType.Support,
            Status = EmergencyRequestStatus.Acknowledged,
            Message = "I am having a really hard moment. Can you call me?",
            AcknowledgedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedRemindersAsync(AmoriDbContext db, Guid userId)
    {
        if (await db.Reminders.AnyAsync()) return;

        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Take daily vitamins",
            Notes = "Take with breakfast.",
            Type = ReminderType.Medication,
            ReminderTime = new TimeOnly(8, 0),
            Recurrence = RecurrenceType.Daily,
            IsEnabled = true,
            NextOccurrenceAt = DateTime.UtcNow.Date.AddDays(1).AddHours(8),
            CreatedAt = DateTime.UtcNow
        };
        db.Reminders.Add(reminder);
        await db.SaveChangesAsync();

        if (!await db.ReminderCompletions.AnyAsync())
        {
            db.ReminderCompletions.Add(new ReminderCompletion
            {
                Id = Guid.NewGuid(),
                ReminderId = reminder.Id,
                CompletedByUserId = userId,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedNotificationsAsync(AmoriDbContext db, Guid userId)
    {
        if (await db.AppNotifications.AnyAsync()) return;

        db.AppNotifications.Add(new AppNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Your partner sent you a hug! 🤗",
            Body = "Jamie is thinking of you.",
            NotificationType = "Hug",
            ReferenceId = Guid.NewGuid().ToString(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedDeviceRegistrationsAsync(AmoriDbContext db, Guid userId)
    {
        if (await db.DeviceRegistrations.AnyAsync()) return;

        db.DeviceRegistrations.Add(new DeviceRegistration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeviceToken = "ExampleDeviceToken_ABC123_SeedOnly",
            Platform = NotificationPlatform.Ios,
            DeviceIdentifier = "iPhone-Seed-Device",
            IsActive = true,
            LastSeenAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedVoiceNotesAsync(AmoriDbContext db, Guid userId, Guid relationshipId)
    {
        if (await db.VoiceNotes.AnyAsync()) return;

        db.VoiceNotes.Add(new VoiceNote
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RelationshipId = relationshipId,
            Title = "Good morning, love",
            FileKey = "voice-notes/seed/good-morning.m4a",
            DurationSeconds = 18,
            Category = VoiceNoteCategory.GoodMorning,
            IsFavorite = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedMemoriesAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.Memories.AnyAsync()) return;

        var memory = new Memory
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            CreatedByUserId = userId,
            Title = "First holiday together — Santorini",
            Description = "We watched the sunset from Oia. Magical.",
            MemoryDate = new DateOnly(2023, 9, 15),
            Location = "Oia, Santorini, Greece",
            Latitude = 36.4618,
            Longitude = 25.3753,
            Tags = ["travel", "sunset", "romantic"],
            IsFavorite = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        db.Memories.Add(memory);
        await db.SaveChangesAsync();

        if (!await db.MemoryMedia.AnyAsync())
        {
            db.MemoryMedia.Add(new MemoryMedia
            {
                Id = Guid.NewGuid(),
                MemoryId = memory.Id,
                FileKey = "memories/seed/santorini-sunset.jpg",
                MediaType = MemoryMediaType.Photo,
                DurationSeconds = null,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedTimelineAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.TimelineEvents.AnyAsync()) return;

        db.TimelineEvents.Add(new TimelineEvent
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            CreatedByUserId = userId,
            Title = "First date — coffee at The Roast",
            Description = "We talked for four hours and completely lost track of time.",
            EventDate = new DateOnly(2023, 2, 14),
            Location = "The Roast, London",
            EventType = TimelineEventType.FirstDate,
            PhotoKeys = [],
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedSurprisesAsync(AmoriDbContext db, Guid relationshipId, Guid userId, Guid partnerId)
    {
        if (await db.Surprises.AnyAsync()) return;

        db.Surprises.Add(new Surprise
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            CreatedByUserId = userId,
            RecipientUserId = partnerId,
            Title = "A little something for you 💌",
            MessageText = "I was thinking about you and just wanted you to know you make every day better.",
            ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            IsFavorite = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedHugsAsync(AmoriDbContext db, Guid relationshipId, Guid userId, Guid partnerId)
    {
        if (await db.Hugs.AnyAsync()) return;

        db.Hugs.Add(new Hug
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            SenderId = userId,
            RecipientId = partnerId,
            AcknowledgedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedCalendarAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.CalendarEvents.AnyAsync()) return;

        db.CalendarEvents.Add(new CalendarEvent
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            CreatedByUserId = userId,
            Title = "Date night — Italian restaurant",
            Description = "Reservation at Trattoria Bella for 7:30 pm.",
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            StartTime = new TimeOnly(19, 30),
            EndTime = new TimeOnly(22, 0),
            Location = "Trattoria Bella, 12 High Street",
            ReminderEnabled = true,
            ReminderMinutesBefore = 60,
            IsCompleted = false,
            IsShared = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedDateIdeasAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.DateIdeas.AnyAsync()) return;

        db.DateIdeas.Add(new DateIdea
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            CreatedByUserId = userId,
            Title = "Pottery class together",
            Description = "Find a local pottery class and book it as a surprise.",
            Category = DateCategory.Adventure,
            Location = "TBD — local studio",
            EstimatedCost = 45.00m,
            DurationMinutes = 120,
            IsFavorite = true,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedWishlistAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.WishlistItems.AnyAsync()) return;

        db.WishlistItems.Add(new WishlistItem
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            AddedByUserId = userId,
            Name = "Matching star map print",
            Description = "A custom star map showing the sky on the night we first met.",
            Url = "https://example.com/star-map",
            Price = 39.99m,
            Priority = WishlistPriority.Medium,
            IsPurchased = false,
            IsFavorite = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedBucketListAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.BucketListItems.AnyAsync()) return;

        db.BucketListItems.Add(new BucketListItem
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            AddedByUserId = userId,
            Title = "See the Northern Lights together",
            Description = "Plan a trip to Iceland or Norway to see the aurora borealis.",
            Location = "Iceland / Norway",
            Category = BucketListCategory.Travel,
            TargetDate = new DateOnly(2025, 12, 1),
            IsFavorite = true,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedImportantDatesAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.ImportantDates.AnyAsync()) return;

        db.ImportantDates.Add(new ImportantDate
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            CreatedByUserId = userId,
            Name = "Our Anniversary",
            Date = new DateOnly(2023, 2, 14),
            IsRecurring = true,
            ReminderEnabled = true,
            ReminderDaysBefore = 7,
            Notes = "Book a special dinner well in advance.",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedGamesAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.Games.AnyAsync()) return;

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = "Quiz About Us",
            Description = "Test how well you know each other with personalised questions.",
            Type = GameType.QuizAboutUs,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Games.Add(game);
        await db.SaveChangesAsync();

        if (!await db.GameSessions.AnyAsync())
        {
            db.GameSessions.Add(new GameSession
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                UserId = userId,
                RelationshipId = relationshipId,
                Status = GameSessionStatus.Completed,
                Score = 8,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!await db.GameScores.AnyAsync())
        {
            db.GameScores.Add(new GameScore
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                RelationshipId = relationshipId,
                UserId = userId,
                Score = 8,
                PlayedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedQuizAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.QuizQuestions.AnyAsync()) return;

        var quizSession = new QuizSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RelationshipId = relationshipId,
            Score = 3,
            TotalQuestions = 4,
            CompletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        db.QuizSessions.Add(quizSession);

        var question = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            QuestionText = "What was the name of the café where we had our first date?",
            CorrectAnswer = "The Roast",
            Options = ["The Roast", "Café Nero", "Starbucks", "Costa"],
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.QuizQuestions.Add(question);
        await db.SaveChangesAsync();

        if (!await db.QuizAnswers.AnyAsync())
        {
            db.QuizAnswers.Add(new QuizAnswer
            {
                Id = Guid.NewGuid(),
                QuizSessionId = quizSession.Id,
                QuestionId = question.Id,
                AnswerGiven = "The Roast",
                IsCorrect = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedWouldYouRatherAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.WouldYouRatherQuestions.AnyAsync()) return;

        var question = new WouldYouRatherQuestion
        {
            Id = Guid.NewGuid(),
            OptionA = "Live in a cozy cabin in the mountains",
            OptionB = "Live in a beach house by the ocean",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.WouldYouRatherQuestions.Add(question);
        await db.SaveChangesAsync();

        if (!await db.WouldYouRatherAnswers.AnyAsync())
        {
            db.WouldYouRatherAnswers.Add(new WouldYouRatherAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                UserId = userId,
                RelationshipId = relationshipId,
                ChoseOptionA = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedMessagesAsync(AmoriDbContext db, Guid relationshipId, Guid userId, Guid partnerId)
    {
        if (await db.Messages.AnyAsync()) return;

        db.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            SenderId = userId,
            RecipientId = partnerId,
            Text = "Good morning! ☀️ Thinking of you.",
            Category = MessageCategory.LoveNote,
            IsFavorite = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedUserSettingsAsync(AmoriDbContext db, Guid userId)
    {
        if (await db.UserSettings.AnyAsync()) return;

        db.UserSettings.Add(new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PushNotificationsEnabled = true,
            MoodRemindersEnabled = true,
            PartnerActivityNotifications = true,
            HugNotifications = true,
            EmergencyNotifications = true,
            MessageNotifications = true,
            ReminderNotifications = true,
            ProfileVisible = true,
            MoodShareDefault = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedRefreshTokenAsync(AmoriDbContext db, Guid userId)
    {
        if (await db.RefreshTokens.AnyAsync()) return;

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "seed-refresh-token-not-valid-for-auth",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = true,
            RevokedReason = "Seed data — not a real token",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedRelationshipTicketsAsync(AmoriDbContext db, Guid relationshipId, Guid userId, Guid partnerId)
    {
        if (await db.RelationshipTickets.AnyAsync()) return;

        var ticket = new RelationshipTicket
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            CreatedByUserId = userId,
            AssignedToUserId = partnerId,
            Subject = "Would like more quality time together",
            Category = TicketCategory.Relationship,
            Severity = TicketSeverity.Medium,
            Status = TicketStatus.Open,
            Description = "I have been feeling like we have been ships passing lately.",
            Feelings = "Lonely, longing for connection",
            WhatHappened = "We have both been busy with work and haven't had a proper date night in weeks.",
            WhatINeed = "A regular date night commitment.",
            WhatIPreferInFuture = "Weekly check-in and at least one planned activity per week.",
            CreatedAt = DateTime.UtcNow
        };
        db.RelationshipTickets.Add(ticket);
        await db.SaveChangesAsync();

        if (!await db.TicketResponses.AnyAsync())
        {
            db.TicketResponses.Add(new TicketResponse
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                RespondedByUserId = partnerId,
                Content = "You are absolutely right and I hear you. Let us block Sunday evenings just for us.",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedRelationshipInvitationAsync(AmoriDbContext db, Guid relationshipId, Guid userId)
    {
        if (await db.RelationshipInvitations.AnyAsync()) return;

        db.RelationshipInvitations.Add(new RelationshipInvitation
        {
            Id = Guid.NewGuid(),
            RelationshipId = relationshipId,
            InvitedByUserId = userId,
            InviteeEmail = "friend@example.com",
            InviteCode = "AMORI-SEED-0001",
            Status = MemberInviteStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
