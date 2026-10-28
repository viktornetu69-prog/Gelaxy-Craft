using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

/// <summary>
/// SignalR Hub - маршрутизатор для соединения игроков.
/// НЕ хранит игровую логику - только пересылает сообщения.
/// </summary>
public class GalaxyHub : Hub
{
    private static readonly Dictionary<string, string> _players = new();
    private static readonly Dictionary<string, HashSet<string>> _systems = new();

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[INFO] Connection connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = _players.FirstOrDefault(p => p.Value == Context.ConnectionId).Key;
        if (!string.IsNullOrEmpty(playerId))
        {
            await LeaveSystem(playerId);
            _players.Remove(playerId);
            Console.WriteLine($"[INFO] Player disconnected: {playerId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinSystem(string playerId, string systemId)
    {
        Console.WriteLine($"[INFO] Player '{playerId}' joining system '{systemId}'");

        if (_players.ContainsKey(playerId))
            await LeaveSystem(playerId);

        _players[playerId] = Context.ConnectionId;

        if (!_systems.ContainsKey(systemId))
            _systems[systemId] = new HashSet<string>();

        _systems[systemId].Add(Context.ConnectionId);

        var existingConnections = _systems[systemId].ToList();
        await Clients.Clients(existingConnections).SendAsync(
            "PlayerJoinedSystem", playerId, systemId, existingConnections);

        await Clients.Group(systemId).SendAsync("PlayerJoinedSystem", playerId, systemId);
        await Groups.AddToGroupAsync(Context.ConnectionId, systemId);

        Console.WriteLine($"[OK] Player '{playerId}' in system '{systemId}' ({_systems[systemId].Count} total)");
    }

    public async Task LeaveSystem(string playerId)
    {
        if (!_players.TryGetValue(playerId, out var connectionId))
            return;

        var systemId = _systems.FirstOrDefault(s => s.Value.Contains(connectionId)).Key;
        if (!string.IsNullOrEmpty(systemId))
        {
            _systems[systemId].Remove(connectionId);
            if (_systems[systemId].Count == 0)
                _systems.Remove(systemId);

            await Groups.RemoveFromGroupAsync(connectionId, systemId);
            await Clients.Group(systemId).SendAsync("PlayerLeftSystem", playerId);
        }

        _players.Remove(playerId);
    }

    public async Task BroadcastGameEvent(string systemId, string eventType, object data)
    {
        if (!_systems.ContainsKey(systemId)) return;

        var connections = _systems[systemId].ToList();
        await Clients.Clients(connections).SendAsync("GameEventReceived", systemId, eventType, data);
    }

    public async Task<string[]> GetOnlinePlayersInSystem(string systemId)
    {
        if (!_systems.ContainsKey(systemId))
            return Array.Empty<string>();

        var connectionIds = _systems[systemId].ToList();
        return _players.Where(p => connectionIds.Contains(p.Value))
                       .Select(p => p.Key).ToArray();
    }
}
