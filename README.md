# file-listing
파일 탐색 도구

개발자가 선택한 폴더의 모든 폴더, 파일을 나열합니다.


## Usage

``` csharp
private async void button1_Click(object sender, EventArgs e)
{
    FileIndexor fi = new FileIndexor();
    await fi.ListingDirectoryAsync(@"C:\");
    
    listing(fi);
}

private void listing(FileIndexor fi)
{
    FileIndexorNode node = fi.GetRootNode();
    foreach (FileIndexorNode n in node.Nodes)
    {
        make_node(treeView1.Nodes, Path.GetFileName(n.Path.Remove(n.Path.Length - 1)));
        make_tree(n, treeView1.Nodes[treeView1.Nodes.Count - 1]);
    }
    foreach (FileInfo f in new DirectoryInfo(node.Path).GetFiles())
        make_node(treeView1.Nodes, f.Name);
}

private void make_tree(FileIndexorNode fn, TreeNode tn)
{
    foreach (FileIndexorNode n in fn.Nodes)
    {
        make_node(tn.Nodes, Path.GetFileName(n.Path.Remove(n.Path.Length - 1)));
        make_tree(n, tn.Nodes[tn.Nodes.Count - 1]);
    }
    foreach (FileInfo f in new DirectoryInfo(fn.Path).GetFiles())
        make_node(tn.Nodes, f.Name);
}

private void make_node(TreeNodeCollection tnc, string path)
{
    TreeNode tn = new TreeNode(path);
    tnc.Add(tn);
}
```
