// Copyright (c) 2023-2026, winscripter
// Licensed under the MIT License.
// See LICENSE.txt for license details.

namespace G726Library;

/// <summary>
/// Holds the active G.726 decoding and encoding state.
/// </summary>
internal unsafe struct G726Context
{
    /// <summary>
    /// This state variable contains the locked or steady step size multiplier.
    /// </summary>
    public int Yl;

    /// <summary>
    /// This state variable contains the un-locked or non-steady step size multiplier.
    /// </summary>
    public int Yu;

    /// <summary>
    /// Short-term energy estimate.
    /// </summary>
    public int Dms;

    /// <summary>
    /// Long-term energy estimate.
    /// </summary>
    public int Dml;

    /// <summary>
    /// Linear weighting coefficient of <see cref="Yl"/> and <see cref="Yu"/>.
    /// </summary>
    public int Ap;

    /// <summary>
    /// Delayed tone detect.
    /// </summary>
    public int Td;

    /// <summary>
    /// Coefficients of pole portion of prediction filter.
    /// </summary>
    public fixed int A[2];

    /// <summary>
    /// Signs of previous two samples of a partially reconstructed signal.
    /// </summary>
    public fixed int Pk[2];

    /// <summary>
    /// Previous 6 samples of the quantized difference signal represented in an internal floating point format.
    /// </summary>
    public fixed int Dq[6];

    /// <summary>
    /// Previous 2 samples of the quantized difference signal represented in an internal floating point format.
    /// </summary>
    public fixed int Sr[2];

    /// <summary>
    /// Coefficients of zero portion of prediction filter.
    /// </summary>
    public fixed int B[6];

    /// <summary>
    /// Initializes a new instance of the <see cref="G726Context"/> struct.
    /// </summary>
    public G726Context()
    {
        this.Yl = 34816;
        this.Yu = 544;

        // All values of Sr are set to 32
        this.Sr[0] = 32;
        this.Sr[1] = 32;

        // Similarly, all values of Dq are set to 32
        this.Dq[0] = 32;
        this.Dq[1] = 32;
        this.Dq[2] = 32;
        this.Dq[3] = 32;
        this.Dq[4] = 32;
        this.Dq[5] = 32;
    }

    /// <summary>
    /// Computes the estimated signal from 6-zero predictor.
    /// </summary>
    /// <returns>The zero predicotr.</returns>
    public readonly int GetZeroPredictor() =>
        G726Shared.FloatingMultiply(this.B[0] >> 2, this.Dq[0])
            + G726Shared.FloatingMultiply(this.B[1] >> 2, this.Dq[1])
            + G726Shared.FloatingMultiply(this.B[2] >> 2, this.Dq[2])
            + G726Shared.FloatingMultiply(this.B[3] >> 2, this.Dq[3])
            + G726Shared.FloatingMultiply(this.B[4] >> 2, this.Dq[4])
            + G726Shared.FloatingMultiply(this.B[5] >> 2, this.Dq[5]);

    /// <summary>
    /// Computes the estimated signal from 2-pole predictor.
    /// </summary>
    /// <returns>The 2-pole predictor.</returns>
    public readonly int GetTwoPolePredictor() =>
        G726Shared.FloatingMultiply(this.A[1] >> 2, this.Sr[1])
        + G726Shared.FloatingMultiply(this.A[0] >> 2, this.Sr[0]);

    /// <summary>
    /// Computes the quantizer step size of the adaptive quantizer.
    /// </summary>
    /// <returns>The quantizer step size.</returns>
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

    /// <summary>
    /// Updates the codec state variables for each output code.
    /// </summary>
    /// <param name="codeSize">The code size</param>
    /// <param name="y">Quantizer step size</param>
    /// <param name="wi">Scale factor multiplier</param>
    /// <param name="fi">For long/short term energies</param>
    /// <param name="dq">Quantized prediction difference</param>
    /// <param name="sr">Reconstructed signal.</param>
    /// <param name="dqsez">Difference from 2-pole predictor.</param>
    public void Update(int codeSize, int y, int wi, int fi, int dq, int sr, int dqsez)
    {
        int pk0 = (dqsez < 0) ? 1 : 0;
        int mag = dq & 0x7FFF;

        int ylint = this.Yl >> 15;
        int ylfrac = (this.Yl >> 10) & 0x1F;
        int thr1 = (32 + ylfrac) << ylint;
        int thr2 = (ylint > 9) ? 31 << 10 : thr1;
        int dqthr = (thr2 + (thr2 >> 1)) >> 1;
        int tr = this.Td == 0 ? 0 : mag <= dqthr ? 0 : 1;

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

        int a2p = 0;
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
