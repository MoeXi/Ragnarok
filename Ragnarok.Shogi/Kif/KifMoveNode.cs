﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ragnarok.Shogi
{
    using File;

    /// <summary>
    /// 変化のあるkif形式を読み取るときに使います。
    /// </summary>
    public sealed class KifMoveNode
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KifMoveNode()
        {
            VariationInfoList = new List<VariationInfo>();
            CommentList = new List<string>();
        }

        /// <summary>
        /// 手数を取得または設定します。
        /// </summary>
        public int MoveCount
        {
            get;
            set;
        }

        /// <summary>
        /// 指し手があるファイルの行数を取得または設定します。
        /// </summary>
        public int LineNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 指し手を取得または設定します。
        /// </summary>
        public Move Move
        {
            get;
            set;
        }

        /// <summary>
        /// 着手にかかった時間を取得または設定します。
        /// </summary>
        public TimeSpan Duration
        {
            get;
            set;
        }

        /// <summary>
        /// 着手にかかった時間を秒単位で取得または設定します。
        /// </summary>
        public int DurationSeconds
        {
            get { return (int)Duration.TotalSeconds; }
            set { Duration = TimeSpan.FromSeconds(value); }
        }

        /// <summary>
        /// ノードに付随する評価値や変化などを取得または設定します。
        /// </summary>
        public List<VariationInfo> VariationInfoList
        {
            get;
            set;
        }

        /// <summary>
        /// コメント行を取得または設定します。
        /// </summary>
        public List<string> CommentList
        {
            get;
            set;
        }

        /// <summary>
        /// 本譜における次の指し手を取得または設定します。
        /// </summary>
        public KifMoveNode NextNode
        {
            get;
            set;
        }

        /// <summary>
        /// 次の変化を取得します。
        /// </summary>
        public KifMoveNode VariationNode
        {
            get;
            set;
        }

        /// <summary>
        /// 次の指し手に本譜以外の変化があるか調べます。
        /// </summary>
        public bool HasVariationNode
        {
            get;
            set;
        }

        /// <summary>
        /// ノード全体を文字列化します。
        /// </summary>
        public string DebugText
        {
            get
            {
                var sb = new StringBuilder();

                MakeString(sb, 0);
                return sb.ToString();
            }
        }

        /// <summary>
        /// ノード全体を文字列化します。
        /// </summary>
        private void MakeString(StringBuilder sb, int nmoves)
        {
            if (Move != null)
            {
                var str = Move.ToString();
                var hanlen = str.HankakuLength();

                sb.AppendFormat(" - {0}{1}",
                    str, new string(' ', 8 - hanlen));
            }

            if (NextNode != null)
            {
                NextNode.MakeString(sb, nmoves + 1);
            }

            if (VariationNode != null)
            {
                sb.AppendLine();
                sb.Append(new string(' ', 11 * nmoves));

                VariationNode.MakeString(sb, nmoves);
            }
        }

        /// <summary>
        /// 解析された変化を示す正規表現です。
        /// </summary>
        private static readonly Regex KifAnalyzedVariationRegex = new Regex(
            @"\s*(\*|<analyze>)?\s*(?<value>\d+)\s*(.+)\s*(</analyze>)?\s*$");

        /// <summary>
        /// コメント文字列を追加します。
        /// </summary>
        public void AddComment(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            CommentList.Add(comment);
        }

        /// <summary>
        /// コメントから変化行を探します。
        /// </summary>
        public void SetupVariationInfo(Board board)
        {
            for (var i = 0; i < CommentList.Count(); )
            {
                var variationInfo = ParseVariationInfo(CommentList[i], board);
                if (variationInfo != null)
                {
                    VariationInfoList.Add(variationInfo);
                    CommentList.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }

            if (VariationNode != null)
            {
                VariationNode.SetupVariationInfo(board);
            }

            if (Move != null && Move.Validate())
            {
                var bmove = MakeMove(board, new List<Exception>());
                if (bmove == null || !bmove.Validate())
                {
                    Log.Error("'{0}'が正しく着手できません。", bmove);
                    return;
                }

                if (NextNode != null)
                {
                    NextNode.SetupVariationInfo(board);
                }

                board.Undo();
            }
        }

        /// <summary>
        /// 棋譜コメントから評価値や変化の取得します。
        /// </summary>
        private VariationInfo ParseVariationInfo(string comment, Board board)
        {
            var m = KifAnalyzedVariationRegex.Match(comment);
            if (!m.Success)
            {
                return null;
            }

            // 評価値の取得を行います。
            int value;
            if (!int.TryParse(m.Groups["value"].Value, out value))
            {
                return null;
            }

            // 変化の各手を解析して取得します。
            var variationStr = m.Groups[2].Value;
            var variation = new List<BoardMove>();
            var cloned = board.Clone();
            while (true)
            {
                var parsed = string.Empty;
                var move = ShogiParser.ParseMoveEx(variationStr, false, ref parsed);
                if (move == null)
                {
                    break;
                }

                var bmove = cloned.ConvertMove(move, false);
                if (bmove == null || !bmove.Validate())
                {
                    break;
                }

                if (!cloned.DoMove(bmove))
                {
                    break;
                }

                variation.Add(bmove);
                variationStr = variationStr.Substring(parsed.Length);
            }

            return new VariationInfo
            {
                Value = value,
                MoveList = variation,
            };
        }

        /// <summary>
        /// KifMoveNodeからMoveNodeへ構造を変換します。
        /// </summary>
        public MoveNode ConvertToMoveNode(Board board, KifMoveNode head,
                                          out Exception error)
        {
            var errors = new List<Exception>();
            var root =  new MoveNode
            {
                MoveCount = head.MoveCount,
                Duration = head.Duration,
                VariationInfoList = head.VariationInfoList,
                CommentList = head.CommentList,
            };
            // これでrootの子要素に指し手ツリーが設定されます。
            ConvertToMoveNode(board, root, errors);

            error = (
                !errors.Any() ? null :
                errors.Count() == 1 ? errors.FirstOrDefault() :
                new AggregateException(errors));

            return root;
        }

        /// <summary>
        /// KifMoveNodeからMoveNodeへ構造を変換します。
        /// 結果は<paramref name="root"/>以下に設定されます。
        /// </summary>
        private void ConvertToMoveNode(Board board, MoveNode root,
                                       List<Exception> errors)
        {
            for (var node = this; node != null; node = node.VariationNode)
            {
                // 指し手を実際に指してみます。
                var bmove = node.MakeMove(board, errors);
                if (bmove == null)
                {
                    continue;
                }

                var moveNode = new MoveNode
                {
                    Move = bmove,
                    MoveCount = node.MoveCount,
                    Duration = node.Duration,
                    VariationInfoList = node.VariationInfoList,
                    CommentList = node.CommentList,
                };

                // 次の指し手とその変化を変換します。
                if (node.NextNode != null)
                {
                    node.NextNode.ConvertToMoveNode(board, moveNode, errors);
                }

                root.AddNextNode(moveNode);
                board.Undo();
            }
        }

        /// <summary>
        /// このノードの手を実際に指してみて、着手可能か確認します。
        /// </summary>
        private BoardMove MakeMove(Board board, List<Exception> errors)
        {
            var bmove = board.ConvertMove(Move, true);
            if (bmove == null || !bmove.Validate())
            {
                errors.Add(new FileFormatException(
                    LineNumber,
                    string.Format(
                        "{0}手目: 指し手が正しくありません。",
                        MoveCount)));
                return null;
            }

            // 局面を進めます。
            if (!board.DoMove(bmove))
            {
                errors.Add(new FileFormatException(
                    LineNumber,
                    string.Format(
                        "{0}手目の手を指すことができませんでした。",
                        MoveCount)));
                return null;
            }

            return bmove;
        }
    }
}
