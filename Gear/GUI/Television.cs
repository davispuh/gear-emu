using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Gear.PluginSupport;
using Gear.EmulationCore;

namespace Gear.GUI
{
    class Television : PluginBase
    {
        const double ColorCarrier = 1.0 / 3579545.0;
        const double SampleTime = ColorCarrier / 16.0;  // Ammmount of time for 1/16th of a color phase
        const double RoundSquare = ColorCarrier / 64.0;
        const int PixelPitch = 3;                       // Number of pixels to throw away

        private double LastTime;
        private double SampleError;
        private int PixelError;
        private Bitmap Picture;

        private double LowPassGray;
        private double RoundedChroma;
        private double[] BackLog;
        private int BackLogIndex;

        private double SyncTime;
        private bool MidRaster;

        private int Voltage;

        private int RasterX, RasterY;

        private bool Syncing;


        public override string Title
        {
            get
            {
                return "Television";
            }
        }
        
        public Television()
        {
            Picture = new Bitmap(910, 600, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        
            LastTime = 0;
            RasterX = RasterY = 0;
            LowPassGray = 0;
            BackLog = new double[9];
            BackLogIndex = 0;
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.DrawImage(Picture, 0, 0);
        }

        public override void Repaint(bool force) 
        {
            CreateGraphics().DrawImage(Picture, 0, 0); 
        }

        public override void PresentChip(Propeller host)
        {
            host.NotifyOnPins(this);            
        }

        private Color HSVtoRGB( double h, double s, double v )
        {
	        int i;
	        double f, p, q, t;

            v *= 255;

	        if( s == 0 ) {
                return Color.FromArgb( (int)v,(int)v,(int)v );
	        }

	        h *= 5.0 / (2.0 * Math.PI);
            while (h < 0.0)
                h += 5;

            i = (int)h;
            f = h - i;			// factorial part of h
	        p = v * ( 1 - s );
	        q = v * ( 1 - s * f );
	        t = v * ( 1 - s * ( 1 - f ) );

	        switch( i ) {
		        case 0:
                    return Color.FromArgb((int)v, (int)t, (int)p);
		        case 1:
                    return Color.FromArgb((int)q, (int)v, (int)p);
		        case 2:
                    return Color.FromArgb((int)p, (int)v, (int)t);
		        case 3:
                    return Color.FromArgb((int)p, (int)q, (int)v);
		        case 4:
                    return Color.FromArgb((int)t, (int)p, (int)v);
		        default:		// case 5:
                    return Color.FromArgb((int)v, (int)p, (int)q);
	        }

        }

        public override void OnPinChange(double time, PinState[] pins)
        {
            double delta = time - LastTime;
            LastTime = time;

            // Run lowpass on signal to get the current gray level
            if (!Syncing)
            {
                double luma = (Voltage / 7.0);

                // Find the number of samples to display (with taking error into account
                SampleError += delta / SampleTime;
                int samples = (int)SampleError;
                SampleError -= samples;
                
                while (samples-- > 0)
                {
                    const double ratioLowPass = SampleTime / (ColorCarrier/2 + SampleTime);
                    LowPassGray = luma * ratioLowPass + LowPassGray * (1 - ratioLowPass);

                    const double ratioRoundSquare = SampleTime / (RoundSquare + SampleTime);
                    RoundedChroma = luma * ratioRoundSquare + RoundedChroma * (1 - ratioRoundSquare);

                    BackLog[BackLogIndex++] = RoundedChroma;
                    if (BackLogIndex >= BackLog.Length)
                        BackLogIndex = 0;

                    if (PixelError-- > 0)
                    {
                        continue;
                    }
                    PixelError = PixelPitch;

                    double minAmp = 8;
                    double maxAmp = -1;

                    for (int i = 0; i < BackLog.Length; i++)
                    {
                        if (minAmp > BackLog[i])
                            minAmp = BackLog[i];
                        if (maxAmp < BackLog[i])
                            maxAmp = BackLog[i];
                    }                    

                    double saturation = (maxAmp - minAmp);
                    double difference = 1.0;
                    double chroma = 0;

                    for (int i = 0; i < BackLog.Length; i++)
                    {
                        double idxDifference = Math.Abs( BackLog[i] - saturation / 2 );

                        if (idxDifference < difference)
                        {
                            chroma = i / 16.0;
                            difference = idxDifference;
                        }
                    }                    
                    
                    if (RasterX < Picture.Width)
                    {
                        // Proper color screen
                        Picture.SetPixel(RasterX++, RasterY, 
                            HSVtoRGB(
                                0.0,//chroma,
                                // Limit by current gray output
                                saturation,
                                // Overcharge the black level
                                LowPassGray * 0.90 + 0.10 )
                            );
                    }
                }
            }

            Voltage =
                ((pins[12] == PinState.OUTPUT_HI) ? 1 : 0) +
                ((pins[13] == PinState.OUTPUT_HI) ? 2 : 0) +
                ((pins[14] == PinState.OUTPUT_HI) ? 4 : 0);

            if (Voltage == 0)
            {
                if (!Syncing)
                {
                    SyncTime = time;
                    Syncing = true;
                }

                return;
            }
            else
            {
                if (Syncing)
                {
                    double syncClocks = (time - SyncTime) / SampleTime;

                    // We found a HSYNC pulse
                    if (syncClocks < 300)
                    {
                        RasterX = 0;
                        SampleError = 0;

                        RasterY += 2;

                        if (RasterY >= Picture.Height)
                            RasterY = 0;
                    }
                    // Vertical sync pulse
                    else if( RasterY > 100 )
                    {
                        RasterY = MidRaster ? 1 : 0;
                        MidRaster = !MidRaster;
                    }

                    Syncing = false;
                }
            }
        }
    }
}
