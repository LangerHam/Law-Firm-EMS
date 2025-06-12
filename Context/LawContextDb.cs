namespace Law_Firm_EMS.Context
{
    using Law_Firm_EMS.Models;
    using System;
    using System.Data.Entity;
    using System.Linq;

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
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}