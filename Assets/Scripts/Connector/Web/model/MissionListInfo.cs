using System.Collections.Generic;

namespace WebConnector {
    public class MissionListInfo {
        public List<MissionInfo> mainMissions { get; set; }
        public List<MissionInfo> dailyMissions { get; set; }
    }
}