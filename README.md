# Butterfly.Web ![Butterfly Logo](https://raw.githubusercontent.com/firesharkstudios/Butterfly/master/img/logo-40x40.png) 

> Collection of utility methods used in the Butterfly Server

## Getting Started

## Install from Nuget

| Name | Package | Install |
| --- | --- | --- |
| Butterfly.Web | [![nuget](https://img.shields.io/nuget/v/Butterfly.Web.svg)](https://www.nuget.org/packages/Butterfly.Web/) | `nuget install Butterfly.Web` |
| Butterfly.Web.EmbedIO | [![nuget](https://img.shields.io/nuget/v/Butterfly.Web.EmbedIO.svg)](https://www.nuget.org/packages/Butterfly.Web.EmbedIO/) | `nuget install Butterfly.Web.EmbedIO` |
| Butterfly.Web.RedHttpServer | [![nuget](https://img.shields.io/nuget/v/Butterfly.Web.RedHttpServer.svg)](https://www.nuget.org/packages/Butterfly.Web.RedHttpServer/) | `nuget install Butterfly.Web.RedHttpServer` |

### Install from Source Code

git clone https://github.com/firesharkstudios/butterfly-twilio

## Creating a Web Api

### Overview

An [IWebApi](https://butterflyserver.io/docfx/api/Butterfly.Core.WebApi.IWebApi.html) instance allows defining an API for your application like this...

```cs
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

You need an implementation like [EmbedIO](#using-embedio) or [RedHttpServer](#using-redhttpserver) to get an instance of [IWebApi](https://butterflyserver.io/docfx/api/Butterfly.Core.WebApi.IWebApi.html).

### Example Request Handling

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
    Console.WriteLine($"id={data["id"]});
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
    Console.WriteLine($"id={data["id"]});
});
```

See [IHttpRequest](https://butterflyserver.io/docfx/api/Butterfly.Core.WebApi.IHttpRequest.html) for more details.

### Example Response Handling

There are many ways to send a response to a client...

```cs
// Respond with plain text
webApi.OnPost("/api/todo", async(req, res) => {
    await res.WriteAsTextAsync("OK");
});

// Respond with JSON object
webApi.OnPost("/api/todo", async(req, res) => {
    await res.WriteAsJsonAsync(new {
        result = "OK"
    });
});

// Respond with JSON array
webApi.OnPost("/api/todo", async(req, res) => {
    await res.WriteAsJsonAsync(new string[] {
        "OK"
    });
});

// Redirect the client
webApi.OnGet("/api/todo/{id}", (req, res) => {
    res.SendRedirect("/api/todo/not-found");
});
```

See [IHttpResponse](https://butterflyserver.io/docfx/api/Butterfly.Core.WebApi.IHttpResponse.html) for more details.

## Creating a Subscription API

### Overview

An [ISubscriptionApi](https://butterflyserver.io/docfx/api/Butterfly.Core.Channel.ISubscriptionApi.html) instance allows defining a Subscription API that can push real-time data to clients like this...

```cs
subscriptionApi.OnSubscribe("todos", (vars, channel) => {
    return database.CreateAndStartDynamicViewAsync("todo", dataEventTransaction => {
	    channel.Queue(dataEventTransaction);
    });
});
```

Notes
- The *vars* variable allows the client to pass values to the subscription
- The *channel* variable allows accessing the client authentication information
- The subscription handler must return an object implementing *IDisposable* (object will be disposed when the client unsuscribes) 

A common usecase is to return a *DynamicViewSet* instance that pushes initial data and data changes to the client over the channel.

You need an implementation like [EmbedIO](#using-embedio) to get an instance of [ISubscriptionApi](https://butterflyserver.io/docfx/api/Butterfly.Core.Channel.ISubscriptionApi.html).

### Example Simple Subscription

The following javascript client subscribes to an *echo-messages* subscription passing in a *someName* variable and echoing the *messageType* and *message* received from the server to the console...

```js
// Javascript client
channelClient.subscribe({
    channel: 'echo-messages',
    vars: {
        someName = 'Spongebob',
    },
    handler(messageType, message) {
        console.debug(`messageType=${messageType},message=${message}`);
    })
});
```

The above code assumes you have [Butterfly Client](#butterfly-client) installed and have initialized a *WebSocketChannelClient* instance.

The following server code defines the *echo-messages* subscription that uses an instance of the *RunEvery* class to send a message to any subscribed clients every 2 seconds...

```cs
// C# server
subscriptionApi.OnSubscribe("echo-messages", (vars, channel) => {
    int count = 0;
    var someName = vars.GetAs("someName", "");
    return Butterfly.Util.RunEvery(() => {
        channel.Queue("Echo", $"Message #{++count} from {someName}");
    }, 2000);
);
```

Notice that the subscription handler above returns the instance of *RunEvery* which implements *IDisposable*.  The *RunEvery* instance will be disposed when the client unsubscribes (or disconnects for too long).

So, the end result of running the code above would be the following in the client javascript console...

```js
messageType=Echo,message=Message #1 from Spongebob
messageType=Echo,message=Message #2 from Spongebob
messageType=Echo,message=Message #3 from Spongebob
...
```


### Example Dynamic Subscription

The following javascript client subscribes to a *todo-page* subscription and maps the two datasets to the local *todosList* and *tagsList* arrays...

```js
let todosList = [];
let tagsList = [];
channelClient.subscribe({
    channel: 'todo-page',
    vars: {
        userId: '123'
    },
    handler: new ArrayDataEventHandler({
        arrayMapping: {
            todo: todosList
            tag: tagsList
        }
    })
});
```

The above code assumes you have [Butterfly Client](#butterfly-client) installed and have initialized a *WebSocketChannelClient* instance.

The following server code defines the *todo-page* subscription that returns a *DynamicViewSet* containing two *DynamicViews* (one for *todos* and one for *tags*)...

```cs
subscriptionApi.OnSubscribe("todo-page", async(vars, channel) => {
    var dynamicViewSet = database.CreateDynamicViewSet(dataEventTransaction => channel.Queue(dataEventTransaction);

    string userId = vars.GetAs("userId", "");
    if (!string.IsNullOrEmpty(userId)) throw new Exception("Must specify a userId in vars");

    // DynamicViews can include JOINs and will update if 
    // any of the joined tables change the resultset
    // (note this requires using a database like MySQL that supports JOINs)
    dynamicViewSet.CreateDynamicView(
        @"SELECT td.id, td.name, td.user_id, u.name user_name
        FROM todo td
            INNER JOIN user u ON td.user_id=u.id
        WHERE u.id=@userId",
        new {
            userId
        }
    );

    // A channel can return multiple resultsets as well
    dynamicViewSet.CreateDynamicView(
        @"SELECT id, name
        FROM tag
        WHERE user_id=@userId",
        new {
            userId
        }
    );

    // Send initial datasets and send any data changes as they occur    
    await dynamicViewSet.StartAsync();

    return dynamicViewSet;
);
```

So, the end result of running the code above would be a local *todosList* and *tagsList* arrays that automatically stay synchronized with the server.

## API Documentation

Available [here](http://htmlpreview.github.io/?https://github.com/firesharkstudios/butterfly-twilio/blob/master/docs/api/Butterfly.Web.html).

## Contributing

If you'd like to contribute, please fork the repository and use a feature
branch. Pull requests are warmly welcome.

## Licensing

The code is licensed under the [Mozilla Public License 2.0](http://mozilla.org/MPL/2.0/).