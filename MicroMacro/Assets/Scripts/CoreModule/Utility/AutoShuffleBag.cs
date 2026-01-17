using System;
using System.Collections.Generic;

/// <summary>
/// 空になっても自動補充される重複なし乱数生成器
/// </summary>
public class AutoShuffleBag
{
    // 生成する数値の開始値
    private int minimumValue;
    // 生成する数値の終了値
    private int maximumValue;
    // 候補となる数字のリスト
    private List<int> availableNumbers;
    // 乱数生成器
    private Random random;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="minimumValue">開始値</param>
    /// <param name="maximumValue">終了値</param>
    public AutoShuffleBag(int minimumValue, int maximumValue)
    {
        // 引数とメンバ変数の名前が同じなので、thisをつけて区別します
        this.minimumValue = minimumValue;
        this.maximumValue = maximumValue;
        
        // インスタンス作成時のみvarを使用
        random = new Random();
        availableNumbers = new List<int>();

        Refill();
    }

    /// <summary>
    /// 次のランダムな数値を取得します
    /// </summary>
    public int Next()
    {
        // 空なら自動的に補充します
        if (availableNumbers.Count == 0)
            Refill();

        int index = random.Next(availableNumbers.Count);
        int result = availableNumbers[index];

        // 末尾と交換して削除（O(1)）
        int lastIndex = availableNumbers.Count - 1;
        availableNumbers[index] = availableNumbers[lastIndex];
        availableNumbers.RemoveAt(lastIndex);

        return result;
    }

    /// <summary>
    /// 内部リストを再充填します
    /// </summary>
    public void Refill()
    {
        availableNumbers.Clear();

        for (int i = minimumValue; i <= maximumValue; i++)
        {
            availableNumbers.Add(i);
        }
    }
}