using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using REALLY9.Models;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using REALLY9.Services;
using System.Configuration;
using REALLY9.Hubs;
using REALLY9.ModelViews;

internal class Program
{
    private static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var stringconnectDb = builder.Configuration.GetConnectionString("dbREALLY9");
        builder.Services.AddDbContext<Really9Context>(options => options.UseSqlServer(stringconnectDb));
        builder.Services.AddControllersWithViews();
        builder.Services.AddSession();

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddHttpClient();
        builder.Services.AddSignalR();
        





        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(p =>
            {
                p.LoginPath = "/dang-nhap.html";
                p.AccessDeniedPath = "/";
            });
        builder.Services.AddAuthentication("AdminScheme").AddCookie("AdminScheme",p =>
        {
            p.LoginPath = "/admin-dang-nhap.html";
            p.AccessDeniedPath = "/";
        });
        builder.Services.AddRazorPages().AddNToastNotifyToastr(new NToastNotify.ToastrOptions()
        {
            ProgressBar = true,
            PositionClass = ToastPositions.TopRight,
            PreventDuplicates = true,
            CloseButton = true,
            TimeOut = 5000,
        });

            
        builder.Services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseNToastNotify();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseSession();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chatHub");
            endpoints.MapControllerRoute(
              name: "areas",
              pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );
            endpoints.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}"

            );
            
        });

        app.Run();
    }
}