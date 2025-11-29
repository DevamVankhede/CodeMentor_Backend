using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;

namespace CodeMentorAI.API.Hubs;

[Authorize]
public class CollaborationHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _roomUsers = new();
    private static readonly ConcurrentDictionary<string, string> _roomCodes = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, object>> _userCursors = new();

    public async Task JoinRoom(string roomId, string userName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        _roomUsers.AddOrUpdate(roomId, 
            new HashSet<string> { userName }, 
            (key, users) => { users.Add(userName); return users; });

        await Clients.Group(roomId).SendAsync("UserJoined", userName, _roomUsers[roomId].ToList());
        
        // Send current code to new user
        if (_roomCodes.TryGetValue(roomId, out var currentCode))
        {
            await Clients.Caller.SendAsync("CodeUpdate", currentCode, "system");
        }
    }

    public async Task LeaveRoom(string roomId, string userName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        
        if (_roomUsers.TryGetValue(roomId, out var users))
        {
            users.Remove(userName);
            if (users.Count == 0)
            {
                _roomUsers.TryRemove(roomId, out _);
                _roomCodes.TryRemove(roomId, out _);
            }
            else
            {
                await Clients.Group(roomId).SendAsync("UserLeft", userName, users.ToList());
            }
        }
    }

    public async Task SendCodeChange(string roomId, string code, string userName, int cursorPosition)
    {
        _roomCodes[roomId] = code;
        await Clients.OthersInGroup(roomId).SendAsync("CodeUpdate", code, userName, cursorPosition);
    }

    public async Task SendCursorPosition(string roomId, string userName, int line, int column)
    {
        var cursorKey = $"{roomId}:{userName}";
        _userCursors[cursorKey] = new Dictionary<string, object>
        {
            ["line"] = line,
            ["column"] = column,
            ["userName"] = userName
        };

        await Clients.OthersInGroup(roomId).SendAsync("CursorUpdate", userName, line, column);
    }

    public async Task SendChatMessage(string roomId, string userName, string message)
    {
        await Clients.Group(roomId).SendAsync("ChatMessage", userName, message, DateTime.UtcNow);
    }

    public async Task RequestAIHelp(string roomId, string code, string language, string question)
    {
        // This will be handled by the AI service
        await Clients.Group(roomId).SendAsync("AIHelpRequested", question);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up user from all rooms
        var userRooms = _roomUsers.Where(kvp => kvp.Value.Contains(Context.UserIdentifier ?? "")).ToList();
        
        foreach (var room in userRooms)
        {
            await LeaveRoom(room.Key, Context.UserIdentifier ?? "");
        }

        await base.OnDisconnectedAsync(exception);
    }
}