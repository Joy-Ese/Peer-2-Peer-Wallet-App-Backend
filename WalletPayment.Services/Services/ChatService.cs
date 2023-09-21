using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;

namespace WalletPayment.Services.Services
{
    public class ChatService : IChat
    {
        private readonly IHubContext<NotificationSignalR> _hub;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmail _emailService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(DataContext context, IEmail emailService, IHttpContextAccessor httpContextAccessor, ILogger<ChatService> logger, IHubContext<NotificationSignalR> hub)
        {
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into ChatService");
            _hub = hub;
        }

        public async Task<bool> ReadChat(ReadChatsDTO req, string userOrAdmin)
        {
            try
            {
                if (userOrAdmin == "Admin")
                {
                    int adminID;
                    if (_httpContextAccessor.HttpContext == null)
                    {
                        return false;
                    }
                    adminID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                    var allAdminUnread = await _context.Chats.Include("AdminUser").Where(x => x.AdminUserId == adminID && x.AdminUser.Username == req.username).ToListAsync();

                    foreach (var chat in allAdminUnread)
                    {
                        chat.IsChatRead = true;
                    }
                    await _context.SaveChangesAsync();


                    return true;
                }



                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var allUserUnread = await _context.Chats.Include("User").Where(x => x.UserId == userID && x.User.Username == req.username).ToListAsync();

                foreach (var chat in allUserUnread)
                {
                    chat.IsChatRead = true;
                }
                await _context.SaveChangesAsync();


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<GetAllChatsCount> GetAllUnreadChatsCount(string userOrAdmin)
        {
            GetAllChatsCount allChatsCount = new GetAllChatsCount();
            try
            {
                if (userOrAdmin == "Admin")
                {
                    int adminID;
                    if (_httpContextAccessor.HttpContext == null)
                    {
                        return allChatsCount;
                    }
                    adminID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                    var allUnreadChat = await _context.Chats.Where(x => x.AdminUserId == adminID && x.IsChatRead == false).CountAsync();

                    allChatsCount.allChats = allUnreadChat;

                    //await _hub.Clients.All.SendAsync("UpdateChatCount");

                    return allChatsCount;
                }

                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return allChatsCount;
                }
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var allUnreadChats = await _context.Chats.Where(x => x.UserId == userID && x.IsChatRead == false).CountAsync();

                allChatsCount.allChats = allUnreadChats;

                //await _hub.Clients.All.SendAsync("UpdateChatCount");

                return allChatsCount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return allChatsCount;
            }
        }

        public async Task<ResponseModel> UserChat(ChatDTO req, string chattingWIth)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return response;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                Chat newUserChat = new Chat
                {
                    Message = req.message,
                    Date = DateTime.Now,
                    UserId = userID,
                    AdminUserId = null,
                    IsChatRead = false,
                    ChattingWith = chattingWIth
                };

                await _context.Chats.AddAsync(newUserChat);
                await _context.SaveChangesAsync();

                _hub.Clients.All.SendAsync("ReceiveMessage", chattingWIth);

                response.status = true;
                response.message = "Sent!!!";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return response;
            }
        }

