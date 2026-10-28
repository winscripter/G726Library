namespace G726Library;

internal unsafe struct G726Context
{
    public int Yl, Yu, Dms, Dml, Ap, Td;
    public fixed int A[2], Pk[2], Sr[2], B[6], Dq[6];

    public G726Context()
    {
        this.Yl = 34816;
        this.Yu = 544;

        for (int i = 0; i < 2; i++)
        {
            this.Sr[i] = 32;
            this.A[i] = 0;
            this.Pk[i] = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            this.Dq[i] = 32;
            this.B[i] = 0;
        }
    }

    public readonly int GetZeroPredictor() =>
        G726Shared.FloatingMultiply(this.B[0] >> 2, this.Dq[0])
            + G726Shared.FloatingMultiply(this.B[1] >> 2, this.Dq[1])
            + G726Shared.FloatingMultiply(this.B[2] >> 2, this.Dq[2])
            + G726Shared.FloatingMultiply(this.B[3] >> 2, this.Dq[3])
            + G726Shared.FloatingMultiply(this.B[4] >> 2, this.Dq[4])
            + G726Shared.FloatingMultiply(this.B[5] >> 2, this.Dq[5]);

    public readonly int GetTwoPolePredictor() => G726Shared.FloatingMultiply(this.A[1] >> 2, this.Sr[1]) + G726Shared.FloatingMultiply(this.A[0] >> 2, this.Sr[0]);

    public readonly int GetStepSize()
    {
        if (this.Ap >= 256)
        {
            return this.Yu;
        }
        else
        {
            int y = this.Yl >> 6;
            int dif = this.Yu - y;
            int al = this.Ap >> 2;

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

        int ylint = this.Yl >> 15;
        int ylfrac = (this.Yl >> 10) & 0x1F;
        int thr1 = (32 + ylfrac) << ylint;
        int thr2 = (ylint > 9) ? 31 << 10 : thr1;
        int dqthr = (thr2 + (thr2 >> 1)) >> 1;
        tr = this.Td == 0 ? 0 : mag <= dqthr ? 0 : 1;

        this.Yu = y + ((wi - y) >> 5);

        if (this.Yu < 544)
        {
            this.Yu = 544;
        }
        else if (this.Yu > 5120)
        {
            this.Yu = 5120;
        }

        this.Yl += this.Yu + ((-this.Yl) >> 6);

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
                this.A[0] = pks1 == 0 ? 192 : -192;
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
                this.B[cnt] -= codeSize == 5 ? this.B[cnt] >> 9 : this.B[cnt] >> 8;

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

        this.Td = tr == 1 ? 0 : a2p < -11776 ? 1 : 0;

        this.Dms += (fi - this.Dms) >> 5;
        this.Dml += ((fi << 2) - this.Dml) >> 7;

        if (tr == 1)
        {
            this.Ap = 256;
        }
        else if (y < 1536)
        {
            this.Ap += (0x200 - this.Ap) >> 4;
        }
        else if (this.Td == 1)
        {
            this.Ap += (0x200 - this.Ap) >> 4;
        }
        else if (Math.Abs((this.Dms << 2) - this.Dml) >= (this.Dml >> 3))
        {
            this.Ap += (0x200 - this.Ap) >> 4;
        }
        else
        {
            this.Ap += (-this.Ap) >> 4;
        }
    }
}
