﻿using Message.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Message.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        [Route("GetMessage")]
        [Authorize]
        public async Task<IActionResult> GetMessageAsync()
        {
            try
            {
                var message = await _messageService.GetMessageAsync();
                return Ok(message);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpPost]
        [Route("SendMessage")]
        [Authorize]
        public async Task<IActionResult> SendMessageAsync([FromQuery] string message, Guid targetUserId)
        {
            try
            {
                await _messageService.SendMessageAsync(message, targetUserId);
                return Ok("Message sent");
            }
            catch (ArgumentNullException ex) { return StatusCode(400, ex.Message); }
            catch (Exception) { return StatusCode(500, "Server Error"); }
        }
    }
}
