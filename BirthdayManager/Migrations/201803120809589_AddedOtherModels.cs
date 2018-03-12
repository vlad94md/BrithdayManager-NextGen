namespace BirthdayManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOtherModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Arrangements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                        GiftPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GiftDescription = c.String(),
                        Birthday = c.DateTime(nullable: false),
                        IsComplete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId, cascadeDelete: true)
                .Index(t => t.ApplicationUserId);
            
            CreateTable(
                "dbo.Subscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                        ArrangementId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Arrangements", t => t.ArrangementId, cascadeDelete: false)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId, cascadeDelete: false)
                .Index(t => t.ApplicationUserId)
                .Index(t => t.ArrangementId);
            
            CreateTable(
                "dbo.MoneyTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        Date = c.DateTime(nullable: false),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId, cascadeDelete: true)
                .Index(t => t.ApplicationUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MoneyTransactions", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Subscriptions", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Subscriptions", "ArrangementId", "dbo.Arrangements");
            DropForeignKey("dbo.Arrangements", "ApplicationUserId", "dbo.AspNetUsers");
            DropIndex("dbo.MoneyTransactions", new[] { "ApplicationUserId" });
            DropIndex("dbo.Subscriptions", new[] { "ArrangementId" });
            DropIndex("dbo.Subscriptions", new[] { "ApplicationUserId" });
            DropIndex("dbo.Arrangements", new[] { "ApplicationUserId" });
            DropTable("dbo.MoneyTransactions");
            DropTable("dbo.Subscriptions");
            DropTable("dbo.Arrangements");
        }
    }
}
