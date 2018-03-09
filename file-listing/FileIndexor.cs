/***

   Copyright (C) 2017. rollrat. All Rights Reserved.

   03-09-2018:   HyunJun Jeong, Creation

***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace file_listing
{
    public class FileIndexor
    {
        FileIndexorNode node;
        List<Tuple<string, UInt64>> directory_list = new List<Tuple<string, UInt64>>();
        List<Tuple<string, string>> error_list = new List<Tuple<string, string>>();
        string root_directory;

        public string RootDirectory { get { return root_directory; } set { root_directory = value; } }
        public FileIndexorNode Node { get { return node?.Nodes?[0]; } }
        public int Count { get { return directory_list.Count; } }
        public bool OnlyListing { get; set; }
        
        public void Clear()
        {
            directory_list.Clear();
            error_list.Clear();
            node = null;
        }

        public async Task ListingDirectoryAsync(string target_directory)
        {
            root_directory = target_directory;
            node = new FileIndexorNode(target_directory, 0);
            await Task.Run(() => listingFolder(target_directory));
            await Task.Run(() => createNodes());
        }

        public async Task ListingDirectoryWithFilesAsync(string target_directory)
        {

        }

        #region [--- Listing ---]

        private void listingFolder(string path)
        {
            try
            {
                if (!OnlyListing)
                {
                    UInt64 folder_size = 0;

                    foreach (FileInfo f in new DirectoryInfo(path).GetFiles())
                        folder_size += (UInt64)f.Length;

                    lock (directory_list)
                    {
                        if (path.EndsWith("\\"))
                            directory_list.Add(new Tuple<string, UInt64>(path, folder_size));
                        else
                            directory_list.Add(new Tuple<string, UInt64>(path + "\\", folder_size));
                    }
                }
                else
                {
                    lock (directory_list)
                    {
                        if (path.EndsWith("\\"))
                            directory_list.Add(new Tuple<string, UInt64>(path, 0));
                        else
                            directory_list.Add(new Tuple<string, UInt64>(path + "\\", 0));
                    }
                }

                Parallel.ForEach(Directory.GetDirectories(path), n => listingFolder(n));
            }
            catch (Exception ex)
            {
                error_list.Add(new Tuple<string, string>(path, ex.ToString()));
            }
        }

        private Int32 index = 0;
        private void createNodes()
        {
            for (; index < directory_list.Count - 1; index++)
            {
                FileIndexorNode _node = new FileIndexorNode(directory_list[index].Item1, directory_list[index].Item2);
                if (directory_list[index + 1].Item1.Contains(directory_list[index].Item1))
                {
                    node.AddItem(_node);
                    index += 1;
                    createNodesRecursize(ref _node);
                    break;
                }
            }
        }

        private void createNodesRecursize(ref FileIndexorNode parent_node)
        {
            for (; index < directory_list.Count; index++)
            {
                if (directory_list[index].Item1.Contains(node.Path))
                {
                    FileIndexorNode m = new FileIndexorNode(directory_list[index].Item1, directory_list[index].Item2);
                    node.AddItem(m);
                    if (index < directory_list.Count - 1 && 
                        directory_list[index + 1].Item1.Contains(directory_list[index].Item1))
                    {
                        index++;
                        createNodesRecursize(ref m);
                    }
                    node.Size += m.Size;
                }
                else
                {
                    index--;
                    break;
                }
            }
        }

        #endregion

        public FileIndexorNode GetPathNode(string path)
        {
            string[] seperated = path.Split('\\');
            string section = "";
            FileIndexorNode n = node;
            for (int i = 0; i < seperated.Length; i++)
            {
                section += seperated[i] + '\\';
                foreach (FileIndexorNode nd in n.Nodes)
                    if (nd.Path == section)
                    { n = nd; break; }
            }
            return n;
        }

        public FileIndexorNode GetRootNode()
        {
            return GetPathNode(root_directory);
        }

        public List<string> GetDirectoies()
        {
            List<string> result = new List<string>();
            directory_list.ForEach(n => result.Add(n.Item1));
            return result;
        }

    }
}
