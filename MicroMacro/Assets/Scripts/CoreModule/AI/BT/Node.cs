using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreModule.AI.BT
{
    public abstract class Node
    {
        public Status CurrentStatus { get; private set; } = Status.Ready;

        public Status Tick()
        {
            if (CurrentStatus == Status.Ready)
            {
                OnStart();
            }
            
            Status result = OnUpdate();

            if (CurrentStatus == Status.Success || CurrentStatus == Status.Failure)
            {
                OnEnd();
                CurrentStatus = Status.Ready;
            }
            else
            {
                CurrentStatus = result;
            }

            return result;
        }

        protected virtual void OnStart()
        {
        }

        protected abstract Status OnUpdate();

        protected virtual void OnEnd()
        {
        }

        public enum Status
        {
            Ready,
            Success,
            Failure,
            Running,
        }
    }

    /// <summary>
    /// 子要素を持つノード
    /// </summary>
    public abstract class BranchNode : Node
    {
        protected readonly List<Node> Children = new List<Node>();
        protected int ChildIndex;

        /// <summary>
        /// 子要素を追加します
        /// </summary>
        /// <param name="node">子要素となるノード</param>
        public void AddChild(Node node)
        {
            Children.Add(node);
        }

        /// <summary>
        /// 子要素を取得します
        /// </summary>
        /// <returns>子要素の読み取り専用リスト</returns>
        public IReadOnlyList<Node> GetChildren()
        {
            return Children;
        }

        protected override void OnEnd()
        {
            ChildIndex = 0;
        }
    }

    /// <summary>
    /// 指定されたアクションを実行するノード
    /// </summary>
    public class ActionNode : Node
    {
        private readonly Func<Status> action;

        public ActionNode(Func<Status> action)
        {
            this.action = action;
        }

        protected override Status OnUpdate()
        {
            return action();
        }
    }

    /// <summary>
    /// 指定した条件を評価するノード
    /// </summary>
    public class ConditionNode : Node
    {
        private readonly Func<Status> condition;

        public ConditionNode(Func<Status> condition)
        {
            this.condition = condition;
        }

        protected override Status OnUpdate()
        {
            return condition();
        }
    }

    /// <summary>
    /// 指定された条件を満たせば、子を実行するノード
    /// </summary>
    public class DecoratorNode : Node
    {
        private readonly Node child;
        private readonly Func<Status> condition;

        public DecoratorNode(Node child, Func<Status> condition)
        {
            this.child = child;
            this.condition = condition;

            if (child == null)
            {
                Debug.Log("child is null. Failed to create node.");
            }

            if (condition == null)
            {
                Debug.Log("condition is null. Failed to create node.");
            }
        }

        protected override Status OnUpdate()
        {
            Status status = condition();

            if (status == Status.Success)
            {
                return child.Tick();
            }

            return status;
        }
    }

    /// <summary>
    /// 子ノードを先頭から実行します。
    /// 子ノードが全てSuccessであれば、Successを返します。
    /// </summary>
    public class SequenceNode : BranchNode
    {
        protected override Status OnUpdate()
        {
            if (ChildIndex >= Children.Count)
            {
                // 子が全て実行されていたら成功とする
                return Status.Success;
            }

            var child = Children[ChildIndex];
            Status status = child.Tick();

            // 子が成功したら次の子を進める
            if (status == Status.Success)
            {
                ChildIndex++;
            }

            switch (status)
            {
                // 子が成功 or 実行中であれば自分自身を実行中とする
                case Status.Success:
                case Status.Running:
                    return Status.Running;

                // 子が失敗したら自分自身も失敗とする
                case Status.Failure:
                    return Status.Failure;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// 子ノードが一つでもSuccessであれば、Successを返すノード
    /// </summary>
    public class SelectorNode : BranchNode
    {
        protected override Status OnUpdate()
        {
            if (ChildIndex >= Children.Count)
            {
                // 子が全て実行されていたら失敗とする
                return Status.Failure;
            }

            var child = Children[ChildIndex];
            Status status = child.Tick();

            // 子が失敗したら次の子を実行する
            if (status == Status.Failure)
            {
                ChildIndex++;
            }

            switch (status)
            {
                // 子が失敗 or 実行中であれば自分自身を実行中とする
                case Status.Failure:
                case Status.Running:
                    return Status.Running;

                // 子が成功したら処理を終了する
                case Status.Success:
                    return Status.Success;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}