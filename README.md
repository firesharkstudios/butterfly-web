# Butterfly.Web ![Butterfly Logo](https://raw.githubusercontent.com/firesharkstudios/Butterfly/master/img/logo-40x40.png) 

> Simple RESTlike and Subscription API server in C#

## Getting Started

## Install from Nuget

| Name | Package | Install |
| --- | --- | --- |
| Butterfly.Web | [![nuget](https://img.shields.io/nuget/v/Butterfly.Web.svg)](https://www.nuget.org/packages/Butterfly.Web/) | `nuget install Butterfly.Web` |
| Butterfly.Web.EmbedIO | [![nuget](https://img.shields.io/nuget/v/Butterfly.Web.EmbedIO.svg)](https://www.nuget.org/packages/Butterfly.Web.EmbedIO/) | `nuget install Butterfly.Web.EmbedIO` |
| Butterfly.Web.RedHttpServer | [![nuget](https://img.shields.io/nuget/v/Butterfly.Web.RedHttpServer.svg)](https://www.nuget.org/packages/Butterfly.Web.RedHttpServer/) | `nuget install Butterfly.Web.RedHttpServer` |

### Install from Source Code

```git clone https://github.com/firesharkstudios/butterfly-web```

## Overview

*Butterfly.Web* defines an *IWebApi* interface to define RESTlike APIs and defines an *ISubscriptionApi* to stream real-time updates to clients.

*Butterfly.Web.EmbedIO* implements the *Butterfly.Web* interfaces using the lightweight [EmbedIO](https://github.com/unosquare/embedio) web server.

*Butterfly.Web.RedHttpServer* implements the *Butterfly.Web* interfaces using the Kestral based [RedHttpServer](https://github.com/RedHttp/Red).
 
## Using IWebApi

An *IWebApi* instance allows defining a RESTlike API like this...

```cs
// Create an IWebApi instance using EmbedIO or RedHttpServer

webApi.OnPost("/api/todo/insert", async (req, res) => {
    var todo = await req.ParseAsJsonAsync<Dict>();
    // Do something to insert the todo
});

webApi.OnPost("/api/todo/delete", async (req, res) => {
    var id = await req.ParseAsJsonAsync<string>();
    // Do something to delete the todo
});

// Don't forget to compile
webApi.Compile();
```

### Request Handling

There are many ways to receive data from a client...

```cs
// Using path params
webApi.OnGet("/api/todo/{id}", (req, res) => {
    // Opening /api/todo/123 would print id=123 below
    Console.WriteLine($"id={req.PathParams["id"]}");
});

// Using query params
webApi.OnGet("/api/todo", (req, res) => {
    // Opening /api/todo?id=123 would print id=123 below
    Console.WriteLine($"id={req.QueryParams["id"]}");
});

// Using post body that is JSON encoded
webApi.OnPost("/api/todo", async(req, res) => {
    // A javascript client posting JSON data with...
    //     $.ajax('/api/todo', {
    //         method: 'POST',
    //         data: JSON.stringify({ id: "123" }),
    //     });
    // would echod id=123 below
    var data = await req.ParseAsJsonAsync<Dictionary<string, string>>();
    Console.WriteLine($"id={data["id"]}");
});

// Using post body that is URL encoded
webApi.OnPost("/api/todo", async(req, res) => {
    // A javascript client posting JSON data with...
    //     var formData = new FormData();
    //     formData.append("id", "123");
    //     $.ajax('/api/todo', {
    //         method: 'POST',
    //         data: formData,
    //     });
    // would echod id=123 below
    var data = await req.ParseAsUrlEncodedAsync();
    Console.WriteLine($"id={data["id"]}");
});
```

### Response Handling

There are many ways to send a response to a client...

```cs
// Respond with plain text
webApi.OnPost("/api/todo", async(req, res) => {
    await res.WriteAsTextAsync("OK");
});

// Respond with JSON
webApi.OnPost("/api/todo", async(req, res) => {
    await res.WriteAsJsonAsync(new {
        result = "OK"
    });
});

// Redirect the client
webApi.OnGet("/api/todo/{id}", (req, res) => {
    res.SendRedirect("/api/todo/not-found");
});
```

## Using ISubscriptionAPI

An *ISubscriptionApi* instance allows clients to receive real-time updates like this...

```cs
// Create an ISubscriptionApi instance using EmbedIO or RedHttpServer

subscriptionApi.OnSubscribe("echoes", async(vars, channel) => {
    int count = 0;
    return Butterfly.Util.RunEvery(() => {
        channel.Queue("Echo", $"Echo #{++count}");
    }, 2000);
});
```

A client subscribing to the *echoes* subscription above would receive messages every two seconds (first message would have type *Echo* and text *Echo #1*).

The subscription handler must return an *IDisposable* instance that is disposed when the client unsubscribes or disconnects.

## Using EmbedIO

[EmbedIO](https://github.com/unosquare/embedio) is a capable low footprint web server that can be used to implement both the *IWebApi* and *ISubscriptionApi* interfaces. 

The *EmbedIOContext* class is a convenience class that creates *IWebApi* and *ISubscriptionApi* instances using an [EmbedIO](https://github.com/unosquare/embedio) web server.

In the *Package Manager Console*...

```
Install-Package Butterfly.Web.EmbedIO
```

In your application...

```csharp
var context = new Butterfly.Web.EmbedIO.EmbedIOContext("http://+:8000/");

// Declare your Web API and Subscription API like...
context.WebApi.OnPost("/api/todo/insert", async (req, res) => {
   // Do something
});
context.WebApi.OnPost("/api/todo/delete", async (req, res) => {
   // Do something
});
context.SubscriptionApi.OnSubscribe("todos", (vars, channel) => {
   // Do something
});

context.Start();
```

## Using RedHttpServer

[RedHttpServer](https://github.com/rosenbjerg/Red) is a Kestrel/ASP.NET Core based web server that can be used to implement both the *IWebApi* and *ISubscriptionApi* interfaces. 

The *RedHttpServerContext* class is a convenience class that creates *IWebApi* and *ISubscriptionApi* instances using [RedHttpServer](https://github.com/rosenbjerg/Red).

In the *Package Manager Console*...

```
Install-Package Butterfly.RedHttpServer
```

In your application...

```csharp
var context = new Butterfly.Web.RedHttpServer.RedHttpServerContext("http://+:8000/");

// Declare your Web API and Subscription API like...
context.WebApi.OnPost("/api/todo/insert", async (req, res) => {
   // Do something
});
context.WebApi.OnPost("/api/todo/delete", async (req, res) => {
   // Do something
});
context.SubscriptionApi.OnSubscribe("todos", (vars, channel) => {
   // Do something
});

context.Start();
```

## Contributing

If you'd like to contribute, please fork the repository and use a feature
branch. Pull requests are warmly welcome.

## Licensing

The code is licensed under the [Mozilla Public License 2.0](http://mozilla.org/MPL/2.0/).