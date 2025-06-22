namespace Law_Firm_EMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixBillingComputedColumn : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Billings", "RemainingAmount");
            Sql("ALTER TABLE dbo.Billings ADD RemainingAmount AS ([TotalFees] - [PaidAmount])");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Billings", "RemainingAmount");
            AddColumn("dbo.Billings", "RemainingAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
