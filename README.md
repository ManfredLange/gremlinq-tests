# gremlinq-tests

Minimal example that demonstrates how to configure (ExRam.Gremlinq)[https://github.com/ExRam/ExRam.Gremlinq] for properties of type `Guid`.

# JanusGraph Server

For the code in this repo to run, it needs to connect to a JanusGraph server running at localhost at the default port.

The easiest way is to use their Docker image. A working Docker installation is then a prerequisite. To start the JanusGraph server with defaults use the following commands:

```
docker pull janusgraph/janusgraph
docker run -it -p 8182:8182 janusgraph/janusgraph
```

Make sure you wait long enough for the server to report at the console that it has established a channel at port 8182.
