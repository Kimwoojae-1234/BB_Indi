
using System.Collections.Generic;

public class PostData 
{
    public int idx;
    public string title;
    public string content;
    public string inDate;
    public string expirationDate;

    public bool isCanReceive = false;

    public List<AdminPostReward> postReward = new List<AdminPostReward>();

    public override string ToString()
    {
        string result = string.Empty;
        result += $"title : {title}\n";
        result += $"content : {content}\n";
        result += $"inDate : {inDate}\n";

        if (isCanReceive)
        {
            result += "우편아이템\n";
        }
        else
        {
            result += "지원하지 않는 아이템입니다";
        }
        return result ;
    }
}
