// AI에서만 사용하는 상수 모음
public class AIConstants
{
    // 방향 상수
    public static readonly int[][] Directions = new int[][]
    {
        new int[] {1, 0}, // 수직
        new int[] {0, 1}, // 수평
        new int[] {1, 1}, // 대각선 ↘ ↖
        new int[] {1, -1} // 대각선 ↙ ↗
    };
}