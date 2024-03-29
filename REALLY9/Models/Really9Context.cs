﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace REALLY9.Models;

public partial class Really9Context : DbContext
{
    public Really9Context()
    {
    }

    public Really9Context(DbContextOptions<Really9Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Attribute> Attributes { get; set; }

    public virtual DbSet<AttributesPrice> AttributesPrices { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Shipper> Shippers { get; set; }

    public virtual DbSet<TblTinTuc> TblTinTucs { get; set; }

    public virtual DbSet<TranscatStatus> TranscatStatuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-BAOC1O62\\SQLEXPRESS;Database=REALLY9;Integrated Security=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Fullname).HasMaxLength(150);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Salt)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Accounts_Roles");
        });

        modelBuilder.Entity<Attribute>(entity =>
        {
            entity.HasKey(e => e.AttributeId).HasName("PK_Attribute");

            entity.Property(e => e.AttributeId).HasColumnName("AttributeID");
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<AttributesPrice>(entity =>
        {
            entity.ToTable("AttributesPrice");

            entity.Property(e => e.AttributesPriceId).HasColumnName("AttributesPriceID");
            entity.Property(e => e.AttributeId).HasColumnName("AttributeID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Attribute).WithMany(p => p.AttributesPrices)
                .HasForeignKey(d => d.AttributeId)
                .HasConstraintName("FK_AttributesPrice_Attributes");

            entity.HasOne(d => d.Product).WithMany(p => p.AttributesPrices)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_AttributesPrice_Products");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CatId);

            entity.Property(e => e.CatId).HasColumnName("CatID");
            entity.Property(e => e.Alias).HasMaxLength(250);
            entity.Property(e => e.CatName).HasMaxLength(250);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.Thumb).HasMaxLength(250);
            entity.Property(e => e.Title).HasMaxLength(250);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comment");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.CommentText).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Comments)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Comment_Customers");

            entity.HasOne(d => d.Product).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Comment_Products");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.Avatar).HasMaxLength(250);
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsFixedLength();
            entity.Property(e => e.FullName).HasMaxLength(250);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.NextGameTime).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.PasswordReset).HasMaxLength(500);
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.ResetTokenEx).HasMaxLength(500);
            entity.Property(e => e.Salt)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.VerifiAt).HasColumnType("datetime");

            entity.HasOne(d => d.Location).WithMany(p => p.Customers)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK_Customers_Locations");
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.Property(e => e.DiscountId).HasColumnName("DiscountID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Discountcode).HasMaxLength(50);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.NameWithType).HasMaxLength(250);
            entity.Property(e => e.PathWithType).HasMaxLength(250);
            entity.Property(e => e.Slug).HasMaxLength(250);
            entity.Property(e => e.Type).HasMaxLength(20);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Message");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TimeStamp).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_Message_Accounts");

            entity.HasOne(d => d.Customer).WithMany(p => p.Messages)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Message_Customers");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DiscountId).HasColumnName("DiscountID");
            entity.Property(e => e.Orderdate).HasColumnType("datetime");
            entity.Property(e => e.PaymentDay).HasColumnType("datetime");
            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.ShipDate).HasColumnType("datetime");
            entity.Property(e => e.TransactStatusId).HasColumnName("TransactStatusID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Orders_Customers");

            entity.HasOne(d => d.Discount).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DiscountId)
                .HasConstraintName("FK_Orders_Discounts");

            entity.HasOne(d => d.TransactStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.TransactStatusId)
                .HasConstraintName("FK_Orders_TranscatStatus");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ShipDate).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_OrderDetails_Products");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.Property(e => e.PageId).HasColumnName("PageID");
            entity.Property(e => e.Alias).HasMaxLength(250);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PageName).HasMaxLength(250);
            entity.Property(e => e.Thumb).HasMaxLength(250);
            entity.Property(e => e.Title).HasMaxLength(250);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Alias).HasMaxLength(250);
            entity.Property(e => e.CatId).HasColumnName("CatID");
            entity.Property(e => e.Comment).HasMaxLength(100);
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.DateModified).HasColumnType("datetime");
            entity.Property(e => e.ProductName).HasMaxLength(250);
            entity.Property(e => e.ShortDesc).HasMaxLength(250);
            entity.Property(e => e.Thumb).HasMaxLength(250);
            entity.Property(e => e.Title).HasMaxLength(250);
            entity.Property(e => e.Video).HasMaxLength(250);

            entity.HasOne(d => d.Cat).WithMany(p => p.Products)
                .HasForeignKey(d => d.CatId)
                .HasConstraintName("FK_Products_Categories1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Shipper>(entity =>
        {
            entity.Property(e => e.ShipperId).HasColumnName("ShipperID");
            entity.Property(e => e.Company).HasMaxLength(150);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.ShipDate).HasColumnType("datetime");
            entity.Property(e => e.ShipperName).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.Shippers)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Shippers_Orders");
        });

        modelBuilder.Entity<TblTinTuc>(entity =>
        {
            entity.HasKey(e => e.PostId);

            entity.ToTable("tblTinTucs");

            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Alias).HasMaxLength(250);
            entity.Property(e => e.Author).HasMaxLength(250);
            entity.Property(e => e.CatlId).HasColumnName("CatlID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.IsHot).HasColumnName("isHot");
            entity.Property(e => e.IsNewfeed).HasColumnName("isNewfeed");
            entity.Property(e => e.Scontents)
                .HasMaxLength(250)
                .HasColumnName("SContents");
            entity.Property(e => e.Thumb).HasMaxLength(250);
            entity.Property(e => e.Title).HasMaxLength(250);
        });

        modelBuilder.Entity<TranscatStatus>(entity =>
        {
            entity.ToTable("TranscatStatus");

            entity.Property(e => e.TranscatStatusId).HasColumnName("TranscatStatusID");
            entity.Property(e => e.Status).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
