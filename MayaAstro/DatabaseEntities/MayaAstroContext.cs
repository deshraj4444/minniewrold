using Microsoft.EntityFrameworkCore;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;

namespace MayaAstro.DatabaseEntities
{
    public class MayaAstroContext:DbContext
    {
        public MayaAstroContext(DbContextOptions<MayaAstroContext> options) : base(options)
        {

        }
        public DbSet<Contacts> Contacts { get; set; }
        public DbSet<BlogDetail> BlogDetail { get; set; }
        public DbSet<BlogCategory> BlogCategory { get; set; }
        public DbSet<Horoscope> Horoscope { get; set; }
        public DbSet<Rashi> Rashi { get; set; }
        public DbSet<Product> ProductCategory { get; set; }
        public DbSet<ProductDetail> ProductDetail { get; set; }
        public DbSet<ProductSubCategory> ProductSubCategory { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserWebsite> UserWebsite { get; set; }
        public DbSet<Website> Website { get; set; }
        public DbSet<LectureCategory> LectureCategory { get; set; }
        public DbSet<LectureDetail> LectureDetail { get; set; }
        public DbSet<DeviceToken> DeviceToken { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<QuestionDetail> QuestionDetail { get; set; }
        public DbSet<QuickLearn> QuickLearn { get; set; }
        public DbSet<ContentMaster> ContentMaster { get; set; }
        public DbSet<MessageBannerDetail> MessageBannerDetail { get; set; }
        public DbSet<BankDetail> BankDetail { get; set; }
        public DbSet<CommonEnquiry> CommonEnquiry { get; set; }
        public DbSet<GoldPriceData> GoldPriceData { get; set; }
        public DbSet<Announcements> Announcements { get; set; }

        public DbSet<GoldSettings> GoldSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToSnakeCase());

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(property.Name.ToSnakeCase());

                foreach (var key in entity.GetKeys())
                    key.SetName(key.GetName().ToSnakeCase());

                foreach (var key in entity.GetForeignKeys())
                    key.SetConstraintName(key.GetConstraintName().ToSnakeCase());

                foreach (var index in entity.GetIndexes())
                    index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
            }

        }
    }
}





