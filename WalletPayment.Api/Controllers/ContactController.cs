using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private IContact _contactService;

        public ContactController(IContact contactService)
        {
            _contactService = contactService;
        }

        [HttpPost("GetSearchedContact"), Authorize]
        public async Task<IActionResult> GetSearchedContact(string searchInfo)
        {
            var result = await _contactService.GetSearchedContact(searchInfo);
            return Ok(result);
        }

        [HttpGet("GetChatTrnAcctNums"), Authorize]
        public async Task<IActionResult> GetChatTrnAcctNums(string currency, string destAcctUser)
        {
            var result = await _contactService.GetChatTrnAcctNums(currency, destAcctUser);
            return Ok(result);
        }

        //[HttpGet("GetChatsAdmin"), Authorize]
        //public async Task<IActionResult> GetChatsAdmin(string user)
        //{
        //    var result = await _chatService.GetChatsAdmin(user);
        //    return Ok(result);
        //}


    }
}
