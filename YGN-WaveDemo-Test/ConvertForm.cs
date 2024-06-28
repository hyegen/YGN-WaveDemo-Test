using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics.IntegralTransforms;

namespace YGN_WaveDemo_Test
{
    public partial class ConvertForm : Form
    {
        public ConvertForm()
        {
            InitializeComponent();
        }

        private void btnSelectWaveFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Audio Files|*.m4a;*.wav";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    AnalyzeAudioFile(filePath);
                }
            }
        }
        private void AnalyzeAudioFile(string filePath)
        {
            using (var audioFileReader = new AudioFileReader(filePath))
            {
                var sampleProvider = audioFileReader.ToSampleProvider();
                float[] buffer = new float[audioFileReader.WaveFormat.SampleRate];
                int samplesRead;

                var fftSize = 4096;
                var fftBuffer = new Complex[fftSize];
                int fftPos = 0;
                int fftFull = 0;

                while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int n = 0; n < samplesRead; n++)
                    {
                        fftBuffer[fftPos].X = (float)(buffer[n] * FastFourierTransform.HannWindow(fftPos, fftSize));
                        fftBuffer[fftPos].Y = 0;
                        fftPos++;
                        if (fftPos >= fftSize)
                        {
                            fftPos = 0;
                            fftFull++;
                            if (fftFull > 1)
                            {
                                break;
                            }
                        }
                    }
                }

                FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2.0), fftBuffer);

               // var magnitude = fftBuffer.Select(c => Math.Sqrt(c.X * c.X + c.Y * c.Y)).Take(1500).ToArray();
                var magnitude = fftBuffer.Select(c => Math.Sqrt(c.X * c.X + c.Y * c.Y)).Take(500).ToArray();
                //var frequencies = Enumerable.Range(0, magnitude.Length)
                //                            .Select(i => i * audioFileReader.WaveFormat.SampleRate / fftSize)
                //                            .Take(1500)
                //                            .ToArray();
                var frequencies = Enumerable.Range(0, magnitude.Length)
                                       .Select(i => i * audioFileReader.WaveFormat.SampleRate / fftSize)
                                       .Take(500)
                                       .ToArray();

                chart1.Series["Series1"].Points.Clear();
                for (int i = 0; i < frequencies.Length / 2; i++)
                {
                    chart1.Series["Series1"].Points.AddXY(frequencies[i], magnitude[i]);
                }
            }
        }
    }
}