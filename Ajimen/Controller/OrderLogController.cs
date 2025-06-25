using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Ajimen.Data;
using Ajimen.Models;
using System.Collections.Generic;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class OrderLogController : ControllerBase
{
    private readonly AppDbContext _context;
    public OrderLogController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("submitandmail")]
    public IActionResult SubmitAndMail([FromBody] List<OrderLog> orderList)
    {
        if (orderList == null || !orderList.Any())
            return BadRequest("発注内容が空です");

        try
        {
            // データベース保存
            foreach (var order in orderList)
            {
                order.OrderDay = DateTime.Now;
                order.useOrderLog = DateTime.Now;
                _context.OrderLogs.Add(order);
            }
            _context.SaveChanges();

            // メール本文作成
            var body = "以下の商品を発注します：\n\n";
            foreach (var o in orderList)
            {
                body += $"商品コード: {o.ItemId}, 物品名: {o.ItemName}, 発注数: {o.OrderNum}\n";
            }
            var from = "gokubosou2000@gmail.com";    // 送信元
            var to = "kimigainakunattahibi@yahoo.co.jp";        // 送信先
            var subject = "【自動送信】発注内容";

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("gokubosou2000@gmail.com", "riyz xgnl njip aryy"),
                EnableSsl = true,
            };
            var message = new MailMessage(from, to, subject, body);

            smtp.Send(message);

            return Ok("保存＆メール送信完了");
        }
        catch (Exception ex)
        {
            return BadRequest("メール送信失敗: " + ex.Message);
        }
    }
}
