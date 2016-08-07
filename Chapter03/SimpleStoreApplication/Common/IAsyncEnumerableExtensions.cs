using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.ServiceFabric.Data;
using System.Collections;

namespace Common
{
    /// <summary>
    /// I copied this class by https://github.com/Azure-Samples/service-fabric-dotnet-management-party-cluster/blob/sdkupdate/PartyCluster/Common/IAsyncEnumerableExtensions.cs
    /// for learning purpose. The author is the author of the github account.
    /// I added this class for converting IReliableDictionary into Enumerable object. 
    /// The book's sample didn't work. select can't use for IReliableDictinoary
    /// Refer this. http://stackoverflow.com/questions/36877614/what-would-be-the-best-way-to-search-an-ireliabledictionary
    /// </summary>
    public static class IAsyncEnumerableExtensions
    {
        public static IEnumerable<TSource> ToEnumerable<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentException("source");
            }

            return new AsyncEnumerableWrapper<TSource>(source);
        }

        public static async Task ForeachAsync<T>(this IAsyncEnumerable<T> instance, CancellationToken cancellationToken, Action<T> doSomething)
        {
            using (IAsyncEnumerator<T> e = instance.GetAsyncEnumerator())
            {
                while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    doSomething(e.Current);
                }
            }
        }

        public static async Task<int> CountAsync<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            int count = 0;
            using (var asyncEnumerator = source.GetAsyncEnumerator())
            {
                while (await asyncEnumerator.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
                {
                    if (predicate(asyncEnumerator.Current))
                    {
                        count++;
                    }    
                }
            }
            return count;
        }

        internal struct AsyncEnumerableWrapper<TSource> : IEnumerable<TSource>
        {
  
            private IAsyncEnumerable<TSource> source;



            public AsyncEnumerableWrapper(IAsyncEnumerable<TSource> source)
            {
                this.source = source;
            }

            public IEnumerator<TSource> GetEnumerator()
            {
                return new AsyncEnumeratorWrapper<TSource>(this.source.GetAsyncEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        internal struct AsyncEnumeratorWrapper<TSource> : IEnumerator<TSource>
        {
            private IAsyncEnumerator<TSource> source;
            private TSource current;

            public AsyncEnumeratorWrapper(IAsyncEnumerator<TSource> source) 
            {
                this.source = source;
                this.current = default(TSource);
            }
            public TSource Current
            {
                get { return this.current; }
            }

            object IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                if (!this.source.MoveNextAsync(CancellationToken.None).GetAwaiter().GetResult())
                {
                    return false;
                }
                this.current = this.source.Current;
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }


}
