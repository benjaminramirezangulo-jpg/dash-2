using System;
using System.IO;
using UnityEngine;
using Velocity;

public sealed class VelocityReplayPlayer : MonoBehaviour
{
    VelocityGame game;InputReplay replay;int frame;float accumulator,end;
    string captures;int captureCounter;VelocityAudioCapture audioCapture;bool finished;
    public void Begin(VelocityGame target,string path)
    {
        game=target;
        try{replay=InputReplay.Read(path);game.Save.nightmare=replay.Nightmare;game.ExternalDrive=true;game.ShowcaseMode=true;Application.runInBackground=true;Screen.SetResolution(1280,800,false);game.StartRun(replay.Stage);string[] args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="--capture-dir"){captures=args[i+1];Directory.CreateDirectory(captures);audioCapture=game.GetComponent<VelocityAudioCapture>();audioCapture.Begin();}}
        catch(Exception e){Debug.LogError("Replay failed: "+e);Application.Quit(2);}
    }
    void Update()
    {
        if(replay==null)return;
        if(frame>=replay.Frames.Count){
            if(!finished){finished=true;Debug.Log("REPLAY RESULT: "+game.World.State+" time="+game.World.Time+" hp="+game.World.HP+" parries="+game.World.Parries);}
            end+=Time.unscaledDeltaTime;
            if(captures!=null&&end<1.5f&&end>captureCounter*.05f-replay.Frames.Count*Rules.Step)ScreenCapture.CaptureScreenshot(Path.Combine(captures,"frame-"+(captureCounter++).ToString("00000")+".png"));
            if(end>1.6f){if(audioCapture)audioCapture.Finish(Path.Combine(captures,"game-audio.wav"));Application.Quit(game.World.State==RunState.Won?0:1);}return;
        }
        accumulator+=Mathf.Min(Time.unscaledDeltaTime,.1f);
        while(accumulator>=Rules.Step&&frame<replay.Frames.Count){game.ApplyRecorded(replay.Frames[frame++]);accumulator-=Rules.Step;}
        if(captures!=null&&frame>captureCounter*6){ScreenCapture.CaptureScreenshot(Path.Combine(captures,"frame-"+(captureCounter++).ToString("00000")+".png"));}
    }
}
