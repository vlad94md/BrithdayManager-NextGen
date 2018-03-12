namespace BirthdayManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDomainModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DayOfBirth", c => c.Byte(nullable: false));
            AddColumn("dbo.AspNetUsers", "MonthOfBirth", c => c.Byte(nullable: false));
            AddColumn("dbo.AspNetUsers", "Count", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.AspNetUsers", "Location", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Location");
            DropColumn("dbo.AspNetUsers", "Count");
            DropColumn("dbo.AspNetUsers", "MonthOfBirth");
            DropColumn("dbo.AspNetUsers", "DayOfBirth");
        }
    }
}
