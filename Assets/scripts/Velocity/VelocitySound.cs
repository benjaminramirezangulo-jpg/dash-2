using System;
using System.Collections.Generic;
using UnityEngine;

// Small original procedural score and transient sounds, generated locally.
public sealed class VelocitySound : MonoBehaviour
{
    VelocityGame game;AudioSource music;readonly AudioSource[] voices=new AudioSource[16];int voice;
    readonly Dictionary<string,AudioClip> cues=new Dictionary<string,AudioClip>();AudioClip score;int scoreStage=-1;
    readonly Dictionary<string,float> nextCue=new Dictionary<string,float>();
    public int CueCount=>cues.Count;
    public void Initialize(VelocityGame target)
    {
        game=target;music=Voice("Music");music.loop=true;
        for(int i=0;i<voices.Length;i++)voices[i]=Voice("Voice "+i);
        cues["SHOT"]=Synth("pulse",.065f,155,30,.2f);
        cues["DASH"]=Synth("cut",.16f,110,600,.7f);
        cues["KILL"]=Synth("break",.16f,80,25,.8f);
        cues["IMPACT"]=Synth("impact",.04f,650,150,.4f);
        cues["PERFECT"]=Synth("parry",.28f,1700,850,.1f);
        cues["PARRY"]=Synth("sweep",.13f,460,920,.35f);
        cues["SEAL"]=Synth("relay",.4f,440,880,.05f);
        cues["HIT"]=Synth("hurt",.23f,160,45,.45f);
        var suppliedHurt=VelocityAudio.Load("hurt");if(suppliedHurt)cues["LEGACYHURT"]=suppliedHurt;
        cues["DEATH"]=Synth("fall",.45f,150,25,.65f);
        cues["EMPTY"]=Synth("empty",.06f,210,180,.15f);
        cues["PARRYEMPTY"]=cues["EMPTY"];cues["WALL"]=cues["IMPACT"];cues["WIN"]=cues["SEAL"];
        cues["BLOCK"]=Synth("guard",.09f,900,450,.25f);
        cues["PHASE"]=Synth("phase",.35f,120,600,.5f);
        cues["ENEMYSHOT"]=Synth("enemy",.07f,320,110,.15f);cues["GRAZE"]=Synth("graze",.04f,1200,1500,.1f);cues["READY"]=Synth("charge",.055f,520,790,.05f);
    }
    AudioSource Voice(string label){var node=new GameObject(label);node.transform.SetParent(transform,false);var source=node.AddComponent<AudioSource>();source.playOnAwake=false;return source;}
    AudioClip Synth(string name,float seconds,float start,float end,float noise)
    {
        const int rate=22050;float[] samples=new float[(int)(rate*seconds)];var random=new System.Random(name.GetHashCode());double phase=0;
        for(int i=0;i<samples.Length;i++){float t=(float)i/samples.Length,f=Mathf.Lerp(start,end,t);phase+=f/rate;float tone=Mathf.Sin((float)phase*6.283185f)+.22f*Mathf.Sin((float)phase*12.56637f);float n=(float)random.NextDouble()*2-1;float envelope=Mathf.Min(1,t*45)*Mathf.Pow(1-t,2);samples[i]=(tone*(1-noise)+n*noise)*envelope*.55f;}
        var clip=AudioClip.Create("Velocity "+name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
    }
    public void Play(string kind)
    {
        AudioClip clip;if(!cues.TryGetValue(kind,out clip))return;var source=voices[voice++%voices.Length];source.pitch=kind=="KILL"?1+Mathf.Min(game.World.Combo,10)*.025f:1;
        float ready;if(nextCue.TryGetValue(kind,out ready)&&Time.unscaledTime<ready)return;
        nextCue[kind]=Time.unscaledTime+(kind=="PERFECT"?.09f:kind=="BLOCK"?.1f:kind=="KILL"?.035f:kind=="ENEMYSHOT"?.05f:.025f);
        source.volume=game.Save.volume*(kind=="SHOT"?.2f:kind=="IMPACT"?.25f:kind=="ENEMYSHOT"?.12f:kind=="GRAZE"?.15f:kind=="LEGACYHURT"?.12f:.65f);source.clip=clip;source.Play();if(kind=="HIT")Play("LEGACYHURT");
    }
    public void Stage(int index)
    {
        if(scoreStage==index){if(!music.isPlaying)music.Play();return;}scoreStage=index;
        if(score)Destroy(score);score=Compose(index);music.clip=score;music.Play();
    }
    AudioClip Compose(int stage)
    {
        const int rate=22050;float beat=60f/(168+stage*3),length=beat*32;float[] data=new float[(int)(length*rate)];var random=new System.Random(1820+stage);int[] notes={0,0,7,3,0,10,7,3};float root=55*Mathf.Pow(2,stage%3/12f);
        for(int i=0;i<data.Length;i++){
            float t=(float)i/rate,b=t/beat,local=(b-Mathf.Floor(b))*beat,eighth=(b*2-Mathf.Floor(b*2))*beat*.5f;
            float kick=Mathf.Sin(6.283185f*(45*local+5*(1-Mathf.Exp(-local*45))))*Mathf.Exp(-local*28)*.32f;
            float hat=((float)random.NextDouble()*2-1)*Mathf.Exp(-eighth*110)*.07f;
            float snare=((int)b%4==2)?((float)random.NextDouble()*2-1)*Mathf.Exp(-local*28)*.16f:0;
            int step=(int)(b*.5f)%8;float freq=root*Mathf.Pow(2,notes[step]/12f),bass=Mathf.Sin(6.283185f*freq*t)*.13f*Mathf.Exp(-local*5);
            float arpFreq=root*4*Mathf.Pow(2,notes[(int)(b*2)%8]/12f);float arp=Mathf.Sin(6.283185f*arpFreq*t)*Mathf.Exp(-eighth*22)*.045f;
            float fade=Mathf.Min(1,i/(rate*.005f))*Mathf.Min(1,(data.Length-1-i)/(rate*.005f));data[i]=Mathf.Clamp(kick+hat+snare+bass+arp,-.8f,.8f)*fade;
        }
        var clip=AudioClip.Create("Velocity / "+stage,data.Length,1,rate,false);clip.SetData(data,0);return clip;
    }
    public bool ValidateSignals()
    {
        foreach(var clip in cues.Values){float[] samples=new float[clip.samples];clip.GetData(samples,0);double energy=0;foreach(float sample in samples){if(float.IsNaN(sample)||Mathf.Abs(sample)>1)return false;energy+=sample*sample;}if(energy/samples.Length<.00001)return false;}
        return score!=null&&score.length>8;
    }
    public void ExportMusic(string path)
    {
        if(score==null)return;float[] data=new float[score.samples];score.GetData(data,0);
        using(var b=new System.IO.BinaryWriter(System.IO.File.Create(path))){b.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));b.Write(36+data.Length*2);b.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));b.Write(16);b.Write((short)1);b.Write((short)1);b.Write(score.frequency);b.Write(score.frequency*2);b.Write((short)2);b.Write((short)16);b.Write(System.Text.Encoding.ASCII.GetBytes("data"));b.Write(data.Length*2);foreach(float f in data)b.Write((short)(Mathf.Clamp(f,-1,1)*32767));}
    }
    void Update(){if(!game)return;bool playing=game.ScreenName=="play";music.volume=Mathf.Lerp(music.volume,game.Save.volume*game.Save.music*(playing?(game.World!=null&&game.World.Overdrive?.8f:.55f):.12f),Time.unscaledDeltaTime*4);}
    void OnDestroy(){if(score)Destroy(score);var seen=new HashSet<AudioClip>();foreach(var clip in cues.Values)if(seen.Add(clip))Destroy(clip);}
}
