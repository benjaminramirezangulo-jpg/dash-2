using System;
using System.Collections.Generic;
using UnityEngine;
using Velocity;

// One reusable mesh draws the world. UI never determines world scale or aiming.
public sealed class VelocityView : MonoBehaviour
{
    public Camera Camera {get;private set;}
    public float UnitsPerScreenHeight=>Camera.orthographicSize*2;
    VelocityGame game;Mesh mesh;Material material;MeshRenderer render;
    readonly List<Vector3> vertices=new List<Vector3>(128000);
    readonly List<Color32> colors=new List<Color32>(128000);
    readonly List<int> indices=new List<int>(128000);
    readonly List<Mark> marks=new List<Mark>(256);
    struct Mark{public V P,Velocity;public float Life,Max,Size;public bool Ring;}
    V center;float shake;
    static Color32 Ink(float gray,float alpha=1){byte g=(byte)(Mathf.Clamp01(gray)*255);return new Color32(g,g,g,(byte)(Mathf.Clamp01(alpha)*255));}
    static Vector3 Vec(V p)=>new Vector3(p.X,p.Y,0);
    public void Initialize(VelocityGame target,Camera camera)
    {
        game=target;Camera=camera;Camera.orthographic=true;Camera.orthographicSize=10.5f;Camera.transform.position=new Vector3(0,0,-10);Camera.cullingMask=1;
        var world=new GameObject("Ink World");world.transform.SetParent(transform,false);
        mesh=new Mesh{name="Velocity dynamic ink"};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.MarkDynamic();world.AddComponent<MeshFilter>().sharedMesh=mesh;
        render=world.AddComponent<MeshRenderer>();material=new Material(Shader.Find("Sprites/Default"));material.mainTexture=Texture2D.whiteTexture;render.sharedMaterial=material;
    }
    void SetViewport(){float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);Camera.pixelRect=new Rect(0,72*scale,Screen.width,Screen.height-166*scale);}
    V CameraTarget(World world)
    {
        V goal=world.Player+world.Velocity.Unit*(world.DashLeft>0?2.5f:1.8f)+world.Aim*.8f;
        float halfY=Camera.orthographicSize,halfX=halfY*Camera.aspect;
        goal.X=Mathf.Clamp(goal.X,-27+Mathf.Min(halfX,27),27-Mathf.Min(halfX,27));goal.Y=Mathf.Clamp(goal.Y,-17+Mathf.Min(halfY,17),17-Mathf.Min(halfY,17));return goal;
    }
    public void ResetCamera(){if(game.World==null)return;SetViewport();Camera.orthographicSize=10.3f;center=CameraTarget(game.World);Camera.transform.position=new Vector3(center.X,center.Y,-10);marks.Clear();shake=0;}
    public V MouseWorld(){var p=Camera.ScreenToWorldPoint(Input.mousePosition);return new V(p.x,p.y);}
    public Vector2 UI(V p){Vector3 q=Camera.WorldToScreenPoint(Vec(p));return new Vector2(q.x/game.UiScale,(Screen.height-q.y)/game.UiScale);}
    public bool Visible(V p,float margin=0){var q=Camera.WorldToViewportPoint(Vec(p));return q.x>=-margin&&q.x<=1+margin&&q.y>=-margin&&q.y<=1+margin;}
    public void Effect(string kind,V p)
    {
        if(kind=="SHOT"){return;}
        if(kind=="KILL"||kind=="HIT"||kind=="PERFECT"||kind=="DASH"||kind=="SEAL"||kind=="WALL"||kind=="DEATH"||kind=="PHASE"){
            bool cut=kind=="KILL"||kind=="PERFECT";int n=cut?18:kind=="DASH"?4:10;
            for(int i=0;i<n;i++){V d=V.Angle(i*2.39996f+game.World.Time);marks.Add(new Mark{P=p,Velocity=d*(cut?6+i*.45f:3+i*.25f),Life=.18f+i*.008f,Max=.18f+i*.008f,Size=cut?.11f:.06f});}
            marks.Add(new Mark{P=p,Life=cut?.3f:.2f,Max=cut?.3f:.2f,Ring=true,Size=cut?2.2f:1.2f});
            shake=Mathf.Max(shake,kind=="DEATH"?.75f:kind=="PERFECT"?.35f:kind=="KILL"?.23f:.08f);
        }
        if(marks.Count>384)marks.RemoveRange(0,marks.Count-384);
    }
    void Triangle(V a,V b,V c,Color32 color)
    {int i=vertices.Count;vertices.Add(Vec(a));vertices.Add(Vec(b));vertices.Add(Vec(c));colors.Add(color);colors.Add(color);colors.Add(color);indices.Add(i);indices.Add(i+1);indices.Add(i+2);}
    void Quad(V a,V b,V c,V d,Color32 color){Triangle(a,b,c,color);Triangle(a,c,d,color);}
    void Rect(V p,V size,float gray,float alpha=1){V h=size*.5f;Quad(p+new V(-h.X,-h.Y),p+new V(h.X,-h.Y),p+h,p+new V(-h.X,h.Y),Ink(gray,alpha));}
    void Line(V a,V b,float width,float gray=1,float alpha=1){V d=(b-a).Unit,n=new V(-d.Y,d.X)*(width*.5f);Quad(a+n,b+n,b-n,a-n,Ink(gray,alpha));}
    void Circle(V p,float r,float gray=1,float alpha=1,int count=20){for(int i=0;i<count;i++)Triangle(p,p+V.Angle(i*6.283185f/count)*r,p+V.Angle((i+1)*6.283185f/count)*r,Ink(gray,alpha));}
    void Ring(V p,float r,float width,float gray=1,float alpha=1,int count=28){for(int i=0;i<count;i++)Line(p+V.Angle(i*6.283185f/count)*r,p+V.Angle((i+1)*6.283185f/count)*r,width,gray,alpha);}
    void Diamond(V p,float r,float width,float gray=1){V a=p+new V(0,r),b=p+new V(r,0),c=p+new V(0,-r),d=p+new V(-r,0);Line(a,b,width,gray);Line(b,c,width,gray);Line(c,d,width,gray);Line(d,a,width,gray);}
    void Arrow(V p,V d,float size,float gray=1){V n=new V(-d.Y,d.X);Line(p-d*size*.55f+n*size*.45f,p+d*size*.3f,.08f,gray);Line(p+d*size*.3f,p-d*size*.55f-n*size*.45f,.08f,gray);}
    void Dashed(V a,V b,float width,float gray=1){V delta=b-a;int n=Math.Max(2,(int)(delta.Length/.65f));for(int i=0;i<n;i+=2)Line(V.Lerp(a,b,(float)i/n),V.Lerp(a,b,(float)(i+1)/n),width,gray);}
    void LateUpdate()
    {
        if(!game||Camera==null)return;var w=game.World;
        bool show=w!=null&&(game.ScreenName=="play"||game.ScreenName=="pause"||game.ScreenName=="dead"||game.ScreenName=="won");render.enabled=show;if(!show)return;
        float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);SetViewport();
        float zoom=10.3f+(w.DashLeft>0?1.1f:0)+(w.Overdrive?.35f:0);
        Camera.orthographicSize=Mathf.Lerp(Camera.orthographicSize,zoom,1-Mathf.Exp(-dt*6));
        V goal=CameraTarget(w);
        center=V.Lerp(center,goal,1-Mathf.Exp(-dt*16));shake=Mathf.Max(0,shake-dt*4);
        float amount=shake*game.Save.shake*.3f;
        Camera.transform.position=new Vector3(center.X+Mathf.Sin(Time.unscaledTime*153)*amount,center.Y+Mathf.Sin(Time.unscaledTime*117)*amount,-10);
        for(int i=marks.Count-1;i>=0;i--){Mark m=marks[i];m.Life-=dt;m.P+=m.Velocity*dt;if(m.Life<=0)marks.RemoveAt(i);else marks[i]=m;}
        vertices.Clear();indices.Clear();colors.Clear();
        Rect(new V(),new V(52,32),.004f);
        // Sparse floor marks preserve orientation without turning the level into graph paper.
        for(int x=-24;x<=24;x+=3)for(int y=-15;y<=15;y+=3){V p=new V(x,y);if(Visible(p,.1f)&&!w.Stage.Blocked(p,.5f)){Line(p-new V(.06f,0),p+new V(.06f,0),.03f,.05f);Line(p-new V(0,.06f),p+new V(0,.06f),.03f,.05f);}}
        for(int i=1;i<w.Stage.Route.Count;i++){V a=w.Stage.Route[i-1],b=w.Stage.Route[i],d=(b-a).Unit;int n=(int)((b-a).Length/3);for(int j=1;j<=n;j++){V p=V.Lerp(a,b,(float)j/(n+1));if(!w.Stage.Blocked(p,.5f))Arrow(p,d,.48f,.16f);}}
        foreach(var wall in w.Stage.Walls){
            Rect(wall.P+new V(.14f,-.2f),wall.Size+new V(.2f,.2f),0);Rect(wall.P,wall.Size,.075f);V half=wall.Size*.5f;
            V a=wall.P-half,b=wall.P+new V(half.X,-half.Y),c=wall.P+half,d=wall.P+new V(-half.X,half.Y);
            Line(a,b,.05f,.32f);Line(b,c,.06f,.6f);Line(c,d,.07f,.85f);Line(d,a,.05f,.5f);
            if(wall.Size.X>wall.Size.Y){for(float x=a.X+.4f;x<b.X-.3f;x+=.8f)Line(new V(x,a.Y+.1f),new V(x+.4f,Mathf.Min(a.Y+.5f,d.Y-.1f)),.035f,.25f);}
            else for(float y=a.Y+.4f;y<d.Y-.3f;y+=.8f)Line(new V(a.X+.1f,y),new V(Mathf.Min(a.X+.5f,b.X-.1f),y+.4f),.035f,.25f);
        }
        foreach(var s in w.Stage.Seals){if(s.Taken){Ring(s.P,.65f,.035f,.2f);continue;}bool current=s.Kind==w.SealsTaken;float tone=current?1:.18f;
            if(s.DashGate){V tangent=new V(-s.Normal.Y,s.Normal.X);Dashed(s.P-tangent*s.HalfWidth,s.P+tangent*s.HalfWidth,.055f,tone);Circle(s.P-tangent*s.HalfWidth,.18f,tone);Circle(s.P+tangent*s.HalfWidth,.18f,tone);Arrow(s.P-s.Normal*1.2f,s.Normal,.9f,tone);}
            Ring(s.P,.85f,.06f,tone);Diamond(s.P,.53f,.08f,tone);if(current&&w.RelayReady){Ring(s.P,1.15f+.12f*Mathf.Sin(w.Time*6),.04f,.7f);Circle(s.P,.2f);}
            else if(current){Line(s.P-new V(.2f,.2f),s.P+new V(.2f,.2f),.07f);Line(s.P+new V(-.2f,.2f),s.P+new V(.2f,-.2f),.07f);}
        }
        foreach(var beam in w.Stage.Beams)if(beam.Zone==w.Section){
            Circle(beam.A,.19f,.8f);Circle(beam.B,.19f,.8f);
            if(beam.Active(w.Time)){Line(beam.A,beam.B,.35f,.3f);Line(beam.A,beam.B,.1f,1);}
            else if(beam.Warning(w.Time)){Dashed(beam.A,beam.B,.06f,.75f);Ring(beam.A,.4f,.04f);Ring(beam.B,.4f,.04f);}else Line(beam.A,beam.B,.025f,.15f);
        }
        Diamond(w.Stage.Exit,1.2f,.1f,w.ExitOpen?1:.2f);if(w.ExitOpen){Arrow(w.Stage.Exit,new V(1,0),1);Ring(w.Stage.Exit,1.5f,.04f,.7f);}
        foreach(var e in w.Stage.Enemies){
            if(e.HP<=0||!Visible(e.P,.1f))continue;
            if(e.Zone>w.Section){Diamond(e.P,.28f,.04f,.16f);continue;}
            float tell=e.Kind==4?.6f:e.Kind==5?.65f:.48f;
            if(e.Clock>=0&&e.Clock<tell){
                V end=e.P+e.Aim*(e.Kind==4?15:e.Kind==5?8:5),delta=end-e.P;float hit=1;foreach(var wall in w.Stage.Walls){float t;if(Rules.SegmentBox(e.P,end,wall,0,out t))hit=Mathf.Min(hit,t);}
                Dashed(e.P,V.Lerp(e.P,end,hit),.035f,e.Kind==4?.8f:.5f);Ring(e.P,e.Radius+.2f+e.Clock*.3f,.04f,.8f);
            }
            V p=e.P,d=e.Aim.Length>.1f?e.Aim:new V(1,0),n=new V(-d.Y,d.X);Circle(p+new V(.05f,-.15f),e.Radius*.95f,0);
            if(e.Kind==0){Rect(p,new V(.85f,.85f),.9f);Rect(p,new V(.58f,.58f),0);Line(p,p+d*.85f,.17f);Circle(p,.13f);if(e.Guarding){Line(p+d*.65f+n*.65f,p+d*.85f,.13f);Line(p+d*.85f,p+d*.65f-n*.65f,.13f);}}
            else if(e.Kind==1){Ring(p,.57f,.11f);for(int j=0;j<6;j++){V a=V.Angle(j*1.0472f+w.Time*.6f);Line(p+a*.43f,p+a*.76f,.09f,.8f);}Circle(p,.15f);if(e.CounterShield&&e.Guarding){Ring(p,.98f,.075f);for(int j=0;j<4;j++)Arrow(p+V.Angle(j*1.570796f)*1.2f,V.Angle(j*1.570796f+3.14159f),.3f);}}
            else if(e.Kind==2){Triangle(p+d*.77f,p-d*.42f+n*.5f,p-d*.42f-n*.5f,Ink(.9f));Triangle(p+d*.35f,p-d*.23f+n*.24f,p-d*.23f-n*.24f,Ink(0));}
            else if(e.Kind==4){Line(p-d*.45f,p+d*.75f,.32f);Line(p-d*.35f+n*.33f,p-d*.35f-n*.33f,.13f);Circle(p,.17f,0);Line(p+d*.8f+n*.2f,p+d*.8f-n*.2f,.08f);}
            else if(e.Kind==5){Diamond(p,.64f,.12f);Arrow(p+d*.1f,d,.6f);Line(p-d*.55f+n*.65f,p+d*.35f+n*.35f,.1f);Line(p-d*.55f-n*.65f,p+d*.35f-n*.35f,.1f);}
            else {int phase=e.CorePhase;Ring(p,1.15f,e.Guarding?.15f:.04f);Diamond(p,1.6f,.08f);Diamond(p,.68f,.16f);for(int j=0;j<4+phase*2;j++){V a=V.Angle(j*6.283185f/(4+phase*2)+w.Time*.7f);Circle(p+a*1.55f,e.Guarding?.2f:.09f);}if(!e.Guarding)Circle(p,.36f,.9f);else Circle(p,.15f);}
            if(e.Flash>0){Line(p+new V(-.9f,-.7f),p+new V(.9f,.7f),.06f);Ring(p,.85f+e.Flash*4,.04f);}
            if(e.HP<e.MaxHP&&e.Kind!=3){Line(p+new V(-.4f,-.9f),p+new V(.4f,-.9f),.055f,.2f);Line(p+new V(-.4f,-.9f),p+new V(-.4f+.8f*e.HP/e.MaxHP,-.9f),.055f);}
        }
        // Best-run ghost is a quiet secondary silhouette, never a solid obstacle.
        V ghost;if(game.TryGhost(w.Time,out ghost)&&Visible(ghost)){Ring(ghost,.42f,.04f,.35f);Line(ghost,ghost-w.Velocity.Unit*.6f,.035f,.3f);}
        foreach(var s in w.Shots){if(!Visible(s.P,.1f))continue;V d=s.Velocity.Unit;
            if(s.Friendly){Line(s.P-d*.6f,s.P,.09f);Circle(s.P,.09f);}
            else {Circle(s.P,.21f,0,1,8);Ring(s.P,.16f,.065f,1,1,12);Circle(s.P,.055f,1,1,8);Line(s.P-d*.38f,s.P-d*.19f,.035f,.5f);}
        }
        foreach(var m in marks){float a=m.Life/m.Max;if(m.Ring)Ring(m.P,m.Size*(1-a)+.1f,.035f,.8f,a);else Line(m.P,m.P-m.Velocity*.025f,m.Size,.9f,a);}
        V player=w.Player,forward=w.Aim,norm=new V(-forward.Y,forward.X),back=w.Velocity.Length>1?w.Velocity.Unit:forward;
        float wave=Mathf.Sin(w.Time*32)*.12f;Circle(player+new V(.05f,-.14f),.43f,0);
        // Two scarf strips and the masked white head form a silhouette unlike any enemy.
        V scarf=player-back*.22f;Triangle(scarf+norm*.16f,scarf-back*(w.DashLeft>0?2.1f:1.05f)+norm*wave,scarf-norm*.07f,Ink(.8f));
        Triangle(scarf-norm*.16f,scarf-back*(w.DashLeft>0?1.6f:.75f)-norm*(.18f+wave),scarf+norm*.04f,Ink(.65f));
        if(w.DashLeft>0){Line(player-back*2.8f,player,.27f,.4f,.5f);Line(player-back*3.3f+norm*.4f,player-back*.8f+norm*.4f,.04f,.8f);Line(player-back*2.5f-norm*.4f,player-back*.4f-norm*.4f,.04f,.8f);}
        Circle(player,.4f,1,w.Invulnerable>0?.8f:1);Line(player+forward*.12f+norm*.24f,player+forward*.12f-norm*.24f,.16f,0);
        Circle(player+forward*.17f+norm*.13f,.045f);Circle(player+forward*.17f-norm*.13f,.045f);
        Line(player+forward*.36f,player+forward*(.88f-w.Recoil*3),.13f);
        if(w.Recoil>.025f){V p=player+forward*.96f;Triangle(p-forward*.12f+norm*.16f,p+forward*.45f,p-forward*.12f-norm*.16f,Ink(1));}
        if(w.ParryLeft>0)Ring(player,2.3f*(1-w.ParryLeft/.2f),.1f,1,.9f);
        if(w.Invulnerable>0)Ring(player,.65f,.04f,.8f,.7f);
        if(w.Overdrive){Ring(player,.57f,.035f,1,.7f);Line(player-back*4.2f+norm*.65f,player-back*1.6f+norm*.65f,.025f,.5f);}
        if(game.ScreenName=="dead"&&w.DeathCause!="TIME EXPIRED"){Dashed(w.DamageFrom,w.DamageTo,.055f,.8f);Ring(player,1.2f,.06f);Line(player-new V(.6f,.6f),player+new V(.6f,.6f),.1f);}
        mesh.Clear(true);mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0,false);mesh.bounds=new Bounds(Vector3.zero,new Vector3(70,50,2));
    }
    void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
}
