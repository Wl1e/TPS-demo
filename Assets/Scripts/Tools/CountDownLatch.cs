public class CountDownLatch
{
    int m_Cnt = 0;
    public int Cnt => m_Cnt;
    public void Increase() => m_Cnt++;
    public void Decrease() => m_Cnt--;
    public bool IsLockd() => m_Cnt != 0;
    public void Reset() => m_Cnt = 0;
}
