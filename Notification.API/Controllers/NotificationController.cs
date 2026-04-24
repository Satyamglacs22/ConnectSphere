using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.API.DTOs;
using Notification.API.Entities;
using Notification.API.Services.Interfaces;

namespace Notification.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifService;

        public NotificationController(INotificationService notifService)
        {
            _notifService = notifService;
        }

        // GET /api/notifications/{userId}
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(int userId)
        {
            var notifications = await _notifService.GetByRecipient(userId);
            return Ok(notifications);
        }

        // GET /api/notifications/{userId}/unread
        [Authorize]
        [HttpGet("{userId}/unread")]
        public async Task<IActionResult> GetUnread(int userId)
        {
            var notifications = await _notifService.GetUnread(userId);
            return Ok(notifications);
        }

        // GET /api/notifications/{userId}/unreadCount
        [Authorize]
        [HttpGet("{userId}/unreadCount")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _notifService.GetUnreadCount(userId);
            return Ok(new { userId, unreadCount = count });
        }

        // PUT /api/notifications/{id}/read
        [Authorize]
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            try
            {
                await _notifService.MarkAsRead(id);
                return Ok(new { message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/notifications/{userId}/readAll
        [Authorize]
        [HttpPut("{userId}/readAll")]
        public async Task<IActionResult> MarkAllRead(int userId)
        {
            try
            {
                await _notifService.MarkAllRead(userId);
                return Ok(new { message = "All notifications marked as read." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/notifications/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _notifService.DeleteNotif(id);
                return Ok(new { message = "Notification deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/notifications/broadcast
        [Authorize]
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast(
            [FromBody] BroadcastNotificationDto dto)
        {
            try
            {
                await _notifService.SendBulk(
                    dto.UserIds, dto.Title, dto.Message);
                return Ok(new
                {
                    message = $"Broadcast sent to {dto.UserIds.Count} users."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Endpoints called by OTHER services ─────────────────────────────

        // POST /api/notifications/like
        // Called by Like API
        [HttpPost("like")]
        public async Task<IActionResult> LikeNotification(
            [FromBody] LikeNotificationDto dto)
        {
            try
            {
                await _notifService.SendLikeNotif(
                    dto.RecipientId,
                    dto.ActorId,
                    dto.TargetId,
                    dto.TargetType);
                return Ok(new { message = "Like notification sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/notifications/comment
        // Called by Comment API
        [HttpPost("comment")]
        public async Task<IActionResult> CommentNotification(
            [FromBody] CommentNotificationDto dto)
        {
            try
            {
                await _notifService.SendCommentNotif(
                    dto.PostAuthorId,
                    dto.ActorId,
                    dto.PostId);
                return Ok(new { message = "Comment notification sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/notifications/follow
        // Called by Follow API
        [HttpPost("follow")]
        public async Task<IActionResult> FollowNotification(
            [FromBody] FollowNotificationDto dto)
        {
            try
            {
                await _notifService.SendFollowNotif(
                    dto.TargetId,
                    dto.FollowerId,
                    dto.Type);
                return Ok(new { message = "Follow notification sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/notifications/mention
        // Called by Comment API
        [HttpPost("mention")]
        public async Task<IActionResult> MentionNotification(
            [FromBody] MentionNotificationDto dto)
        {
            try
            {
                await _notifService.SendMentionNotif(
                    dto.MentionedUserId,
                    dto.ActorId,
                    dto.PostId);
                return Ok(new { message = "Mention notification sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}