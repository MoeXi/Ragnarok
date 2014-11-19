﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using IrrKlang;

namespace Ragnarok.Extra.Sound
{
    /// <summary>
    /// 音声ファイルを再生します。
    /// </summary>
    /// <remarks>
    /// 使用dllが正しく初期化できない場合、このクラスを使った時点で
    /// 例外が発生します。
    /// </remarks>
    internal sealed class SoundBackend
    {
        private class SoundStopEventReceiver : ISoundStopEventReceiver
        {
            private EventHandler<SoundStopEventArgs> stopCallback;

            public SoundStopEventReceiver(EventHandler<SoundStopEventArgs> stopCallback)
            {
                this.stopCallback = stopCallback;
            }

            public void OnSoundStopped(ISound sound, StopEventCause cause,
                                       object userData)
            {
                var handler = Interlocked.Exchange(ref this.stopCallback, null);

                if (handler != null)
                {
                    var reason = ConvertReason(cause);

                    handler(null, new SoundStopEventArgs(reason));
                }
            }

            private SoundStopReason ConvertReason(StopEventCause cause)
            {
                switch (cause)
                {
                    case StopEventCause.SoundFinishedPlaying:
                        return SoundStopReason.FinishedPlaying;
                    case StopEventCause.SoundStoppedByUser:
                        return SoundStopReason.StoppedByUser;
                    case StopEventCause.SoundStoppedBySourceRemoval:
                        return SoundStopReason.StoppedBySourceRemoval;
                }

                throw new ArgumentException("cause");
            }
        }

        private readonly ISoundEngine engine;

        /// <summary>
        /// 音声を再生できるかどうかを取得します。
        /// </summary>
        public bool CanUseSound
        {
            get
            {
                return (engine != null);
            }
        }

        /// <summary>
        /// ボリュームを0-100の間で取得または設定します。
        /// </summary>
        public double Volume
        {
            get
            {
                if (engine == null)
                {
                    return 0;
                }

                return Math.Round(engine.SoundVolume * 100);
            }
            set
            {
                if (engine == null)
                {
                    return;
                }

                var volume = (float)value / 100.0f;

                // 設定可能な音量値は0.0～1.0
                engine.SoundVolume = Math.Max(Math.Min(volume, 1.0f), 0.0f);
            }
        }

        /// <summary>
        /// ファイルパスを英数字のIDに変換します。
        /// </summary>
        /// <remarks>
        /// IrrKlangでは日本語のファイル名が使えないため、
        /// base64変換したファイル名をファイルIDとして利用しています。
        /// </remarks>
        private static string GetFileId(string filepath)
        {
            var byteArray = Encoding.UTF8.GetBytes(filepath);

            // base64を使います。
            return Convert.ToBase64String(byteArray);
        }

        /// <summary>
        /// 音声ファイルを読み込みます。
        /// </summary>
        private ISoundSource LoadSoundSource(string filepath)
        {
            // irrKlangは日本語ファイルが読めないので、
            // ストリームから再生することにする。
            var stream = new FileStream(filepath, FileMode.Open);

            // 日本語のファイル名をbase64で英数字の羅列に変換します。
            var fileid = GetFileId(filepath);

            return engine.AddSoundSourceFromIOStream(stream, fileid);
        }

        /// <summary>
        /// サウンドソースをキャッシュから取得し、なければファイルを読み込みます。
        /// </summary>
        private ISoundSource GetSoundSource(string filename)
        {
            // 音声のフルパスを取得します。
            var filepath = Path.GetFullPath(filename);
            if (!File.Exists(filepath))
            {
                return null;
            }

            // irrKlangは日本語のファイル名が読めないので、
            // ストリームから再生する。
            var fileid = GetFileId(filepath);
            var source = engine.GetSoundSource(fileid, false);
            if (source == null)
            {
                source = LoadSoundSource(filepath);
                if (source == null)
                {
                    return null;
                }
            }

            return source;
        }

        /// <summary>
        /// SEを再生します。
        /// </summary>
        public ISound PlaySE(string filename, double volume,
                             EventHandler<SoundStopEventArgs> stopCallback = null)
        {
            if (engine == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            var source = GetSoundSource(filename);
            if (source == null)
            {
                throw new InvalidDataException(
                    "音声ファイルの読み込みに失敗しました。");
            }

            // 音量を設定します。
            source.DefaultVolume = MathEx.Between(0.0f, 1.0f, (float)volume);

            // 再生
            var sound = engine.Play2D(source, false, false, false);
            if (sound == null)
            {
                throw new InvalidOperationException(
                    "音声ファイルの再生に失敗しました。");
            }

            if (stopCallback != null)
            {
                sound.setSoundStopEventReceiver(
                    new SoundStopEventReceiver(stopCallback));
            }

            return sound;
        }

        /// <summary>
        /// 音声プレイヤーオブジェクトを初期化します。
        /// </summary>
        public SoundBackend()
        {
            engine = new ISoundEngine();
        }
    }
}
