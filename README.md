Wcf Client Factory
================

Introduction
------------
WcfClientFactory is a WCF client proxy factory, that uses a class derived from ClientBase<T> to call the WCF service.


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
