using System;
using System.IO;
using UnityEngine;

public static class VelocityAudio
{
    public static AudioClip Load(string name)
    {
        try{
            string path=Path.Combine(Application.streamingAssetsPath,"VelocityAudio",name+".wav");
            using(var reader=new BinaryReader(File.OpenRead(path))){
                if(new string(reader.ReadChars(4))!="RIFF")return null;reader.ReadInt32();if(new string(reader.ReadChars(4))!="WAVE")return null;
                int channels=1,rate=44100,bits=16,format=1;byte[] bytes=null;
                while(reader.BaseStream.Position+8<=reader.BaseStream.Length){string id=new string(reader.ReadChars(4));int size=reader.ReadInt32();if(size<0||size>reader.BaseStream.Length-reader.BaseStream.Position)return null;long next=reader.BaseStream.Position+size+(size%2);
                    if(id=="fmt "){format=reader.ReadInt16();channels=reader.ReadInt16();rate=reader.ReadInt32();reader.ReadInt32();reader.ReadInt16();bits=reader.ReadInt16();}
                    else if(id=="data")bytes=reader.ReadBytes(size);reader.BaseStream.Position=next;
                }
                if(bytes==null||format!=1||channels<1||channels>2||rate<=0||(bits!=8&&bits!=16))return null;
                float[] data=new float[bytes.Length/(bits/8)];for(int i=0;i<data.Length;i++)data[i]=bits==16?BitConverter.ToInt16(bytes,i*2)/32768f:(bytes[i]-128)/128f;
                AudioClip clip=AudioClip.Create(name,data.Length/channels,channels,rate,false);clip.SetData(data,0);return clip;
            }
        }catch(Exception e){Debug.LogWarning("Audio unavailable: "+name+" "+e.Message);return null;}
    }
}
