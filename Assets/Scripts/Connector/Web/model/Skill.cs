using System.Collections.Generic;

namespace WebConnector
{
    public class Skill
    {
        public int id { get; set; }
        public string position { get; set; }
        public List<int?> effects { get; set; }
        public string invokeCondition { get; set; }
        public string validity { get; set; }
        public string restrictionType { get; set; }
        public int restrictionCount { get; set; }
        public List<int?> matchSkills { get; set; }
        public List<int?> invokeRate { get; set; }
        public List<int?> scopeRate { get; set; }
        public List<int?> value { get; set; }
    }
}