# gremlinq-tests
Repro repo for Gremlinq issues

# JanusGraph Server

For the code in this repo to run, it needs to connect to a JanusGraph server running at localhost at the default port.

The easiest way is to use their Docker image. To start the JanusGraph server with defaults use the following commands:

```
docker pull janusgraph/janusgraph
docker run -it -p 8182:8182 janusgraph/janusgraph
```

Make sure you wait long enough for the server to report at the console that it has establish a channel at port 8182.
