Wcf Client Factory
================

Introduction
------------
WcfClientFactory is a WCF client proxy factory, that generates at emits at runtime a proxy client. The generated 
proxy uses calls to Channel property of ClientBase<T> derived class to implement the service interface methods.<br/>
*Note : currently, WcfClientFactory does not support client proxy creation of duplex services*

Inspiration
---------------
I was inspired by the awesome [WCF Dynamic Client Proxy ]( http://wcfdynamicclient.codeplex.com/) project:
I've seen lots of WCF client proxy code that was almost identical and I could easily see why project such as "WCF Dynamic Client Proxy"
could be useful. Since those proxies were using custom base class inherited from ClientBase<T>, I couldn't use WCF Dynamic Client Proxy project. Also I wanted to learn about IL generation, so it got me started with this project. <br/>


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

License
--------
Apache 2.0, see LICENSE

Acknowledgments
---------------
I'm using the following libraries in this project:
* [FastRflect](http://fasterflect.codeplex.com/) - a library that makes reflection code more readable and simple
* [FluentIL](http://fluentil.org/) - awesome library for easier IL generation
