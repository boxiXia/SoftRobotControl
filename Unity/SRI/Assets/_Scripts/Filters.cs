using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Filters
{
    public class ButterWorth
    {
        public readonly int n;    // Filter order
        private readonly int N;
        public float[] B;
        public float[] A;
        private int k = 0;
        private readonly float[] X;
        private readonly float[] Y;
        public ButterWorth(float[] B_arr, float[] A_arr)
        {
            B = B_arr;
            A = A_arr;
            N = A.Length;
            n = N - 1; // order
            X = new float[N];
            Y = new float[N];
        }
        public float Filtering(float x)
        {
            float y = 0;
            int i;
            int latestIndex = k % N;
            X[latestIndex] = x;
            for (int m = 1; m < N; m++)
            {
                i = k > m ? (k - m) % N : (k + N - m) % N;
                y += B[m] * X[i] - A[m] * Y[i];
            }
            y += B[0] * X[latestIndex];
            Y[latestIndex] = y;
            k++;
            return y;
        }
    }
}
