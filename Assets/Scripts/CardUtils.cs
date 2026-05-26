using WebConnector;

namespace Utils {
    /// <summary>
    /// 선수카드 관련 유틸리티
    /// </summary>
    public class CardUtils {
        /// <summary>
        /// CardId 로부터 선수타입 가져오기
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public static PlayerType detectPlayerTypeFrom(int cardId) {
            int pTypeCode = ((int)(cardId / 1000000)) % 10;
            return (PlayerType)pTypeCode;
        }
        /// <summary>
        /// CardId로부터 PlayerId 가져오기
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public static int detectPlayerIdFrom(int cardId) {
            return (int)(cardId / 100);
        }
        /// <summary>
        /// CardId로부터 연도 (두자리) 가져오기
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public static int detectYearFrom(int cardId) {
            return cardId % 100;
        }
        /// <summary>
        /// cardId 로부터 CardType 구하기
        /// </summary>
        public static CardType detectCardTypeFrom(int cardId)
        {
            return ((cardId % 100) == 50) ? CardType.Legend : CardType.Normal;
        }
        /// <summary>
        /// cardId 로부터 Pitcher인지 구분하기
        /// </summary>
        public static bool isPitcher(int cardId)
        {
            bool ret = false;
            if (detectPlayerTypeFrom(cardId) == PlayerType.Pitcher) ret = true;
            return ret;
        }
    }
}