namespace BirthdayManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameBalance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Balance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.AspNetUsers", "Count");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Count", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.AspNetUsers", "Balance");
        }
    }
}
