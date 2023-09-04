using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface IChat
    {
        Task<bool> ReadChat(ReadChatsDTO req, string userOrAdmin);
        Task<GetAllChatsCount> GetAllUnreadChatsCount(string userOrAdmin);
        Task<ResponseModel> UserChat(ChatDTO req, string chattingWIth);
        Task<ResponseModel> AdminChat(ChatDTO req, string chattingWIth);
        Task<List<ChatDetails>> GetChatsAdmin(string user);
        Task<List<ChatDetails>> GetChatsUser(string user);
        Task<RespModel> GenPinSendEmail(GenratePinDTO req);
        Task<bool> ValidatePin(ValidatePinDTO req);

        ////////////////////////////////////////////
        Task<ResponseModel> UserOutChat(ChatDTO req, string outAppUser);
        Task<List<ChatDetails>> GetOutChatsUser(string admin, string uName);
        //Task<PaginatedResult<ChatDetails>> GetOutChatsUserPag(string admin, string uName, int page, int pageSize);

        Task<RespModel> InitiateChats(string chattingWith);
        Task<List<InitiatedChats>> GetInitiatedChats();
        Task<List<User2UserChat>> GetUser2UserChats(string chatWith);
        Task<ResponseModel> User2User(User2UserChatDTO req, string chattingWith);
    }
}
