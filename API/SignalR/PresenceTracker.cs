using System;

namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = 
        new Dictionary<string, List<string>>();

    public Task UserConnected(string username, string connectionId)
    {
        lock(OnlineUsers)
        {
            if(OnlineUsers.ContainsKey(username))
                OnlineUsers[username].Add(connectionId);
            else
                OnlineUsers.Add(username, new List<string>{ connectionId });
        }

        return Task.CompletedTask;
    }

    public Task UserDisonnected(string username, string connectionId)
    {
        lock(OnlineUsers)
        {
            if(!OnlineUsers.ContainsKey(username))
                return Task.CompletedTask;

            OnlineUsers[username].Remove(connectionId);

            // if no connections available for that user
            if(OnlineUsers[username].Count == 0)
                OnlineUsers.Remove(username);
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers; 

        lock(OnlineUsers)
        {
            // Get all usernames from Online Users dictionary
            onlineUsers = OnlineUsers.OrderBy(u => u.Key).Select(u => u.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }

    // Method to get connections for a User
    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds;

        lock(OnlineUsers)
        {
            connectionIds = OnlineUsers.GetValueOrDefault(username);
        }

        return Task.FromResult(connectionIds);
    }
}
