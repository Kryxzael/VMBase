using System;
using System.Diagnostics;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            ParentVM parent = new ParentVM();
            ChildVM  child1 = parent.Child;
            ChildVM  child2 = parent.Child;

            parent.RunNotify();

            Debug.Assert(child1 == child2, "The two variables don't point to the same value");
            Debug.Assert(child1.IsDisposed, "The old child VM should be disposed at this point");
            Debug.Assert(parent.Child != child1, "The new child should not be equal to the old");
            Debug.Assert(!parent.Child.IsDisposed, "The new child should not be disposed yet");
        }
    }
}
