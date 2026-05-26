using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class MyMath
    {
        public static int mathiSqrt(uint x)
        {
            uint m = 0x40000000;
            uint r = 0, nr;

            do
            {
                nr = m + r;
                r >>= 1;

                if (nr <= x)
                {
                    x -= nr;
                    r += m;
                }

                m >>= 2;
            } while (m != 0);

            return ((int)r);
        }


        public static int[] nSIN = new int[360]
    {
	    0,17,34,52,69,87,104,121,139,156,173,190,207,224,241,258,275,292,309,325,342,358,374,390,406,422,438,453,469,484,499,515,529,544,559,573,587,601,615,629,642,656,669,681,694,707,719,731,743,754,
	    766,777,788,798,809,819,829,838,848,857,866,874,882,891,898,906,913,920,927,933,939,945,951,956,961,965,970,974,978,981,984,987,990,992,994,996,997,998,999,999,1000,999,999,998,997,996,994,992,990,987,
		
	    984,981,978,974,970,965,961,956,951,945,939,933,927,920,913,906,898,891,882,874,866,857,848,838,829,819,809,798,788,777,766,754,743,731,719,707,694,681,669,656,642,629,615,601,587,573,559,544,529,515,
	    499,484,469,453,438,422,406,390,374,358,342,325,309,292,275,258,241,224,207,190,173,156,139,121,104,87,69,52,34,17,0,-17,-34,-52,-69,-87,-104,-121,-139,-156,-173,-190,-207,-224,-241,-258,-275,-292,-309,-325,
	    -342,-358,-374,-390,-406,-422,-438,-453,-469,-484,-500,-515,-529,-544,-559,-573,-587,-601,-615,-629,-642,-656,-669,-681,-694,-707,-719,-731,-743,-754,-766,-777,-788,-798,-809,-819,-829,-838,-848,-857,-866,-874,-882,-891,-898,-906,-913,-920,-927,-933,
	    -939,-945,-951,-956,-961,-965,-970,-974,-978,-981,-984,-987,-990,-992,-994,-996,-997,-998,-999,-999,-1000,-999,-999,-998,-997,-996,-994,-992,-990,-987,-984,-981,-978,-974,-970,-965,-961,-956,-951,-945,-939,-933,-927,-920,-913,-906,-898,-891,-882,-874,
	    -866,-857,-848,-838,-829,-819,-809,-798,-788,-777,-766,-754,-743,-731,-719,-707,-694,-681,-669,-656,-642,-629,-615,-601,-587,-573,-559,-544,-529,-515,-500,-484,-469,-453,-438,-422,-406,-390,-374,-358,-342,-325,-309,-292,-275,-258,-241,-224,-207,-190,
	    -173,-156,-139,-121,-104,-87,-69,-52,-34,-17
    };
        public static int[] nCOS = new int[360]
    {
	    1000,999,999,998,997,996,994,992,990,987,984,981,978,974,970,965,961,956,951,945,939,933,927,920,913,906,898,891,882,874,866,857,848,838,829,819,809,798,788,777,766,754,743,731,719,707,694,681,669,656,
	    642,629,615,601,587,573,559,544,529,515,500,484,469,453,438,422,406,390,374,358,342,325,309,292,275,258,241,224,207,190,173,156,139,121,104,87,69,52,34,17,0,-17,-34,-52,-69,-87,-104,-121,-139,-156,
	    -173,-190,-207,-224,-241,-258,-275,-292,-309,-325,-342,-358,-374,-390,-406,-422,-438,-453,-469,-484,-499,-515,-529,-544,-559,-573,-587,-601,-615,-629,-642,-656,-669,-681,-694,-707,-719,-731,-743,-754,-766,-777,-788,-798,-809,-819,-829,-838,-848,-857,
	    -866,-874,-882,-891,-898,-906,-913,-920,-927,-933,-939,-945,-951,-956,-961,-965,-970,-974,-978,-981,-984,-987,-990,-992,-994,-996,-997,-998,-999,-999,-1000,-999,-999,-998,-997,-996,-994,-992,-990,-987,-984,-981,-978,-974,-970,-965,-961,-956,-951,-945,
	    -939,-933,-927,-920,-913,-906,-898,-891,-882,-874,-866,-857,-848,-838,-829,-819,-809,-798,-788,-777,-766,-754,-743,-731,-719,-707,-694,-681,-669,-656,-642,-629,-615,-601,-587,-573,-559,-544,-529,-515,-500,-484,-469,-453,-438,-422,-406,-390,-374,-358,
	    -342,-325,-309,-292,-275,-258,-241,-224,-207,-190,-173,-156,-139,-121,-104,-87,-69,-52,-34,-17,0,17,34,52,69,87,104,121,139,156,173,190,207,224,241,258,275,292,309,325,342,358,374,390,406,422,438,453,469,484,
	    500,515,529,544,559,573,587,601,615,629,642,656,669,681,694,707,719,731,743,754,766,777,788,798,809,819,829,838,848,857,866,874,882,891,898,906,913,920,927,933,939,945,951,956,961,965,970,974,978,981,
	    984,987,990,992,994,996,997,998,999,999
    };

        public static int COSAll(int nD)
        {
            return nCOS[nD];
            //return MC_mathCos100(nD)*10;
        }
        public static int SINAll(int nD)
        {
            return nSIN[nD];
            //return MC_mathSin100(nD)*10;
        }

        public static int GetScalarVal(int xval, int yval)
        {
            return mathiSqrt((uint)((xval * xval) + (yval * yval)));
        }

        public static int Abs(int value)
        {
            if (value < 0) return -value;
            else return value;
        }


        public static int GetXPos_by_Equation(int yPos, int X1, int Y1, int X2, int Y2)
        {
            int xVal;

            if (X2 == X1 || Y2 == Y1) return X1;

            xVal = ((yPos - (X2 * Y1 - X1 * Y2) / (X2 - X1)) * (X2 - X1)) / (Y2 - Y1);

            return xVal;
        }

        public static int GetYPos_by_Equation(int xPos, int X1, int Y1, int X2, int Y2)//하하호호
        {
            int yVal;

            if (X2 == X1 || Y2 == Y1) return Y1;

            yVal = (Y2 - Y1) * xPos / (X2 - X1) + (X2 * Y1 - X1 * Y2) / (X2 - X1);

            return yVal;
        }


        public static int UTIL_GetRand(int range)
        {
            return Random.Range(0, range - 1);
        }

        public static float UTIL_GetRand(float range)
        {
            return Random.Range(0, range - 1);
        }

        public static int UTIL_GetRandom(int range)
        {
            int r = range - 1;
            return Random.Range(-r, r);
        }




        public static int MinMaxCheck(int val, int min, int max)
        {
            if (val < min) return (min);
            if (val > max) return (max);
            return (val);
        }

        public static float MinMaxCheck(float val, float min, float max)
        {
            if (val < min) return (min);
            if (val > max) return (max);
            return (val);
        }


        public static int Factorial(int value)
        {
            int count = 0;

            for (int i = 1; i <= value; i++)
            {
                count += i;
                //val = (val*2);
            }

            return count;

        }

        /*    public static float Factorial(int value)
            {
                float count = 0;

                for (int i = 1; i <= value; i++)
                {
                    float val = (i / 100.0f);
                    count += val;
                }


                return count;

            }*/

        public static float getSlope(float x1, float x2, float y1, float y2)
        {
            if (x1 == x2) return 99999;
            else if (y1 == y2) return 0;
            else return (y2 - y1) / (x2 - x1);
        }


        public static float getDistance(float x1, float x2, float y1, float y2)
        {
            float disX = (x2 - x1);
            float disY = (y2 - y1);

            return Mathf.Sqrt((disX * disX) + (disY * disY));
        }


        //2차방정식 근의 공식
        public static float getEquation(float a, float b, float c, bool bBig)
        {
            //2차방정식의 근의 공식 (bBig값이 true이면 큰값을 리턴)
            float val1;
            float bb4ac = b * b - 4 * a * c;
            if (bb4ac < 0) return 0;
            float _4abc = (bBig == true ? 1 : -1) * Mathf.Sqrt(bb4ac);
            val1 = (-b + _4abc) / (2 * a);
            return val1;
        }


        public static float getCircleEquation(float xy, float radius)
        {
            return Mathf.Sqrt(radius * radius - xy * xy);
        }

        public static float getEllipseEquation(float x, float y, float a, float b)
        {
            return ((x * x) / (a * a) + (y * y) / (b * b));
        }

        public static int SetMinMax(int value, int min, int max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
        public static float SetMinMax(float value, float min, float max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }

        public static bool Half()
        {
            if (Random.Range(0, 100) < 50) return true;
            else return false;
        }

        public static int Percent()
        {
            return Random.Range(0, 100);
        }

        public static float PercentF()
        {
            return Random.Range(0.0f, 1.0f);
        }


        public static float Round2(float num)
        {
            float foo = Mathf.Round(num * 100);
            return foo / 100;

        }
    }
}