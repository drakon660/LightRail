namespace LightRail.Wsdl.Core;

public class TreeNode<T> where T : NodeElement
{
    public int Level { get; set; }
    public T Data { get; set; }
    private TreeNode<T> Parent { get; set; }
    public List<TreeNode<T>> Children { get; }
    public TreeNode<T> FirstChild => Children?.FirstOrDefault();
    public bool HasMultipleChildren => Children?.Count > 1;

    private TreeNode(T data, int level)
    {
        Data = data;
        Children = new List<TreeNode<T>>();
        Level = level;
    }

    public static TreeNode<T> Create(T data, int level) => new TreeNode<T>(data, level);

    private void SetParent(TreeNode<T> parent)
    {
        Parent = parent;
    }

    public void AddChildAtLevel(int level, TreeNode<T> child)
    {
        if (level < 0)
        {
            throw new ArgumentException("Level must be a non-negative integer.");
        }
        if (level == 0 || level == 1)
        {
            child.SetParent(this);
            Children.Add(child);
        }
        else if (Children.Count > 0)
        {
            Children[^1].AddChildAtLevel(level - 1, child);
        }
        else
        {
            throw new ArgumentException("Level exceeds depth of tree.");
        }
    }
}
