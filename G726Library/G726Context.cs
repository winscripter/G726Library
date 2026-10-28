namespace G726Library;

internal unsafe struct G726Context
{
    public int Yl, Yu, Dms, Dml, Ap, Td;
    public fixed int A[2], Pk[2], Sr[2], B[6], Dq[6];

    public G726Context()
    {
        Yl = 34816;
        Yu = 544;

        for (int i = 0; i < 2; i++)
        {
            Sr[i] = 32;
            A[i] = 0;
            Pk[i] = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            Dq[i] = 32;
            B[i] = 0;
        }
    }

    public readonly int GetZeroPredictor()
    {
        int sezi = G726Shared.FloatingMultiply(B[0] >> 2, Dq[0]);
        
        for (int i = 1; i < 6; i++)
        {
            sezi += G726Shared.FloatingMultiply(B[i] >> 2, Dq[i]);
        }

        return sezi;
    }

    public readonly int GetTwoPolePredictor() => G726Shared.FloatingMultiply(A[1] >> 2, Sr[1]) + G726Shared.FloatingMultiply(A[0] >> 2, Sr[0]);

    public readonly int GetStepSize()
    {
        if (Ap >= 256)
        {
            return Yu;
        }
        else
        {
            int y = Yl >> 6;
            int dif = Yu - y;
            int al = Ap >> 2;

            if (dif > 0)
            {
                y += (dif * al) >> 6;
            }
            else if (dif < 0)
            {
                y += ((dif * al) + 0x3F) >> 6;
            }

            return y;
        }
    }

    public void Update(int codeSize, int y, int wi, int fi, int dq, int sr, int dqsez)
    {
        int tr, a2p;

        int pk0 = (dqsez < 0) ? 1 : 0;

        int mag = dq & 0x7FFF;

        int ylint = Yl >> 15;
        int ylfrac = (Yl >> 10) & 0x1F;
        int thr1 = (32 + ylfrac) << ylint;
        int thr2 = (ylint > 9) ? 31 << 10 : thr1;
        int dqthr = (thr2 + (thr2 >> 1)) >> 1;
        tr = Td == 0 ? 0 : mag <= dqthr ? 0 : 1;

        Yu = y + ((wi - y) >> 5);

        if (Yu < 544)
        {
            Yu = 544;
        }
        else if (Yu > 5120)
        {
            Yu = 5120;
        }

        Yl += Yu + ((-Yl) >> 6);

        if (tr == 1)
        {
            this.A[0] = 0;
            this.A[1] = 0;
            this.B[0] = 0;
            this.B[1] = 0;
            this.B[2] = 0;
            this.B[3] = 0;
            this.B[4] = 0;
            this.B[5] = 0;
            a2p = 0;
        }
        else
        {
            int pks1 = pk0 ^ this.Pk[0];

            a2p = this.A[1] - (this.A[1] >> 7);
            if (dqsez != 0)
            {
                int fa1 = (pks1 != 0) ? this.A[0] : -this.A[0];
                if (fa1 < -8191)
                {
                    a2p -= 0x100;
                }
                else if (fa1 > 8191)
                {
                    a2p += 0xFF;
                }
                else
                {
                    a2p += fa1 >> 5;
                }

                if ((pk0 ^ this.Pk[1]) != 0)
                {
                    if (a2p <= -12160)
                    {
                        a2p = -12288;
                    }
                    else if (a2p >= 12416)
                    {
                        a2p = 12288;
                    }
                    else
                    {
                        a2p -= 0x80;
                    }
                }
                else if (a2p <= -12416)
                {
                    a2p = -12288;
                }
                else if (a2p >= 12160)
                {
                    a2p = 12288;
                }
                else
                {
                    a2p += 0x80;
                }
            }

            this.A[1] = a2p;

            this.A[0] -= this.A[0] >> 8;
            if (dqsez != 0)
            {
                this.A[0] = (pks1 == 0 ? 192 : -192);
            }

            int a1ul = 15360 - a2p;
            if (this.A[0] < -a1ul)
            {
                this.A[0] = -a1ul;
            }
            else if (this.A[0] > a1ul)
            {
                this.A[0] = a1ul;
            }

            for (int cnt = 0; cnt < 6; cnt++)
            {
                this.B[cnt] -= (codeSize == 5 ? this.B[cnt] >> 9 : this.B[cnt] >> 8);

                if ((dq & 0x7FFF) != 0)
                {
                    this.B[cnt] += ((dq ^ this.Dq[cnt]) >= 0) ? 128 : -128;
                }
            }
        }

        for (int cnt = 5; cnt > 0; cnt--)
        {
            this.Dq[cnt] = this.Dq[cnt - 1];
        }

        if (mag == 0)
        {
            this.Dq[0] = (dq >= 0) ? 0x20 : 0xFC20;
        }
        else
        {
            int exp = G726Shared.QuantizeWithinRange(mag, G726Shared.Power2Table, 15);
            this.Dq[0] = (short)((dq >= 0) ?
                (exp << 6) + ((mag << 6) >> exp) :
                (exp << 6) + ((mag << 6) >> exp) - 0x400);
        }

        this.Sr[1] = this.Sr[0];
        if (sr == 0)
        {
            this.Sr[0] = 0x20;
        }
        else if (sr > 0)
        {
            int exp = G726Shared.QuantizeWithinRange(sr, G726Shared.Power2Table, 15);
            this.Sr[0] = (exp << 6) + ((sr << 6) >> exp);
        }
        else if (sr > -32768)
        {
            mag = -sr;
            int exp = G726Shared.QuantizeWithinRange(mag, G726Shared.Power2Table, 15);
            this.Sr[0] = (exp << 6) + ((mag << 6) >> exp) - 0x400;
        }
        else
        {
            this.Sr[0] = 0xFC20;
        }

        this.Pk[1] = this.Pk[0];
        this.Pk[0] = pk0;

        Td = tr == 1 ? 0 : a2p < -11776 ? 1 : 0;

        Dms += (fi - Dms) >> 5;
        Dml += (((fi << 2) - Dml) >> 7);

        if (tr == 1)
        {
            Ap = 256;
        }
        else if (y < 1536)
        {
            Ap += (0x200 - Ap) >> 4;
        }
        else if (Td == 1)
        {
            Ap += (0x200 - Ap) >> 4;
        }
        else if (Math.Abs((Dms << 2) - Dml) >= (Dml >> 3))
        {
            Ap += (0x200 - Ap) >> 4;
        }
        else
        {
            Ap += (-Ap) >> 4;
        }
    }
}
