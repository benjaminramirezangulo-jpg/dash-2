using System;
using System.Collections.Generic;
using System.IO;

namespace Velocity
{
    public struct ControlFrame
    {
        public V Move,Aim;public bool Fire,Dash,Focus,Parry;
        public ControlFrame(V move,V aim,bool fire,bool dash,bool focus,bool parry){Move=move;Aim=aim;Fire=fire;Dash=dash;Focus=focus;Parry=parry;}
        public void Apply(World world){world.Tick(Rules.Step,Move,Aim,Fire,Dash,Focus,Parry);}
    }
    public sealed class InputReplay
    {
        public int Stage;public bool Nightmare;public readonly List<ControlFrame> Frames=new List<ControlFrame>();
        public void Write(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using(var b=new BinaryWriter(File.Create(path))){b.Write(0x44534833);b.Write(Stage);b.Write(Nightmare);b.Write(Frames.Count);foreach(var f in Frames){b.Write(f.Move.X);b.Write(f.Move.Y);b.Write(f.Aim.X);b.Write(f.Aim.Y);b.Write((byte)((f.Fire?1:0)|(f.Dash?2:0)|(f.Focus?4:0)|(f.Parry?8:0)));}}
        }
        public static InputReplay Read(string path)
        {
            using(var b=new BinaryReader(File.OpenRead(path))){
                if(b.ReadInt32()!=0x44534833)throw new InvalidDataException("Not a version 3 replay");
                var result=new InputReplay{Stage=b.ReadInt32(),Nightmare=b.ReadBoolean()};int n=b.ReadInt32();
                if(result.Stage<0||result.Stage>5||n<0||n>24000||b.BaseStream.Length!=13L+17L*n)throw new InvalidDataException("Invalid replay size or stage");
                for(int i=0;i<n;i++){float x=b.ReadSingle(),y=b.ReadSingle(),a=b.ReadSingle(),c=b.ReadSingle();byte flags=b.ReadByte();if(!Finite(x)||!Finite(y)||!Finite(a)||!Finite(c))throw new InvalidDataException("Non-finite replay input");result.Frames.Add(new ControlFrame(new V(x,y),new V(a,c),(flags&1)!=0,(flags&2)!=0,(flags&4)!=0,(flags&8)!=0));}return result;
            }
        }
        static bool Finite(float f)=>!float.IsNaN(f)&&!float.IsInfinity(f)&&Math.Abs(f)<100000;
    }
    public sealed class GhostTrace
    {
        public readonly List<V> Positions=new List<V>();public readonly List<float> Times=new List<float>();public float Duration;
        public void Add(World w){if(Times.Count==0||w.Time-Times[Times.Count-1]>=.032f){Times.Add(w.Time);Positions.Add(w.Player);}Duration=w.Time;}
        public bool At(float time,out V p)
        {
            p=new V();if(Times.Count==0||time>Duration)return false;
            int lo=0,hi=Times.Count-1;while(lo<hi){int mid=(lo+hi)/2;if(Times[mid]<time)lo=mid+1;else hi=mid;}
            if(lo==0){p=Positions[0];return true;}float range=Times[lo]-Times[lo-1];p=V.Lerp(Positions[lo-1],Positions[lo],Rules.Clamp((time-Times[lo-1])/Math.Max(.0001f,range),0,1));return true;
        }
        public void Write(string path)
        {
            string temp=path+".tmp";using(var b=new BinaryWriter(File.Create(temp))){b.Write(0x47485333);b.Write(Duration);b.Write(Times.Count);for(int i=0;i<Times.Count;i++){b.Write(Times[i]);b.Write(Positions[i].X);b.Write(Positions[i].Y);}}
            if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
        }
        public static GhostTrace Read(string path)
        {
            try{using(var b=new BinaryReader(File.OpenRead(path))){if(b.ReadInt32()!=0x47485333)return null;var g=new GhostTrace{Duration=b.ReadSingle()};int n=b.ReadInt32();if(n<1||n>8000||b.BaseStream.Length!=12L+n*12L||float.IsNaN(g.Duration)||g.Duration<=0||g.Duration>200)return null;float last=-1;for(int i=0;i<n;i++){float t=b.ReadSingle(),x=b.ReadSingle(),y=b.ReadSingle();if(float.IsNaN(t)||float.IsNaN(x)||float.IsNaN(y)||t<last||t>200||Math.Abs(x)>28||Math.Abs(y)>18)return null;g.Times.Add(t);g.Positions.Add(new V(x,y));last=t;}return g;}}catch{return null;}
        }
    }
}
