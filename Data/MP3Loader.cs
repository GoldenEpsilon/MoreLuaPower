

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using NAudio;
using NAudio.Wave;
using System.Reflection;

namespace MP3Loader
{
    

    public static class NAudioPlayer
    {
        public static AudioClip FromMp3Data(byte[] data)
        {
            // Load the data into a stream
            MemoryStream mp3stream = new MemoryStream(data);
            // Convert the data in the stream to WAV format
            Mp3FileReader mp3audio = new Mp3FileReader(mp3stream);
            WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3audio);
            // Convert to WAV data
            WAV wav = new WAV(AudioMemStream(waveStream).ToArray());
            Debug.Log(wav);
            AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            // Return the clip
            return audioClip;
        }

        private static MemoryStream AudioMemStream(WaveStream waveStream)
        {
            MemoryStream outputStream = new MemoryStream();
            using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
            {
                byte[] bytes = new byte[waveStream.Length];
                waveStream.Position = 0;
                waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
                waveFileWriter.Write(bytes, 0, bytes.Length);
                waveFileWriter.Flush();
            }
            return outputStream;
        }
    }

    /* From http://answers.unity3d.com/questions/737002/wav-byte-to-audioclip.html */
    public class WAV
    {

        // convert two bytes to one float in the range -1 to 1
        static float bytesToFloat(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            short s = (short)((secondByte << 8) | firstByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0F;
        }

        static int bytesToInt(byte[] bytes, int offset = 0)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                value |= ((int)bytes[offset + i]) << (i * 8);
            }
            return value;
        }
        // properties
        public float[] LeftChannel { get; internal set; }
        public float[] RightChannel { get; internal set; }
        public int ChannelCount { get; internal set; }
        public int SampleCount { get; internal set; }
        public int Frequency { get; internal set; }

        public WAV(byte[] wav)
        {

            // Determine if mono or stereo
            ChannelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get the frequency
            Frequency = bytesToInt(wav, 24);

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            SampleCount = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            if (ChannelCount == 2) SampleCount /= 2;        // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            LeftChannel = new float[SampleCount];
            if (ChannelCount == 2) RightChannel = new float[SampleCount];
            else RightChannel = null;

            // Write to double array/s:
            int i = 0;
            while (pos < wav.Length)
            {
                LeftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                pos += 2;
                if (ChannelCount == 2)
                {
                    RightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;
            }
        }

        public override string ToString()
        {
            return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
        }
    }

    public static class FileOperations
    {

        static FileOperations()
        {
            Assembly.LoadFrom(Assembly.GetAssembly(typeof(FileOperations)).Location.Replace("MoreLuaPower.dll","dependencies" + Path.DirectorySeparatorChar + "NAudio" + Path.DirectorySeparatorChar + "NAudio.dll"));
        }

        public static void Handle(FileInfo info)
        {
            S.I.StartCoroutine(LoadMP3(info.FullName, info.Name));
            
        }

        static IEnumerator LoadMP3(string audioFilePath, string name)
        {
            
            yield return new WaitForEndOfFrame();
            UnityWebRequest uwr = UnityWebRequest.Get(audioFilePath);
           
            uwr.uri = new Uri(uwr.uri.AbsoluteUri.Replace("http://localhost", "file://"));
            uwr.url = uwr.url.Replace("http://localhost", "file://");

            DownloadHandler dhac = uwr.downloadHandler;
            yield return uwr.SendWebRequest();

            if (uwr.isHttpError || uwr.isNetworkError)
            {
                Debug.LogErrorFormat("Could not load audio file for {0}. Error: {1}", audioFilePath, uwr.error);
                yield break;
            }

            AudioClip currentClip = NAudioPlayer.FromMp3Data(dhac.data);

            if (currentClip != null)
            {
                name = name.Replace(".mp3", "");
                if (!LuaPowerData.customMusic.ContainsKey(name)) LuaPowerData.customMusic.Add(name, new LuaPowerData.CustomMusic(currentClip, 1.0f, 0.0f));
                else LuaPowerData.customMusic[name] = new LuaPowerData.CustomMusic(currentClip, 1.0f, 0.0f);
            }
            else
            {
                Debug.LogError("Could not load mp3: " + audioFilePath);
            }
        }
    }

}