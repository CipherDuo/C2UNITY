using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO reformat as modelViewPresenter
public static class VideoUtility
{
    public static byte[] MultiCompress(byte[] data, int times)
    {
        for (int i = 0; i < times; i++)
        {
            data = CLZF2.Compress(data);
        }
        return data;
    }

    public static byte[] MultiDecompress(byte[] data, int times)
    {
        for (int i = 0; i < times; i++)
        {
            data = CLZF2.Decompress(data);
        }
        return data;
    }
}
