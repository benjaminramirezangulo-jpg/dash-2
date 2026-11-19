using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Velocity;

public sealed class VelocityGame : MonoBehaviour
{
    public World World {get;private set;}
    public VelocitySave Save {get;private set;}
    public VelocityView View {get;private set;}
    public string ScreenName="title";
    public float UiScale=1;
    public bool ExternalDrive,Practice,ShowcaseMode;
    public int AudioCount=>sound==null?0:sound.CueCount;
    public int ClippedButtons {get;private set;}
    public int Checkpoint {get;private set;}
    public InputReplay Recording {get;private set;}
    Texture2D pixel,disc;Font font;VelocitySound sound;
    float accumulator,runClock,toastLeft,emptyLeft,parryEmptyLeft;
    bool dashQueued,parryQueued,focusLost;
    int attempts=1,observedSection;string returnFromSettings="title",toast="";
    string reviewDirectory;int reviewIndex;
    GUIStyle text,button;readonly Dictionary<int,GUIStyle> sizes=new Dictionary<int,GUIStyle>();
    readonly GUIContent measurement=new GUIContent();
    readonly Dictionary<string,GhostTrace> ghosts=new Dictionary<string,GhostTrace>();GhostTrace trace,bestGhost;
    static Color Gray(float x,float a=1)=>new Color(x,x,x,a);
    void Awake()
    {
        Save=VelocitySave.Load();font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pixel=new Texture2D(1,1);pixel.SetPixel(0,0,Color.white);pixel.Apply();
        disc=new Texture2D(32,32,TextureFormat.RGBA32,false);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float a=Mathf.Clamp01(16-Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(16,16)));disc.SetPixel(x,y,new Color(1,1,1,a));}disc.Apply();
        var camera=new GameObject("Ink Camera").AddComponent<Camera>();camera.transform.SetParent(transform);camera.tag="MainCamera";camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Gray(.025f);
        gameObject.AddComponent<AudioListener>();gameObject.AddComponent<VelocityAudioCapture>();View=gameObject.AddComponent<VelocityView>();View.Initialize(this,camera);
        sound=gameObject.AddComponent<VelocitySound>();sound.Initialize(this);
        QualitySettings.vSyncCount=0;Application.targetFrameRate=240;Time.fixedDeltaTime=Rules.Step;Application.runInBackground=false;
        string[] args=Environment.GetCommandLineArgs();
        for(int i=0;i<args.Length;i++){
            if(args[i]=="--dash-tests"&&i+1<args.Length)gameObject.AddComponent<VelocityRuntimeTests>().Begin(this,args[++i]);
            if(args[i]=="--dash-stage"&&i+1<args.Length){int n;if(int.TryParse(args[++i],out n))StartRun(Mathf.Clamp(n-1,0,5));}
            if(args[i]=="--dash-replay"&&i+1<args.Length)gameObject.AddComponent<VelocityReplayPlayer>().Begin(this,args[++i]);
            if(args[i]=="--review-dir"&&i+1<args.Length){reviewDirectory=args[++i];Directory.CreateDirectory(reviewDirectory);}
        }
    }
    string GhostPath(int stage)=>Path.Combine(Path.GetDirectoryName(VelocitySave.PathName),"velocity-v3-"+(World.Nightmare?"nightmare-":"hard-")+"stage-"+stage+".ghost");
    public bool TryGhost(float time,out V p){p=new V();return Save.ghost&&bestGhost!=null&&bestGhost.At(time,out p);}
    public void StartRun(int index,bool practice=false,int checkpoint=0)
    {
        Practice=practice;Checkpoint=practice?Mathf.Clamp(checkpoint,0,2):0;
        World=new World(index,Save.nightmare);if(Practice)World.RestorePractice(Checkpoint);
        World.Event=Feedback;ScreenName="play";accumulator=0;dashQueued=parryQueued=false;runClock=0;toastLeft=emptyLeft=parryEmptyLeft=0;observedSection=World.Section;
        Recording=new InputReplay{Stage=index,Nightmare=Save.nightmare};trace=new GhostTrace();trace.Add(World);
        string path=GhostPath(index);if(!ghosts.TryGetValue(path,out bestGhost)){bestGhost=GhostTrace.Read(path);ghosts[path]=bestGhost;}
        View.ResetCamera();sound.Stage(index);
    }
    public void Retry(){if(World==null)return;attempts++;StartRun(World.Index,Practice,Practice?Checkpoint:0);}
    public void Pause(){if(ScreenName=="play"){ScreenName="pause";accumulator=0;dashQueued=parryQueued=false;}}
    void OnApplicationFocus(bool hasFocus){focusLost=!hasFocus;if(!hasFocus&&!ShowcaseMode)Pause();}
    void Update()
    {
        float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);toastLeft=Mathf.Max(0,toastLeft-dt);emptyLeft=Mathf.Max(0,emptyLeft-dt);parryEmptyLeft=Mathf.Max(0,parryEmptyLeft-dt);
        if(reviewDirectory!=null&&Input.GetKeyDown(KeyCode.F12)){
            string path=Path.Combine(reviewDirectory,"native-"+(reviewIndex++).ToString("00"));ScreenCapture.CaptureScreenshot(path+".png");
            File.WriteAllText(path+".txt",ScreenName+"\n"+(World==null?"no run":World.Time+" "+World.Player.X+" "+World.Player.Y+" "+World.Dashes+" "+World.Parries+" practice="+Practice));
        }
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(ScreenName=="play")Pause();else if(ScreenName=="pause"){focusLost=false;ScreenName="play";}else if(ScreenName=="settings"){Save.Store();ScreenName=returnFromSettings;}else ScreenName="title";
        }
        if(Input.GetKeyDown(KeyCode.R)&&World!=null&&(ScreenName=="play"||ScreenName=="pause"||ScreenName=="dead"||ScreenName=="won"))Retry();
        if(Input.GetKeyDown(KeyCode.C)&&World!=null&&(ScreenName=="dead"||ScreenName=="pause"))StartRun(World.Index,true,Checkpoint);
        if(Input.GetKeyDown(KeyCode.Return)){
            if(ScreenName=="title")StartRun(Save.unlocked-1);
            else if(ScreenName=="dead")Retry();
            else if(ScreenName=="won"){if(World.Index<5)StartRun(World.Index+1,Practice);else ScreenName="title";}
        }
        if(ScreenName!="play"||World==null||focusLost||ExternalDrive)return;
        V input=new V(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));V aim=View.MouseWorld()-World.Player;
        Advance(Time.unscaledDeltaTime,input,aim,Input.GetMouseButton(0),Input.GetKeyDown(KeyCode.Space)||Input.GetMouseButtonDown(1),Input.GetKey(KeyCode.LeftShift),Input.GetKeyDown(KeyCode.F));
    }
    V Assist(V aim)
    {
        if(!Save.aimAssist||aim.Length<.01f)return aim;
        V direction=aim.Unit;float best=.985f;Enemy target=null;
        foreach(var e in World.Stage.Enemies)if(e.HP>0&&e.Zone<=World.Section&&(e.P-World.Player).Length<13){V d=(e.P-World.Player).Unit;float dot=V.Dot(d,direction);if(dot>best&&Rules.Visible(World.Player,e.P,World.Stage.Walls)){best=dot;target=e;}}
        return target==null?aim:V.Lerp(direction,(target.P-World.Player).Unit,.3f).Unit;
    }
    public void Advance(float frameDelta,V input,V aim,bool fire,bool dash,bool focus,bool parry)
    {
        if(ScreenName!="play"||World==null)return;
        if(World.State!=RunState.Playing){ScreenName=World.State==RunState.Won?"won":"dead";return;}
        dashQueued|=dash;parryQueued|=parry;accumulator+=Mathf.Clamp(frameDelta,0,.1f);runClock+=Mathf.Clamp(frameDelta,0,.1f);V adjusted=Assist(aim);
        while(accumulator+0.0000001f>=Rules.Step){
            ApplyRecorded(new ControlFrame(input,adjusted,fire,dashQueued,focus,parryQueued));accumulator-=Rules.Step;dashQueued=parryQueued=false;
            if(World.State!=RunState.Playing){accumulator=0;break;}
        }
    }
    public void ApplyRecorded(ControlFrame frame)
    {
        if(World==null||World.State!=RunState.Playing)return;
        frame.Apply(World);Recording.Frames.Add(frame);trace.Add(World);
        if(World.Section>observedSection){Checkpoint=World.Section;observedSection=World.Section;}
        if(World.State==RunState.Dead)ScreenName="dead";
        if(World.State==RunState.Won){
            ScreenName="won";int i=World.Index;
            if(!Practice&&!ShowcaseMode){
                var times=World.Nightmare?Save.nightmareBest:Save.best;var splits=World.Nightmare?Save.nightmareSplits:Save.bestSplits;var medals=World.Nightmare?Save.nightmareMedals:Save.medals;
                Save.unlocked=Mathf.Max(Save.unlocked,Mathf.Min(6,i+2));bool pb=times[i]<=0||World.Time<times[i];
                if(pb){times[i]=World.Time;for(int s=0;s<3;s++)splits[i*3+s]=World.Splits[s];
                    try{Directory.CreateDirectory(Path.GetDirectoryName(VelocitySave.PathName));trace.Write(GhostPath(i));Recording.Write(Path.ChangeExtension(GhostPath(i),".replay"));ghosts[GhostPath(i)]=trace;}catch(Exception e){Debug.LogWarning("Ghost unavailable: "+e.Message);}}
                medals[i]=Mathf.Max(medals[i],Rank(World));Save.Store();if(pb){toast="PERSONAL BEST";toastLeft=4;}
            }
        }
    }
    public static int Rank(World w){if(w.State!=RunState.Won)return 0;return w.Time<=w.Stage.Par&&w.DamageTaken==0&&w.MasteryComplete&&w.MovingTime/Math.Max(.01f,w.Time)>=.78f?3:w.Time<=w.Stage.Par*1.4f?2:1;}
    void Feedback(string kind,V at)
    {
        View.Effect(kind,at);sound.Play(kind);
        if(kind=="PERFECT"){toast="PERFECT PARRY / OVERDRIVE";toastLeft=.9f;}
        if(kind=="EMPTY")emptyLeft=.3f;if(kind=="PARRYEMPTY")parryEmptyLeft=.3f;
        if(kind=="SEAL"){
            int split=World.SealsTaken-1;float best=(World.Nightmare?Save.nightmareSplits:Save.bestSplits)[World.Index*3+split],delta=best>0?World.Time-best:0;
            toast="SECTOR "+(split+1)+"  /  "+World.Time.ToString("0.00")+"s"+(best>0?"   "+(delta>0?"+":"")+delta.ToString("0.00"):"   CLEAN LINE");toastLeft=1.6f;
        }
        if(kind=="WALL"){emptyLeft=.12f;}
        if(kind=="BLOCK"){toast="GUARDED / FLANK, PARRY, OR WAIT FOR THE OPENING";toastLeft=.8f;}
        if(kind=="CORE"){toast="THE CORE / PARRY THE PULSE. CUT WHEN IT OPENS.";toastLeft=2.5f;}
        if(kind=="COREBLOCK"&&toastLeft<.3f){toast="CORE GUARD / COUNTER AFTER THE PULSE";toastLeft=.8f;}
        if(kind=="PHASE"){toast="CORE / NEXT PHASE";toastLeft=.75f;}
        if(kind=="COUNTERBLOCK"&&toastLeft<.3f){toast="SPINNER SHIELD / RETURN ITS FIRE WITH F";toastLeft=1;}
    }
    void SetupGUI()
    {
        if(text!=null)return;text=new GUIStyle(GUI.skin.label){font=font,richText=false};text.normal.textColor=Color.white;
        button=new GUIStyle(GUI.skin.button){font=font,fontSize=17,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(18,12,0,0)};button.normal.background=pixel;button.normal.textColor=Color.black;button.hover.background=pixel;button.hover.textColor=Color.black;button.active.background=pixel;button.active.textColor=Color.black;
    }
    GUIStyle Style(int size){GUIStyle s;if(!sizes.TryGetValue(size,out s)){s=new GUIStyle(text){fontSize=size};sizes.Add(size,s);}return s;}
    void Label(float x,float y,float w,float h,string value,int size=16,float gray=1){GUI.color=Color.white;var s=Style(size);s.normal.textColor=Gray(Mathf.LinearToGammaSpace(Mathf.Clamp01(gray)));GUI.Label(new Rect(x,y,w,h),value,s);}
    void Box(Rect r,float gray=1,float alpha=1){GUI.color=Gray(gray,alpha);GUI.DrawTexture(r,pixel);GUI.color=Color.white;}
    void Outline(Rect r,float thickness=1,float gray=1){Box(new Rect(r.x,r.y,r.width,thickness),gray);Box(new Rect(r.x,r.yMax-thickness,r.width,thickness),gray);Box(new Rect(r.x,r.y,thickness,r.height),gray);Box(new Rect(r.xMax-thickness,r.y,thickness,r.height),gray);}
    void Line(Vector2 a,Vector2 b,float width,float gray=1,float alpha=1){var old=GUI.matrix;Vector2 d=b-a;GUI.matrix=old*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg),Vector3.one);Box(new Rect(0,-width/2,d.magnitude,width),gray,alpha);GUI.matrix=old;}
    void Dot(Vector2 p,float r,float gray=1){GUI.color=Gray(gray);GUI.DrawTexture(new Rect(p.x-r,p.y-r,2*r,2*r),disc);GUI.color=Color.white;}
    bool Button(float x,float y,float w,string label,float height=48){GUI.color=Color.white;measurement.text=label;if(button.CalcSize(measurement).x>w)ClippedButtons++;return GUI.Button(new Rect(x,y,w,height),label,button);}
    void Settings(){returnFromSettings=ScreenName;ScreenName="settings";}
    void OnGUI()
    {
        SetupGUI();UiScale=Mathf.Max(.35f,Mathf.Min(Screen.width/1440f,Screen.height/900f));
        ClippedButtons=0;
        GUI.matrix=Matrix4x4.Scale(new Vector3(UiScale,UiScale,1));float width=Screen.width/UiScale,height=Screen.height/UiScale;
        if(ScreenName=="title"||ScreenName=="levels"||ScreenName=="settings"||ScreenName=="how")DrawMenu(width,height);
        else if(World!=null){DrawHUD(width,height);if(ScreenName!="play")DrawOverlay(width,height);}
        GUI.matrix=Matrix4x4.identity;
    }
    void DrawMenu(float w,float h)
    {
        Box(new Rect(0,0,w,h),.025f);for(int i=0;i<18;i++)Line(new Vector2(w*.6f+i*55,0),new Vector2(w*.3f+i*55,h),1,.1f);
        Label(64,36,900,28,"DASH / FIGHT     //     THIRD CUT",16,.75f);Line(new Vector2(64,80),new Vector2(w-64,80),1,.35f);
        if(ScreenName=="title"){
            Label(56,120,780,140,"DASH",132);Label(56,238,780,145,"/ FIGHT",132);
            Label(66,403,700,50,"FIND THE LINE. BREAK THE LIMIT.",24);
            Label(66,454,700,66,"A white scarf. A black room. No time to hesitate.\nSix speed trials built for the next perfect run.",19,.75f);
            if(Button(68,548,395,"ENTER / CONTINUE")){attempts=1;StartRun(Save.unlocked-1);}
            if(Button(68,609,395,"THE SIX TRIALS / PRACTICE"))ScreenName="levels";
            if(Button(68,670,188,"HOW TO FLOW"))ScreenName="how";if(Button(275,670,188,"SETTINGS"))Settings();
            if(Button(68,731,188,"QUIT"))Application.Quit();
            Vector2 p=new Vector2(w*.76f,h*.43f);float pulse=1+Mathf.Sin(Time.unscaledTime*1.8f)*.025f;
            for(int i=0;i<5;i++){float r=(85+i*37)*pulse;Vector2 a=p+new Vector2(0,-r),b=p+new Vector2(r,0),c=p+new Vector2(0,r),d=p-new Vector2(r,0);Line(a,b,1,.25f+i*.1f);Line(b,c,1,.25f+i*.1f);Line(c,d,1,.25f+i*.1f);Line(d,a,1,.25f+i*.1f);}
            Line(p-new Vector2(140,-30),p-new Vector2(18,0),19,.75f);Line(p-new Vector2(110,45),p-new Vector2(15,2),12,.55f);Dot(p,32);Line(p+new Vector2(-22,-4),p+new Vector2(22,-4),12,0);Dot(p+new Vector2(-10,-4),3);Dot(p+new Vector2(10,-4),3);
            Label(w*.61f,h*.73f,550,90,"20 / RUN   64 / DASH\nCUTS REFILL DASH. PARRIES FEED FLOW.",17,.75f);
        }else if(ScreenName=="levels"){
            Label(64,110,1150,75,"SIX WAYS TO BREAK A ROOM",47);Label(68,194,1150,32,"Complete runs unlock the campaign. Practice any trial; checkpoints never write records.",18,.75f);
            for(int i=0;i<6;i++){
                int col=i%2,row=i/2;float x=68+col*(w-136)/2,y=251+row*165,card=(w-160)/2;
                Outline(new Rect(x,y,card-18,152),1,.45f);Label(x+18,y+14,card-45,32,(i+1).ToString("00")+" / "+Stage.Names[i],23);
                Label(x+18,y+49,card-45,25,new[]{"DASH CHAINS","HAIRPIN SPRINT","RADIAL PARRY","LASER TIMING","PURSUIT CIRCUIT","THREE-PHASE CORE"}[i],14,.65f);
                Label(x+18,y+76,card-45,24,Stage.MasteryDescriptions[i],12,.75f);
                float time=(Save.nightmare?Save.nightmareBest:Save.best)[i];int rank=(Save.nightmare?Save.nightmareMedals:Save.medals)[i];string best=time>0?time.ToString("0.00")+"s / "+new[]{"-","B","A","S"}[rank]:"NO RECORD";Label(x+18,y+109,280,25,best,16,.8f);
                GUI.enabled=i<Save.unlocked;if(Button(x+card-295,y+102,110,i<Save.unlocked?"RUN":"LOCKED",35)){attempts=1;StartRun(i);}GUI.enabled=true;
                if(Button(x+card-172,y+102,136,"PRACTICE",35)){attempts=1;StartRun(i,true);}
            }
            if(Button(68,h-100,200,"BACK"))ScreenName="title";
        }else if(ScreenName=="how"){
            Label(64,115,1100,75,"FAST IS A MECHANIC.",56);
            string[] names={"01 / MOVE WITH INTENT","02 / CUT THROUGH THE FIRE","03 / PARRY AT THE LAST MOMENT","04 / KEEP THE DRIVE ALIVE","05 / LEARN A ROOM, THEN PERFECT IT"};
            string[] descriptions={"WASD moves at full speed. SHIFT gives precise control. Aim and fire with the mouse.","SPACE or RMB dashes. Follow ribbon arrows in Crosscut. Dash kills return a charge.","F returns nearby fire. Spindle shields require a counter. The final core opens after its pulse.","Kills and moving grazes build FLOW. Razorwire is a beam breach; Redline is an escape, not a cleanup.","The HUD gives each trial's objective and S-rank mastery. R retries; C after death opens checkpoint practice."};
            for(int i=0;i<5;i++){float y=235+i*104;Label(70,y,1250,35,names[i],22);Label(70,y+39,1250,48,descriptions[i],18,.75f);}
            if(Button(70,h-100,300,"ENTER THE FIRST TRIAL"))StartRun(0);if(Button(390,h-100,180,"BACK"))ScreenName="title";
        }else{
            Label(64,112,1200,75,"TUNE YOUR FLOW",54);float x=72,y=232;
            Label(x,y,440,32,"MASTER VOLUME",20);Save.volume=GUI.HorizontalSlider(new Rect(x+440,y+13,360,24),Save.volume,0,1);Label(x+835,y,200,32,Mathf.RoundToInt(Save.volume*100)+"%",20);
            y+=65;Label(x,y,440,32,"MUSIC",20);Save.music=GUI.HorizontalSlider(new Rect(x+440,y+13,360,24),Save.music,0,1);Label(x+835,y,200,32,Mathf.RoundToInt(Save.music*100)+"%",20);
            y+=65;Label(x,y,440,32,"SCREEN SHAKE",20);Save.shake=GUI.HorizontalSlider(new Rect(x+440,y+13,360,24),Save.shake,0,1);Label(x+835,y,200,32,Mathf.RoundToInt(Save.shake*100)+"%",20);
            y+=64;if(Button(x,y,800,"REDUCED FLASHES / "+(Save.reducedFlash?"ON":"OFF")))Save.reducedFlash=!Save.reducedFlash;
            y+=60;if(Button(x,y,800,"SOFT AIM ASSIST / "+(Save.aimAssist?"ON":"OFF")))Save.aimAssist=!Save.aimAssist;
            y+=60;if(Button(x,y,800,"PERSONAL BEST GHOST / "+(Save.ghost?"ON":"OFF")))Save.ghost=!Save.ghost;
            y+=60;if(Button(x,y,800,"DIFFICULTY / "+(Save.nightmare?"NIGHTMARE: ONE HIT":"HARD: TWO HITS")))Save.nightmare=!Save.nightmare;
            Label(x,y+63,1200,36,"Difficulty applies next attempt. Hard and Nightmare have separate records and ghosts.",17,.7f);
            if(Button(x,h-100,300,"SAVE & BACK")){Save.Store();ScreenName=returnFromSettings;}
        }
        Label(66,h-36,w-132,25,"WASD MOVE    SPACE / RMB DASH    MOUSE AIM + FIRE    F PARRY    SHIFT PRECISION    R RETRY    ESC PAUSE",13,.6f);
    }
    void DrawHUD(float w,float h)
    {
        if(Event.current.type!=EventType.Repaint)return;
        var model=World;Box(new Rect(0,0,w,94),.025f);Box(new Rect(0,h-70,w,70),.025f);
        Line(new Vector2(30,90),new Vector2(w-30,90),1,.3f);Line(new Vector2(30,h-71),new Vector2(w-30,h-71),1,.3f);
        Label(32,13,650,35,(model.Index+1).ToString("00")+" / "+model.Stage.Name,25);
        Label(34,53,760,25,(Practice?"PRACTICE / ":"")+model.ObjectiveText,13,.75f);
        Label(w-285,12,265,40,(model.Stage.Limit-model.Time).ToString("00.00"),31);
        Label(w-285,54,270,25,"RUN "+model.Time.ToString("0.00")+"  /  PAR "+model.Stage.Par.ToString("0.0"),14,.75f);
        float flowWidth=220,flowX=w/2-110;Label(flowX,14,280,28,model.Overdrive?"OVERDRIVE":"FLOW  "+Mathf.RoundToInt(model.Flow*100)+"%",17,model.Overdrive?1:.7f);
        Box(new Rect(flowX,49,flowWidth,5),.18f);Box(new Rect(flowX,49,flowWidth*model.Flow,5));Label(flowX,63,420,23,model.MasteryName+" "+Math.Min(model.MasteryTarget,model.MasteryProgress)+"/"+model.MasteryTarget,12,.7f);
        Label(34,h-56,210,33,"HP  "+new string('|',Math.Max(0,model.HP)),25);Label(170,h-51,200,28,model.Nightmare?"NIGHTMARE":"HARD",14,.65f);
        Label(320,h-51,120,28,"DASH",16,emptyLeft>0?.5f:1);
        for(int i=0;i<2;i++){Outline(new Rect(380+i*64,h-47,52,17),1,emptyLeft>0?.35f:.9f);float f=i<model.Charges?1:i==model.Charges?model.RechargeClock/Rules.Recharge:0;Box(new Rect(383+i*64,h-44,46*Mathf.Clamp01(f),11),emptyLeft>0?.35f:1);}
        Label(550,h-51,350,28,"F / PARRY "+(model.ParryCooldown<=0?"READY":model.ParryCooldown.ToString("0.0")+"s"),16,parryEmptyLeft>0?.35f:.85f);
        Label(w-270,h-55,250,34,model.Velocity.Length.ToString("00")+"  VELOCITY",23);Label(w-270,h-29,250,22,"R RETRY  /  ESC PAUSE",12,.65f);
        if(toastLeft>0){float size=toast.Contains("PERFECT")?22:18;Box(new Rect(w/2-325,103,780,40),0,.72f);Label(w/2-310,108,760,38,toast,(int)size);}
        if(Practice)Label(34,110,600,32,"CHECKPOINT PRACTICE / RECORDS DISABLED",16,.75f);
        if(runClock<3.5f&&ScreenName=="play"&&!ShowcaseMode){
            Box(new Rect(30,h-155,980,61),0,.8f);Label(45,h-148,970,26,model.ObjectiveText,15,.9f);Label(45,h-123,970,24,"S RANK: "+model.MasteryName+" "+model.MasteryTarget+" / UNDER PAR / NO DAMAGE / KEEP MOVING",14,.65f);
        }
        foreach(var s in model.Stage.Seals)if(!s.Taken&&s.Kind==model.SealsTaken&&View.Visible(s.P)){
            Vector2 p=View.UI(s.P);Label(p.x-65,p.y+42,350,28,model.RelayReady?(s.DashGate?"DASH IN ARROW DIRECTION":model.Stage.Mode==TrialMode.Pursuit?"CHECKPOINT":"RELAY / OPEN"):"GUARDS / "+model.SectionAlive+" LEFT",14,.85f);
        }
        Enemy boss=null;foreach(var e in model.Stage.Enemies)if(e.Kind==3&&e.HP>0&&e.Zone<=model.Section)boss=e;
        if(boss!=null){float x=w/2-180;Box(new Rect(x,h-115,360,7),.16f);Box(new Rect(x,h-115,360*boss.HP/boss.MaxHP,7));Label(x,h-146,550,30,"CORE / PHASE "+(boss.CorePhase+1)+" / "+(boss.Guarding?"GUARDED":"OPEN: STRIKE"),16);}
        DrawObjective(w,h);DrawMap(new Rect(w-196,112,164,104));
        if(Input.GetKey(KeyCode.Tab)){Box(new Rect(w/2-280,h/2-195,560,390),0,.94f);DrawMap(new Rect(w/2-260,h/2-175,520,350));}
        Vector2 mouse=ShowcaseMode?View.UI(model.Player+model.Aim*4):new Vector2(Input.mousePosition.x/UiScale,(Screen.height-Input.mousePosition.y)/UiScale);Dot(mouse,2);Line(mouse+new Vector2(-12,0),mouse+new Vector2(-5,0),1);Line(mouse+new Vector2(5,0),mouse+new Vector2(12,0),1);Line(mouse+new Vector2(0,-12),mouse+new Vector2(0,-5),1);Line(mouse+new Vector2(0,5),mouse+new Vector2(0,12),1);
    }
    void DrawObjective(float w,float h)
    {
        V target=World.ExitOpen?World.Stage.Exit:World.Stage.Seals[Math.Min(2,World.SealsTaken)].P;string name=World.ExitOpen?"EXIT":World.Stage.Mode==TrialMode.Pursuit?"CHECKPOINT":World.Stage.Mode==TrialMode.Hairpin?"RIBBON":World.Stage.Mode==TrialMode.Timing?"BREACH":"RELAY";
        if(World.Stage.NeedsClear&&World.SectionAlive>0){float best=999;foreach(var e in World.Stage.Enemies)if(e.HP>0&&e.Zone<=World.Section){float distance=(e.P-World.Player).Length;if(distance<best){best=distance;target=e.P;name="TARGET";}}}
        if(View.Visible(target))return;Vector2 p=View.UI(target),origin=View.UI(World.Player),d=(p-origin).normalized;
        Vector2 clamped=new Vector2(Mathf.Clamp(p.x,45,w-45),Mathf.Clamp(p.y,137,h-110));Vector2 n=new Vector2(-d.y,d.x);
        Line(clamped-d*12+n*7,clamped+d*2,2);Line(clamped+d*2,clamped-d*12-n*7,2);Label(clamped.x-28,clamped.y+14,150,24,name,12,.8f);
    }
    void DrawMap(Rect r)
    {
        Box(r,0,.75f);Outline(r,1,.3f);
        foreach(var wall in World.Stage.Walls){Vector2 p=MapPoint(wall.P,r);Box(new Rect(p.x-wall.Size.X*r.width/108,p.y-wall.Size.Y*r.height/68,wall.Size.X*r.width/54,wall.Size.Y*r.height/34),.25f);}
        foreach(var s in World.Stage.Seals)if(!s.Taken)Dot(MapPoint(s.P,r),s.Kind==World.SealsTaken?3:2,s.Kind==World.SealsTaken?1:.3f);
        foreach(var e in World.Stage.Enemies)if(e.HP>0&&e.Zone<=World.Section)Dot(MapPoint(e.P,r),1.5f,.6f);
        Vector2 player=MapPoint(World.Player,r);Dot(player,3.2f);Outline(new Rect(player.x-4.5f,player.y-4.5f,9,9),1,.8f);
    }
    static Vector2 MapPoint(V p,Rect r)=>new Vector2(r.x+(p.X+27)/54*r.width,r.y+(17-p.Y)/34*r.height);
    void DrawOverlay(float w,float h)
    {
        Box(new Rect(0,94,w,h-165),0,.65f);float x=w/2-350,y=h/2-225;Box(new Rect(x-28,y-24,756,470),.035f);Outline(new Rect(x-28,y-24,756,470),1,.5f);
        string title=ScreenName=="dead"?"ONE MORE CUT.":ScreenName=="won"?(World.Index==5?"LIMIT BROKEN.":"LINE COMPLETE."):"BREATHE. THEN MOVE.";
        Label(x,y,730,76,title,49);
        if(ScreenName=="pause"){
            Label(x,y+88,730,36,Practice?"CHECKPOINT PRACTICE / SECTOR "+(Checkpoint+1):"PAUSED / THE CLOCK IS STOPPED",18,.75f);
            if(Button(x,y+155,700,"RESUME")){focusLost=false;ScreenName="play";}
            if(Button(x,y+218,340,"SETTINGS"))Settings();if(Button(x+360,y+218,340,"C / CHECKPOINT PRACTICE"))StartRun(World.Index,true,Checkpoint);
            if(Button(x,y+282,340,"TRIAL SELECT"))ScreenName="levels";if(Button(x+360,y+282,340,"MAIN MENU"))ScreenName="title";
        }else if(ScreenName=="dead"){
            Label(x,y+88,730,34,"LAST HIT / "+World.DeathCause+"     ATTEMPT "+attempts,19,.8f);
            string hint=World.DeathCause=="BULLET"?"The white trace marks the shot. Dash through it, or parry close." :World.DeathCause=="BEAM"?"The dashed warning comes first. Cross during your dash." :World.DeathCause=="TIME EXPIRED"?"Plan the route. Kill on the move and commit to the next relay.":"Rushers commit after their tell. Cut them before they reach you.";
            Label(x,y+127,710,54,hint,17,.75f);
            if(Button(x,y+202,700,"R / ENTER / RETRY IMMEDIATELY"))Retry();
            if(Button(x,y+267,700,"C / PRACTICE FROM SECTOR "+(Math.Min(2,Checkpoint)+1)))StartRun(World.Index,true,Checkpoint);
            if(Button(x,y+332,340,"TRIAL SELECT"))ScreenName="levels";if(Button(x+360,y+332,340,"MAIN MENU"))ScreenName="title";
        }else{
            string rank=Practice?"PRACTICE":(World.Nightmare?"NIGHTMARE / ":"")+new[]{"-","B","A","S"}[Rank(World)]+" RANK";
            Label(x,y+87,730,40,rank+"   /   "+World.Time.ToString("0.00")+"s   /   "+World.Score.ToString("000000")+" PTS",24);
            Label(x,y+133,730,50,"CHAIN "+World.BestCombo+"   PARRIES "+World.Parries+"   GRAZES "+World.Grazes+"   MOVING "+Mathf.RoundToInt(World.MovingTime/Math.Max(.01f,World.Time)*100)+"%",16,.8f);
            for(int i=0;i<3;i++)Label(x+i*236,y+174,235,30,"0"+(i+1)+" / "+World.Splits[i].ToString("0.00")+"s",16,.75f);
            if(Button(x,y+225,700,"ENTER / "+(World.Index==5?"BACK TO THE SIX TRIALS":"NEXT TRIAL"))){if(World.Index<5)StartRun(World.Index+1,Practice);else ScreenName="levels";}
            if(Button(x,y+289,340,"R / CHASE YOUR GHOST"))Retry();if(Button(x+360,y+289,340,"TRIAL SELECT"))ScreenName="levels";
            Label(x,y+366,730,42,"MASTERY / "+World.MasteryName+" "+World.MasteryProgress+"/"+World.MasteryTarget+(World.MasteryComplete?" COMPLETE":" INCOMPLETE")+"   /   S REQUIRES MASTERY, SPEED AND NO DAMAGE",13,.7f);
        }
    }
    void OnDestroy(){if(pixel)Destroy(pixel);if(disc)Destroy(disc);}
}
