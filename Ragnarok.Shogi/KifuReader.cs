﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ragnarok.Shogi
{
    using File;

    /// <summary>
    /// 棋譜の読み込みを行います。
    /// </summary>
    public static class KifuReader
    {
        /// <summary>
        /// sjisが基本。
        /// </summary>
        private static Encoding DefaultEncoding =
            Encoding.GetEncoding("Shift_JIS");

        /// <summary>
        /// 棋譜をファイルから読み込みます。
        /// </summary>
        public static KifuObject LoadFile(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                throw new ArgumentNullException("filepath");
            }

            using (var stream = new FileStream(filepath, FileMode.Open))
            using (var reader = new StreamReader(stream, DefaultEncoding))
            {
                return Load(reader);
            }
        }

        /// <summary>
        /// 棋譜を文字列から読み込みます。
        /// </summary>
        public static KifuObject LoadFrom(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            using (var reader = new StringReader(text))
            {
                return Load(reader);
            }
        }

        /// <summary>
        /// 棋譜の読み込みます。
        /// </summary>
        public static KifuObject Load(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var kifuReaders = new IKifuReader[]
            {
                new KifReader(),
                new Ki2Reader(),
            };

            // すべての形式のファイルを読み込んでみます。
            foreach (var kifuReader in kifuReaders)
            {
                try
                {
                    var obj = kifuReader.Load(reader);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
                catch
                {
                    // 違う形式のファイルだった場合
                }
            }

            throw new ArgumentException(
                "棋譜の読み込みに失敗しました。");
        }
    }
}
