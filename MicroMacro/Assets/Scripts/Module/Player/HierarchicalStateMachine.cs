using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Module.Player
{
    /// <summary>
    /// 階層型有限状態マシン
    /// </summary>
    public class HierarchicalStateMachine
    {
        /// <summary>
        /// ステートマシンの開始状態
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// 現在のステート
        /// </summary>
        public State Current => rootGroup.Current;

        private readonly Dictionary<Type, Dictionary<Type, Func<bool>>> currentTransition = new();
        private readonly StateGroup rootGroup;
        private readonly Dictionary<Type, State> states = new();
        private readonly Dictionary<Type, StateGroup> stateGroups = new();
        private readonly Dictionary<Type, Dictionary<Type, Func<bool>>> transitions = new();

        public HierarchicalStateMachine()
        {
            // ルートのステートグループを作成する
            rootGroup = CreateGroup();
        }

        /// <summary>
        /// ステートマシンを実行します
        /// </summary>
        public void Start<TState>() where TState : State
        {
            Type type = typeof(TState);

            if (!states.ContainsKey(type))
            {
                throw new ArgumentException("Parent not registered");
            }

            State state = states[type];

            if (!state.IsRoot)
            {
                throw new ArgumentException("State is not root");
            }

            EnterState(type);
            Started = true;
        }

        public void Update()
        {
            if (!Started)
            {
                return;
            }

            // 登録された遷移状態を確認して遷移する
            if (CheckTransit(out Type from, out Type to))
            {
                Transit(from, to);
            }

            // ステートを更新する
            rootGroup.Update();
        }

        private bool CheckTransit(out Type from, out Type to)
        {
            bool doTransition = false;
            from = null;
            to = null;

            // 現在有効なステートの遷移条件を確認する
            foreach (var (fromType, transitionGroup) in currentTransition)
            {
                from = fromType;

                foreach (var (toType, condition) in transitionGroup)
                {
                    to = toType;

                    // 遷移条件を満たした場合は遷移する
                    if (condition())
                    {
                        Debug.Log("Transition: " + from + " -> " + to);
                        doTransition = true;
                        break;
                    }
                }

                if (doTransition)
                {
                    break;
                }
            }

            return doTransition;
        }

        public void UpdatePhysics()
        {
            if (!Started)
            {
                return;
            }

            // ステートを固定フレームで更新する
            rootGroup.UpdatePhysics();
        }

        /// <summary>
        /// ステートの遷移を行います
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void Transit(Type from, Type to)
        {
            if (!currentTransition.TryGetValue(from, out var transitionGroup) ||
                !transitionGroup.ContainsKey(to))
            {
                throw new ArgumentException("Transition not registered");
            }

            // 移動元のステートを終了する
            stateGroups[from].Exit();

            // 移動先のステートを開始する
            stateGroups[to].Enter(states[to]);
        }

        private void EnterState(Type type)
        {
            if (!states.ContainsKey(type))
            {
                throw new ArgumentException("State not registered");
            }

            State state = states[type];
            rootGroup.Enter(state);
        }

        /// <summary>
        /// ルートステートを追加します
        /// </summary>
        public void AddState<TState>(TState state) where TState : State
        {
            // ルートグループに追加
            rootGroup.Add(state);
            
            // ステートの登録
            RegisterState(state, rootGroup);
        }

        public void AddState<TState, TParent>(TState state, bool isDefault = false) where TState : State where TParent : State
        {
            Type parentType = typeof(TParent);

            if (!states.ContainsKey(parentType))
            {
                throw new ArgumentException("Parent not registered");
            }

            if (!states[parentType].IsRoot && states[parentType].Group.Contains(state))
            {
                throw new ArgumentException("State already registered");
            }

            RegisterState(state, states[parentType].Group);

            if (isDefault)
            {
                states[parentType].Group.AddAsDefault(state);
            }
            else
            {
                states[parentType].Group.Add(state);
            }

            state.Parent = states[parentType];
        }


        /// <summary>
        /// 遷移条件を追加します
        /// </summary>
        public void AddTransition<TFrom, TTo>(Func<bool> condition) where TFrom : State where TTo : State
        {
            Type from = typeof(TFrom);
            Type to = typeof(TTo);

            if (!states.ContainsKey(from) || !states.ContainsKey(to))
            {
                throw new ArgumentException("State not registered");
            }

            if (transitions[from].ContainsKey(to))
            {
                throw new ArgumentException("Transition already registered");
            }

            State fromState = states[from];
            State toState = states[to];

            // 異なる階層のステートの遷移を許可しない
            if (fromState.Parent != toState.Parent)
            {
                throw new ArgumentException("Transition between different parents");
            }

            // 遷移条件を追加する
            transitions[from].Add(to, condition);
        }

        /// <summary>
        /// ステートをステートマシンに登録します
        /// </summary>
        private void RegisterState<TState>(TState state, StateGroup stateGroup) where TState : State
        {
            Type stateType = typeof(TState);

            if (states.ContainsKey(stateType))
            {
                throw new ArgumentException("State already registered");
            }

            // ステートの初期化
            state.Type = stateType;
            state.Group = CreateGroup();

            // ステートを登録する
            states.Add(stateType, state);
            
            // ステートグループを登録する
            stateGroups[stateType] = stateGroup;
            
            // 遷移条件の初期化
            transitions.Add(stateType, new Dictionary<Type, Func<bool>>());
        }

        private StateGroup CreateGroup()
        {
            var stateGroup = new StateGroup();

            stateGroup.OnEnter += st =>
            {
                // 遷移先のトランジションを追加する
                currentTransition.Add(st.Type, transitions[st.Type]);
            };

            stateGroup.OnExit += st =>
            {
                // 遷移先のトランジションを削除する
                currentTransition.Remove(st.Type);
            };

            return stateGroup;
        }

        /// <summary>
        /// ステートの抽象クラス
        /// </summary>
        public abstract class State
        {
            public bool IsRoot => Parent == null;
            public Type Type { get; internal set; }
            
            /// <summary>
            /// 親ステート
            /// </summary>
            public State Parent { get; internal set; }
            
            /// <summary>
            /// ステートが持つステートグループ
            /// </summary>
            internal StateGroup Group { get; set; }

            /// <summary>
            /// ステートを抜けたときに発行するトークン
            /// </summary>
            protected CancellationToken CancellationToken => stateCanceller.Token;
            private CancellationTokenSource stateCanceller;

            internal void EnableCanceller()
            {
                stateCanceller = new CancellationTokenSource();
            }

            internal void DisposeCanceller()
            {
                stateCanceller.Cancel();
                stateCanceller.Dispose();

                stateCanceller = null;
            }

            internal abstract void OnEnter();
            internal abstract void OnExit();
            internal abstract void Update();
            internal abstract void UpdatePhysics();
        }

        /// <summary>
        /// ステートグループを管理するクラス
        /// </summary>
        internal class StateGroup
        {
            public bool IsEmpty => Children.Count == 0;
            
            /// <summary>
            /// デフォルトで有効化されるステート
            /// </summary>
            public State Default { get; private set; }
            
            /// <summary>
            /// 現在有効なステート
            /// </summary>
            public State Current { get; private set; }
            
            /// <summary>
            /// ステートグループに所属するステート
            /// </summary>
            private HashSet<State> Children { get; } = new HashSet<State>();

            public event Action<State> OnEnter;
            public event Action<State> OnExit;

            public void AddAsDefault(State state)
            {
                Add(state);
                Default = state;
            }

            public void Add(State state)
            {
                if (Children.Contains(state))
                {
                    throw new ArgumentException("State already registered");
                }

                // 初期状態が未設定の場合はデフォルトとして設定する
                if (IsEmpty)
                {
                    Default = state;
                }

                Children.Add(state);
            }

            public bool Contains(State state)
            {
                return Children.Contains(state);
            }

            public void Update()
            {
                if (Current == null)
                {
                    return;
                }

                // 子要素を先に更新する
                Current.Group.Update();
                Current.Update();
            }

            public void UpdatePhysics()
            {
                if (Current == null)
                {
                    return;
                }

                // 子要素を先に更新する
                Current.Group.UpdatePhysics();
                Current.UpdatePhysics();
            }

            public void Enter(State state = null)
            {
                if (IsEmpty)
                {
                    return;
                }

                Current = state ?? Default;

                // 子要素の状態を先に有効化する
                Current.Group.Enter();

                Current.EnableCanceller();
                Current.OnEnter();
                OnEnter?.Invoke(Current);
            }

            public void Exit()
            {
                if (IsEmpty)
                {
                    return;
                }

                // 子要素の状態を先に無効化する
                Current.Group.Exit();

                Current.OnExit();
                Current.DisposeCanceller();
                OnExit?.Invoke(Current);
                Current = null;
            }
        }
    }
}