using System.Collections.Generic;

namespace TSP {
    class PriorityQueue  {

        private List<PartialPath> nodes;
        private Dictionary<PartialPath, int> positions;

        public PriorityQueue() {
            nodes = new List<PartialPath>();
            nodes.Add(null);
            positions = new Dictionary<PartialPath, int>();
        }

        /*
         * Removes the root node (the minimum).
         * Replaces the root with the lowest right-most node.
         * This node is then "sifted" down until the queue is sorted.
         * O(logn) since it has to go through log n levels in the heap.
         */
        public PartialPath DeleteMin() {
            PartialPath min = nodes[1];
            PartialPath last = nodes[nodes.Count - 1];
            positions[last] = 1;
            positions[min] = -1;
            nodes.Remove(last);
            if (!Empty()) {
                nodes[1] = last;
                SiftDown(nodes[1]);
            }
            return min;
        }

        /*
         * Inserts a node at the lowest right-most spot.
         * BubbleUp is then called which resorts the queue.
         * O(logn) since it has to go through log n levels in the heap.
         */
        public void Insert(PartialPath node) {
            nodes.Add(node);
            positions.Add(node, nodes.Count - 1);
            BubbleUp(node);
        }

        /*
         * Returns if the queue is empty (O(1)).
         */
        public bool Empty() {
            return nodes.Count == 1; ;
        }

        /*
         * Compares node to its parent.
         * If the node is smaller than its parent they are swapped.
         * As it sorts this method goes through at most log n levels in the heap.
         * Therefore the Big-O is O(logn).
         */
        private void BubbleUp(PartialPath node) {
            while (positions[node] != 1 && node.Priority < Parent(node).Priority) {
                Swap(node, Parent(node));
            }
        }

        /*
         * Compares node to its two children.
         * If it is larger than a child, it swaps with the smallest child.
         * As it sorts this method goes through at most log n levels in the heap.
         * Therefore the Big-O is O(logn). 
         */
        private void SiftDown(PartialPath node) {
            while (SmallestChild(node) != null && node.Priority > SmallestChild(node).Priority) {
                Swap(node, SmallestChild(node));
            }
        }

        /*
         * Returns the parent of a given node (O(1)).
         */
        private PartialPath Parent(PartialPath node) {
            return nodes[positions[node] / 2];
        }

        /*
         * Returns the smallest child of a given node.
         * If the node has no children 0 is returned (O(1)).
         */
        private PartialPath SmallestChild(PartialPath node) {
            int posFirstChild = positions[node] * 2;
            int posSecondChild = posFirstChild + 1;
            if (posFirstChild > nodes.Count - 1) {
                return null; //no children
            }
            else if (posSecondChild > nodes.Count - 1) {
                return nodes[posFirstChild]; //only one child
            }
            else {
                return nodes[posFirstChild].Priority < nodes[posSecondChild].Priority ?
                nodes[posFirstChild] : nodes[posSecondChild]; //return whichever child is smaller
            }
        }

        /*
         * Swaps two nodes in the array.
         * Also swaps their positions in the positions array (O(1)).
         */
        private void Swap(PartialPath node1, PartialPath node2) {
            int pos1 = positions[node1];
            int pos2 = positions[node2];
            nodes[pos1] = node2;
            nodes[pos2] = node1;
            positions[node1] = pos2;
            positions[node2] = pos1;
        }
    }
}