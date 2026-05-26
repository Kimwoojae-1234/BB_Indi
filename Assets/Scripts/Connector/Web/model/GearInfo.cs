namespace WebConnector
{
    public class GearInfo
    {
        public long gearSeq { get; set; }
        /// <summary>
        /// 장착된 장비라면 장착된 선수카드 seq
        /// </summary>
        public long cardSeq { get; set; }
        /// <summary>
        /// 장비 아이디
        /// GearUtils.detecGearTypeFrom(gearId) 로 GearType 을 구할 수 있다.
        /// </summary>
        public int gearId { get; set; }
        /// <summary>
        /// 장비 강화 레벨
        /// </summary>
        public int reinforceLev { get; set; }
        /// <summary>
        /// 현재 레벨의 보유 경험치 (총누적이 아님)
        /// </summary>
        public int exp { get; set; }
    }
}