using CompressionForce.Data.Entities;
using CompressionForce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompressionForce.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ================= CORE TABLES =================
        public virtual DbSet<AlarmLog> AlarmLogs { get; set; }
        public virtual DbSet<AuditTrail> AuditTrails { get; set; }
        public virtual DbSet<BatchEntity> Batches { get; set; }
        public virtual DbSet<BatchHistoryEntity> BatchHistories { get; set; }
        public virtual DbSet<CurrentBatchEntity> CurrentBatches { get; set; }

        // ================= PRIVILEGES =================
        public virtual DbSet<Privilage> Privilages { get; set; }
        public virtual DbSet<PrivilageHistory> PrivilageHistories { get; set; }

        // ================= RECIPES =================
        public DbSet<RecipeEntity> Recipes => Set<RecipeEntity>();
    public DbSet<RecipeHistoryEntity> RecipeHistories => Set<RecipeHistoryEntity>();
    public DbSet<LookupValueEntity> LookupValues => Set<LookupValueEntity>();
        public DbSet<RecipeEntity> RecipesforBatches { get; set; }

        // ================= RESULTS =================
        public virtual DbSet<ResultEjectLoadS1B> ResultEjectLoadS1Bs { get; set; }
    public virtual DbSet<ResultEjectLoadS2B> ResultEjectLoadS2Bs { get; set; }
    public virtual DbSet<ResultMainLoadS1B> ResultMainLoadS1Bs { get; set; }
    public virtual DbSet<ResultMainLoadS2B> ResultMainLoadS2Bs { get; set; }
    public virtual DbSet<ResultMainSrelS1B> ResultMainSrelS1Bs { get; set; }
    public virtual DbSet<ResultMainSrelS2B> ResultMainSrelS2Bs { get; set; }
    public virtual DbSet<ResultPreLoadS1B> ResultPreLoadS1Bs { get; set; }
    public virtual DbSet<ResultPreLoadS2B> ResultPreLoadS2Bs { get; set; }

        // ================= CALIBRATION =================

        public virtual DbSet<ServoCalibration> ServoCalibrations { get; set; }
        public DbSet<LoadCell> LoadCells { get; set; }
        public DbSet<LoadCellCalibration> LoadCellCalibrations { get; set; }

        // ================= USERS & SECURITY =================

        public virtual DbSet<UserLogin> UserLogins { get; set; }
    public virtual DbSet<UserLoginHistroy> UserLoginHistroys { get; set; }
    public virtual DbSet<UserManagement> UserManagements { get; set; }
    public virtual DbSet<UserManagementHistroy> UserManagementHistroys { get; set; }
    public virtual DbSet<UserSetting> UserSettings { get; set; }
    public DbSet<SecuritySettings> SecuritySettings { get; set; }

        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<GroupPrivilege> GroupPrivileges { get; set; }

        // ================= PLC STATUS =================
        public DbSet<PlcStatus> PlcStatuses { get; set; }


        // ================= MODEL CONFIGURATION =================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- JSONB MAPPINGS ----------
            modelBuilder.Entity<RecipeEntity>()
                .Property(r => r.Parameters)
                .HasColumnType("jsonb");

            modelBuilder.Entity<RecipeHistoryEntity>()
                .Property(r => r.OldParameters)
                .HasColumnType("jsonb")
                .IsRequired(false);

            modelBuilder.Entity<RecipeHistoryEntity>()
                .Property(r => r.NewParameters)
                .HasColumnType("jsonb")
                .IsRequired(false);

            modelBuilder.Entity<CurrentBatchEntity>()
                .Property(e => e.Parameters)
                .HasColumnType("jsonb");

            // Call partial method for any additional configurations
            OnModelCreatingPartial(modelBuilder);

            // ---------- PLC STATUS MAPPING (CRITICAL FIX) ----------
            modelBuilder.Entity<PlcStatus>(entity =>
            {
                entity.ToTable("plc_status", "public");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.PlcIp)
                      .HasMaxLength(50);

                entity.Property(e => e.LastUpdated)
                      .IsRequired();

                entity.Property(e => e.PlcHeartbeat)
                      .IsRequired();

                entity.Property(e => e.IsLocalDbConnected)
                      .IsRequired();

                entity.Property(e => e.IsPlcConnected)
                      .IsRequired();
            });

            // ---------- PARTIAL CONFIGS ----------
            OnModelCreatingPartial(modelBuilder);
    }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
