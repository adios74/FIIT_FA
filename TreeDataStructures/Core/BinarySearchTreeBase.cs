using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => throw new NotImplementedException();
    public ICollection<TValue> Values => throw new NotImplementedException();
    
    
    public virtual void Add(TKey key, TValue value)
    {
        TNode node = CreateNode(key, value);
        var current = Root;
        Root ??= node;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp < 0)
            {
                if (current.Left != null)
                {
                    current = current.Left;
                }
                else
                {
                    current.Left = node;
                    node.Parent = current;
                    break;
                }
            }
            else
            {
                if (current.Right != null)
                {
                    current = current.Right;
                }
                else
                {
                    current.Right = node;
                    node.Parent = current;
                    break;
                }
            }
        }

        Count++;
        OnNodeAdded(node);
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        TNode? parent = node.Parent;
        if (node.Left == null && node.Right == null)
        {
            if (parent != null)
            {
                if (parent.Left == node)
                {
                    parent.Left = null;
                    OnNodeRemoved(parent, null);
                }
                else if (parent.Right == node)
                {
                    parent.Right = null;
                    OnNodeRemoved(parent, null);
                }
            }
            else
            {
                Root = null;
                OnNodeRemoved(parent, null);
            }
            
        }
        else if (node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(parent, node.Left);
        }
        else if (node.Left == null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(parent, node.Right);
        }
        else
        {
            var newElement = node;
            newElement = newElement.Right;

            while (newElement.Left != null)
            {
                newElement = newElement.Left;
            }

            if (newElement.Parent != node)
            {
                Transplant(newElement, newElement.Right);
                newElement.Right = node.Right;
                newElement.Right.Parent = newElement;
            }
            
            Transplant(node, newElement);
            newElement.Left = node.Left;
            newElement.Left.Parent = newElement;
            OnNodeRemoved(parent, newElement);
        }
        
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        TNode? y = x.Parent;
        if (y == null)
        {
            throw new ArgumentException("Поворот невозможен из-за отсутствия элемента-родителя.");
        }
        
        if (y.Parent == null)
        {
            Root = x;
            x.Parent = null;
        }
        else
        {
            if (y.IsLeftChild)
            {
                y.Parent.Left = x;
            }
            else
            {
                y.Parent.Right = x;
            }
            x.Parent = y.Parent;
        }

        y.Parent = x;
        y.Right = x.Left;
        x.Left?.Parent = y;
        x.Left = y;
    }

    protected void RotateRight(TNode y)
    {
        TNode? x = y.Parent;
        if (x == null)
        {
            throw new ArgumentException("Поворот невозможен из-за отсутствия элемента-родителя.");
        }

        if (x.Parent == null)
        {
            Root = y;
            y.Parent = null;
        }
        else
        {
            if (x.IsLeftChild)
            {
                x.Parent.Left = y;
            }
            else
            {
                x.Parent.Right = y;
            }
            y.Parent = x.Parent;
        }
        
        x.Parent = y;
        x.Left = y.Right;
        y.Right?.Parent = x;
        y.Right = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        RotateRight(x);
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        RotateLeft(y);
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        if (x.Parent == null)
        {
            throw new ArgumentException("Поворот невозможен из-за отсутствия элемента-родителя.");
        }
        RotateLeft(x.Parent);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        if (y.Parent == null)
        {
            throw new ArgumentException("Поворот невозможен из-за отсутствия элемента-родителя.");
        }
        RotateRight(y.Parent);
        RotateRight(y);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => InOrderTraversal(Root);
    
    private IEnumerable<TreeEntry<TKey, TValue>>  InOrderTraversal(TNode? node)
    {
        if (node == null) {  yield break; }
        throw new NotImplementedException();
    }
    
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => throw new NotImplementedException();
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => throw new NotImplementedException();
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private Stack<TNode>? _stack;
        private readonly TNode? _root;
        private TNode? _currentNode;
        private bool _started;
        
        // probably add something here
        private readonly TraversalStrategy _strategy; // or make it template parameter?

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            _stack = null;
            _currentNode = null;
            _started = false;
        }
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        public TreeEntry<TKey, TValue> Current
        {
            get
            {
                if (_currentNode == null)
                {
                    throw new InvalidOperationException();
                }
                return new TreeEntry<TKey, TValue>(_currentNode.Key, _currentNode.Value, 0);
            }
        }
        object IEnumerator.Current => Current;
        
        
        public bool MoveNext()
        {
            if (_strategy == TraversalStrategy.InOrder)
            {
                if (!_started)
                {
                    _started = true;
                    _stack = new Stack<TNode>();
                    
                    if (_root == null)
                    {
                        return false;
                    }
                    
                    _currentNode = _root;
                    _stack.Push(_currentNode);
                    while (_currentNode.Left != null)
                    {
                        _stack.Push(_currentNode.Left);
                        _currentNode = _currentNode.Left;
                    }

                    if (_stack.Count > 0)
                    {
                        _currentNode = _stack.Pop();
                    }
                    
                    if (_currentNode != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (_currentNode == null)
                    {
                        throw new InvalidOperationException();
                    }
                    if (_currentNode.Right == null)
                    {
                        if (_stack?.Count > 0)
                        {
                            _currentNode = _stack.Pop();
                            return true;
                        }
                        else
                        {
                            _currentNode = null;
                            _started = false;
                            return false;
                        }
                    }
                    _currentNode = _currentNode.Right;
                    _stack?.Push(_currentNode);
                    while (_currentNode.Left != null)
                    {
                        _stack?.Push(_currentNode.Left);
                        _currentNode = _currentNode.Left;
                    }

                    if (_stack?.Count > 0)
                    {
                        _currentNode = _stack.Pop();
                        return true;
                    }
                    else
                    {
                        _currentNode = null;
                        _started = false;
                        return false;
                    }
                }
            } else if (_strategy == TraversalStrategy.PreOrder)
            {
                if (_root == null)
                {
                    return false;
                }
                _currentNode = _root;

                if (!_started)
                {
                    _started = true;
                    return true;
                }
                else if (_currentNode.Left != null && _currentNode.Right != null && _started)
                {
                    _stack?.Push(_currentNode);
                    _currentNode = _currentNode.Left;
                    return true;
                }
                else if (_currentNode.Left != null && _started)
                {
                    _currentNode = _currentNode.Left;
                    return true;
                }
                else if (_currentNode.Right != null && _started)
                {
                    _currentNode = _currentNode.Right;
                    return true;
                }
                else if (_started)
                {
                    if (_stack?.Count > 0)
                    {
                        _currentNode = _stack?.Pop().Right;
                        return true; 
                    }
                    return false;
                }
                
            } else if (_strategy == TraversalStrategy.PostOrder)
            {
                Stack<TNode>? tops = new Stack<TNode>();

                if (_root == null)
                {
                    return false;
                }
                
                _currentNode = _root;

                while (_currentNode.Left != null || _currentNode.Right != null)
                {
                    if (_currentNode.Left != null && _currentNode.Right != null)
                    {
                        tops.Push(_currentNode);
                    }
                    else if (_currentNode.Left != null)
                    {
                        _stack?.Push(_currentNode);
                        _currentNode = _currentNode.Left;
                    }
                    else if (_currentNode.Right != null)
                    {
                        _stack?.Push(_currentNode);
                        _currentNode = _currentNode.Right;
                    }
                }
            }
            throw new NotImplementedException("Strategy not implemented");
        }
        
        public void Reset()
        {
            _currentNode = null;
            _stack?.Clear();
            _started = false;
        }

        
        public void Dispose()
        {
            _currentNode = null;
            _stack?.Clear();
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}