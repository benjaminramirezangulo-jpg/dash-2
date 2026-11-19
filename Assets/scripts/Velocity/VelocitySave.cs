using System;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class VelocitySave
{
    public int version=3,unlocked=1;
    public float volume=.65f,shake=.35f,music=.45f;
    public bool reducedFlash,nightmare,ghost=true,aimAssist=true;
    public float[] best=new float[6];
    public int[] medals=new int[6];
    public float[] bestSplits=new float[18];
    public float[] nightmareBest=new float[6],nightmareSplits=new float[18];
    public int[] nightmareMedals=new int[6];
    public static string PathName=Path.Combine(Application.persistentDataPath,"velocity-v3.json");
    public void Validate()
    {
        unlocked=Mathf.Clamp(unlocked,1,6);
        volume=float.IsNaN(volume)||float.IsInfinity(volume)?.65f:Mathf.Clamp01(volume);
        shake=float.IsNaN(shake)||float.IsInfinity(shake)?.35f:Mathf.Clamp01(shake);
        music=float.IsNaN(music)||float.IsInfinity(music)?.45f:Mathf.Clamp01(music);
        if(best==null||best.Length!=6)best=new float[6];if(medals==null||medals.Length!=6)medals=new int[6];
        if(bestSplits==null||bestSplits.Length!=18)bestSplits=new float[18];
        if(nightmareBest==null||nightmareBest.Length!=6)nightmareBest=new float[6];if(nightmareMedals==null||nightmareMedals.Length!=6)nightmareMedals=new int[6];if(nightmareSplits==null||nightmareSplits.Length!=18)nightmareSplits=new float[18];
        for(int i=0;i<6;i++){if(float.IsNaN(best[i])||float.IsInfinity(best[i])||best[i]<0)best[i]=0;medals[i]=Mathf.Clamp(medals[i],0,3);}
        for(int i=0;i<18;i++)if(float.IsNaN(bestSplits[i])||float.IsInfinity(bestSplits[i])||bestSplits[i]<0)bestSplits[i]=0;
        for(int i=0;i<6;i++){if(float.IsNaN(nightmareBest[i])||float.IsInfinity(nightmareBest[i])||nightmareBest[i]<0)nightmareBest[i]=0;nightmareMedals[i]=Mathf.Clamp(nightmareMedals[i],0,3);}
        for(int i=0;i<18;i++)if(float.IsNaN(nightmareSplits[i])||float.IsInfinity(nightmareSplits[i])||nightmareSplits[i]<0)nightmareSplits[i]=0;
    }
    public static VelocitySave Load()
    {
        try{var s=JsonUtility.FromJson<VelocitySave>(File.ReadAllText(PathName));if(s==null)return new VelocitySave();s.Validate();return s;}catch{return new VelocitySave();}
    }
    public bool Store()
    {
        try{Validate();Directory.CreateDirectory(Path.GetDirectoryName(PathName));string temp=PathName+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(this,true));if(File.Exists(PathName))File.Replace(temp,PathName,PathName+".bak");else File.Move(temp,PathName);return true;}catch(Exception e){Debug.LogWarning("Save unavailable: "+e.Message);return false;}
    }
}
