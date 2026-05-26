using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace WebConnector {
    public class GameCardInfo {
        /// <summary>
        /// 실제팀 정보면 userId. 스냅샷이거나 실제팀이 아니면 0
        /// </summary>
        public long teamId { get; set; }
        public long cardSeq { get; set; }
        public int cardId { get; set; }
        /// <summary>
        /// 승급단계
        /// </summary>
        public int grade { get; set; }
        /// <summary>
        /// 강화단계
        /// </summary>
        public int reinforce { get; set; }
        /// <summary>
        /// 경험치값
        /// </summary>
        public int exp { get; set; }
        public int level { get; set; }
        /// <summary>
        /// 시즌모드에서 선발투수의 경우 1번 출전후 4경기 출전 불가 제약을 위한 값.
        /// 출전직후 4로 세팅되며 이후 경기당 1씩 감소하며 0이되면 출전 가능상태
        /// </summary>
        public int bench { get; set; }
        /// <summary>
        /// 잠금 상태. true이면 잠김
        /// </summary>
        public bool lockup { get; set; }
        /// <summary>
        /// 보유 능력치(스탯) 정보. (key: 능력치 코드, value: array of [기본능력치, 성장 능력치, 장비 능력치, 시너지 상승 능력치])
        /// 시너지 상승 능력치는 인게임에만 세팅되며, 아웃게임에서는 0으로 세팅된다.
        /// 클라이언트에서 LineupInfo.synergyUp 값을 통해 처리해야 한다. (라인업을 변경하면 영향받는 선수가 달라지므로 LineupInfo에 저장되어 있음)
        /// </summary>
        public Dictionary<CardAbCode, int[]> abilities { get; set; }
        /// <summary>
        /// 보유 스킬 목록
        /// </summary>
        public List<CardSkill> skills { get; set; }
        /// <summary>
        /// 해당 선수카드에 장착된 장비 목록(gearSeq)
        /// GameInfoBundle.gears 에서 정보를 참조할 수 있다.
        /// </summary>
        public List<long> gears { get; set; }
        /// <summary>
        /// 인게임 전달 정보. 인게임 정보 전달시에만 세팅되는 정보
        /// </summary>
        public Lineup lineup { get; set; }
        /// <summary>
        /// 인게임 전달 정보. 투수인경우 보유 구종 유형에 따른 상세구종 정보
        /// </summary>
        public Dictionary<CardAbCode, string> pitchTypes { get; set; }
        /// <summary>
        /// 선수타입
        /// </summary>
        public PlayerType PlayerType {
            get { return CardUtils.detectPlayerTypeFrom(cardId); }
        }
        public int PlayerId {
            get { return CardUtils.detectPlayerIdFrom(cardId); }
        }
        /// <summary>
        /// 카드 타입
        /// </summary>
        public CardType CardType
        {
            get { return CardUtils.detectCardTypeFrom(cardId); }
        }

        [Obsolete("삭제됨")]
        public Dictionary<int, int[]> abs { get; set; }
        [Obsolete("삭제됨")]
        public int odr { get; set; }
        [Obsolete("삭제됨")]
        public int fatigue { get; set; }
        [Obsolete("삭제됨")]
        public int abPt { get; set; }
        [Obsolete("삭제됨")]
        public int skPt { get; set; }
    }
}
