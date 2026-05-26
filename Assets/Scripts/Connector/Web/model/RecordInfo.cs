using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 선수카드 기록 정보
    /// </summary>
    public class RecordInfo
    {
        /// <summary>
        /// 투수 기록 (기록이 있는 선수만 존재합니다)
        /// </summary>
        public List<GameRecordPitcher> pitchers { get; set; }
        /// <summary>
        /// 타자 기록 (기록이 있는 선수만 존재합니다)
        /// </summary>
        public List<GameRecordHitter> hitters { get; set; }

        private Dictionary<string, GameRecordPitcher> _pitchersMap;
        /// <summary>
        /// 투수기록 리턴
        /// </summary>
        public GameRecordPitcher GetGameRecordPitcher(long cardSeq, int cardId) {
            if (_pitchersMap == null) {
                _pitchersMap = new Dictionary<string, GameRecordPitcher>();

                if (pitchers != null)
                    pitchers.ForEach(item => _pitchersMap.Add(item.cardSeq + "_" + item.cardId, item));
            }
            string key = cardSeq + "_" + cardId;
            if (_pitchersMap.ContainsKey(key) == true)
            {
                return _pitchersMap[key];
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, GameRecordHitter> _hittersMap;
        /// <summary>
        /// 타자기록 리턴
        /// </summary>
        public GameRecordHitter GetGameRecordHitter(long cardSeq, int cardId) {
            if (_hittersMap == null) {
                _hittersMap = new Dictionary<string, GameRecordHitter>();
                if (hitters != null)
                    hitters.ForEach(item => _hittersMap.Add(item.cardSeq + "_" + item.cardId, item));
            }

            string key = cardSeq + "_" + cardId;
            if (_hittersMap.ContainsKey(key) == true)
            {
                return _hittersMap[key];
            }
            else
            {
                return null;
            }
        }
    }
}