﻿using System.Runtime.CompilerServices;

namespace SimpleChain;

public static class ChainExtensions
{
    public static Chain<T> ToChain<T>(this Task<T> source, CancellationToken cancellationToken = default) => 
        new Chain<T>(source, new ChainState(cancellationToken));

    public static Chain<T> ToChain<T>(this T source, CancellationToken cancellationToken = default) =>
        Task.Run(() => source, cancellationToken).ToChain(cancellationToken);

    public static TaskAwaiter<T> GetAwaiter<T>(this Chain<T> chain) => chain.Task.GetAwaiter();

    public static TaskAwaiter GetAwaiter(this Chain chain) => chain.Task.GetAwaiter();

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain,
        Func<T, CancellationToken, ChainState, TResult> func) =>
        chain.AddNodeInternal(async task =>
            func(await task, chain.CancellationToken, chain.State),
            chain.CancellationToken);

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain,
        Func<T, CancellationToken, TResult> func) =>
        AddNode(chain, (obj, ct, _) => func(obj, ct));

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain,
        Func<T, TResult> func) =>
        AddNode(chain, (obj, _, _) => func(obj));

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain,
        Func<T, CancellationToken, ChainState, Task<TResult>> func) =>
        chain.AddNodeInternal(async task =>
            await func(await task, chain.CancellationToken, chain.State),
            chain.CancellationToken);

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain,
        Func<T, CancellationToken, Task<TResult>> func) =>
        AddNode(chain, (obj, ct, _) => func(obj, ct));

    public static Chain<TResult> AddNode<T, TResult>(this Chain<T> chain,
        Func<T, Task<TResult>> func) =>
        AddNode(chain, (obj, _, _) => func(obj));

    public static Chain AddNode<T>(this Chain<T> chain,
        Action<T, CancellationToken, ChainState> action) =>
        chain.AddNodeInternal(async task =>
            action(await task, chain.CancellationToken, chain.State),
            chain.CancellationToken);

    public static Chain AddNode<T>(this Chain<T> chain,
        Action<T, CancellationToken> action) =>
        AddNode(chain, (obj, ct, _) => action(obj, ct));

    public static Chain AddNode<T>(this Chain<T> chain, Action<T> action) =>
        AddNode(chain, (obj, _, _) => action(obj));
}