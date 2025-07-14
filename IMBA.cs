using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RIKA_IMBANIKA_AUDIO
{
    public class IMBA
    {
        public float _bpm;
        public string _name;
        public string _lastPath;
        public string _firstPath;
        public DateTime _creation;
        public DateTime _lastSave;
        public DateTime _lastLoad;
        public ulong _musicMakerTrackNumber;
        public ulong _musicMakerSavesCounter;
        public ulong _trackSavesCounter;
        public UInt32 _createdInAppVersion;
        public UInt32 _lastSavedInAppVersion;

        public static IMBA Load(string path)
        {
            IMBA imba = new IMBA();

            Stream stream = File.OpenRead(path);
            BinaryReader br = new BinaryReader(stream);

            imba._createdInAppVersion = br.ReadUInt32();
            imba._lastSavedInAppVersion = br.ReadUInt32();

            imba._bpm = br.ReadSingle();
            imba._name = br.ReadString();
            imba._lastPath = br.ReadString();

            var what = br.ReadInt64();
            imba._creation = DateTime.FromBinary(what);

            long what2 = br.ReadInt64();
            imba._lastSave = DateTime.FromBinary(what2);

            imba._firstPath = br.ReadString();

            imba._musicMakerTrackNumber = br.ReadUInt64();
            imba._musicMakerSavesCounter = br.ReadUInt64();
            imba._trackSavesCounter = br.ReadUInt64();

            long what3 = br.ReadInt64();
            imba._lastLoad = DateTime.FromBinary(what3);
            imba._lastLoad = DateTime.UtcNow;

            stream.Close();
            br.Dispose();

            return imba;
       }

        public static void Save(IMBA imba, string path)
        {
            Stream stream = File.OpenWrite(path);
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(imba._createdInAppVersion);
            bw.Write(imba._lastSavedInAppVersion);
            bw.Write(imba._bpm);
            bw.Write(imba._name);
            bw.Write(imba._lastPath);
            bw.Write(imba._creation.ToBinary());
            bw.Write(imba._lastSave.ToBinary());
            bw.Write(imba._firstPath);
            bw.Write(imba._musicMakerTrackNumber);
            bw.Write(imba._musicMakerSavesCounter);
            bw.Write(imba._trackSavesCounter);
            bw.Write(imba._lastLoad.ToBinary());

            stream.Close();
            bw.DisposeAsync();
        }

        public string ToString()
        {
            string str = $"Created in app version = {_createdInAppVersion}\r\n";
            str += $"Last saved in app version = {_lastSavedInAppVersion}\r\n";
            str += $"Bpm = {_bpm}\r\n";
            str += $"Name = {_name}\r\n";
            str += $"Last path = {_lastPath}\r\n";
            str += $"Creation = {_creation}\r\n";
            str += $"Last Save = {_lastSave}\r\n";
            str += $"First Path = {_firstPath}\r\n";
            str += $"Track Number = {_musicMakerTrackNumber}\r\n";
            str += $"Music Maker Saves Counter = {_musicMakerSavesCounter}\r\n";
            str += $"Track Saves Counter = {_trackSavesCounter}\r\n";
            str += $"Last Load = {_lastLoad}\r\n";

            return str;
        }
    }
}
