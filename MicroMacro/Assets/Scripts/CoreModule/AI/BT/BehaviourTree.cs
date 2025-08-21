using UnityEngine;

namespace CoreModule.AI.BT
{
    public class BehaviourTree
    {
        private readonly Node rootNode;

        public BehaviourTree(Node rootNode)
        {
            this.rootNode = rootNode;

            if (rootNode == null)
            {
                Debug.LogError("RootNode is null. Failed to create BehaviourTree.");
            }
        }

        public void Tick()
        {
            rootNode.Tick();
        }
    }
}