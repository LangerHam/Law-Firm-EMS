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

            context.FormTypeEntity.AddOrUpdate(
                s => s.TypeName,
                new FormType { TypeName = "G-28", TemplatePath = "~/Resources/Forms/G-28.pdf" },
                new FormType { TypeName = "G-1145", TemplatePath = "~/Resources/Forms/G-1145.pdf" },
                new FormType { TypeName = "I-140", TemplatePath = "~/Resources/Forms/I-140.pdf" },
                new FormType { TypeName = "I-907", TemplatePath = "~/Resources/Forms/I-907.pdf" },
                new FormType { TypeName = "ETA 750 Part B", TemplatePath = "~/Resources/Forms/ETA750B.pdf" });

            context.StatusTypeEntity.AddOrUpdate(
                s => s.StatusName,
                new StatusType { StatusName = "Pending" },
                new StatusType { StatusName = "Approved" },
                new StatusType { StatusName = "Rejected" },
                new StatusType { StatusName = "In Progress" },
                new StatusType { StatusName = "Completed" },
                new StatusType { StatusName = "Submitted"},
                new StatusType { StatusName = "Not Submitted"});

            context.RoleEntity.AddOrUpdate(
                s => s.RoleName,
                new Role { RoleName = "Admin" },
                new Role { RoleName = "Client" },
                new Role { RoleName = "Consultant" });
        }
    }
}
