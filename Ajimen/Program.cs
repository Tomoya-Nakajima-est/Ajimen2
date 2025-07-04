﻿using Microsoft.EntityFrameworkCore;
using Ajimen.Data;
using Ajimen.Models;
using Microsoft.AspNetCore.Identity;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 3;
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // 🔧 SQLiteデータベースを使用するように設定（app.db というファイルに保存される）
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=app.db"));

        // REST APIのための各種サービスを登録
        builder.Services.AddControllers();             // コントローラー機能
        builder.Services.AddEndpointsApiExplorer();    // SwaggerでAPI情報表示のため
        builder.Services.AddSwaggerGen();              // Swagger UIの自動生成

        // 🔓 CORSポリシーの定義：Vue.js（http://localhost:5173）からのアクセスを許可、vueのURLのポート番号
        //builder.Services.AddCors(options =>  ローカル用
        //{
        //    options.AddPolicy("AllowVueApp",
        //        policy => policy.WithOrigins("http://localhost:5173")
        //                        .AllowAnyHeader()
        //                        .AllowAnyMethod());
        //});

        builder.Services.AddCors(options =>   //公開用
        {
            options.AddPolicy("AllowVueApp",
                policy => policy
                    .AllowAnyOrigin()      // 本番では使わないこと！
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowVueApp");

        app.MapControllers();

        // DB初期化（マイグレーション不要、テーブル作成＆初期データ登録）
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedItem.Initialize(db);
            SeedOrderLog.Initialize(db);
            SeedShift.Initialize(db);
            SeedAttendance.Initialize(db);
            // 👇これを追加！！
            SeedStockLog.Initialize(db);

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedIdentityUsers.InitializeAsync(userManager); // 非同期で初期ユーザー登録
        }

        app.Run();
    }

}
//ちくわ
//ちくわ2