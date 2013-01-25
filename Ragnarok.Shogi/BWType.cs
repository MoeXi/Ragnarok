﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Ragnarok.Utility;

namespace Ragnarok.Shogi
{
    /// <summary>
    /// 先後の情報を示します。
    /// </summary>
    [DataContract()]
    public enum BWType
    {
        /// <summary>
        /// 駒箱の駒などです。
        /// </summary>
        [EnumMember()]
        [LabelDescription(Label = "手番なし")]
        None = 0,

        /// <summary>
        /// 先手です。
        /// </summary>
        [EnumMember()]
        [LabelDescription(Label = "先手番")]
        Black = 1,

        /// <summary>
        /// 後手です。
        /// </summary>
        [EnumMember()]
        [LabelDescription(Label = "後手番")]
        White = 2,
    }

    /// <summary>
    /// <see cref="BWType"/>の拡張メソッド用クラスです。
    /// </summary>
    public static class BWTypeExtension
    {
        /// <summary>
        /// 手番の先後を入れ替えます。
        /// </summary>
        public static BWType Toggle(this BWType self)
        {
            if (self != BWType.None)
            {
                return (
                    self == BWType.Black ?
                    BWType.White :
                    BWType.Black);
            }
            else
            {
                return BWType.None;
            }
        }

        /// <summary>
        /// 先手なら+1、後手なら-1を返します。
        /// </summary>
        public static int Sign(this BWType self)
        {
            return (self == BWType.White ? -1 : +1);
        }
    }
}
