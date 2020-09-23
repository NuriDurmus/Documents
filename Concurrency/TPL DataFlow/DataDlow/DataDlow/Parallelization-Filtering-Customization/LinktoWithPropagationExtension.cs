using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace DataDlow.Parallelization_Filtering_Customization
{
    public static class LinktoWithPropagationExtension
    {
        public static IDisposable LinkToWithPropagation<T>(this ISourceBlock<T> source,ITargetBlock<T> target)
        {
            return source.LinkTo(target, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });
        }
    }
}
