namespace Law_Firm_EMS.Context
{
    using Law_Firm_EMS.Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public class LawContextDb : DbContext
    {
        // Your context has been configured to use a 'LawContextDb' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'Law_Firm_EMS.Context.LawContextDb' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'LawContextDb' 
        // connection string in the application configuration file.
        public LawContextDb()
            : base("name=LawContextDb")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<Users> UsersEntity { get; set; }
        public virtual DbSet<Billing> BillingEntity { get; set; }
        public virtual DbSet<Consultant> ConsultantEntity { get; set; }
        public virtual DbSet<Client> ClientEntity { get; set; }
        public virtual DbSet<Document> DocumentEntity { get; set; }
        public virtual DbSet<DocumentType> DocumentTypeEntity { get; set; }
        public virtual DbSet<Evaluation> EvaluationEntity { get; set; }
        public virtual DbSet<Form> FormEntity { get; set; }
        public virtual DbSet<FormType> FormTypeEntity { get; set; }
        public virtual DbSet<HR> HREntity { get; set; }
        public virtual DbSet<Invoice> InvoiceEntity { get; set; }
        public virtual DbSet<Leave> LeaveEntity { get; set; }
        public virtual DbSet<Role> RoleEntity { get; set; }
        public virtual DbSet<StatusType> StatusTypeEntity { get; set; }
        public virtual DbSet<Tasks> TasksEntity { get; set; }
        public virtual DbSet<Transaction> TransactionEntity { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>()
                .HasRequired(d => d.UploadedByUser)
                .WithMany()
                .HasForeignKey(d => d.UploadedByUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tasks>()
                .HasRequired(t => t.AssignedByHR)
                .WithMany(h => h.AssignedTasks)
                .HasForeignKey(t => t.AssignedByHRID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tasks>()
                .HasRequired(t => t.AssignedToConsultant)
                .WithMany(c => c.AssignedTasks)
                .HasForeignKey(t => t.AssignedToConsultantID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Invoice>()
                .HasRequired(i => i.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ClientID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Invoice>()
                .HasRequired(i => i.Consultant)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ConsultantID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Invoice>()
                .HasRequired(i => i.GeneratedByHR)
                .WithMany(h => h.GeneratedInvoices)
                .HasForeignKey(i => i.GeneratedByHRID)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}