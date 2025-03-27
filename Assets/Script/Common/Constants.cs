public class Constants
{
    public const string ServerURL = "http://localhost:3000";
    public const string GameServerURL = "ws://localhost:3000";
    public const int BoardSize = 15;
    public const int ReplayMaxRecordSize = 10;
    public const int WIN_COUNT = 5;
    //무승부 확인을 위한 최소 착수 수
    public const int MinCountForDrawCheck = 150;
    public const int RAING_POINTS = 10;
    
    public enum MultiplayManagerState
    {
        CreateRoom,     // 방 생성
        JoinRoom,       // 생성된 방에 참여
        StartGame,      // 생성한 방에 다른 유저가 참여해서 게임 시작
        SwitchAI,       // 15초 후 매칭 실패 시 AI 플레이로 전환 알림
        ExitRoom,       // 자신이 방을 빠져 나왔을 때
        EndGame,        // 상대방이 접속을 끊거나 방을 나갔을 때
        DoSurrender,      // 상대방이 항복했을 때
        SurrenderConfirmed // 항복 요청이 성공적으로 전송되었을 때
    };
}