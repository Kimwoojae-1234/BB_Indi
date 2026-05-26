
using System;

namespace WebConnector {
    /// <summary>
    /// 우편함 정보
    /// </summary>
    public class PostInfo {
        /// <summary>
        /// 우편 Seq
        /// </summary>
        public long postSeq { get; set; }
        /// <summary>
        /// 우편 제목
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 우편 메세지
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 기한
        /// </summary>
        public DateTime dueDate { get; set; }

        

        /// <summary>
        /// 우편함 첨부 아이템
        /// </summary>
        [Obsolete("삭제됨")]
        public class AttachItem {
            private int itemId;
            private int cnt;

            public AttachItem(string attach) {
                string[] arr = attach.Split(':');
                this.itemId = int.Parse(arr[0]);
                this.cnt = int.Parse(arr[1]);
            }

            public int ItemId { get { return itemId; } }
            public int Cnt { get { return cnt; } }
        }
    }
}