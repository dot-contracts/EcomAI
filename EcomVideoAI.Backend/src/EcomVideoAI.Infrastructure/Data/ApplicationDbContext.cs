using EcomVideoAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EcomVideoAI.Domain.Enums;
using EcomVideoAI.Domain.ValueObjects;
using System.Text.Json;
using System.Collections.Generic;

namespace EcomVideoAI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // User and Authentication
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<UserPreferences> UserPreferences { get; set; } = null!;
        public DbSet<NotificationSettings> NotificationSettings { get; set; } = null!;

        // Video and Content
        public DbSet<Video> Videos { get; set; } = null!;
        public DbSet<ProductData> ProductData { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;

        // Subscription and Billing
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;
        
        // TODO: Uncomment when payment entity configurations are fixed
        // public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
        // public DbSet<Payment> Payments { get; set; } = null!;
        // public DbSet<Invoice> Invoices { get; set; } = null!;
        // public DbSet<UsageCredit> UsageCredits { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations
            ConfigureUserEntity(modelBuilder);
            ConfigureRefreshTokenEntity(modelBuilder);
            ConfigureEmailVerificationTokenEntity(modelBuilder);
            ConfigureUserRoleEntity(modelBuilder);
            ConfigureUserPreferencesEntity(modelBuilder);
            ConfigureNotificationSettingsEntity(modelBuilder);
            ConfigureVideoEntity(modelBuilder);
            ConfigureProductDataEntity(modelBuilder);
            ConfigureProductImageEntity(modelBuilder);
            ConfigureProductCategoryEntity(modelBuilder);
            ConfigureSubscriptionPlanEntity(modelBuilder);
            ConfigureUserSubscriptionEntity(modelBuilder);
            
            // TODO: Uncomment and fix property mismatches when implementing payment functionality
            /*
            ConfigurePaymentMethodEntity(modelBuilder);
            ConfigurePaymentEntity(modelBuilder);
            ConfigureInvoiceEntity(modelBuilder);
            ConfigureUsageCreditEntity(modelBuilder);
            */
            
            // Ignore payment-related entities until they are properly configured
            modelBuilder.Ignore<PaymentMethod>();
            modelBuilder.Ignore<Payment>();
            modelBuilder.Ignore<Invoice>();
            modelBuilder.Ignore<UsageCredit>();
        }

        private static void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.EmailVerified)
                    .HasColumnName("email_verified")
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(512)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(100);

                entity.Property(e => e.AvatarUrl)
                    .HasColumnName("avatar_url")
                    .HasMaxLength(500);

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phone_number")
                    .HasMaxLength(20);

                entity.Property(e => e.DateOfBirth)
                    .HasColumnName("date_of_birth");

                entity.Property(e => e.Timezone)
                    .HasColumnName("timezone")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Locale)
                    .HasColumnName("locale")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .IsRequired();

                entity.Property(e => e.LastLoginAt)
                    .HasColumnName("last_login_at");

                entity.Property(e => e.FailedLoginAttempts)
                    .HasColumnName("failed_login_attempts")
                    .IsRequired();

                entity.Property(e => e.LockedUntil)
                    .HasColumnName("locked_until");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(100);

                // Indexes
                entity.HasIndex(e => e.Email)
                    .HasDatabaseName("ix_users_email")
                    .IsUnique();

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("ix_users_is_active");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("ix_users_created_at");

                // Relationships
                entity.HasMany(e => e.Videos)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.RefreshTokens)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Roles)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Preferences)
                    .WithOne()
                    .HasForeignKey<UserPreferences>("UserId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.NotificationSettings)
                    .WithOne()
                    .HasForeignKey<NotificationSettings>("UserId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasMaxLength(512)
                    .IsRequired();

                entity.Property(e => e.ExpiresAt)
                    .HasColumnName("expires_at")
                    .IsRequired();

                entity.Property(e => e.IsRevoked)
                    .HasColumnName("is_revoked")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.HasIndex(e => e.Token)
                    .HasDatabaseName("ix_refresh_tokens_token")
                    .IsUnique();

                entity.HasIndex(e => e.ExpiresAt)
                    .HasDatabaseName("ix_refresh_tokens_expires_at");
            });
        }

        private static void ConfigureEmailVerificationTokenEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.ToTable("email_verification_tokens");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasMaxLength(512)
                    .IsRequired();

                entity.Property(e => e.ExpiresAt)
                    .HasColumnName("expires_at")
                    .IsRequired();

                entity.Property(e => e.UsedAt)
                    .HasColumnName("used_at");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                // Indexes
                entity.HasIndex(e => e.Token)
                    .HasDatabaseName("ix_email_verification_tokens_token")
                    .IsUnique();

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_email_verification_tokens_user_id");

                entity.HasIndex(e => e.ExpiresAt)
                    .HasDatabaseName("ix_email_verification_tokens_expires_at");

                // Foreign Key
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureUserRoleEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.RoleName)
                    .HasColumnName("role_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_user_roles_user_id");
            });
        }

        private static void ConfigureUserPreferencesEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPreferences>(entity =>
            {
                entity.ToTable("user_preferences");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.DefaultVideoResolution)
                    .HasColumnName("default_video_resolution")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.DefaultVideoDuration)
                    .HasColumnName("default_video_duration")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(100);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_user_preferences_user_id")
                    .IsUnique();
            });
        }

        private static void ConfigureNotificationSettingsEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationSettings>(entity =>
            {
                entity.ToTable("notification_settings");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.VideoCompleted)
                    .HasColumnName("video_completed")
                    .IsRequired();

                entity.Property(e => e.VideoFailed)
                    .HasColumnName("video_failed")
                    .IsRequired();

                entity.Property(e => e.SubscriptionRenewal)
                    .HasColumnName("subscription_renewal")
                    .IsRequired();

                entity.Property(e => e.SubscriptionExpired)
                    .HasColumnName("subscription_expired")
                    .IsRequired();

                entity.Property(e => e.PaymentFailed)
                    .HasColumnName("payment_failed")
                    .IsRequired();

                entity.Property(e => e.NewFeatures)
                    .HasColumnName("new_features")
                    .IsRequired();

                entity.Property(e => e.MarketingUpdates)
                    .HasColumnName("marketing_updates")
                    .IsRequired();

                entity.Property(e => e.WeeklyDigest)
                    .HasColumnName("weekly_digest")
                    .IsRequired();

                entity.Property(e => e.EmailEnabled)
                    .HasColumnName("email_enabled")
                    .IsRequired();

                entity.Property(e => e.PushEnabled)
                    .HasColumnName("push_enabled")
                    .IsRequired();

                entity.Property(e => e.SmsEnabled)
                    .HasColumnName("sms_enabled")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(100);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_notification_settings_user_id")
                    .IsUnique();
            });
        }

        private static void ConfigureVideoEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Video>(entity =>
            {
                entity.ToTable("videos");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(1000);

                entity.Property(e => e.TextPrompt)
                    .HasColumnName("text_prompt")
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(e => e.InputType)
                    .HasColumnName("input_type")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Resolution)
                    .HasColumnName("resolution")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.DurationSeconds)
                    .HasColumnName("duration_seconds")
                    .IsRequired();

                entity.Property(e => e.ImageUrl)
                    .HasColumnName("image_url")
                    .HasMaxLength(500);

                entity.Property(e => e.VideoUrl)
                    .HasColumnName("video_url")
                    .HasMaxLength(500);

                entity.Property(e => e.ThumbnailUrl)
                    .HasColumnName("thumbnail_url")
                    .HasMaxLength(500);

                entity.Property(e => e.FreepikTaskId)
                    .HasColumnName("freepik_task_id")
                    .HasMaxLength(100);

                entity.Property(e => e.FreepikImageTaskId)
                    .HasColumnName("freepik_image_task_id")
                    .HasMaxLength(100);

                entity.Property(e => e.CompletedAt)
                    .HasColumnName("completed_at");

                entity.Property(e => e.ErrorMessage)
                    .HasColumnName("error_message")
                    .HasMaxLength(2000);

                entity.Property(e => e.FileSizeBytes)
                    .HasColumnName("file_size_bytes")
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(100);

                entity.Property(e => e.Metadata)
                    .HasColumnName("metadata")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => v == null ? null : JsonSerializer.Deserialize<VideoMetadata>(v, (JsonSerializerOptions?)null)
                    );

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_videos_user_id");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("ix_videos_status");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("ix_videos_created_at");

                entity.HasIndex(e => e.FreepikTaskId)
                    .HasDatabaseName("ix_videos_freepik_task_id")
                    .IsUnique()
                    .HasFilter("freepik_task_id IS NOT NULL");

                entity.HasIndex(e => e.FreepikImageTaskId)
                    .HasDatabaseName("ix_videos_freepik_image_task_id")
                    .IsUnique()
                    .HasFilter("freepik_image_task_id IS NOT NULL");
            });
        }

        private static void ConfigureProductDataEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductData>(entity =>
            {
                entity.ToTable("product_data");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id");

                entity.Property(e => e.OriginalUrl)
                    .HasColumnName("original_url")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.Domain)
                    .HasColumnName("domain")
                    .HasMaxLength(100);

                entity.Property(e => e.Platform)
                    .HasColumnName("platform")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(2000);

                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(e => e.OriginalPrice)
                    .HasColumnName("original_price")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DiscountPercent)
                    .HasColumnName("discount_percent");

                entity.Property(e => e.Brand)
                    .HasColumnName("brand")
                    .HasMaxLength(100);

                entity.Property(e => e.Sku)
                    .HasColumnName("sku")
                    .HasMaxLength(100);

                entity.Property(e => e.Availability)
                    .HasColumnName("availability")
                    .HasMaxLength(50);

                entity.Property(e => e.Rating)
                    .HasColumnName("rating")
                    .HasColumnType("decimal(3,2)");

                entity.Property(e => e.ReviewCount)
                    .HasColumnName("review_count")
                    .IsRequired();

                entity.Property(e => e.MainImageUrl)
                    .HasColumnName("main_image_url")
                    .HasMaxLength(500);

                entity.Property(e => e.ScrapingStatus)
                    .HasColumnName("scraping_status")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.ScrapedAt)
                    .HasColumnName("scraped_at");

                entity.Property(e => e.ErrorMessage)
                    .HasColumnName("error_message")
                    .HasMaxLength(1000);

                entity.Property(e => e.Metadata)
                    .HasColumnName("metadata")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                    );

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(100);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_product_data_user_id");

                entity.HasIndex(e => e.OriginalUrl)
                    .HasDatabaseName("ix_product_data_original_url");

                entity.HasIndex(e => e.ScrapingStatus)
                    .HasDatabaseName("ix_product_data_scraping_status");
            });
        }

        private static void ConfigureProductImageEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("product_images");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ProductDataId)
                    .HasColumnName("product_data_id")
                    .IsRequired();

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.AltText)
                    .HasColumnName("alt_text")
                    .HasMaxLength(200);

                entity.Property(e => e.Width)
                    .HasColumnName("width");

                entity.Property(e => e.Height)
                    .HasColumnName("height");

                entity.Property(e => e.FileSizeBytes)
                    .HasColumnName("file_size_bytes");

                entity.Property(e => e.IsPrimary)
                    .HasColumnName("is_primary")
                    .IsRequired();

                entity.Property(e => e.SortOrder)
                    .HasColumnName("sort_order")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.HasIndex(e => e.ProductDataId)
                    .HasDatabaseName("ix_product_images_product_data_id");

                // Relationship
                entity.HasOne(e => e.ProductData)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.ProductDataId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureProductCategoryEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("product_categories");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(e => e.ParentId)
                    .HasColumnName("parent_id");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .IsRequired();

                entity.Property(e => e.SortOrder)
                    .HasColumnName("sort_order")
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(100);

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("ix_product_categories_name");

                entity.HasIndex(e => e.ParentId)
                    .HasDatabaseName("ix_product_categories_parent_id");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("ix_product_categories_is_active");

                // Self-referencing relationship
                entity.HasOne(e => e.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureSubscriptionPlanEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("subscription_plans");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(e => e.PriceMonthly)
                    .HasColumnName("price_monthly")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.PriceYearly)
                    .HasColumnName("price_yearly")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(e => e.VideoCreditsMonthly)
                    .HasColumnName("video_credits_monthly")
                    .IsRequired();

                entity.Property(e => e.MaxVideoDuration)
                    .HasColumnName("max_video_duration")
                    .IsRequired();

                entity.Property(e => e.MaxResolution)
                    .HasColumnName("max_resolution")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.AiProviders)
                    .HasColumnName("ai_providers")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                    );

                entity.Property(e => e.Features)
                    .HasColumnName("features")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                    );

                entity.Property(e => e.IsPopular)
                    .HasColumnName("is_popular")
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .IsRequired();

                entity.Property(e => e.TrialDays)
                    .HasColumnName("trial_days")
                    .IsRequired();

                entity.Property(e => e.SortOrder)
                    .HasColumnName("sort_order")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(100);

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("ix_subscription_plans_name");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("ix_subscription_plans_is_active");

                entity.HasIndex(e => e.SortOrder)
                    .HasDatabaseName("ix_subscription_plans_sort_order");
            });
        }

        private static void ConfigureUserSubscriptionEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.ToTable("user_subscriptions");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.SubscriptionPlanId)
                    .HasColumnName("subscription_plan_id")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.BillingCycle)
                    .HasColumnName("billing_cycle")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.PricePaid)
                    .HasColumnName("price_paid")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(e => e.StartedAt)
                    .HasColumnName("started_at")
                    .IsRequired();

                entity.Property(e => e.EndsAt)
                    .HasColumnName("ends_at")
                    .IsRequired();

                entity.Property(e => e.CancelledAt)
                    .HasColumnName("cancelled_at");

                entity.Property(e => e.TrialEndsAt)
                    .HasColumnName("trial_ends_at");

                entity.Property(e => e.AutoRenew)
                    .HasColumnName("auto_renew")
                    .IsRequired();

                entity.Property(e => e.PaymentMethodId)
                    .HasColumnName("payment_method_id");

                entity.Property(e => e.StripeSubscriptionId)
                    .HasColumnName("stripe_subscription_id")
                    .HasMaxLength(100);

                entity.Property(e => e.Metadata)
                    .HasColumnName("metadata")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                    );

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(100);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_user_subscriptions_user_id");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("ix_user_subscriptions_status");

                entity.HasIndex(e => e.StripeSubscriptionId)
                    .HasDatabaseName("ix_user_subscriptions_stripe_subscription_id");

                entity.HasIndex(e => e.EndsAt)
                    .HasDatabaseName("ix_user_subscriptions_ends_at");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SubscriptionPlan)
                    .WithMany(p => p.UserSubscriptions)
                    .HasForeignKey(e => e.SubscriptionPlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentMethodId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        // TODO: Uncomment and fix property mismatches when implementing payment functionality
        /*
        private static void ConfigurePaymentMethodEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("payment_methods");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Provider)
                    .HasColumnName("provider")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Last4Digits)
                    .HasColumnName("last_4_digits")
                    .HasMaxLength(4);

                entity.Property(e => e.Brand)
                    .HasColumnName("brand")
                    .HasMaxLength(50);

                entity.Property(e => e.ExpiryMonth)
                    .HasColumnName("expiry_month");

                entity.Property(e => e.ExpiryYear)
                    .HasColumnName("expiry_year");

                entity.Property(e => e.IsDefault)
                    .HasColumnName("is_default")
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .IsRequired();

                entity.Property(e => e.StripePaymentMethodId)
                    .HasColumnName("stripe_payment_method_id")
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_payment_methods_user_id");

                entity.HasIndex(e => e.StripePaymentMethodId)
                    .HasDatabaseName("ix_payment_methods_stripe_payment_method_id");
            });
        }

        private static void ConfigurePaymentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.PaymentMethodId)
                    .HasColumnName("payment_method_id");

                entity.Property(e => e.SubscriptionId)
                    .HasColumnName("subscription_id");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(e => e.StripePaymentIntentId)
                    .HasColumnName("stripe_payment_intent_id")
                    .HasMaxLength(100);

                entity.Property(e => e.StripeChargeId)
                    .HasColumnName("stripe_charge_id")
                    .HasMaxLength(100);

                entity.Property(e => e.ProcessedAt)
                    .HasColumnName("processed_at");

                entity.Property(e => e.RefundedAt)
                    .HasColumnName("refunded_at");

                entity.Property(e => e.FailureReason)
                    .HasColumnName("failure_reason")
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_payments_user_id");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("ix_payments_status");

                entity.HasIndex(e => e.StripePaymentIntentId)
                    .HasDatabaseName("ix_payments_stripe_payment_intent_id");
            });
        }

        private static void ConfigureInvoiceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoices");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.PaymentId)
                    .HasColumnName("payment_id");

                entity.Property(e => e.InvoiceNumber)
                    .HasColumnName("invoice_number")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Subtotal)
                    .HasColumnName("subtotal")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Tax)
                    .HasColumnName("tax")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Total)
                    .HasColumnName("total")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.IssuedAt)
                    .HasColumnName("issued_at")
                    .IsRequired();

                entity.Property(e => e.DueAt)
                    .HasColumnName("due_at");

                entity.Property(e => e.PaidAt)
                    .HasColumnName("paid_at");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_invoices_user_id");

                entity.HasIndex(e => e.InvoiceNumber)
                    .HasDatabaseName("ix_invoices_invoice_number")
                    .IsUnique();

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("ix_invoices_status");
            });
        }

        private static void ConfigureUsageCreditEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsageCredit>(entity =>
            {
                entity.ToTable("usage_credits");

                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.VideoId)
                    .HasColumnName("video_id");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .IsRequired();

                entity.Property(e => e.Reason)
                    .HasColumnName("reason")
                    .HasMaxLength(200);

                entity.Property(e => e.ExpiresAt)
                    .HasColumnName("expires_at");

                entity.Property(e => e.IsUsed)
                    .HasColumnName("is_used")
                    .IsRequired();

                entity.Property(e => e.UsedAt)
                    .HasColumnName("used_at");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_usage_credits_user_id");

                entity.HasIndex(e => e.Type)
                    .HasDatabaseName("ix_usage_credits_type");

                entity.HasIndex(e => e.ExpiresAt)
                    .HasDatabaseName("ix_usage_credits_expires_at");
            });
        }
        */

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql();
            }
            
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
} 