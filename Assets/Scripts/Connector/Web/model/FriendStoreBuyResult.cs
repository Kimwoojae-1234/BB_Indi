using System.Collections.Generic;

namespace WebConnector {
    public class FriendStoreBuyResult {
        /// <summary>
        /// 구매 후 총재화 (갱신용)
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 구매후 지급된 아이템 총갯수 (갱신용)
        /// </summary>
        public Dictionary<int, int> items { get; set; }

        private List<string> _assetExprs;
        public List<string> assetExprs {
            set {
                this._assetExprs = value;
            }
        }

        
    }
}