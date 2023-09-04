using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Interfaces;
using WalletPayment.Services.Services;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private IChat _chatService;

        public ChatController(IChat chatService)
        {
            _chatService = chatService;
        }

        [HttpPut("ReadChat"), Authorize]
        public async Task<IActionResult> ReadChat(ReadChatsDTO req, string userOrAdmin)
        {
            var result = await _chatService.ReadChat(req, userOrAdmin);
            return Ok(result);
        }

        [HttpGet("GetAllUnreadChatsCount"), Authorize]
        public async Task<IActionResult> GetAllUnreadChatsCount(string userOrAdmin)
        {
            var result = await _chatService.GetAllUnreadChatsCount(userOrAdmin);
            return Ok(result);
        }

        [HttpPost("UserChat"), Authorize]
        public async Task<IActionResult> UserChat(ChatDTO req, string chattingWIth)
        {
            var result = await _chatService.UserChat(req, chattingWIth);
            return Ok(result);
        }

        [HttpPost("AdminChat"), Authorize]
        public async Task<IActionResult> AdminChat(ChatDTO req, string chattingWIth)
        {
            var result = await _chatService.AdminChat(req, chattingWIth);
            return Ok(result);
        }

        [HttpGet("GetChatsAdmin"), Authorize]
        public async Task<IActionResult> GetChatsAdmin(string user)
        {
            var result = await _chatService.GetChatsAdmin(user);
            return Ok(result);
        }

        [HttpGet("GetChatsUser"), Authorize]
        public async Task<IActionResult> GetChatsUser(string user)
        {
            var result = await _chatService.GetChatsUser(user);
            return Ok(result);
        }

        [HttpPost("GenPinSendEmail")]
        public async Task<IActionResult> GenPinSendEmail(GenratePinDTO req)
        {
            var result = await _chatService.GenPinSendEmail(req);
            return Ok(result);
        }

        [HttpPost("ValidatePin")]
        public async Task<IActionResult> ValidatePin(ValidatePinDTO req)
        {
            var result = await _chatService.ValidatePin(req);
            return Ok(result);
        }
        //////////////////////////////////////////////////////
        [HttpPost("UserOutChat")]
        public async Task<IActionResult> UserOutChat(ChatDTO req, string outAppUser)
        {
            var result = await _chatService.UserOutChat(req, outAppUser);
            return Ok(result);
        }

        [HttpGet("GetOutChatsUser")]
        public async Task<IActionResult> GetOutChatsUser(string admin, string uName)
        {
            var result = await _chatService.GetOutChatsUser(admin, uName);
            return Ok(result);
        }

        //[HttpGet("GetOutChatsUserPag")]
        //public async Task<IActionResult> GetOutChatsUserPag(string admin, string uName, int page, int pageSize)
        //{
        //    var result = await _chatService.GetOutChatsUserPag(admin, uName, page, pageSize);
        //    return Ok(result);
        //}


        [HttpPost("InitiateChats"), Authorize]
        public async Task<IActionResult> InitiateChats(string chattingWith)
        {
            var result = await _chatService.InitiateChats(chattingWith);
            return Ok(result);
        }

        [HttpGet("GetInitiatedChats"), Authorize]
        public async Task<IActionResult> GetInitiatedChats()
        {
            var result = await _chatService.GetInitiatedChats();
            return Ok(result);
        }

        [HttpGet("GetUser2UserChats"), Authorize]
        public async Task<IActionResult> GetUser2UserChats(string chatWith)
        {
            var result = await _chatService.GetUser2UserChats(chatWith);
            return Ok(result);
        }

        [HttpPost("User2User"), Authorize]
        public async Task<IActionResult> User2User(User2UserChatDTO req, string chattingWith)
        {
            var result = await _chatService.User2User(req, chattingWith);
            return Ok(result);
        }




    }
}
