using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class ContactService : IContact
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ContactService> _logger;

        public ContactService(DataContext context, IHttpContextAccessor httpContextAccessor, ILogger<ContactService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into ContactService");
        }

        public async Task<RespnModel<SearchContactModel>> GetSearchedContact(string searchInfo)
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new RespnModel<SearchContactModel>
                    {
                        status =  false,
                        message = "null",
                        result = null
                    };
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var getUserInfo = await _context.Users.Where(x => x.Username == searchInfo || x.PhoneNumber == searchInfo
                        || x.Email == searchInfo).FirstOrDefaultAsync();


                if (getUserInfo == null) return new RespnModel<SearchContactModel>
                {
                    status = false,
                    message = "This user does not exist!!",
                    result = null,
                };

                var image = await _context.Images.Where(x => x.UserId == getUserInfo.Id).FirstOrDefaultAsync();

                if (image == null) return new RespnModel<SearchContactModel>
                {
                    status = true,
                    message = "Successful",
                    result = new SearchContactModel
                    {
                        firstName = getUserInfo.FirstName,
                        lastName = getUserInfo.LastName,
                        userName = getUserInfo.Username,
                        imageDetails = null,
                    }
                };

                return new RespnModel<SearchContactModel>
                {
                    status = true,
                    message = "Successful",
                    result = new SearchContactModel
                    {
                        firstName = getUserInfo.FirstName,
                        lastName = getUserInfo.LastName,
                        userName = getUserInfo.Username,
                        imageDetails = image.FileData,
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new RespnModel<SearchContactModel>
                {
                    status = false,
                    message = "Null",
                    result = null
                };
            }
        }

        public async Task<ChatTrnModel> GetChatTrnAcctNums(string currency, string destAcctUser)
        {
            ChatTrnModel chatTrn = new ChatTrnModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return chatTrn;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var loggedInUserAcctNum = await _context.Accounts
                    .Where(x => x.UserId == userID && x.Currency == currency)
                    .Select(x => x.AccountNumber).FirstOrDefaultAsync();

                var destUserID = await _context.Users
                    .Where(x => x.Username == destAcctUser)
                    .Select(x => x.Id).FirstOrDefaultAsync();

                var getDestUserAcctNum = await _context.Accounts
                    .Where(x => x.UserId == destUserID && x.Currency == currency)
                    .Select(x => x.AccountNumber).FirstOrDefaultAsync();

                chatTrn.sourceAccount = loggedInUserAcctNum;
                chatTrn.destinationAccount = getDestUserAcctNum;
                return chatTrn;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return chatTrn;
            }
        }

        public async Task<bool> GetOnlineStatus(string chattingWith)
        {
            try
            {
                var getUserOnlineStatus = await _context.Users.Where(x => x.Username == chattingWith).Select(x => x.IsUserLogin).FirstOrDefaultAsync();

                return getUserOnlineStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<List<UnreadChats>> GetUnreadChatCount()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<UnreadChats>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userLoggedIn = await _context.Users.Where(x => x.Id == userID).Select(x => x.Username).FirstOrDefaultAsync();

                var unreadChatsGrouped = await _context.UserToUserChats
                    .Where(x => x.RecipientUsername == userLoggedIn && !x.IsChatRead)
                    //.Where(x => x.SenderUsernmae != userLoggedIn)
                    .GroupBy(x => x.SenderUsernmae)
                    .Select(group => new UnreadChats 
                    {
                        senderUsername = group.Key,
                        unreadChat = group.Count()
                    })
                    .ToListAsync();

                return unreadChatsGrouped;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<UnreadChats>();
            }
        }



    }
}
