namespace WebConnector
{
    /// <summary>
    /// 아이템 사용 결과
    /// </summary>
    public class ItemUseResult
    {
        /// <summary>
        /// 사용후 보유한 아이템 개수
        /// </summary>
        public int holdCnt { get; set; }
        /// <summary>
        /// 해당 아이템의 효과에 대한 결과 객체
        /// </summary>
        public ItemEffectResult effResult { get; set; }
    }
}