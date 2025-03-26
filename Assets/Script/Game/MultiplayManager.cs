using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketIOClient;
using UnityEngine;

public class CreateRoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

public class JoinRoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
    [JsonProperty("opponentRating")]
    public int opponentRating { get; set; }
    [JsonProperty("opponentNickname")]
    public string opponentNickname { get; set; }
    [JsonProperty("opponentImageIndex")]
    public int opponentImageIndex { get; set; }
    [JsonProperty("isBlack")]
    public Boolean isBlack { get; set; }
}

public class StartGameData
{
    [JsonProperty("opponentId")]
    public string opponentId { get; set; }
    [JsonProperty("opponentRating")]
    public int opponentRating { get; set; }
    [JsonProperty("opponentNickname")]
    public string opponentNickname { get; set; }
    [JsonProperty("opponentImageIndex")]
    public int opponentImageIndex { get; set; }
    [JsonProperty("isBlack")]
    public Boolean isBlack { get; set; }
}


public class PositionData
{
    [JsonProperty("x")]
    public int x { get; set; }

    [JsonProperty("y")]
    public int y { get; set; }
}

public class MoveData
{
    [JsonProperty("position")]
    public PositionData position { get; set; }
}


public class MessageData
{
    [JsonProperty("message")]
    public string message { get; set; }
}

public class MultiplayManager : IDisposable
{
    private SocketIOUnity _socket;
    private event Action<Constants.MultiplayManagerState, object> _onMultiplayStateChanged;
    public Action<MoveData> OnOpponentMove;
    
    public MultiplayManager(Action<Constants.MultiplayManagerState, object> onMultiplayStateChanged)
    {
        _onMultiplayStateChanged = onMultiplayStateChanged;
        try
        {
            var serverUrl = new Uri(Constants.GameServerURL);
        
            _socket = new SocketIOUnity(serverUrl, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });

            _socket.On("createRoom", CreateRoom);
            _socket.On("joinRoom", JoinRoom);
            _socket.On("startGame", StartGame);
            _socket.On("switchAI", SwitchAI);
            _socket.On("exitRoom", ExitRoom);
            _socket.On("endGame", EndGame);
            _socket.On("doOpponent", DoOpponent);

            _socket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError("MultiplayManager 생성 중 오류 발생: " + e.Message);
        }
    }
    
    public async void RegisterPlayer(string nickname, int rating, int imageIndex)
    {
        // 연결될 때까지 대기
        while (!_socket.Connected)
        {
            Debug.Log("소켓 연결 대기 중...");
            await Task.Delay(100); // 0.1초 대기 후 다시 확인
        }
        _socket.Emit("registerPlayer", new { nickname, rating, imageIndex });
    }
    
    private void CreateRoom(SocketIOResponse response)
    {
        var data = response.GetValue<CreateRoomData>();
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.CreateRoom, data.roomId);
    }
    
    private void JoinRoom(SocketIOResponse response)
    {
        var data = response.GetValue<JoinRoomData>();
        Debug.Log($"룸에 참여: 룸 ID - {data.roomId}, 상대방 등급 - {data.opponentRating}, 상대방 이름 - {data.opponentNickname}, 흑/백 여부 - {data.isBlack}, 상대방 이미지 인덱스 - {data.opponentImageIndex}");
    
        // 필요한 데이터 사용
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.JoinRoom, data);
    }

    private void SwitchAI(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        Debug.Log("switchAI: " + data.message);
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.SwitchAI, data.message);
    }
    
    private void StartGame(SocketIOResponse response)
    {
        var data = response.GetValue<StartGameData>();
        Debug.Log($"게임 시작: 상대방 ID - {data.opponentId}, 상대방 등급 - {data.opponentRating}, 상대방 이름 - {data.opponentNickname}, 흑/백 여부 - {data.isBlack}, 상대방 이미지 인덱스 - {data.opponentImageIndex}");
    
        // 필요한 데이터 사용
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.StartGame, data);
    }

    
    // 서버로 부터 상대방의 마커 정보를 받기 위한 메서드
    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<MoveData>();

        if (data != null && data.position != null)
        {
            Vector2Int opponentPosition = new Vector2Int(data.position.x, data.position.y);
            Debug.Log($"상대방의 위치: {opponentPosition}");
            OnOpponentMove?.Invoke(new MoveData { position = data.position });
        }
        else
        {
            Debug.LogError("DoOpponent: 데이터가 올바르지 않습니다.");
        }
    }


    // 플레이어의 마커 위치를 서버로 전달하기 위한 메서드
    public void SendPlayerMove(string roomId, Vector2Int position)
    {
        Debug.Log($"내 위치: {position}");
        _socket.Emit("doPlayer", new
        {
            roomId,
            position = new { x = position.x, y = position.y }
        });
    }

    
    private void ExitRoom(SocketIOResponse response)
    {
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.ExitRoom, null);
    }

    private void EndGame(SocketIOResponse response)
    {
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.EndGame, null);
    }

    public void LeaveRoom(string roomId)
    {
        _socket.Emit("leaveRoom", new { roomId });
    }

    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
        }
    }
}
