﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;

using SharpGL;

namespace Ragnarok.Forms.Shogi.GL
{
    /// <summary>
    /// 文字列用のテクスチャクラスです。
    /// </summary>
    public class TextTexture : Texture
    {
        private string text = string.Empty;
        private Font font = new Font(FontFamily.GenericSansSerif, 40);
        private Color color = Color.White;
        private Color edgeColor = Color.Black;
        private double edgeLength = 1.0;
        private bool updated = true;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        [CLSCompliant(false)]
        public TextTexture(OpenGL gl)
            : base(gl)
        {
        }

        /// <summary>
        /// 描画する文字列を取得または設定します。
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (this.text != value)
                {
                    this.text = value;
                    this.updated = true;
                }
            }
        }

        /// <summary>
        /// 描画するフォントを取得または設定します。
        /// </summary>
        public Font Font
        {
            get { return this.font; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (this.font != value)
                {
                    this.font = value;
                    this.updated = true;
                }
            }
        }

        /// <summary>
        /// 文字列の色を取得または設定します。
        /// </summary>
        public Color Color
        {
            get { return this.color; }
            set
            {
                if (this.color != value)
                {
                    this.color = value;
                    this.updated = true;
                }
            }
        }

        /// <summary>
        /// 縁取りの色を取得または設定します。
        /// </summary>
        public Color EdgeColor
        {
            get { return this.edgeColor; }
            set
            {
                if (this.edgeColor != value)
                {
                    this.edgeColor = value;
                    this.updated = true;
                }
            }
        }

        /// <summary>
        /// 縁取りの色を取得または設定します。
        /// </summary>
        public double EdgeLength
        {
            get { return this.edgeLength; }
            set
            {
                if (this.edgeLength != value)
                {
                    this.edgeLength = value;
                    this.updated = true;
                }
            }
        }

        /// <summary>
        /// 必要ならテクスチャの更新処理を行います。
        /// </summary>
        public void UpdateTexture()
        {
            if (!this.updated)
            {
                return;
            }

            // テクスチャサイズを取得。
            var bounds = MeasureSize();

            // 文字列用のテクスチャを作成。
            using (var bitmap = CreateTextBitmap(bounds))
            {
                if (!Create(bitmap))
                {
                    Log.Error(
                        "文字列テクスチャの作成に失敗しました。");
                    return;
                }
            }

            this.updated = false;
        }

        /// <summary>
        /// 描画する文字列のサイズ(=テキストテクスチャ用のサイズ)を取得します。
        /// </summary>
        private Rectangle MeasureSize()
        {
            using (var bitmap = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bitmap))
            using (var path = new GraphicsPath())
            using (var pen = new Pen(EdgeColor, (float)EdgeLength))
            {
                // 時間かかるかも。
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // GraphicsPath.GetBoundsではPen.MiterLimitの値が考慮され
                // デフォルト値=10のままだと、GetBoundsの返り値が
                // 異様に大きな矩形になってしまいます。
                pen.MiterLimit = 1;

                // StringFormat.GenericTypographicを指定すると
                // ビットマップから不要な空白が取り除かれます。
                path.AddString(
                    Text, Font.FontFamily, (int)Font.Style, Font.SizeInPoints,
                    new Point(0, 0),
                    StringFormat.GenericTypographic);
                var bounds = path.GetBounds(new Matrix(), pen);

                return new Rectangle(
                    (int)Math.Floor(bounds.Left),
                    (int)Math.Floor(bounds.Top),
                    (int)Math.Ceiling(bounds.Width),
                    (int)Math.Ceiling(bounds.Height));
            }
        }

        /// <summary>
        /// 文字列が描画されたビットマップを作成します。
        /// </summary>
        private Bitmap CreateTextBitmap(Rectangle bounds)
        {
            var bitmap = new Bitmap(bounds.Width, bounds.Height,
                                    PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bitmap))
            using (var path = new GraphicsPath())
            using (var brush = new SolidBrush(Color))
            using (var pen = new Pen(EdgeColor, (float)EdgeLength))
            {
                // 時間かかるかも。
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // GraphicsPath.GetBoundsではPen.MiterLimitの値が考慮され
                // デフォルト値=10のままだと、GetBoundsの返り値が
                // 異様に大きな矩形になってしまいます。
                pen.MiterLimit = 1;
                
                // GraphicsPathの場合、描画原点が０にならないことがあるため
                // 矩形領域の左上を原点として描画しています。
                path.AddString(
                    Text, Font.FontFamily, (int)Font.Style, Font.SizeInPoints,
                    new Point(-bounds.Left, -bounds.Top),
                    StringFormat.GenericTypographic);
                
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            return bitmap;
        }
    }
}