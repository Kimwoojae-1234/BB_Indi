namespace WebConnector
{
    /// <summary>
    /// 선수강화 결과
    /// </summary>
    public class ReinforceResult
    {
        /// <summary>
        /// 강화후 재화 잔액
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 강화성공인경우 강화후 cardInfo (abilities,skillIds 변경)
        /// 강화실패인경우 null
        /// </summary>
        public GameCardInfo cardInfo { get; set; }
    }
}