using System;

namespace WebConnector {
    public class FriendInfo {
        public long teamId { get; set; }
        public string name { get; set; }
        public TeamCode team { get; set; }
        public int level { get; set; }
        public int teamPw { get; set; }
        /// <summary>
        /// 마지막 로그인 시각
        /// </summary>
        public DateTime lastLogin { get; set; }
        /// <summary>
        /// true이면 오늘 우정포인트 보냈음.
        /// </summary>
        public bool sentPoint { get; set; }
        /// <summary>
        /// 친구관계
        /// </summary>
        public FriendStatus friendSt { get; set; }
    }
}