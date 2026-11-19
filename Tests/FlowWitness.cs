using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Velocity;

// Test controller uses ordinary movement, aiming, shooting, dash and clear inputs.
// It never edits health, enemy state, timers, positions or objective state.
static class FlowWitness
{
    const int Width=53,Height=33;
    static V FindStep(World w,V goal)
    {
        bool[,] blocked=new bool[Width,Height];
        int gx=0,gy=0;float best=float.MaxValue;
        for(int x=0;x<Width;x++)for(int y=0;y<Height;y++){V p=new V(x-26,y-16);blocked[x,y]=w.Stage.Blocked(p,.65f);float d=(p-goal).Length;if(!blocked[x,y]&&d<best){best=d;gx=x;gy=y;}}
        int[,] distance=new int[Width,Height];for(int x=0;x<Width;x++)for(int y=0;y<Height;y++)distance[x,y]=9999;
        var q=new Queue<int>();q.Enqueue(gx+gy*Width);distance[gx,gy]=0;
        int[] dx={1,-1,0,0},dy={0,0,1,-1};
        while(q.Count>0){int item=q.Dequeue(),x=item%Width,y=item/Width;for(int i=0;i<4;i++){int nx=x+dx[i],ny=y+dy[i];if(nx<0||nx>=Width||ny<0||ny>=Height||blocked[nx,ny]||distance[nx,ny]!=9999)continue;distance[nx,ny]=distance[x,y]+1;q.Enqueue(nx+ny*Width);}}
        V target=w.Player;best=float.MaxValue;
        for(int x=Math.Max(0,(int)(w.Player.X+26)-2);x<=Math.Min(Width-1,(int)(w.Player.X+26)+2);x++)for(int y=Math.Max(0,(int)(w.Player.Y+16)-2);y<=Math.Min(Height-1,(int)(w.Player.Y+16)+2);y++){
            if(blocked[x,y]||distance[x,y]>=9999)continue;V p=new V(x-26,y-16);bool visible=true;foreach(var wall in w.Stage.Walls){float t;if(Rules.SegmentBox(w.Player,p,wall,Rules.Radius-.01f,out t)){visible=false;break;}}if(!visible)continue;
            float score=distance[x,y]+(p-w.Player).Length*.55f;if(score<best){best=score;target=p;}
        }
        if((target-goal).Length<1.5f&&!w.Stage.Blocked(goal,.5f))target=goal;
        return (target-w.Player).Unit;
    }
    static bool Visible(World w,V p,V q,float radius=0){foreach(var wall in w.Stage.Walls){float t;if(Rules.SegmentBox(p,q,wall,radius,out t))return false;}return true;}
    public static int Main(string[] args)
    {
        int reaction=args.Length>1?int.Parse(args[1]):1,tries=args.Length>2?int.Parse(args[2]):64;bool mastery=args.Length>3&&args[3]=="mastery";
        var report=new StringBuilder("stage,attempt,won,time,hp,kills,seals,dashes,shots,parries,movingRatio,coreAttacks,mode,mastery,masteryTarget,liveBeamCuts\n");int failed=0;
        for(int stage=0;stage<6;stage++){
            bool won=false;
            for(int attempt=0;attempt<tries&&!won;attempt++){
                World w=new World(stage);var recording=new InputReplay{Stage=stage};int dashes=0,shots=0;w.Event=(kind,p)=>{if(kind=="DASH")dashes++;if(kind=="SHOT")shots++;};V desired=new V();ControlFrame held=new ControlFrame();
                for(int tick=0;tick<13000&&w.State==RunState.Playing;tick++){
                    if(tick%reaction!=0){held.Dash=held.Parry=false;recording.Frames.Add(held);held.Apply(w);continue;}
                    Enemy nearest=null;float nearestD=float.MaxValue;Enemy visible=null;float visibleD=float.MaxValue;
                    foreach(var e in w.Stage.Enemies)if(e.HP>0&&e.Zone<=w.Section){float d=(e.P-w.Player).Length;if(d<nearestD){nearest=e;nearestD=d;}if(d<visibleD&&Visible(w,w.Player,e.P)){visible=e;visibleD=d;}}
                    if(mastery&&w.Stage.Mode==TrialMode.Pursuit){nearest=null;nearestD=999;foreach(var e in w.Stage.Enemies)if(e.HP>0&&e.Kind==5&&e.Zone<=w.Section&&(e.P-w.Player).Length<nearestD){nearest=e;nearestD=(e.P-w.Player).Length;}if(nearest!=null&&Visible(w,w.Player,nearest.P)){visible=nearest;visibleD=nearestD;}}
                    V goal=w.Stage.Exit;
                    Seal seal=w.SealsTaken<3?w.Stage.Seals[w.SealsTaken]:null;float sealD=seal==null?float.MaxValue:(seal.P-w.Player).Length;
                    // Alternate campaign routes to exercise different pressure and timing.
                    if(nearest!=null&&(w.Stage.NeedsClear||(mastery&&w.Stage.Mode==TrialMode.Pursuit)))goal=nearest.P;else if(seal!=null)goal=seal.P;
                    if(seal!=null&&seal.DashGate&&w.RelayReady&&V.Dot(w.Player-seal.P,seal.Normal)>-.8f)goal=seal.P-seal.Normal*2.2f;
                    bool liveWindow=true;
                    if(mastery&&seal!=null&&w.Stage.Mode==TrialMode.Timing&&w.ActiveBeamCuts<2){var beam=w.Stage.Beams[w.SealsTaken];liveWindow=beam.Active(w.Time)&&beam.Phase(w.Time)<3.25f;if(!liveWindow){V tangent=new V(-seal.Normal.Y,seal.Normal.X);goal=seal.P-seal.Normal*4+tangent*((float)Math.Sin(tick*.035+attempt)*3);}}
                    if(tick%4==0)desired=FindStep(w,goal);
                    if(w.Stage.NeedsClear&&visible!=null&&visible.Guarding&&visibleD<7){V toward=(visible.P-w.Player).Unit;V tangent=new V(-toward.Y,toward.X)*(attempt%2==0?1:-1);desired=(tangent+toward*(visibleD>3.5f?.5f:-.6f)).Unit;}
                    V aim=visible!=null?(visible.P-w.Player).Unit:desired;
                    if(reaction>1)aim=V.Angle((float)Math.Atan2(aim.Y,aim.X)+(float)Math.Sin(tick*.037+attempt)*.08f);
                    V move=desired;float best=float.MinValue;bool danger=false;
                    for(int candidate=0;candidate<17;candidate++){
                        V input=candidate==16?new V():V.Angle(candidate*6.283185f/16);V next=Rules.Move(w.Player,input*(Rules.Speed*.16f),w.Stage.Walls,Rules.Radius);
                        float score=V.Dot(input,desired)*2-(next-w.Player-input*(Rules.Speed*.16f)).Length*2;
                        foreach(var b in w.Shots)if(!b.Friendly&&(b.P-w.Player).Length<8){
                            V relative=b.P-w.Player,velocity=b.Velocity-input*Rules.Speed;float time=Rules.Clamp(-V.Dot(relative,velocity)/Math.Max(.001f,V.Dot(velocity,velocity)),0,.23f);float clearance=(relative+velocity*time).Length;
                            if(clearance<.95f)score-=(.95f-clearance)*14;
                            if((b.P+b.Velocity*.045f-w.Player).Length<2.1f)danger=true;
                        }
                        if(nearest!=null&&nearestD<2&&w.Charges==0)score-=Math.Max(0,2-(next-nearest.P).Length)*3;
                        if(score>best){best=score;move=input;}
                    }
                    bool dash=false;
                    if(w.DashLeft<=0&&w.Charges>0){
                        if(liveWindow&&seal!=null&&seal.DashGate&&w.RelayReady&&V.Dot(w.Player-seal.P,seal.Normal)<-.6f&&sealD<5&&Visible(w,w.Player,seal.P+seal.Normal*1.2f,.31f)){move=(seal.P+seal.Normal*1.4f-w.Player).Unit;dash=true;}
                        else if(visible!=null&&!visible.Guarding&&visibleD>1&&visibleD<7.8f&&(w.Stage.NeedsClear||V.Dot((visible.P-w.Player).Unit,desired)>.7f)&&Visible(w,w.Player,visible.P,.31f)){move=(visible.P-w.Player).Unit;dash=true;}
                        else if(danger&&w.ParryCooldown>.01f)dash=true;
                        else if((goal-w.Player).Length>9&&V.Dot(move,desired)>.9f&&tick%(60+attempt*3)==0)dash=true;
                    }
                    bool parry=danger&&w.ClearReady;bool focus=(nearest==null||!w.Stage.NeedsClear)&&(seal==null||!seal.DashGate)&&(goal-w.Player).Length<1.7f;
                    var control=new ControlFrame(move,aim,visible!=null,dash,focus,parry);held=control;recording.Frames.Add(control);control.Apply(w);
                }
                int attacks=0;foreach(var enemy in w.Stage.Enemies)if(enemy.Kind==3)attacks=enemy.Attacks;
                won=w.State==RunState.Won&&(!mastery||(w.MasteryComplete&&w.DamageTaken==0&&w.Time<=w.Stage.Par&&w.MovingTime/Math.Max(.01f,w.Time)>=.78f));report.AppendLine((stage+1)+","+attempt+","+won+","+w.Time.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)+","+w.HP+","+w.Kills+","+w.SealsTaken+","+dashes+","+shots+","+w.Parries+","+(w.MovingTime/Math.Max(.01f,w.Time)).ToString("0.000",System.Globalization.CultureInfo.InvariantCulture)+","+attacks+","+w.Stage.Mode+","+w.MasteryProgress+","+w.MasteryTarget+","+w.ActiveBeamCuts);
                Console.WriteLine("Stage "+(stage+1)+" attempt "+attempt+": "+w.State+" "+w.Time.ToString("0.00")+"s kills "+w.Kills+" seals "+w.SealsTaken+" at "+w.Player.X.ToString("0.00")+","+w.Player.Y.ToString("0.00")+" active "+w.SectionAlive);
                if(won){string folder=Path.GetDirectoryName(Path.GetFullPath(args.Length>0?args[0]:"flow-witness.csv"));recording.Write(Path.Combine(folder,"stage-"+(stage+1)+".replay"));}
            }
            if(!won)failed++;
        }
        File.WriteAllText(args.Length>0?args[0]:"flow-witness.csv",report.ToString());Console.WriteLine("Stages with input-only completion witness: "+(6-failed)+"/6");return failed==0?0:1;
    }
}
