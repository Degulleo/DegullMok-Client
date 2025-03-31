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

public class RevengeData
{
    [JsonProperty("message")]
    public string message { get; set; }
    [JsonProperty("isBlack")]
    public Boolean isBlack { get; set; }
}

public class MultiplayManager : IDisposable
{
    private SocketIOUnity _socket;
    private event Action<Constants.MultiplayManagerState, object> _onMultiplayStateChanged;
    public Action<MoveData> OnOpponentMove;
    
    private string _roomId;
    
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
            _socket.On("doSurrender", DoSurrender);
            _socket.On("surrenderConfirmed", SurrenderConfirmed);
            _socket.On("receiveTimeout", ReceiveTimeout);
            // 무승부 관련
            _socket.On("receiveDrawRequest", ReceiveDrawRequest);
            _socket.On("drawRequestSent", DrawRequestSent);
            _socket.On("drawAccepted", DrawAccepted);
            _socket.On("drawConfirmed", DrawConfirmed);
            _socket.On("drawRejected", DrawRejected);
            _socket.On("drawRejectionConfirmed", DrawRejectionConfirmed);
            // 재대결 관련
            _socket.On("receiveRevengeRequest", ReceiveRevengeRequest);
            _socket.On("revengeRequestSent", RevengeRequestSent);
            _socket.On("revengeAccepted", RevengeAccepted);
            _socket.On("revengeConfirmed", RevengeConfirmed);
            _socket.On("revengeRejected", RevengeRejected);
            _socket.On("revengeRejectionConfirmed", RevengeRejectionConfirmed);
            // 강제 종료 처리
            _socket.On("opponentDisconnected", OpponentDisconnected);
            
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
        _roomId = data.roomId;
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.CreateRoom, data.roomId);
    }
    
    private void JoinRoom(SocketIOResponse response)
    {
        var data = response.GetValue<JoinRoomData>();
        Debug.Log($"룸에 참여: 룸 ID - {data.roomId}, 상대방 등급 - {data.opponentRating}, 상대방 이름 - {data.opponentNickname}, 흑/백 여부 - {data.isBlack}, 상대방 이미지 인덱스 - {data.opponentImageIndex}");
        _roomId = data.roomId;
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

    public void LeaveRoom() // MultiplayManager에 있는 _roomId 사용, 매개변수 필요 X
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("LeaveRoom 호출 실패: _roomId가 설정되지 않음");
            return;
        }

        _socket.Emit("leaveRoom", new { roomId = _roomId });
        _roomId = null; // 방 나가면 roomId 초기화
    }
    
    public void ForceQuit()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("Disconnect 호출 실패: _roomId가 설정되지 않음");
            return;
        }

        _socket.Emit("disconnect", new { roomId = _roomId });
        _roomId = null; // 방 나가면 roomId 초기화
    }
    
    public void RequestSurrender()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("RequestSurrender 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("requestSurrender",new { roomId = _roomId });
    }
    
    private void DoSurrender(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
    
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.DoSurrender, data.message);
    }

    private void SurrenderConfirmed(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
    
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.SurrenderConfirmed, data.message);
    }

    /// <summary>
    /// 타임 아웃 요청
    /// </summary>
    public void SendTimeout()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("SendTimeout 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("sendTimeout",new { roomId = _roomId });
    }
    
    /// <summary>
    /// 타임 아웃 수신
    /// </summary>
    /// <param name="response"></param>
    private void ReceiveTimeout(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
    
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.ReceiveTimeout, data.message);
    }

    #region 무승부

    public void RequestDraw()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("requestDraw 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("requestDraw",new { roomId = _roomId });
    }
    
    private void ReceiveDrawRequest(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.ReceiveDrawRequest, data.message);
    }
    
    private void DrawRequestSent(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.DrawRequestSent, data.message);
    }

    public void AcceptDraw()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("acceptDraw 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("acceptDraw", new { roomId = _roomId });
    }
    
    private void DrawAccepted(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.DrawAccepted, data.message);
    }

    private void DrawConfirmed(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.DrawConfirmed, data.message);
    }

    public void RejectDraw()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("rejectDraw 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("rejectDraw", new { roomId = _roomId });
    }
    
    private void DrawRejected(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.DrawRejected, data.message);
    }

    private void DrawRejectionConfirmed(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.DrawRejectionConfirmed, data.message);
    }
    
    private void DerawRejectionConfirmed(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.DrawRejectionConfirmed, data.message);
    }

    #endregion

    #region 재대결

    public void RequestRevengeRequest()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("requestDraw 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("requestRevenge",new { roomId = _roomId });
    }
    
    private void ReceiveRevengeRequest(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.ReceiveRevengeRequest, data.message);
    }
    
    private void RevengeRequestSent(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.RevengeRequestSent, data.message);
    }

    public void AcceptRevenge()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("acceptRevenge 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("acceptRevenge", new { roomId = _roomId });
    }
    
    private void RevengeAccepted(SocketIOResponse response)
    {
        var data = response.GetValue<RevengeData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.RevengeAccepted, data);
    }

    private void RevengeConfirmed(SocketIOResponse response)
    {
        var data = response.GetValue<RevengeData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.RevengeConfirmed, data);
    }

    public void RejectRevenge()
    {
        if (string.IsNullOrEmpty(_roomId))
        {
            Debug.LogError("rejectRevenge 호출 실패: _roomId가 설정되지 않음");
            return;
        }
        _socket.Emit("rejectRevenge", new { roomId = _roomId });
    }
    
    private void RevengeRejected(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.RevengeRejected, data.message);
    }

    private void RevengeRejectionConfirmed(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.RevengeRejectionConfirmed, data.message);
    }

    private void OpponentDisconnected(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();
        
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayManagerState.OpponentDisconnected, data.message);
    }

    #endregion

    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
            _roomId = null;
        }
    }

    public string GetRoomId()
    {
        return _roomId;
    }
}
