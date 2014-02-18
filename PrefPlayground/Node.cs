using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrefPlayground
{
    public class Node
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Node Next { get; set; }

        private Node _root;

        public Node Link(long id, string name)
        {
            if (id > 0)
            {
                Next = new Node { Id = id, Name = name };
                Next._root = _root ?? this;
                return Next;
            }
            else
            {
                return this;
            }
        }

        public Node Done()
        {
            return _root ?? this;
        }
    }
}