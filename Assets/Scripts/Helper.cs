using System.Collections;
using System.Collections.Generic;
using System;

public static class Helper
{

    //FIXME doc
    // n.wikipedia.org/wiki/Fisher–Yates_shuffle
    private static Random rng = new Random();  

    public static void Fisher_Yates_shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}
