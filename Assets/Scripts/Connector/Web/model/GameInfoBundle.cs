using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 초기로딩 게임데이터 모음
    /// </summary>
    public class GameInfoBundle
    {
        /// <summary>
        /// 구단
        /// </summary>
        public TeamCode team { get; set; }
        /// <summary>
        /// 팀명
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 팀 레벨
        /// </summary>
        public int level { get; set; }
        /// <summary>
        /// 팀 경험치
        /// </summary>
        public int exp { get; set; }
        /// <summary>
        /// 보유 재화 [루비, 골드, 우정포인트]
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 플레이볼 정보 [보유수, 마지막 충전으로부터 지난 초(보유수가 max일때는 0)]
        /// </summary>
        public int[] playballInfo { get; set; }
        /// <summary>
        /// 선수카드 슬롯 사이즈
        /// </summary>
        public int cardSlotSize { get; set; }
        
        [Obsolete("삭제됨")]
        public List<LineupInfo> mainLineup { get; set; }
        [Obsolete("삭제됨")]
        public List<LineupInfo> subLineup { get; set; }
        public LineupType primaryLineup { get; set; }
        /// <summary>
        /// 라인업 정보
        /// </summary>
        public Dictionary<LineupType, List<LineupInfo>> lineups { get; set; }

        /// <summary>
        /// 보유한 모든 선수카드 정보
        /// </summary>
        public List<GameCardInfo> gameCards { get; set; }
        /// <summary>
        /// 보유 아이템 dic of {item_id, count}
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 보유한 모든 장비 정보
        /// </summary>
        public List<GearInfo> gears { get; set; }
        /// <summary>
        /// 상점내 정보
        /// </summary>
        public StoreLobbyInfo storeInfo { get; set; }
        /// <summary>
        /// 특별 훈련 정보
        /// </summary>
        public List<TrainingInfo> trainingInfos { get; set; }
        /// <summary>
        /// 정규시즌 요약 정보, null이면 시즌 시작하지 않음.
        /// </summary>
        public SeasonSummary sspSum { get; set; }
        /// <summary>
        /// 라이브매치
        /// </summary>
        public LivePlaySummary lvpSum { get; set; }
        /// <summary>
        /// 쟁탈전 요약 정보
        /// </summary>
        public RacePlaySummary rcpSum { get; set; }
        /// <summary>
        /// 9회말 2아웃 주간보상 존재 여부
        /// </summary>
        public bool wopRwdWeekly { get; set; }
        /// <summary>
        /// 잭팟 정보. array of [ruby, mileage]
        /// </summary>
        public int[] jackPotInfo { get; set; }
        /// <summary>
        /// null이 아니면 진행중인 라이브 매치가 존재함.
        /// 진행중인 경기에 대해서 취소하려면 RestService.LivePlay_Cancel() 를 호출해주고,
        /// 계속 진행하려면 lvpPairingInfo.newLivePlayPvpService() 를 통해 LivePlayPvpService 를 인게임으로 넘겨준다.
        /// </summary>
        public LivePlayPairingInfo lvpPairingInfo { get; set; }
        /// <summary>
        /// null이 아니면 보유한 정액 상품의 만료시각
        /// </summary>
        public DateTime commuterGoods { get; set; }
        /// <summary>
        /// 가장 최근 우편 시퀀스 번호 (지급된 것 포함)
        /// </summary>
        public long latestPostSeq { get; set; }
}
}