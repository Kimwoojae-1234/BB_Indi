using WebConnector;

namespace Utils {
    /// <summary>
    /// 아이템관련 유틸리티
    /// </summary>
    public class ItemUtils {
        /// <summary>
        /// 해당 품목의 아이디와 타입으로 아이템 아이디를 만든다.
        /// (ItemID 구조 - 앞 두자리는 ItemType, 뒤 6자리는 해당 아이템의 아이디)
        /// </summary>
        public static int makeItemId(ItemType type, int realItemId) {
            return ((int)type * 100000) + realItemId;
        }

        /// <summary>
        /// ItemId로 부터 ItemType 알아내기
        /// </summary>
        public static ItemType detectItemTypeFrom(int itemId) {
            int typeVal = (int)(itemId / 100000);
            return (ItemType)typeVal;
        }

        /// <summary>
        /// ItemId에서 ItemTypeCode를 뗀 구체적인 아이디 알아내기
        /// </summary>
        public static int detectSpecificId(int itemId) {
            return itemId % 100000;
        }
    }
}