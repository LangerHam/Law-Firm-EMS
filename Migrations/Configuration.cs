namespace Law_Firm_EMS.Migrations
{
    using Law_Firm_EMS.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Law_Firm_EMS.Context.LawContextDb>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Law_Firm_EMS.Context.LawContextDb context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            context.DocumentTypeEntity.AddOrUpdate(
          s => s.TypeName, 
          new DocumentType { TypeName = "LoR" },
          new DocumentType { TypeName = "EIA" },
          new DocumentType { TypeName = "PES" });
        }
    }
}
