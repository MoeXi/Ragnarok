﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ragnarok.Presentation.Utility
{
    /// <summary>
    /// TextBoxに関する拡張機能を提供します。
    /// </summary>
    public static class TextBoxUtil
    {
        /// <summary>
        /// テキスト表示を自動で右端までスクロールさせるための添付プロパティです。
        /// </summary>
        public static readonly DependencyProperty AutoHorizontalScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "AutoHorizontalScrollToEnd", typeof(bool),
                typeof(ListBoxUtil),
                new UIPropertyMetadata(false,
                    OnAutoHorizontalScrollToEndChanged));

        /// <summary>
        /// テキスト表示を自動で右端までスクロールさせるかどうかを取得します。
        /// </summary>
        public static bool GetAutoHorizontalScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoHorizontalScrollToEndProperty);
        }

        /// <summary>
        /// テキスト表示を自動で右端までスクロールさせるかどうかを設定します。
        /// </summary>
        public static void SetAutoHorizontalScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoHorizontalScrollToEndProperty, value);
        }

        private static void OnAutoHorizontalScrollToEndChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var textBox = s as TextBox;

            var handler = new TextChangedEventHandler(
                (s1, e1) =>
                {
                    // キャレット位置を最後にします。
                    textBox.CaretIndex = int.MaxValue;

                    // 表示をスクロールさせます。
                    var rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
                    textBox.ScrollToHorizontalOffset(rect.Right);
                });

            if ((bool)e.NewValue)
            {
                textBox.TextChanged += handler;
            }
            else
            {
                textBox.TextChanged -= handler;
            }
        }
    }
}