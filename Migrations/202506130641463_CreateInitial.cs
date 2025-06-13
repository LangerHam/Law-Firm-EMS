namespace Law_Firm_EMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateInitial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Billings",
                c => new
                    {
                        ClientID = c.Int(nullable: false),
                        TotalFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaidAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RemainingAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ClientID)
                .ForeignKey("dbo.Clients", t => t.ClientID)
                .Index(t => t.ClientID);
            
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Phone = c.String(maxLength: 20),
                        CaseDetails = c.String(),
                        StatusID = c.Int(nullable: false),
                        AssignedConsultantID = c.Int(),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Consultants", t => t.AssignedConsultantID)
                .ForeignKey("dbo.StatusTypes", t => t.StatusID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.StatusID)
                .Index(t => t.AssignedConsultantID);
            
            CreateTable(
                "dbo.Consultants",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Phone = c.String(maxLength: 20),
                        ProfilePhotoPath = c.String(),
                        HRID = c.Int(),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.HRs", t => t.HRID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.HRID);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        DocumentID = c.Int(nullable: false, identity: true),
                        Instructions = c.String(),
                        AssignedToConsultantID = c.Int(nullable: false),
                        AssignedByHRID = c.Int(nullable: false),
                        StatusID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DocumentID)
                .ForeignKey("dbo.HRs", t => t.AssignedByHRID)
                .ForeignKey("dbo.Consultants", t => t.AssignedToConsultantID)
                .ForeignKey("dbo.StatusTypes", t => t.StatusID, cascadeDelete: true)
                .Index(t => t.AssignedToConsultantID)
                .Index(t => t.AssignedByHRID)
                .Index(t => t.StatusID);
            
            CreateTable(
                "dbo.HRs",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.Leaves",
                c => new
                    {
                        LeaveID = c.Int(nullable: false, identity: true),
                        Type = c.String(nullable: false, maxLength: 50),
                        FromDate = c.DateTime(nullable: false),
                        ToDate = c.DateTime(nullable: false),
                        RequestedByConsultantID = c.Int(nullable: false),
                        ApprovedByHRID = c.Int(),
                        StatusID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LeaveID)
                .ForeignKey("dbo.HRs", t => t.ApprovedByHRID)
                .ForeignKey("dbo.Consultants", t => t.RequestedByConsultantID, cascadeDelete: true)
                .ForeignKey("dbo.StatusTypes", t => t.StatusID, cascadeDelete: true)
                .Index(t => t.RequestedByConsultantID)
                .Index(t => t.ApprovedByHRID)
                .Index(t => t.StatusID);
            
            CreateTable(
                "dbo.StatusTypes",
                c => new
                    {
                        StatusID = c.Int(nullable: false, identity: true),
                        StatusName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.StatusID);
            
            CreateTable(
                "dbo.Evaluations",
                c => new
                    {
                        EvalID = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Details = c.String(),
                        DateGiven = c.DateTime(nullable: false),
                        ConsultantID = c.Int(nullable: false),
                        EvaluatedByHRID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EvalID)
                .ForeignKey("dbo.Consultants", t => t.ConsultantID, cascadeDelete: true)
                .ForeignKey("dbo.HRs", t => t.EvaluatedByHRID, cascadeDelete: true)
                .Index(t => t.ConsultantID)
                .Index(t => t.EvaluatedByHRID);
            
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        InvoiceID = c.Int(nullable: false, identity: true),
                        InvoiceDate = c.DateTime(nullable: false),
                        ExportPath = c.String(),
                        SalaryPaid = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ClientPaymentReceived = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CompanyProfit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ClientID = c.Int(nullable: false),
                        ConsultantID = c.Int(nullable: false),
                        GeneratedByHRID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InvoiceID)
                .ForeignKey("dbo.Clients", t => t.ClientID)
                .ForeignKey("dbo.Consultants", t => t.ConsultantID)
                .ForeignKey("dbo.HRs", t => t.GeneratedByHRID)
                .Index(t => t.ClientID)
                .Index(t => t.ConsultantID)
                .Index(t => t.GeneratedByHRID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false, maxLength: 256),
                        PasswordHash = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        RoleID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Roles", t => t.RoleID, cascadeDelete: true)
                .Index(t => t.Email, unique: true)
                .Index(t => t.RoleID);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        RoleName = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.RoleId);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        DocumentID = c.Int(nullable: false),
                        UploadPath = c.String(nullable: false),
                        ClientID = c.Int(nullable: false),
                        UploadedByUserID = c.Int(nullable: false),
                        DocumentTypeID = c.Int(nullable: false),
                        StatusID = c.Int(nullable: false),
                        ParentDocumentID = c.Int(),
                    })
                .PrimaryKey(t => t.DocumentID)
                .ForeignKey("dbo.Clients", t => t.ClientID, cascadeDelete: true)
                .ForeignKey("dbo.DocumentTypes", t => t.DocumentTypeID, cascadeDelete: true)
                .ForeignKey("dbo.Documents", t => t.ParentDocumentID)
                .ForeignKey("dbo.StatusTypes", t => t.StatusID, cascadeDelete: true)
                .ForeignKey("dbo.Tasks", t => t.DocumentID)
                .ForeignKey("dbo.Users", t => t.UploadedByUserID)
                .Index(t => t.DocumentID)
                .Index(t => t.ClientID)
                .Index(t => t.UploadedByUserID)
                .Index(t => t.DocumentTypeID)
                .Index(t => t.StatusID)
                .Index(t => t.ParentDocumentID);
            
            CreateTable(
                "dbo.DocumentTypes",
                c => new
                    {
                        DocumentTypeID = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false, maxLength: 50),
                        TemplatePath = c.String(),
                    })
                .PrimaryKey(t => t.DocumentTypeID);
            
            CreateTable(
                "dbo.Forms",
                c => new
                    {
                        FormID = c.Int(nullable: false, identity: true),
                        UploadPath = c.String(nullable: false),
                        ClientID = c.Int(nullable: false),
                        FormTypeID = c.Int(nullable: false),
                        StatusID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FormID)
                .ForeignKey("dbo.Clients", t => t.ClientID, cascadeDelete: true)
                .ForeignKey("dbo.FormTypes", t => t.FormTypeID, cascadeDelete: true)
                .ForeignKey("dbo.StatusTypes", t => t.StatusID, cascadeDelete: true)
                .Index(t => t.ClientID)
                .Index(t => t.FormTypeID)
                .Index(t => t.StatusID);
            
            CreateTable(
                "dbo.FormTypes",
                c => new
                    {
                        FormTypeID = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false, maxLength: 100),
                        TemplatePath = c.String(),
                    })
                .PrimaryKey(t => t.FormTypeID);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        TransactionID = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentDate = c.DateTime(nullable: false),
                        BillingID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionID)
                .ForeignKey("dbo.Billings", t => t.BillingID, cascadeDelete: true)
                .Index(t => t.BillingID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "BillingID", "dbo.Billings");
            DropForeignKey("dbo.Billings", "ClientID", "dbo.Clients");
            DropForeignKey("dbo.Clients", "UserID", "dbo.Users");
            DropForeignKey("dbo.Clients", "StatusID", "dbo.StatusTypes");
            DropForeignKey("dbo.Forms", "StatusID", "dbo.StatusTypes");
            DropForeignKey("dbo.Forms", "FormTypeID", "dbo.FormTypes");
            DropForeignKey("dbo.Forms", "ClientID", "dbo.Clients");
            DropForeignKey("dbo.Consultants", "UserID", "dbo.Users");
            DropForeignKey("dbo.Tasks", "StatusID", "dbo.StatusTypes");
            DropForeignKey("dbo.Documents", "UploadedByUserID", "dbo.Users");
            DropForeignKey("dbo.Documents", "DocumentID", "dbo.Tasks");
            DropForeignKey("dbo.Documents", "StatusID", "dbo.StatusTypes");
            DropForeignKey("dbo.Documents", "ParentDocumentID", "dbo.Documents");
            DropForeignKey("dbo.Documents", "DocumentTypeID", "dbo.DocumentTypes");
            DropForeignKey("dbo.Documents", "ClientID", "dbo.Clients");
            DropForeignKey("dbo.Tasks", "AssignedToConsultantID", "dbo.Consultants");
            DropForeignKey("dbo.Tasks", "AssignedByHRID", "dbo.HRs");
            DropForeignKey("dbo.HRs", "UserID", "dbo.Users");
            DropForeignKey("dbo.Users", "RoleID", "dbo.Roles");
            DropForeignKey("dbo.Consultants", "HRID", "dbo.HRs");
            DropForeignKey("dbo.Invoices", "GeneratedByHRID", "dbo.HRs");
            DropForeignKey("dbo.Invoices", "ConsultantID", "dbo.Consultants");
            DropForeignKey("dbo.Invoices", "ClientID", "dbo.Clients");
            DropForeignKey("dbo.Evaluations", "EvaluatedByHRID", "dbo.HRs");
            DropForeignKey("dbo.Evaluations", "ConsultantID", "dbo.Consultants");
            DropForeignKey("dbo.Leaves", "StatusID", "dbo.StatusTypes");
            DropForeignKey("dbo.Leaves", "RequestedByConsultantID", "dbo.Consultants");
            DropForeignKey("dbo.Leaves", "ApprovedByHRID", "dbo.HRs");
            DropForeignKey("dbo.Clients", "AssignedConsultantID", "dbo.Consultants");
            DropIndex("dbo.Transactions", new[] { "BillingID" });
            DropIndex("dbo.Forms", new[] { "StatusID" });
            DropIndex("dbo.Forms", new[] { "FormTypeID" });
            DropIndex("dbo.Forms", new[] { "ClientID" });
            DropIndex("dbo.Documents", new[] { "ParentDocumentID" });
            DropIndex("dbo.Documents", new[] { "StatusID" });
            DropIndex("dbo.Documents", new[] { "DocumentTypeID" });
            DropIndex("dbo.Documents", new[] { "UploadedByUserID" });
            DropIndex("dbo.Documents", new[] { "ClientID" });
            DropIndex("dbo.Documents", new[] { "DocumentID" });
            DropIndex("dbo.Users", new[] { "RoleID" });
            DropIndex("dbo.Users", new[] { "Email" });
            DropIndex("dbo.Invoices", new[] { "GeneratedByHRID" });
            DropIndex("dbo.Invoices", new[] { "ConsultantID" });
            DropIndex("dbo.Invoices", new[] { "ClientID" });
            DropIndex("dbo.Evaluations", new[] { "EvaluatedByHRID" });
            DropIndex("dbo.Evaluations", new[] { "ConsultantID" });
            DropIndex("dbo.Leaves", new[] { "StatusID" });
            DropIndex("dbo.Leaves", new[] { "ApprovedByHRID" });
            DropIndex("dbo.Leaves", new[] { "RequestedByConsultantID" });
            DropIndex("dbo.HRs", new[] { "UserID" });
            DropIndex("dbo.Tasks", new[] { "StatusID" });
            DropIndex("dbo.Tasks", new[] { "AssignedByHRID" });
            DropIndex("dbo.Tasks", new[] { "AssignedToConsultantID" });
            DropIndex("dbo.Consultants", new[] { "HRID" });
            DropIndex("dbo.Consultants", new[] { "UserID" });
            DropIndex("dbo.Clients", new[] { "AssignedConsultantID" });
            DropIndex("dbo.Clients", new[] { "StatusID" });
            DropIndex("dbo.Clients", new[] { "UserID" });
            DropIndex("dbo.Billings", new[] { "ClientID" });
            DropTable("dbo.Transactions");
            DropTable("dbo.FormTypes");
            DropTable("dbo.Forms");
            DropTable("dbo.DocumentTypes");
            DropTable("dbo.Documents");
            DropTable("dbo.Roles");
            DropTable("dbo.Users");
            DropTable("dbo.Invoices");
            DropTable("dbo.Evaluations");
            DropTable("dbo.StatusTypes");
            DropTable("dbo.Leaves");
            DropTable("dbo.HRs");
            DropTable("dbo.Tasks");
            DropTable("dbo.Consultants");
            DropTable("dbo.Clients");
            DropTable("dbo.Billings");
        }
    }
}
