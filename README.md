Wcf Client Factory
================

Introduction
------------
WcfClientFactory is a WCF client proxy factory, that generates at emits at runtime a proxy client. The generated 
proxy uses calls to Channel property of ClientBase<T> derived class to implement the service interface methods.<br/>
Since .Net 3.5 there were [performance improvements in proxy client creation](http://blogs.msdn.com/b/wenlong/archive/2007/10/27/performance-improvement-of-wcf-client-proxy-creation-and-best-practices.aspx)
that made usage of the following pattern for each web-service method feasible:<br/>
1. new ClientBase<T> based proxy <br/>
2. Call service method via ClientBase<T> Channel property <br/>
3. Close connection <br/>

License
--------
Apache 2.0, see LICENSE

Hello World example
-------------------
Assuming service interface
```C#
[ServiceContract]
public interface IFoo
{
  int Add(int x,int y);
}
```

The client is created and used with the following code:
```C#
var proxyClient = WcfClientProxyFactory.CreateInstance<IFoo>("FooServiceName","http://hostname/service");
var result = proxyClient.Add(3,4);
```

The proxy client does not need to be disposed at the end of the usage because internally, during each 
method call, the generated proxy client instantiates the communcation channel with a
class derived from ClientBase<T> and disposes it at the end of method invocation.

Example of custom ClientBase<T> channel class in the generated proxy
--------------------------------------------------------------------
Assuming the following custom client base
```C#
public class FooClientBase : ClientBase<IFoo>
{
  public FooClientBase(string endpointConfigurationName, string remoteAddress)
    : base(endpointConfigurationName, remoteAddress)
  {            
  }
  
  //custom stuff
}
```

The client is created with the following code
```C#
var proxyClient = WcfClientProxyFactory.CreateInstance<IFoo,FooClientBase>("FooServiceName","http://hostname/service");
```
