using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KOBRecordUtil
{
    public static string GetBattingAvg(float ab, float hit)
    {
        if (ab == 0 || hit == 0)
        {
            return ".000";
        }
        else
        {
            float value = hit / ab;
            if(value >= 1)
            {
                return string.Format("{0:0.000}", value);
            }
            else
            {
                return string.Format("{0:.000}", value);
            }
        }
    }


    public static string GetObp(float pa, float onbase)
    {
        if (pa == 0 || onbase == 0)
        {
            return ".000";
        }
        else
        {
            float value = onbase / pa;
            if (value >= 1)
            {
                return string.Format("{0:0.000}", value);
            }
            else
            {
                return string.Format("{0:.000}", value);
            }
        }
    }


    public static string GetSlg(float ab, float totalbase)
    {
        if (ab == 0 || totalbase == 0)
        {
            return ".000";
        }
        else
        {
            float value = totalbase / ab;
            if (value >= 1)
            {
                return string.Format("{0:0.000}", value);
            }
            else
            {
                return string.Format("{0:.000}", value);
            }
        }
    }

    public static string GetOps(float pa, float onbase, float ab, float totalbase)
    {
        if (ab == 0 || totalbase == 0 || pa == 0 || onbase == 0)
        {
            return ".000";
        }
        else
        {
            float value1 = onbase / pa;
            float value2 = totalbase / ab;
            float value = value1 + value2;
            if (value >= 1)
            {
                return string.Format("{0:0.000}", value);
            }
            else
            {
                return string.Format("{0:.000}", value);
            }
        }
    }

}
