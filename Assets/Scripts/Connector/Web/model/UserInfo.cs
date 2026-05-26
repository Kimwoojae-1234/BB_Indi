using System;
using System.Collections.Generic;
using System.Text;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class UserInfo {
        /// <summary>
        /// 유저 팀 아이디(팀코드가 아님)
        /// </summary>
        public long userId { get; set; }
        /// <summary>
        /// 유저 팀코드
        /// </summary>
        public TeamCode team { get; set; }
        /// <summary>
        /// 유저 팀이름
        /// </summary>
        public string teamName { get; set; }
        /// <summary>
        /// 팀레벨
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// 팀경험치
        /// </summary>
        public int exp { get; set; }
        /// <summary>
        /// 루비
        /// </summary>
        public int ruby { get; set; }
        /// <summary>
        /// 골드
        /// </summary>
        public int gold { get; set; }
        /// <summary>
        /// 마일리지
        /// </summary>
        public int mileage { get; set; }
        /// <summary>
        /// 우정포인트
        /// </summary>
        public int friendPoint { get; set; }
        /// <summary>
        /// VIP Level
        /// </summary>
        public int vipLev { get; set; }
        /// <summary>
        /// 시즌모드 하트정보. array as [보유하트수, 마지막 충전으로부터 지난 초(보유하트수가 max일때는 0)]
        /// </summary>
        public int[] heartInfo { get; set; }
        /// <summary>
        /// 랭킹전 티켓 정보. array as [보유티켓수, 마지막 충전으로부터 지난 초(보유티켓수가 max일때는 0)]
        /// </summary>
        public int[] ticketInfo { get; set; }
        /// <summary>
        /// 카드 슬롯 사이즈
        /// </summary>
        public int cardSlotSize { get; set; }
    }
}