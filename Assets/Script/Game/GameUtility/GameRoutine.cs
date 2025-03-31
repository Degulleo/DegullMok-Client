using UnityEngine;

public partial class GameLogic
{
    // 돌 카운터 증가 함수
    public void CountStoneCounter() => _totalStoneCounter++;

    // 착수 버튼 클릭시 호출되는 함수
    public void OnConfirm() => CurrentPlayerState.ProcessMove(this, CurrentTurn, SelectedRow, SelectedCol);

    // 스톤의 상태변경 명령함수
    private void SetStoneNewState(Enums.StoneState state, int row, int col)
    {
        StoneController.SetStoneState(state, row, col);
    }

    public void SetStoneSelectedState(int row, int col)
    {
        if (_board[row, col] != Enums.PlayerType.None) return;

        if (StoneController.GetStoneState(row, col) != Enums.StoneState.None &&
            CurrentTurn == Enums.PlayerType.PlayerA) return;
        // 첫수 및 중복 확인
        if ((SelectedRow != row || SelectedCol != col) && (SelectedRow != -1 && SelectedCol != -1))
        {
            StoneController.SetStoneState(Enums.StoneState.None, SelectedRow, SelectedCol);
        }

        (SelectedRow, SelectedCol) = (row, col);

        StoneController.SetStoneState(Enums.StoneState.Selected, row, col);
    }

    // 보드에 돌추가 함수
    public void SetNewBoardValue(Enums.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Enums.PlayerType.None) return;

        AudioManager.Instance.PlayStoneSound(); //사운드 추가
        
        switch (playerType)
        {
            case Enums.PlayerType.PlayerA:
                StoneController.SetStoneType(Enums.StoneType.Black, row, col);
                StoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
                _board[row, col] = Enums.PlayerType.PlayerA;
                LastNSelectedSetting(row, col);

                ReplayManager.Instance.RecordStonePlaced(Enums.StoneType.Black, row, col); //기보 데이터 저장
                break;
            case Enums.PlayerType.PlayerB:
                StoneController.SetStoneType(Enums.StoneType.White, row, col);
                StoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
                _board[row, col] = Enums.PlayerType.PlayerB;
                LastNSelectedSetting(row, col);

                ReplayManager.Instance.RecordStonePlaced(Enums.StoneType.White, row, col);
                break;
        }
    }

    // 돌 지우는 함수
    public void RemoveStone(int row, int col)
    {
        _board[row, col] = Enums.PlayerType.None;
        StoneController.SetStoneType(Enums.StoneType.None, row, col);
        StoneController.SetStoneState(Enums.StoneState.None, row, col);
    }

    // 마지막 좌표와 선택 좌표 세팅
    private void LastNSelectedSetting(int row, int col)
    {
        //첫수 확인
        if (_lastRow != -1 || _lastCol != -1)
        {
            StoneController.SetStoneState(Enums.StoneState.None, _lastRow, _lastCol);
        }

        //마지막 좌표 저장
        (_lastRow, _lastCol) = (row, col);

        //선택 좌표 초기화
        (SelectedRow, SelectedCol) = (-1, -1);
    }

    // 게임 끝
    public void EndGame(Enums.GameResult result)
    {
        SetState(null);
        ReplayManager.Instance.SaveReplayDataResult(result);
        //TODO: 게임 종료 후 행동 구현
        ChangeGameInProgress(false);
        // 인게임 버튼 표시
        GameManager.Instance.SetButtonsIndicator(false);
    }

    public void SetLastPositioned(int row, int col)
    {
        _lastRow = row;
        _lastCol = col;
    }
}