using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Models.Dtos;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifService;

        public NotificationController(INotificationService notifService)
        {
            _notifService = notifService;
        }

        [HttpGet("recipient/{recipientId:int}")]
        public async Task<IActionResult> GetByRecipient([FromRoute] int recipientId)
        {
            var notifications = await _notifService.GetByRecipient(recipientId);
            return Ok(notifications);
        }

        [HttpPut("read/{notificationId:int}")]
        public async Task<IActionResult> MarkAsRead([FromRoute] int notificationId)
        {
            await _notifService.MarkAsRead(notificationId);
            return NoContent();
        }

        [HttpPut("read-all/{recipientId:int}")]
        public async Task<IActionResult> MarkAllRead([FromRoute] int recipientId)
        {
            await _notifService.MarkAllRead(recipientId);
            return NoContent();
        }

        [HttpGet("unread-count/{recipientId:int}")]
        public async Task<IActionResult> GetUnreadCount([FromRoute] int recipientId)
        {
            var unreadCount = await _notifService.GetUnreadCount(recipientId);
            return Ok(new { recipientId, unreadCount });
        }

        [HttpDelete("{notificationId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int notificationId)
        {
            await _notifService.DeleteNotification(notificationId);
            return NoContent();
        }

        [HttpPost("send-bulk")]
        public async Task<IActionResult> SendBulk([FromBody] SendBulkRequest request)
        {
            await _notifService.SendBulk(request.RecipientIds, request.Title, request.Message);
            return Accepted(new { message = "Bulk notifications queued for dispatch." });
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            await _notifService.Send(new Notification
            {
                RecipientId = request.RecipientId,
                Type = request.Type,
                Title = request.Title,
                Message = request.Message,
                Channel = request.Channel,
                RelatedId = request.RelatedId,
                RelatedType = request.RelatedType
            });

            return Accepted(new { message = "Notification sent." });
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            await _notifService.SendEmail(request.Email, request.Subject, request.Body);
            return Accepted(new { message = "Email sent." });
        }

        [HttpPost("send-sms")]
        public async Task<IActionResult> SendSms([FromBody] SendSmsRequest request)
        {
            await _notifService.SendSms(request.PhoneNumber, request.Body);
            return Accepted(new { message = "SMS sent." });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _notifService.GetAll();
            return Ok(notifications);
        }
    }
}
