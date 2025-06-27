using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Ajimen.Models;

namespace Ajimen.Data
{
    public static class SeedIdentityUsers // 初期データをデータベースに投入するクラス
    {
        // AppDbContextを受け取り、初期データを追加するメソッド
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user1 = new ApplicationUser
                {
                    UserName = "0001",
                    Email = "aaa@aiueo.com",
                    UserFullName = "谷川大虎",
                    UserRole = "経営者"
                };
                await userManager.CreateAsync(user1, "123qwe");

                var user2 = new ApplicationUser
                {
                    UserName = "0002",
                    Email = "t-nakajima@est.co.jp",
                    UserFullName = "中島朋弥",
                    UserRole = "正社員"
                };
                await userManager.CreateAsync(user2, "789zxc");
                var user3 = new ApplicationUser
                {
                    UserName = "0003",
                    Email = "yanai@est.co.jp",
                    UserFullName = "柳井晴",
                    UserRole = "アルバイト"
                };
                await userManager.CreateAsync(user3, "456asd");
                var user4 = new ApplicationUser
                {
                    UserName = "0004",
                    Email = "ddd@aiueo.com",
                    UserFullName = "有賀颯宇",
                    UserRole = "アルバイト"
                };
                await userManager.CreateAsync(user4, "rtyfgh");
            }
        }
    }
}