using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebConnector {
    public class TeamInfo {
        public string name { get; set; }
	    public TeamCode team { get; set; }
        /// <summary>
        /// 장착된 세트덱 아이디 목록
        /// </summary>
        public List<int> setdeckIds { get; set; }
        /// <summary>
        /// 보유한 모든선수카드
        /// </summary>
        public List<GameCardInfo> cards { get; set; }
    }
}
