namespace DevToys.UI.Framework.Threading;

public delegate Task AsyncTypedEventHandler<TSender, TResult>(TSender sender, TResult args);