        public async Task<ResponseModel> AdminChat(ChatDTO req, string chattingWIth)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string adminUsername;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return response;
                }

                adminUsername = _httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserName)?.Value;

                var getID = await _context.Adminss.Where(x => x.Username == adminUsername).Select(x => x.Id).FirstOrDefaultAsync();

                Chat newAdminChat = new Chat
                {
                    Message = req.message,
                    Date = DateTime.Now,
                    UserId = null,
                    AdminUserId = getID,
                    IsChatRead = false,
                    ChattingWith = chattingWIth
                };

                await _context.Chats.AddAsync(newAdminChat);
                await _context.SaveChangesAsync();

                _hub.Clients.All.SendAsync("ReceiveMessage", chattingWIth);

                response.status = true;
                response.message = "Sent!!!";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return response;
            }
        }

        public async Task<List<ChatDetails>> GetChatsAdmin(string user)
        {
            try
            {
                int adminID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<ChatDetails>();
                }

                adminID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                int getUserID = await _context.Users.Where(x => x.Username == user).Select(x => x.Id).FirstOrDefaultAsync();

                var chatList = new List<ChatDetails>();

                var allchats = await _context.Chats.ToListAsync();
                var allChatAdmin = allchats.Where(x => x.AdminUserId == adminID || x.UserId == getUserID).ToList();

                foreach (var item in allChatAdmin)
                {
                    var chatDetails = new ChatDetails()
                    {
                        message = item.Message,
                        date = item.Date,
                        idUser = item.UserId,
                        idAdmin = item.AdminUserId
                    };
                    chatList.Add(chatDetails);
                }

                return chatList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<ChatDetails>();
            }
        }

        public async Task<List<ChatDetails>> GetChatsUser(string user)
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<ChatDetails>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                int getAdminID = await _context.Adminss.Where(x => x.Username == user).Select(x => x.Id).FirstOrDefaultAsync();

                var chatList = new List<ChatDetails>();

                var allchats = await _context.Chats.ToListAsync();
                //var allChatUser = allchats.Where(x => (x.UserId == userID || x.AdminUserId == getAdminID) && x.ChattingWith == user).ToList();
                var allChatUser = allchats.Where(x => x.UserId == userID || x.AdminUserId == getAdminID).ToList();


                foreach (var item in allChatUser)
                {
                    var chatDetails = new ChatDetails()
                    {
                        message = item.Message,
                        date = item.Date,
                        idUser = item.UserId,
                        idAdmin = item.AdminUserId
                    };
                    chatList.Add(chatDetails);
                }

                return chatList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<ChatDetails>();
            }
        }

        public string SixDigitsGenerator()
        {
            Random random = new Random();
            string randomSix = random.Next(100000, 999999).ToString();
            return randomSix;
        }

        public async Task<RespModel> GenPinSendEmail(GenratePinDTO req)
        {
            RespModel resp = new RespModel();
            try
            {
                var user = await _context.Users.AnyAsync(x => x.Email == req.email && x.Username == req.uName);
                if (!user)
                {
                    resp.status = false;
                    resp.message = "Incorrect username or email";
                    return resp;
                }

                string generatedDigits = SixDigitsGenerator();

                LockedOutUser newLockedUser = new LockedOutUser
                {
                    Email = req.email,
                    SixDigitPin = generatedDigits,
                    PinGenerationTime = DateTime.Now,
                    Username = req.uName,
                };

                await _context.LockedOutUsers.AddAsync(newLockedUser);
                await _context.SaveChangesAsync();

                var subject = "Important Notice!!!";
                var body = $"Dear {req.uName}, thank you for verifying your mail. Find your six digits pin {generatedDigits} to commence chat with an admin. " +
                    $"Note: This pin will expire in 10 minutes.";

                _emailService.SendEmail(subject, req.email, body);


                resp.status = true;
                resp.message = "Successfully verified";
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return resp;
            }
        }

        public async Task<bool> ValidatePin(ValidatePinDTO req)
        {
            try
            {
                int PinExpirationMinutes = 10;
                DateTime checkNow = DateTime.Now;

                var getLastLockedUserPin = await _context.LockedOutUsers.Where(x => x.Username == req.uName).OrderByDescending(x => x.PinGenerationTime).FirstOrDefaultAsync();
                
                if (req.pin == getLastLockedUserPin.SixDigitPin && checkNow - getLastLockedUserPin.PinGenerationTime <= TimeSpan.FromMinutes(PinExpirationMinutes))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<ResponseModel> UserOutChat(ChatDTO req, string outAppUser)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var getUserID = await _context.Users.Where(x => x.Username == outAppUser).Select(x => x.Id).FirstOrDefaultAsync();

                Chat newUserChat = new Chat
                {
                    Message = req.message,
                    Date = DateTime.Now,
                    UserId = getUserID,
                    AdminUserId = null,
                    IsChatRead = true,
                };

                await _context.Chats.AddAsync(newUserChat);
                await _context.SaveChangesAsync();

                response.status = true;
                response.message = "Sent!!!";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return response;
            }
        }

        public async Task<List<ChatDetails>> GetOutChatsUser(string admin, string uName)
        {
            try
            {
                var getUserID = await _context.Users.Where(x => x.Username == uName).Select(x => x.Id).FirstOrDefaultAsync();
                int getAdminID = await _context.Adminss.Where(x => x.Username == admin).Select(x => x.Id).FirstOrDefaultAsync();

                var chatList = new List<ChatDetails>();

                var chats = await _context.Chats.ToListAsync();

                var allChatUser = chats.Where(x => x.UserId == getUserID || x.AdminUserId == getAdminID).ToList();

                foreach (var item in allChatUser)
                {
                    var chatDetails = new ChatDetails()
                    {
                        message = item.Message,
                        date = item.Date,
                        idUser = item.UserId,
                        idAdmin = item.AdminUserId
                    };
                    chatList.Add(chatDetails);
                }

                return chatList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<ChatDetails>();
            }
        }

        //public async Task<PaginatedResult<ChatDetails>> GetOutChatsUserPag(string admin, string uName, int page, int pageSize)
        //{
        //    try
        //    {
        //        var getUserID = await _context.Users.Where(x => x.Username == uName).Select(x => x.Id).FirstOrDefaultAsync();

        //        int getAdminID = await _context.Adminss.Where(x => x.Username == admin).Select(x => x.Id).FirstOrDefaultAsync();

        //        var chatList = new List<ChatDetails>();

        //        var totalCount = await _context.Chats.CountAsync(x => x.UserId == getUserID || x.AdminUserId == getAdminID);

        //        var chats = await _context.Chats.Where(x => x.UserId == getUserID || x.AdminUserId == getAdminID)
        //            .OrderByDescending(x => x.Date)
        //            .Skip((page - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        foreach (var item in chats)
        //        {
        //            var chatDetails = new ChatDetails()
        //            {
        //                message = item.Message,
        //                date = item.Date,
        //                idUser = item.UserId,
        //                idAdmin = item.AdminUserId
        //            };
        //            chatList.Add(chatDetails);
        //        }

        //        return new PaginatedResult<ChatDetails>
        //        {
        //            items = chatList,
        //            totalCount = totalCount
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
        //        return new PaginatedResult<ChatDetails>
        //        {
        //            items = new List<ChatDetails>(),
        //            totalCount = 0
        //        };
        //    }
        //}


        public async Task<RespModel> InitiateChats(string chattingWith)
        {
            RespModel resp = new RespModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    resp.status = false;
                    resp.message = "Error occured";
                    return resp;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);
                var receiveFrom = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();

                InitiatedChat initiatedChat = new InitiatedChat
                {
                    IsChatInitiated = true,
                    StartedWith = chattingWith,
                    ReceivedFrom = receiveFrom.Username,
                };

                await _context.InitiatedChats.AddAsync(initiatedChat);
                await _context.SaveChangesAsync();

                resp.status = true;
                resp.message = "Initiated!!!";
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return resp;
            }
        }

        public async Task<List<InitiatedChats>> GetInitiatedChats()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<InitiatedChats>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);
                var receiveFrom = await _context.Users.Where(x => x.Id == userID).FirstOrDefaultAsync();

                var initiatedChatsList = new List<InitiatedChats>();

                var initiatedChats = await _context.InitiatedChats.Where(x => x.StartedWith == receiveFrom.Username || x.ReceivedFrom == receiveFrom.Username).ToListAsync();

                foreach (var item in initiatedChats)
                {
                    var initChats = new InitiatedChats()
                    {
                        startedWith = item.StartedWith,
                        receivedFrom = item.ReceivedFrom,
                    };
                    initiatedChatsList.Add(initChats);
                }

                return initiatedChatsList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<InitiatedChats>();
            }
        }

        public async Task<List<User2UserChat>> GetUser2UserChats()
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return new List<User2UserChat>();
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var loggedInUsername = await _context.Users.Where(x => x.Id == userID).Select(x => x.Username).FirstOrDefaultAsync();

                var chatList = new List<User2UserChat>();

                var chats = await _context.UserToUserChats.Where(x => x.SenderUsernmae == loggedInUsername || x.RecipientUsername == loggedInUsername).ToListAsync();

                foreach (var item in chats)
                {
                    var chatDetails = new User2UserChat()
                    {
                        message = item.Message,
                        date = item.Date,
                        sender = item.SenderUsernmae,
                        recipient = item.RecipientUsername
                    };
                    chatList.Add(chatDetails);
                }

                return chatList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return new List<User2UserChat>();
            }
        }

        public async Task<ResponseModel> User2User(User2UserChatDTO req,string chattingWith)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return response;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var loggedInUsername = await _context.Users.Where(x => x.Id == userID).Select(x => x.Username).FirstOrDefaultAsync();


                UserToUserChat newUser2UserChat = new UserToUserChat
                {
                    Message = req.message,
                    Date = DateTime.Now,
                    SenderUsernmae = loggedInUsername,
                    RecipientUsername = chattingWith,
                    IsChatRead = false,
                };

                await _context.UserToUserChats.AddAsync(newUser2UserChat);
                await _context.SaveChangesAsync();

                _hub.Clients.All.SendAsync("User2UserReceiveMessage", chattingWith);

                response.status = true;
                response.message = "Sent!!!";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return response;
            }
        }

        public async Task<bool> ReadUserChats(string chattingWith)
        {
            try
            {
                int userID;
                if (_httpContextAccessor.HttpContext == null)
                {
                    return false;
                }

                userID = Convert.ToInt32(_httpContextAccessor.HttpContext.User?.FindFirst(CustomClaims.UserId)?.Value);

                var userLoggedIn = await _context.Users.Where(x => x.Id == userID).Select(x => x.Username).FirstOrDefaultAsync();

                var chats = await _context.UserToUserChats.Where(x => x.SenderUsernmae == userLoggedIn && x.RecipientUsername == chattingWith && x.IsChatRead == false).ToListAsync();

                foreach (var item in chats)
                {
                    item.IsChatRead = true;
                }
                await _context.SaveChangesAsync();

                if (chats == null) return false;

                await _hub.Clients.All.SendAsync("UpdatChatCount", userLoggedIn);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }





    }
}
