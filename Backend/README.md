# 🚀 Галактика Свитч - Backend

## SignalR Server для MMO RTS RPG

### Запуск

```bash
dotnet run
```

### Сервер
- **URL:** http://localhost:5000
- **Hub:** /galaxyhub

### API (SignalR)

| Method | Направление | Описание |
|--------|------------|----------|
| `JoinSystem(PlayerId, SystemId)` | C→S | Игрок входит в систему |
| `LeaveSystem(PlayerId)` | C→S | Игрок покидает систему |
| `BroadcastGameEvent(SystemId, Type, Data)` | C→S | Рассылка события |

| Event | Направление | Описание |
|-------|------------|----------|
| `PlayerJoinedSystem` | S→C | Игрок вошёл |
| `PlayerLeftSystem` | S→C | Игрок вышел |
| `GameEventReceived` | S→C | Игровое событие |
