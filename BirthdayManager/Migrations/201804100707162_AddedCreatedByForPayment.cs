namespace BirthdayManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCreatedByForPayment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MoneyTransactions", "CreatedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MoneyTransactions", "CreatedBy");
        }
    }
}
