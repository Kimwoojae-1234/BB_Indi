namespace WebConnector {
    public class MissionInfo {
        /// <summary>
        /// 미션 아이디
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 현재까지 달성중인 값
        /// </summary>
        public int attainVal { get; set; }
        /// <summary>
        /// 목표 달성값
        /// </summary>
        public int goal { get; set; }
        /// <summary>
        /// 해당 미션 최종 완료 여부
        /// </summary>
        public bool rwdComplete { get; set; }
    }
}