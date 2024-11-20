using System;
using Newtonsoft.Json.Converters;

namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = 
        new Dictionary<string, List<string>>();

    public Task<bool> UserConnected(string username, string connectionId)
    {
        bool isOnline = false;
        lock(OnlineUsers)
        {
            if(OnlineUsers.ContainsKey(username))
                OnlineUsers[username].Add(connectionId);
            else{
                OnlineUsers.Add(username, new List<string>{ connectionId });
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisonnected(string username, string connectionId)
    {
        bool isOffline = false;
        lock(OnlineUsers)
        {
            if(!OnlineUsers.ContainsKey(username))
                return Task.FromResult(isOffline);

            OnlineUsers[username].Remove(connectionId);

            // if no connections available for that user
            if(OnlineUsers[username].Count == 0){
                OnlineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
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
