using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Listener dynamics plus opt-in capture of this game's mix. No microphone/desktop audio.
public sealed class VelocityAudioCapture : MonoBehaviour
{
    readonly object gate=new object();List<float> samples;volatile bool recording;int channels=2,rate;float gain=1,release;
    public float CapturedPeak {get;private set;}
    void Awake(){rate=AudioSettings.outputSampleRate;release=1-Mathf.Exp(-1f/(rate*.08f));}
    public void Begin(){lock(gate){samples=new List<float>(2000000);CapturedPeak=0;recording=true;}}
    void OnAudioFilterRead(float[] data,int channelCount)
    {
        for(int i=0;i<data.Length;i+=channelCount){float peak=0;for(int c=0;c<channelCount;c++)peak=Math.Max(peak,Math.Abs(data[i+c]));float target=peak>.88f?.88f/peak:1;gain=target<gain?target:gain+(1-gain)*release;
            for(int c=0;c<channelCount;c++){data[i+c]*=gain;if(recording)CapturedPeak=Math.Max(CapturedPeak,Math.Abs(data[i+c]));}}
        if(recording)lock(gate){channels=channelCount;if(samples.Count<rate*channelCount*120)samples.AddRange(data);}
    }
    public bool Finish(string path)
    {
        recording=false;float[] data;lock(gate)data=samples==null?new float[0]:samples.ToArray();if(data.Length==0)return false;
        using(var b=new BinaryWriter(File.Create(path))){b.Write(Encoding.ASCII.GetBytes("RIFF"));b.Write(36+data.Length*2);b.Write(Encoding.ASCII.GetBytes("WAVEfmt "));b.Write(16);b.Write((short)1);b.Write((short)channels);b.Write(rate);b.Write(rate*channels*2);b.Write((short)(channels*2));b.Write((short)16);b.Write(Encoding.ASCII.GetBytes("data"));b.Write(data.Length*2);foreach(float value in data)b.Write((short)(Mathf.Clamp(value,-1,1)*32767));}return true;
    }
}
