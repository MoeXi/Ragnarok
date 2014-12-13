﻿#if TESTS
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

using Ragnarok.Utility;

namespace Ragnarok.Forms.Tests
{
    /// <summary>
    /// Ragnarok.Utility.Color4bのテストを行います。
    /// </summary>
    /// <remarks>
    /// Drawing.Colorの色リストと値を比較するため、
    /// このアセンブリでテストを行っています。
    /// </remarks>
    [TestFixture]
    public sealed class Color4bTest
    {
        private List<Tuple<string, Color4b>> GetColor4bList()
        {
            var flags = BindingFlags.GetProperty | BindingFlags.Public |
                        BindingFlags.Static;

            return typeof(Color4bs)
                .GetProperties(flags)
                .Where(_ => _ != null)
                .Where(_ => _.PropertyType == typeof(Color4b))
                .Select(_ => Tuple.Create(_.Name, (Color4b)_.GetValue(null, null)))
                .ToList();
        }

        private List<Tuple<string, Color>> GetDrawingColorList()
        {
            var flags = BindingFlags.GetProperty | BindingFlags.Public |
                        BindingFlags.Static;

            return typeof(Color)
                .GetProperties(flags)
                .Where(_ => _ != null)
                .Where(_ => _.PropertyType == typeof(Color))
                .Select(_ => Tuple.Create(_.Name, (Color)_.GetValue(null, null)))
                .ToList();
        }

        /// <summary>
        /// RagnarokとDrawingの色リストを比較します。
        /// </summary>
        [Test]
        public void ColorListTest()
        {
            var ragnarokList = GetColor4bList();
            var drawingList = GetDrawingColorList();

            ragnarokList.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            drawingList.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            Assert.AreEqual(drawingList.Count(), ragnarokList.Count());
            for (var i = 0; i < drawingList.Count(); ++i)
            {
                var ragnarokPair = ragnarokList[i];
                var drawingPair = drawingList[i];

                Console.WriteLine("Color.{0}", drawingPair.Item1);

                Assert.AreEqual(drawingPair.Item1, ragnarokPair.Item1);
                Assert.AreEqual(drawingPair.Item2.A, ragnarokPair.Item2.A);
                Assert.AreEqual(drawingPair.Item2.R, ragnarokPair.Item2.R);
                Assert.AreEqual(drawingPair.Item2.G, ragnarokPair.Item2.G);

                // DarkSeaGreenの青成分はForms側のコメントと実際の値が違うためパス
                if (drawingPair.Item1 != "DarkSeaGreen")
                {
                    Assert.AreEqual(drawingPair.Item2.B, ragnarokPair.Item2.B);
                }
            }
        }

        /// <summary>
        /// Color.Parseのテストを行います。
        /// </summary>
        [Test]
        public void ParseTest()
        {
            Assert.AreEqual(Color4bs.Blue, Color4b.Parse("Blue"));
            Assert.AreEqual(Color4bs.Blue, Color4b.Parse("blue"));
            Assert.AreEqual(Color4bs.Blue, Color4b.Parse(" blue "));
            Assert.AreEqual(Color4bs.Blue, Color4b.Parse(" blUe"));
            Assert.AreEqual(Color4bs.Blue, Color4b.Parse("BLUE"));

            var color = Color4b.FromValue(0xFFCCBBAA);
            Assert.AreEqual(Color4bs.Black, Color4b.Parse("#FF000000"));
            Assert.AreEqual(Color4bs.YellowGreen, Color4b.Parse("#FF9ACD32"));
            Assert.AreEqual(Color4bs.YellowGreen, Color4b.Parse("#9ACD32"));
            Assert.AreEqual(color, Color4b.Parse("#FFCCBBAA"));
            Assert.AreEqual(color, Color4b.Parse("#CCBBAA"));
            Assert.AreEqual(color, Color4b.Parse("#FCBA"));
            Assert.AreEqual(color, Color4b.Parse("#CBA"));
            Assert.AreEqual(color, Color4b.Parse(" #CBA "));

            Assert.Catch<FormatException>(() => Color4b.Parse(" bl Ue"));
            Assert.Catch<FormatException>(() => Color4b.Parse(" blxUe"));
            Assert.Catch<FormatException>(() => Color4b.Parse(" blUex"));

            Assert.Catch<FormatException>(() => Color4b.Parse("#0xFFCCBBAA"));
            Assert.Catch<FormatException>(() => Color4b.Parse("# C B A"));
            Assert.Catch<FormatException>(() => Color4b.Parse("#d0C0BcA"));
        }
    }
}
#endif