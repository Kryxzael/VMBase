using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMBase;

namespace TestProject
{
    public class ParentVM : ViewModelBase<object>
    {
        public ChildVM Child => RegisterChild(() => new ChildVM());

        public ParentVM() : base(null)
        {  }

        public void RunNotify()
        {
            Notify(nameof(Child));
        }
    }
}
