using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Shared
{
    public enum CommandType
    {
        Login,
        LoginResponse,
        Logout,
        GetChats,
        ChatsList,
        GetMessages,
        MessagesList,
        SendMessage,
        NewMessage,
        UserStatusChanged,
        CreateChat,
        ChatCreated,
        GetDepartments,
        DepartmentsList,

        GetAvailableUsers,
        AvailableUsersList,
        CreatePrivateChat,
        CreateGroupChat,
        MessagesRead
    }
}
