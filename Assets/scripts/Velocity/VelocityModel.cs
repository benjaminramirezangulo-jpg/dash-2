using System;
using System.Collections.Generic;

namespace Velocity
{
    public struct V
    {
        public float X, Y;
        public V(float x, float y) { X=x; Y=y; }
        public float Length => (float)Math.Sqrt(X*X+Y*Y);
        public V Unit => Length>0.00001f ? this/Length : new V();
        public static V operator +(V a,V b)=>new V(a.X+b.X,a.Y+b.Y);
        public static V operator -(V a,V b)=>new V(a.X-b.X,a.Y-b.Y);
        public static V operator *(V a,float s)=>new V(a.X*s,a.Y*s);
        public static V operator /(V a,float s)=>new V(a.X/s,a.Y/s);
        public static float Dot(V a,V b)=>a.X*b.X+a.Y*b.Y;
        public static V Lerp(V a,V b,float t)=>a+(b-a)*t;
        public static V Angle(float a)=>new V((float)Math.Cos(a),(float)Math.Sin(a));
    }
    public struct Wall
    {
        public V P, Size;
        public Wall(float x,float y,float w,float h){P=new V(x,y);Size=new V(w,h);}
        public bool Contains(V p,float r=0)=>Math.Abs(p.X-P.X)<Size.X/2+r && Math.Abs(p.Y-P.Y)<Size.Y/2+r;
    }
    public static class Rules
    {
        public const float Step=1f/120, Speed=20, FocusSpeed=8, DashSpeed=64, DashDuration=.125f,
            Recharge=.62f, Radius=.32f, Acceleration=320, Brake=400, Buffer=.12f;
        public const int MaxBullets=768;
        public static float Clamp(float v,float a,float b)=>Math.Max(a,Math.Min(b,v));
        public static V Approach(V current,V target,float delta){V d=target-current;return d.Length<=delta?target:current+d.Unit*delta;}
        public static V Turn(V current,V target,float radians)
        {
            if(current.Length<.1f)return target.Unit;float a=(float)Math.Atan2(current.Y,current.X),b=(float)Math.Atan2(target.Y,target.X),delta=b-a;
            while(delta>Math.PI)delta-=6.2831853f;while(delta<-Math.PI)delta+=6.2831853f;return V.Angle(a+Clamp(delta,-radians,radians));
        }
        public static bool SegmentBox(V a,V b,Wall wall,float radius,out float time)
        {
            V half=wall.Size*.5f+new V(radius,radius),lo=wall.P-half,hi=wall.P+half,d=b-a;
            float enter=0,exit=1;time=0;
            for(int axis=0;axis<2;axis++){
                float p=axis==0?a.X:a.Y, v=axis==0?d.X:d.Y, mn=axis==0?lo.X:lo.Y,mx=axis==0?hi.X:hi.Y;
                if(Math.Abs(v)<.000001f){if(p<mn||p>mx)return false;continue;}
                float t1=(mn-p)/v,t2=(mx-p)/v;if(t1>t2){float t=t1;t1=t2;t2=t;}
                enter=Math.Max(enter,t1);exit=Math.Min(exit,t2);if(enter>exit)return false;
            }
            time=enter;return true;
        }
        public static bool SegmentCircle(V a,V b,V p,float r,out float time)
        {
            V d=b-a,m=a-p;float aa=V.Dot(d,d),c=V.Dot(m,m)-r*r;time=0;
            if(c<=0)return true;if(aa<.0000001f)return false;
            float bb=V.Dot(m,d),disc=bb*bb-aa*c;if(disc<0)return false;
            time=(-bb-(float)Math.Sqrt(disc))/aa;return time>=0&&time<=1;
        }
        public static V Move(V pos,V delta,List<Wall> walls,float radius)
        {
            // Float rounding can leave a body exactly on an expanded face. Separate
            // that contact before sweeping so moving away never returns a t=0 hit.
            for(int pass=0;pass<2;pass++)foreach(Wall w in walls)if(w.Contains(pos,radius+.00005f)){
                float halfWidth=w.Size.X*.5f+radius,halfHeight=w.Size.Y*.5f+radius;V d=pos-w.P;
                if(halfWidth-Math.Abs(d.X)<halfHeight-Math.Abs(d.Y))pos.X=w.P.X+(d.X<0?-1:1)*(halfWidth+.001f);
                else pos.Y=w.P.Y+(d.Y<0?-1:1)*(halfHeight+.001f);
            }
            float first=1;foreach(Wall w in walls){float t;if(SegmentBox(pos,pos+delta,w,radius,out t))first=Math.Min(first,Math.Max(0,t-.0001f));}
            if(first>=1)return pos+delta;
            V reached=pos+delta*first,remain=delta*(1-first);
            V x=new V(remain.X,0),y=new V(0,remain.Y);float tx=1,ty=1;
            foreach(Wall w in walls){float t;if(SegmentBox(reached,reached+x,w,radius,out t))tx=Math.Min(tx,Math.Max(0,t-.0001f));}
            reached+=x*tx;
            foreach(Wall w in walls){float t;if(SegmentBox(reached,reached+y,w,radius,out t))ty=Math.Min(ty,Math.Max(0,t-.0001f));}
            return reached+y*ty;
        }
        public static bool Visible(V a,V b,List<Wall> walls,float radius=0)
        {foreach(var wall in walls){float t;if(SegmentBox(a,b,wall,radius,out t))return false;}return true;}
    }
    public sealed class Enemy
    {
        public V P,Home,Aim;public int Kind,HP,MaxHP,Zone,LastDash=-1,CorePhase,Attacks;public float Clock,Phase,Flash,Stagger,Vulnerable;public bool CounterShield;
        public Enemy(V p,int kind,int hp,float delay){P=Home=p;Kind=kind;HP=MaxHP=hp;Clock=delay;}
        public float Radius=>Kind==3?1.15f:Kind==5?.65f:.52f;
        public bool Guarding=>(Kind==0&&Clock>.48f&&Stagger<=0)||((Kind==3||CounterShield)&&Vulnerable<=0);
    }
    public sealed class Shot { public V P,Previous,Velocity;public bool Friendly,Grazed;public float Life;public int Damage; }
    public sealed class Seal
    {
        public V P,Normal;public float HalfWidth=1.4f;public bool Taken;public int Kind;
        public Seal(float x,float y,int kind){P=new V(x,y);Kind=kind;}
        public bool DashGate=>Normal.Length>.5f;
        public bool Crossed(V before,V after)
        {
            if(!DashGate)return (after-P).Length<1.15f;
            float a=V.Dot(before-P,Normal),b=V.Dot(after-P,Normal);if(a>0||b<=0||b-a<.00001f)return false;
            V intersection=V.Lerp(before,after,-a/(b-a)),tangent=new V(-Normal.Y,Normal.X);
            return Math.Abs(V.Dot(intersection-P,tangent))<=HalfWidth;
        }
    }
    public enum TrialMode { Chain,Hairpin,Counter,Timing,Pursuit,Core }
    public sealed class Beam
    {
        public V A,B;public float Offset,ArmedAt=-1;public int Zone;public bool Triggered;
        public Beam(V a,V b,float offset,int zone=0){A=a;B=b;Offset=offset;Zone=zone;}
        public float Phase(float time)=>Triggered?(ArmedAt<0?0:(time-ArmedAt+1.8f)%3.6f):(time+Offset)%3.6f;
        public bool Active(float time)=>Phase(time)>=2.7f;
        public bool Warning(float time)=>Phase(time)>=1.8f&&!Active(time);
        public bool Touches(V point,float radius){V d=B-A;float t=Rules.Clamp(V.Dot(point-A,d)/Math.Max(.001f,V.Dot(d,d)),0,1);return (point-(A+d*t)).Length<radius+.12f;}
    }
    public sealed class Stage
    {
        public string Name,Lesson,Signature;public float Limit,Par;public V Spawn,Exit;public TrialMode Mode;
        public bool NeedsClear=>Mode!=TrialMode.Timing&&Mode!=TrialMode.Pursuit;
        public List<Wall> Walls=new List<Wall>();public List<Enemy> Enemies=new List<Enemy>();public List<Seal> Seals=new List<Seal>();public List<Beam> Beams=new List<Beam>();public List<V> Route=new List<V>();
        public static readonly string[] Names={"SLIPSTREAM","CROSSCUT","THE SPINDLE","RAZORWIRE","REDLINE","TERMINAL VELOCITY"};
        public static readonly string[] MasteryDescriptions={"S: BUILD A FIVE-KILL CHAIN","S: CUT ALL THREE DIRECTIONAL RIBBONS","S: BREAK THREE SHIELDS WITH RETURNED FIRE","S: DASH THROUGH TWO LIVE BEAMS","S: COUNTER ALL THREE RUSHERS","S: BREAK ALL THREE CORE PHASES"};
        void Add(float x,float y,int kind,int zone,float delay=1)
        {
            V p=new V(x,y);if(Blocked(p,.8f))throw new InvalidOperationException("Obstructed authored enemy: "+Name+" "+x+","+y);
            Enemies.Add(new Enemy(p,kind,kind==3?60:kind==5?4:kind==2||kind==4?2:3,delay){Zone=zone});
        }
        void Relay(float x,float y){Seals.Add(new Seal(x,y,Seals.Count));}
        public static Stage Create(int index)
        {
            if(index<0||index>5)throw new ArgumentOutOfRangeException(nameof(index));
            Stage s=new Stage{Name=Names[index],Mode=(TrialMode)index,Limit=50+index*6,Par=new[]{8f,12f,18f,12f,12f,20f}[index],Spawn=new V(-23,-10),Exit=new V(23,10),
                Lesson=new[]{"CUT. RECHARGE. KEEP THE LINE.","BRAKE LATE. TURN HARD.","PARRY THE RING. BREAK THE ORBIT.","READ THE WINDUP. CUT THE BEAM.","THE HUNTERS HAVE LEARNED TO TURN.","THREE SECTORS. ONE LAST BREATH."}[index],
                Signature=new[]{"DASH CHAINS","HAIRPIN SPRINT","RADIAL PARRY","LASER TIMING","PURSUIT CIRCUIT","THREE-PHASE CORE"}[index]};
            s.Walls.AddRange(new[]{new Wall(0,-16.5f,54,1),new Wall(0,16.5f,54,1),new Wall(-26.5f,0,1,34),new Wall(26.5f,0,1,34)});
            switch(index){
                case 0:
                    s.Walls.AddRange(new[]{new Wall(-8,-9,2,14),new Wall(-8,13,2,6),new Wall(7,-1,2,17),new Wall(17,-4,9,2)});
                    s.Relay(-15,-5);s.Relay(0,7);s.Relay(18,9);
                    s.Add(-19,-3,0,0);s.Add(-13,1,2,0);s.Add(-18,5,0,0);
                    s.Add(-3,5,0,1);s.Add(2,10,1,1);s.Add(2,1,2,1);
                    s.Add(14,11,0,2);s.Add(21,2,2,2);s.Add(16,5,1,2);
                    s.Route.AddRange(new[]{s.Spawn,new V(-17,-7),new V(-12,2),new V(-6,4),new V(1,8),new V(7,11),new V(18,9),s.Exit});break;
                case 1:
                    s.Spawn=new V(-23,12);s.Walls.AddRange(new[]{new Wall(-9,6,2,20),new Wall(7,-6,2,20),new Wall(-18,0,8,2),new Wall(16,4,7,2)});
                    s.Relay(-14,-10);s.Relay(0,11);s.Relay(19,-9);
                    s.Add(-21,-5,0,0);s.Add(-14,-6,4,0);s.Add(-12,-12,2,0);
                    s.Add(-4,8,4,1);s.Add(2,10,5,1);s.Add(1,3,0,1);s.Add(-4,-4,2,1);
                    s.Add(14,-7,5,2);s.Add(22,-3,4,2);s.Add(21,1,0,2);s.Add(13,-12,2,2);
                    s.Route.AddRange(new[]{s.Spawn,new V(-23,-3),new V(-14,-10),new V(-9,-7),new V(-2,-3),new V(0,11),new V(7,8),new V(12,0),new V(19,-9),new V(23,0),s.Exit});break;
                case 2:
                    s.Spawn=new V(-23,0);s.Exit=new V(23,0);s.Walls.AddRange(new[]{new Wall(-8,0,2,15),new Wall(8,0,2,15),new Wall(0,6,8,2),new Wall(0,-6,8,2)});
                    s.Relay(-14,11);s.Relay(0,-11);s.Relay(17,10);
                    s.Add(-17,7,1,0);s.Add(-20,12,1,0);s.Add(-12,6,2,0);s.Add(-18,-5,2,0);
                    s.Add(0,-10,1,1);s.Add(-4,-12,4,1);s.Add(4,-11,1,1);s.Add(0,0,2,1);
                    s.Add(15,5,1,2);s.Add(21,8,1,2);s.Add(12,12,5,2);s.Add(20,-5,2,2);
                    s.Route.AddRange(new[]{s.Spawn,new V(-16,11),new V(-8,11),new V(-5,0),new V(-6,-10),new V(0,-11),new V(6,-10),new V(11,0),new V(17,10),s.Exit});break;
                case 3:
                    s.Spawn=new V(-23,-11);s.Exit=new V(23,-11);s.Walls.AddRange(new[]{new Wall(-8,3,2,18),new Wall(8,-3,2,18),new Wall(-18,-1,7,2),new Wall(18,0,7,2)});
                    s.Relay(-15,11);s.Relay(0,-11);s.Relay(17,10);
                    s.Add(-21,4,4,0);s.Add(-13,7,4,0);s.Add(-18,12,0,0);s.Add(-23,0,2,0);
                    s.Add(-3,-8,4,1);s.Add(3,-12,5,1);s.Add(1,-2,0,1);s.Add(-3,4,2,1);
                    s.Add(13,8,4,2);s.Add(21,5,5,2);s.Add(21,12,1,2);s.Add(13,-6,4,2);
                    s.Beams.Add(new Beam(new V(-23,2),new V(-10,2),0,0));s.Beams.Add(new Beam(new V(-6,-5),new V(6,-5),1.4f,1));s.Beams.Add(new Beam(new V(10,3),new V(24,3),.8f,2));
                    s.Route.AddRange(new[]{s.Spawn,new V(-23,1),new V(-15,11),new V(-12,-9),new V(0,-11),new V(5,8),new V(12,10),new V(17,10),new V(23,-4),s.Exit});break;
                case 4:
                    s.Spawn=new V(-23,10);s.Walls.AddRange(new[]{new Wall(-17,0,12,2),new Wall(-7,-7,2,12),new Wall(0,7,10,2),new Wall(8,-5,2,15),new Wall(18,0,10,2)});
                    s.Relay(-14,-11);s.Relay(1,12);s.Relay(19,-9);
                    s.Add(-22,-5,5,0);s.Add(-13,-7,2,0);s.Add(-10,-12,2,0);s.Add(-12,5,0,0);
                    s.Add(-3,12,5,1);s.Add(5,11,2,1);s.Add(2,2,2,1);s.Add(2,-5,1,1);s.Add(-3,-8,4,1);
                    s.Add(15,-7,5,2);s.Add(21,-4,2,2);s.Add(12,-11,2,2);s.Add(22,6,4,2);s.Add(14,6,2,2);
                    s.Route.AddRange(new[]{s.Spawn,new V(-24,-3),new V(-14,-11),new V(-7,-14),new V(-2,-2),new V(-7,10),new V(1,12),new V(9,7),new V(12,-5),new V(19,-9),new V(24,3),s.Exit});break;
                case 5:
                    s.Exit=new V(23,-10);s.Walls.AddRange(new[]{new Wall(-8,0,2,18),new Wall(7,0,2,17),new Wall(-18,-2,8,2),new Wall(16,-5,6,2),new Wall(19,8,2,5)});
                    s.Relay(-15,11);s.Relay(0,-11);s.Relay(20,12);
                    s.Add(-22,3,5,0);s.Add(-14,6,1,0);s.Add(-20,10,4,0);s.Add(-12,-5,2,0);
                    s.Add(-4,-10,1,1);s.Add(3,-12,5,1);s.Add(2,0,4,1);s.Add(-3,5,2,1);s.Add(3,10,1,1);
                    s.Add(16,2,3,2,1.5f);s.Add(12,8,4,2);s.Add(23,4,5,2);s.Add(12,-9,2,2);
                    s.Beams.Add(new Beam(new V(-6,0),new V(5,0),0,1));s.Beams.Add(new Beam(new V(10,-2),new V(24,-2),1,2));
                    s.Route.AddRange(new[]{s.Spawn,new V(-24,1),new V(-15,11),new V(-8,12),new V(-3,0),new V(0,-11),new V(7,-12),new V(11,-1),new V(15,10),new V(20,12),new V(24,6),s.Exit});break;
            }
            if(s.Mode==TrialMode.Hairpin){
                s.Seals[0].P=new V(-9,-7);s.Seals[0].Normal=new V(1,0);
                s.Seals[1].P=new V(7,8);s.Seals[1].Normal=new V(1,0);
                s.Seals[2].P=new V(23,2);s.Seals[2].Normal=new V(0,1);
            }
            if(s.Mode==TrialMode.Counter)foreach(var e in s.Enemies)if(e.Kind==1)e.CounterShield=true;
            if(s.Mode==TrialMode.Timing){
                for(int i=0;i<3;i++){s.Beams[i].Triggered=true;s.Seals[i].P=(s.Beams[i].A+s.Beams[i].B)*.5f;s.Seals[i].HalfWidth=(s.Beams[i].B-s.Beams[i].A).Length*.5f;s.Seals[i].Normal=new V(0,i==2?-1:1);}
            }
            return s;
        }
        public bool Blocked(V p,float r){foreach(Wall w in Walls)if(w.Contains(p,r))return true;return false;}
        int[,] field;bool[,] walkable;readonly Queue<int> navigationQueue=new Queue<int>(1800);V fieldTarget=new V(999,999);
        static readonly int[] neighborX={1,-1,0,0},neighborY={0,0,1,-1};
        public V Navigate(V from,V target)
        {
            if(Rules.Visible(from,target,Walls,.50f))return (target-from).Unit;
            if(field==null||(fieldTarget-target).Length>1){
                fieldTarget=target;
                if(field==null){field=new int[53,33];walkable=new bool[53,33];for(int x=0;x<53;x++)for(int y=0;y<33;y++)walkable[x,y]=!Blocked(new V(x-26,y-16),.66f);}
                int gx=0,gy=0;float best=9999;
                for(int x=0;x<53;x++)for(int y=0;y<33;y++){field[x,y]=9999;V p=new V(x-26,y-16);float d=(p-target).Length;if(d<best&&walkable[x,y]){best=d;gx=x;gy=y;}}
                navigationQueue.Clear();navigationQueue.Enqueue(gx+gy*53);field[gx,gy]=0;
                while(navigationQueue.Count>0){int n=navigationQueue.Dequeue(),x=n%53,y=n/53;for(int i=0;i<4;i++){int a=x+neighborX[i],b=y+neighborY[i];if(a<0||a>52||b<0||b>32||field[a,b]!=9999||!walkable[a,b])continue;field[a,b]=field[x,y]+1;navigationQueue.Enqueue(a+b*53);}}
            }
            float score=99999;V next=from;
            for(int x=Math.Max(0,(int)(from.X+26)-2);x<=Math.Min(52,(int)(from.X+26)+2);x++)for(int y=Math.Max(0,(int)(from.Y+16)-2);y<=Math.Min(32,(int)(from.Y+16)+2);y++){
                V p=new V(x-26,y-16);float s=field[x,y]+(p-from).Length*.6f;
                if(field[x,y]<9999&&s<score&&Rules.Visible(from,p,Walls,.50f)){score=s;next=p;}
            }
            return (next-from).Unit;
        }
    }
    public enum RunState { Playing,Dead,Won }
    public sealed class World
    {
        public Stage Stage;public int Index,HP,Charges=2,Kills,Score,Combo,BestCombo,Parries,Grazes,Dashes,DamageTaken,GateCuts,ActiveBeamCuts,CounterBreaks,RushCounters;public bool Nightmare;
        public float Time,DashLeft,RechargeClock,DashBuffer,Invulnerable,ShotClock,ComboClock,Recoil,Flow,ParryLeft,ParryCooldown,SpeedPeak,Distance,MovingTime;
        public string DeathCause="";public V DamageFrom,DamageTo,Player,PreviousPlayer,Velocity,Aim=new V(1,0),DashDirection=new V(1,0);public RunState State;
        public readonly List<Shot> Shots=new List<Shot>();readonly Stack<Shot> pool=new Stack<Shot>();
        public readonly float[] Splits=new float[3];public Action<string,V> Event;int dashSerial;
        public World(int index,bool nightmare=false){Index=index;Nightmare=nightmare;Stage=Stage.Create(index);Player=PreviousPlayer=Stage.Spawn;HP=nightmare?1:2;}
        public int SealsTaken {get{int n=0;foreach(var s in Stage.Seals)if(s.Taken)n++;return n;}}
        public int Alive {get{int n=0;foreach(var e in Stage.Enemies)if(e.HP>0)n++;return n;}}
        public int Section=>Math.Min(2,SealsTaken);
        public int SectionAlive {get{int n=0;foreach(var e in Stage.Enemies)if(e.HP>0&&e.Zone==Section)n++;return n;}}
        public bool RelayReady=>!Stage.NeedsClear||SectionAlive==0;
        public bool ExitOpen=>(!Stage.NeedsClear||Alive==0)&&SealsTaken==3;
        public int MasteryTarget=>Stage.Mode==TrialMode.Chain?5:Stage.Mode==TrialMode.Timing?2:3;
        public int MasteryProgress {get{switch(Stage.Mode){case TrialMode.Chain:return BestCombo;case TrialMode.Hairpin:return GateCuts;case TrialMode.Counter:return CounterBreaks;case TrialMode.Timing:return ActiveBeamCuts;case TrialMode.Pursuit:return RushCounters;default:foreach(var e in Stage.Enemies)if(e.Kind==3)return e.CorePhase+(e.HP<=0?1:0);return 0;}}}
        public bool MasteryComplete=>MasteryProgress>=MasteryTarget;
        static readonly string[] masteryNames={"CHAIN","RIBBON CUTS","COUNTER BREAKS","LIVE BEAM CUTS","RUSH COUNTERS","CORE PHASES"};
        public string MasteryName=>masteryNames[Index];
        public string ObjectiveText {get{switch(Stage.Mode){case TrialMode.Hairpin:return SectionAlive>0?"CLEAR GUARDS, THEN DASH THROUGH THE RIBBON":"DASH THROUGH THE RIBBON IN ARROW DIRECTION";case TrialMode.Counter:return "RETURN FIRE TO BREAK THE SPINNER SHIELDS";case TrialMode.Timing:return "BREACH THE BEAM LINE WITH A DASH / KILLS OPTIONAL";case TrialMode.Pursuit:return "RACE THE CHECKPOINTS / HUNTERS DO NOT NEED TO DIE";default:return "CLEAR THE SECTOR, THEN TOUCH ITS RELAY";}}}
        public bool Overdrive=>Flow>=.7f;
        public float NormalSpeed=>Rules.Speed+(Overdrive?4:0);
        public bool ClearReady=>ParryCooldown<=0;
        void Emit(string text,V at){if(Event!=null)Event(text,at);}
        public void AddShot(V p,V direction,float speed,bool friendly)
        {
            if(Shots.Count>=Rules.MaxBullets)return;
            Shot s=pool.Count>0?pool.Pop():new Shot();s.P=s.Previous=p;s.Velocity=direction.Unit*speed;s.Friendly=friendly;s.Grazed=false;s.Damage=friendly&&Overdrive?2:1;s.Life=3;Shots.Add(s);
        }
        public void Damage(string cause="CONTACT",V from=default(V),V to=default(V))
        {
            if(State!=RunState.Playing||DashLeft>0||Invulnerable>0)return;
            HP--;DamageTaken++;Combo=0;Flow=Math.Max(0,Flow-.45f);Invulnerable=.7f;DeathCause=cause;DamageFrom=from;DamageTo=to;Emit("HIT",Player);
            if(HP<=0){State=RunState.Dead;Velocity=new V();Emit("DEATH",Player);}
        }
        public void Hit(Enemy e,int damage)
        {
            if(e.HP<=0)return;
            if(e.CounterShield&&e.Vulnerable<=0){Emit("COUNTERBLOCK",e.P);return;}
            if(e.Kind==3){
                if(e.Vulnerable<=0){Emit("COREBLOCK",e.P);return;}
                int floor=e.CorePhase==0?e.MaxHP*2/3:e.CorePhase==1?e.MaxHP/3:0;e.HP=Math.Max(floor,e.HP-damage);e.Flash=.07f;
                if(e.HP==floor&&e.CorePhase<2){e.CorePhase++;e.Vulnerable=0;e.Clock=e.CorePhase==1?.7f:.55f;Emit("PHASE",e.P);return;}
            }else {e.HP-=damage;e.Flash=.07f;}
            if(e.HP>0){Emit("IMPACT",e.P);return;}
            Kills++;Combo++;BestCombo=Math.Max(Combo,BestCombo);ComboClock=3.25f;Score+=100*Math.Min(Combo,10);
            Flow=Rules.Clamp(Flow+.14f,0,1);RechargeClock+=.25f;Emit("KILL",e.P);
        }
        void FireEnemy(Enemy e)
        {
            float a=(float)Math.Atan2(e.Aim.Y,e.Aim.X),speed=24+Index*1.5f;
            int phase=e.Kind==3?e.CorePhase:0;e.Attacks++;
            if(e.Kind==1||e.Kind==3){int n=e.Kind==3?10+phase*4:8;for(int j=0;j<n;j++){V d=V.Angle(j*6.283185f/n+e.Phase*.65f);AddShot(e.P+d*.75f,d,speed+phase*2,false);}}
            if(e.Kind==4){for(int j=0;j<3;j++)AddShot(e.P+e.Aim*.75f,V.Angle(a+(j-1)*.045f),42,false);}
            else if(e.Kind!=1&&e.Kind!=5){int n=e.Kind==3?3+phase*2:3;for(int j=0;j<n;j++)AddShot(e.P+e.Aim*.75f,V.Angle(a+(j-(n-1)*.5f)*.14f),speed+2,false);}
            e.Clock=(e.Kind==3?1.15f-phase*.18f:e.Kind==4?1.65f:e.Kind==2?1.5f:1.35f)-Index*.025f;
            if(e.Kind==3)e.Vulnerable=.43f;
            Emit("ENEMYSHOT",e.P);
        }
        void UpdateRelay(bool dashing)
        {
            int section=SealsTaken;
            if(section<3){Seal s=Stage.Seals[section];if(!s.Taken&&RelayReady&&(!s.DashGate||dashing)&&s.Crossed(PreviousPlayer,Player)){
                if(s.DashGate){GateCuts++;if(Stage.Mode==TrialMode.Timing&&Stage.Beams[section].Active(Time))ActiveBeamCuts++;Emit("THREAD",s.P);}
                s.Taken=true;Splits[section]=Time;Score+=500;Charges=2;ParryCooldown=0;Flow=Rules.Clamp(Flow+.18f,0,1);Emit("SEAL",s.P);foreach(var e in Stage.Enemies)if(e.Zone==section+1){e.Clock=Math.Max(e.Clock,.85f);if(e.Kind==3)Emit("CORE",e.P);}
            }}
        }
        public void Tick(float dt,V input,V aim,bool fire,bool dash,bool focus,bool clear)
        {
            if(State!=RunState.Playing||dt<=0)return;
            Time+=dt;PreviousPlayer=Player;Invulnerable=Math.Max(0,Invulnerable-dt);Recoil=Math.Max(0,Recoil-dt);ParryCooldown=Math.Max(0,ParryCooldown-dt);ParryLeft=Math.Max(0,ParryLeft-dt);
            if(Time>=Stage.Limit){HP=0;State=RunState.Dead;DeathCause="TIME EXPIRED";Emit("TIME",Player);return;}
            if(aim.Length>.01f)Aim=aim.Unit;if(input.Length>1)input=input.Unit;
            ComboClock-=dt;if(ComboClock<=0)Combo=0;Flow=Math.Max(0,Flow-dt*(input.Length>.1f?.025f:.18f));
            if(dash)DashBuffer=Rules.Buffer;else DashBuffer=Math.Max(0,DashBuffer-dt);
            if(Charges<2){RechargeClock+=dt;if(RechargeClock>=Rules.Recharge){Charges++;RechargeClock-=Rules.Recharge;Emit("READY",Player);}}else RechargeClock=0;
            if(DashLeft<=0&&DashBuffer>0){
                if(Charges>0){Charges--;Dashes++;dashSerial++;DashLeft=Rules.DashDuration;DashDirection=input.Length>.01f?input.Unit:Aim;DashBuffer=0;Emit("DASH",Player);}
                else if(dash)Emit("EMPTY",Player);
            }
            bool dashing=DashLeft>0;
            if(dashing){if(input.Length>.1f)DashDirection=Rules.Turn(DashDirection,input.Unit,dt*5.5f);Velocity=DashDirection*Rules.DashSpeed;}
            else Velocity=Rules.Approach(Velocity,input*(focus?Rules.FocusSpeed:NormalSpeed),(Velocity.Length>NormalSpeed+.1f?650:input.Length>0?Rules.Acceleration:Rules.Brake)*dt);
            float attempted=Velocity.Length*dt;V attemptedVelocity=Velocity;
            Player=Rules.Move(Player,Velocity*dt,Stage.Walls,Rules.Radius);V traveled=Player-PreviousPlayer;float moved=traveled.Length;
            if(Math.Abs(traveled.X)<Math.Abs(attemptedVelocity.X*dt)*.2f)Velocity.X=traveled.X/dt;
            if(Math.Abs(traveled.Y)<Math.Abs(attemptedVelocity.Y*dt)*.2f)Velocity.Y=traveled.Y/dt;
            Distance+=moved;if(moved>dt*2)MovingTime+=dt;SpeedPeak=Math.Max(SpeedPeak,moved/dt);
            if(dashing&&moved<attempted*.25f){DashLeft=0;Emit("WALL",Player);}
            if(clear){if(ParryCooldown<=0){ParryLeft=.14f;ParryCooldown=.8f;Emit("PARRY",Player);}else Emit("PARRYEMPTY",Player);}
            ShotClock-=dt;if(fire&&ShotClock<=0){AddShot(Player+Aim*.7f,Aim,90,true);ShotClock=Overdrive?.065f:.09f;Recoil=.045f;Emit("SHOT",Player);}
            foreach(var beam in Stage.Beams)if(beam.Triggered&&beam.Zone==Section&&beam.ArmedAt<0&&((beam.A+beam.B)*.5f-Player).Length<14)beam.ArmedAt=Time;
            foreach(Beam beam in Stage.Beams)if(beam.Zone==Section&&beam.Active(Time)&&beam.Touches(Player,Rules.Radius))Damage("BEAM",beam.A,beam.B);
            foreach(Enemy e in Stage.Enemies){
                e.Flash=Math.Max(0,e.Flash-dt);e.Stagger=Math.Max(0,e.Stagger-dt);e.Vulnerable=Math.Max(0,e.Vulnerable-dt);if(e.HP<=0||e.Zone>Section)continue;
                float distance=(e.P-Player).Length;if(e.Stagger>0)continue;
                if(e.Kind==2&&distance<16)e.P=Rules.Move(e.P,Stage.Navigate(e.P,Player)*(6.5f+Index*.4f)*dt,Stage.Walls,.52f);
                if(e.Kind==5&&e.Clock<0){e.P=Rules.Move(e.P,e.Aim*34*dt,Stage.Walls,.62f);if(e.Clock<-.24f)e.Clock=1.3f;}
                if((e.P-Player).Length<e.Radius+Rules.Radius+.03f){
                    if(dashing){if(e.LastDash!=dashSerial){
                        e.LastDash=dashSerial;
                        if(e.Guarding&&(e.Kind==3||e.CounterShield||V.Dot(DashDirection,e.Aim)<-.55f)){DashLeft=0;Velocity=DashDirection*-6;Invulnerable=Math.Max(Invulnerable,.12f);Emit(e.CounterShield?"COUNTERBLOCK":e.Kind==3?"COREBLOCK":"BLOCK",e.P);}
                        else {Hit(e,e.Kind==3?8:9);if(e.HP<=0){Charges=Math.Min(2,Charges+1);if(e.Kind==5)RushCounters++;}Flow=Rules.Clamp(Flow+.1f,0,1);}
                    }}
                    else if(ParryLeft>0&&e.Kind==5){e.Stagger=.75f;Hit(e,4);if(e.HP<=0)RushCounters++;Parries++;Flow=1;Emit("PERFECT",e.P);}
                    else Damage(e.Kind==5?"RUSHER":"CONTACT",e.P,Player);
                }
                e.Phase+=dt;
                if(distance>11.5f||!Rules.Visible(e.P,Player,Stage.Walls,.04f)){e.Clock=Math.Max(.65f,e.Clock);continue;}
                float tell=e.Kind==4?.6f:e.Kind==5?.65f:.48f;
                if(e.Clock>tell)e.Aim=Rules.Turn(e.Aim,(Player-e.P).Unit,dt*(e.Kind==0?2:6));
                e.Clock-=dt;
                if(e.Clock<=0&&e.Kind!=5)FireEnemy(e);
            }
            for(int i=Shots.Count-1;i>=0;i--){
                Shot s=Shots[i];s.Previous=s.P;V end=s.P+s.Velocity*dt;float first=1;Enemy target=null;bool hitPlayer=false,remove=false;
                if(!s.Friendly&&ParryLeft>0&&(s.P-Player).Length<2.3f&&Rules.Visible(Player,s.P,Stage.Walls,.01f)){
                    s.Friendly=true;s.Damage=3;V dir=Aim;Enemy nearest=null;float best=20;
                    foreach(var e in Stage.Enemies)if(e.HP>0&&e.Zone<=Section){float d=(e.P-Player).Length;if(d<best&&Rules.Visible(Player,e.P,Stage.Walls)){best=d;nearest=e;}}
                    if(nearest!=null)dir=(nearest.P-s.P).Unit;s.Velocity=dir*72;end=s.P+s.Velocity*dt;Parries++;Charges=Math.Min(2,Charges+1);Flow=1;Score+=75;Emit("PERFECT",s.P);
                }
                foreach(Wall w in Stage.Walls){float t;if(Rules.SegmentBox(s.P,end,w,.08f,out t)&&t<first){first=t;remove=true;}}
                if(s.Friendly){foreach(Enemy e in Stage.Enemies){float t;if(e.HP>0&&e.Zone<=Section&&Rules.SegmentCircle(s.P,end,e.P,e.Radius+.06f,out t)&&t<=first){first=t;target=e;remove=true;}}}
                else {
                    float t;if(Rules.SegmentCircle(s.P-PreviousPlayer,end-Player,new V(),Rules.Radius+.08f,out t)&&t<=first){first=t;hitPlayer=true;remove=true;}
                    else if(!s.Grazed&&!dashing&&Velocity.Length>12&&Rules.SegmentCircle(s.P,end,Player,.95f,out t)){s.Grazed=true;Grazes++;Flow=Rules.Clamp(Flow+.08f,0,1);Score+=25;Emit("GRAZE",Player);}
                }
                s.P=V.Lerp(s.P,end,first);s.Life-=dt;
                if(target!=null){
                    if(target.CounterShield&&s.Damage<3)Emit("COUNTERBLOCK",target.P);
                    else if(target.Kind==3&&target.Guarding)Emit("COREBLOCK",target.P);
                    else if(target.Guarding&&s.Damage<3&&V.Dot(s.Velocity.Unit,target.Aim)<-.55f)Emit("BLOCK",target.P);
                    else {if(s.Damage>=3){target.Stagger=.35f;target.Vulnerable=.5f;}bool wasCounter=target.CounterShield;Hit(target,s.Damage);if(wasCounter&&target.HP<=0)CounterBreaks++;}
                }
                if(hitPlayer)Damage("BULLET",s.Previous,s.Previous+s.Velocity.Unit*8);
                if(remove||s.Life<=0){pool.Push(s);Shots.RemoveAt(i);}
            }
            UpdateRelay(dashing);
            if(dashing){DashLeft=Math.Max(0,DashLeft-dt);if(DashLeft<dt*.001f)DashLeft=0;}
            if(State==RunState.Playing&&ExitOpen&&(Player-Stage.Exit).Length<1.3f){State=RunState.Won;Score+=(int)Math.Max(0,(Stage.Limit-Time)*50);Emit("WIN",Player);}
        }
        public void RestorePractice(int section)
        {
            section=(int)Rules.Clamp(section,0,2);for(int i=0;i<section;i++){Stage.Seals[i].Taken=true;foreach(var e in Stage.Enemies)if(e.Zone==i)e.HP=0;}
            Player=PreviousPlayer=section==0?Stage.Spawn:Stage.Seals[section-1].P;HP=Nightmare?1:2;Charges=2;Flow=0;Shots.Clear();Invulnerable=.8f;
        }
    }
}
