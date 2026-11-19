using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Velocity;

// Opt-in in the real Windows player: --dash-tests D:\path\results
// No network, account, NUnit installation or Editor license is required.
public sealed class VelocityRuntimeTests : MonoBehaviour
{
    VelocityGame game;string directory;int passed,failed;readonly StringBuilder cases=new StringBuilder();
    readonly List<string> errors=new List<string>();
    public void Begin(VelocityGame target,string output){game=target;directory=output;Directory.CreateDirectory(directory);Application.runInBackground=true;Application.logMessageReceived+=OnLog;StartCoroutine(Run());}
    void OnLog(string condition,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(condition+" "+stack);}
    void Check(string name,Func<bool> check){try{if(!check())throw new Exception("Assertion failed");passed++;cases.AppendLine("<testcase name=\""+name+"\" />");Debug.Log("RUNTIME PASS: "+name);}catch(Exception e){failed++;cases.AppendLine("<testcase name=\""+name+"\"><failure>"+System.Security.SecurityElement.Escape(e.ToString())+"</failure></testcase>");Debug.LogWarning("RUNTIME FAIL: "+name+": "+e);}}
    IEnumerator Capture(string name)
    {
        yield return new WaitForEndOfFrame();
        Texture2D shot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(directory,name+".png"),shot.EncodeToPNG());
        Color32[] pixels=shot.GetPixels32();int chromatic=0,white=0,dark=0;foreach(Color32 c in pixels){if(Math.Abs(c.r-c.g)>3||Math.Abs(c.g-c.b)>3)chromatic++;if(c.r>180)white++;if(c.r<45)dark++;}
        Check(name+" monochrome palette",()=>chromatic<pixels.Length*.001f);
        Check(name+" visible ink and negative space",()=>white>pixels.Length*.002f&&dark>pixels.Length*.35f);
        Check(name+" action button labels fit their controls",()=>game.ClippedButtons==0);
        if(name=="01-title"){
            int readable=0;float scale=game.UiScale;
            for(int y=(int)(443*scale);y<(int)(511*scale);y++)for(int x=(int)(66*scale);x<(int)(620*scale);x++)if(pixels[(shot.height-1-y)*shot.width+x].r>=145)readable++;
            Check("instructional subtitle has readable contrast",()=>readable>80);
        }
        Destroy(shot);
    }
    IEnumerator Run()
    {
        // Use an isolated save location, preserving any real progress and settings.
        string realPath=VelocitySave.PathName;VelocitySave.PathName=Path.Combine(directory,"isolated-save.json");
        game.ExternalDrive=true;game.Save.nightmare=false;game.Save.volume=.65f;game.Save.music=.45f;Screen.SetResolution(1280,800,false);yield return new WaitForSecondsRealtime(4);
        game.ScreenName="title";yield return Capture("01-title");
        Check("campaign boots in actual Unity player",()=>game!=null&&Camera.allCamerasCount>0);
        Check("distinct original sound cues are loaded",()=>game.AudioCount>=15);
        Check("120Hz fixed timestep configured",()=>Math.Abs(Time.fixedDeltaTime-Rules.Step)<.00001f);
        foreach(int fps in new[]{30,60,120,240}){
            int rate=fps;
            Check("real frame bridge maintains speed at "+rate+" fps",()=>{game.StartRun(0);game.World.Stage.Enemies.Clear();game.World.Stage.Walls.RemoveRange(4,game.World.Stage.Walls.Count-4);game.World.Player=new V(0,0);for(int i=0;i<rate;i++)game.Advance(1f/rate,new V(1,0),new V(1,0),false,false,false,false);return Math.Abs(game.World.Time-1)<.01f&&Math.Abs(game.World.Player.X-19.4556f)<.15f;});
        }
        Check("dash press survives frame without simulation tick",()=>{game.StartRun(0);game.Advance(1f/240,new V(1,0),new V(1,0),false,true,false,false);game.Advance(1f/240,new V(1,0),new V(1,0),false,false,false,false);return game.World.Charges==1&&game.World.DashLeft>0;});
        Check("frame bridge clamps a long stall",()=>{game.StartRun(0);game.Advance(2,new V(1,0),new V(1,0),false,false,false,false);return game.World.Time<.11f;});
        Check("save round trip",()=>{var s=new VelocitySave{unlocked=4,volume=.4f};s.best[2]=34.5f;s.medals[2]=3;return s.Store()&&VelocitySave.Load().unlocked==4&&VelocitySave.Load().best[2]==34.5f;});
        Check("corrupt save recovers",()=>{File.WriteAllText(VelocitySave.PathName,"broken json");return VelocitySave.Load().unlocked==1;});
        Check("invalid save values sanitized",()=>{var s=new VelocitySave{unlocked=99,volume=float.NaN,shake=9,best=new float[1],medals=null};s.Validate();return s.unlocked==6&&s.volume==.65f&&s.shake==1&&s.best.Length==6&&s.medals.Length==6;});
        Check("save handles unwritable destination",()=>{string old=VelocitySave.PathName;VelocitySave.PathName=directory;bool result=new VelocitySave().Store();VelocitySave.PathName=old;return !result;});
        game.ScreenName="settings";yield return Capture("02-settings");game.ScreenName="levels";yield return Capture("03-stage-select");
        game.ScreenName="how";yield return Capture("07-how-to-flow");
        int[] widths={1280,1600,1920};int[] heights={720,1000,1080};
        for(int i=0;i<3;i++){
            Screen.SetResolution(widths[i],heights[i],false);yield return null;yield return null;
            game.StartRun(0);game.ScreenName="pause";yield return Capture("layout-"+widths[i]+"x"+heights[i]);
            Check("layout scale "+widths[i],()=>game.UiScale>.35f&&game.UiScale<2);
            Check("readable avatar diameter "+widths[i],()=>.8f/game.View.UnitsPerScreenHeight*game.View.Camera.pixelHeight>=21);
        }
        game.StartRun(0);game.ScreenName="play";
        foreach(V corner in new[]{new V(-25.68f,-15.68f),new V(-25.68f,15.68f),new V(25.68f,-15.68f),new V(25.68f,15.68f)}){
            game.World.Player=corner;game.View.ResetCamera();for(int i=0;i<20;i++)yield return null;
            Check("camera keeps edge player fully inside view "+corner.X+" "+corner.Y,()=>{var p=game.View.Camera.WorldToViewportPoint(new Vector3(corner.X,corner.Y,0));return p.x>.025f&&p.x<.975f&&p.y>.025f&&p.y<.975f;});
        }
        Check("world aiming round trip respects camera viewport",()=>{var camera=game.View.Camera;Vector3 p=new Vector3(15,5,0),screen=camera.WorldToScreenPoint(p),back=camera.ScreenToWorldPoint(screen);return Vector3.Distance(p,back)<.001f;});
        Check("practice is isolated from campaign records",()=>{float old=game.Save.best[0];game.StartRun(0,true,1);foreach(var e in game.World.Stage.Enemies)e.HP=0;foreach(var s in game.World.Stage.Seals)s.Taken=true;game.World.Player=game.World.Stage.Exit;game.Advance(Rules.Step,new V(),new V(1,0),false,false,false,false);return game.Save.best[0]==old&&game.Practice;});
        Check("checkpoint retry restores the intended encounter",()=>{game.StartRun(5,true,2);game.Retry();return game.World.SealsTaken==2&&game.World.HP==2&&game.World.Alive>0&&game.Practice;});
        Check("procedural audio has audible unclipped signals and a loop",()=>{return game.GetComponent<VelocitySound>().ValidateSignals();});
        Check("listener mix is isolated from individual audio sources",()=>game.GetComponents<AudioSource>().Length==0);
        game.GetComponent<VelocitySound>().ExportMusic(Path.Combine(directory,"original-score.wav"));
        var mix=game.GetComponent<VelocityAudioCapture>();mix.Begin();for(int i=0;i<30;i++){game.GetComponent<VelocitySound>().Play("PERFECT");game.GetComponent<VelocitySound>().Play("KILL");game.GetComponent<VelocitySound>().Play("HIT");}
        yield return new WaitForSecondsRealtime(.6f);
        Check("actual listener mix captures non-silent game audio",()=>mix.Finish(Path.Combine(directory,"mixed-audio-test.wav"))&&mix.CapturedPeak>.01f);
        Check("simultaneous game feedback stays below clipping",()=>mix.CapturedPeak<=.881f);
        for(int stage=0;stage<6;stage++){
            int index=stage;string path=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(directory)),"stage-"+(stage+1)+".replay");
            Check("normal input replay completes real player stage "+(stage+1),()=>{var replay=InputReplay.Read(path);game.StartRun(index);foreach(var frame in replay.Frames)game.ApplyRecorded(frame);return game.World.State==RunState.Won&&game.World.HP>0&&game.World.SealsTaken==3;});
        }
        for(int i=0;i<6;i++){
            game.StartRun(i);yield return Capture("stage-"+(i+1));
            Check("stage "+(i+1)+" runtime reset",()=>game.World.HP==2&&game.World.Charges==2&&game.World.Time==0);
            Check("stage "+(i+1)+" objective completion",()=>{var w=game.World;foreach(var e in w.Stage.Enemies)for(int hit=0;hit<3&&e.HP>0;hit++){e.Vulnerable=1;w.Hit(e,100);}foreach(var s in w.Stage.Seals){w.Player=s.DashGate?s.P-s.Normal*.3f:s.P;if(s.DashGate){w.DashLeft=.04f;w.DashDirection=s.Normal;}w.Tick(Rules.Step,s.Normal,new V(1,0),false,false,false,false);}w.Player=w.Stage.Exit;w.Tick(Rules.Step,new V(),new V(1,0),false,false,false,false);return w.State==RunState.Won&&w.ExitOpen;});
            Check("stage "+(i+1)+" exposes its own objective and mastery",()=>game.World.ObjectiveText.Length>10&&game.World.MasteryName.Length>3);
        }
        game.StartRun(0);game.ScreenName="pause";
        Check("paused frame bridge rejects movement and dash",()=>{game.Advance(.1f,new V(1,0),new V(1,0),true,true,false,true);return game.World.Time==0&&game.World.Charges==2&&game.World.Shots.Count==0;});
        Check("pause freezes game clock",()=>game.World.Time==0);
        yield return new WaitForSecondsRealtime(.2f);Check("pause stays frozen across rendered frames",()=>game.World.Time==0);
        game.World.Damage();game.World.Invulnerable=0;game.World.Damage();game.ScreenName="dead";yield return Capture("04-death");
        Check("frame bridge exposes real death state",()=>{game.StartRun(0);game.World.HP=1;game.World.Damage();game.Advance(Rules.Step,new V(),new V(),false,false,false,false);return game.ScreenName=="dead";});
        Check("victory unlocks and saves next stage",()=>{game.StartRun(0);foreach(var e in game.World.Stage.Enemies)e.HP=0;foreach(var s in game.World.Stage.Seals)s.Taken=true;game.World.Player=game.World.Stage.Exit;game.Advance(Rules.Step,new V(),new V(1,0),false,false,false,false);return game.ScreenName=="won"&&game.Save.unlocked>=2&&VelocitySave.Load().unlocked>=2;});
        Check("Nightmare records remain separate from Hard",()=>{float hard=game.Save.best[0];game.Save.nightmare=true;game.StartRun(0);foreach(var e in game.World.Stage.Enemies)e.HP=0;foreach(var s in game.World.Stage.Seals)s.Taken=true;game.World.Player=game.World.Stage.Exit;game.Advance(Rules.Step,new V(),new V(1,0),false,false,false,false);bool ok=game.Save.nightmareBest[0]>0&&game.Save.best[0]==hard;game.Save.nightmare=false;return ok;});
        game.ScreenName="won";yield return Capture("06-victory");
        Check("dead retry recreates state",()=>{game.StartRun(0);return game.World.State==RunState.Playing&&game.World.HP==2;});
        // Render a busy final-stage battle while running the same model at 120Hz.
        game.StartRun(5);game.ShowcaseMode=true;game.World.Stage.Limit=120;foreach(var e in game.World.Stage.Enemies)e.Zone=0;
        int frames=0,peakBullets=0,slowFrames=0,startCollections=GC.CollectionCount(0);double total=0,max=0;var frameTimes=new List<float>();
        for(int frame=0;frame<2400;frame++){
            var w=game.World;w.Invulnerable=1; // Soak-only fixture; normal gameplay damage tested separately.
            if(frame%240==0)for(int j=0;j<420;j++){V p=new V(-24+(j%35)*1.4f,-14+(j/35)*2.4f);if(!w.Stage.Blocked(p,.3f)&&(p-w.Player).Length>2)w.AddShot(p,V.Angle(j*.71f),12,false);}
            for(int tick=0;tick<2;tick++)w.Tick(Rules.Step,V.Angle(frame*.025f),V.Angle(frame*.11f),true,frame%70==0&&tick==0,false,false);
            peakBullets=Math.Max(peakBullets,w.Shots.Count);yield return null;float ms=Time.unscaledDeltaTime*1000;if(frame>120){frames++;frameTimes.Add(ms);total+=ms;max=Math.Max(max,ms);if(ms>16.667)slowFrames++;}
        }
        frameTimes.Sort();double p95=frameTimes[(int)(frameTimes.Count*.95)];
        double p99=frameTimes[(int)(frameTimes.Count*.99)];
        File.WriteAllText(Path.Combine(directory,"performance.json"),"{\"frames\":"+frames+",\"averageMs\":"+(total/frames).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"p95Ms\":"+p95.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"p99Ms\":"+p99.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"maxMs\":"+max.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"peakBullets\":"+peakBullets+",\"framesOver16ms\":"+slowFrames+",\"gcCollections\":"+(GC.CollectionCount(0)-startCollections)+"}");
        Check("sustained fixture actually reaches dense projectile load",()=>peakBullets>300);
        Check("rendered stress run above 60 fps average",()=>total/frames<16.667);
        Check("rendered stress run p95 below 25ms",()=>p95<25);
        Check("high speed frame pacing p99 below 16ms",()=>p99<16.667);
        yield return Capture("05-combat-stress");
        Check("no Unity errors or exceptions",()=>errors.Count==0);
        File.WriteAllText(Path.Combine(directory,"runtime-tests.xml"),"<?xml version=\"1.0\"?><testsuite name=\"Unity player integration and style\" tests=\""+(passed+failed)+"\" failures=\""+failed+"\">"+cases+"</testsuite>");
        File.WriteAllText(Path.Combine(directory,"runtime-errors.txt"),string.Join("\n",errors.ToArray()));
        VelocitySave.PathName=realPath;Debug.Log("RUNTIME RESULT: "+passed+" passed, "+failed+" failed");Application.Quit(failed==0?0:1);
    }
    void OnDestroy(){Application.logMessageReceived-=OnLog;}
}
