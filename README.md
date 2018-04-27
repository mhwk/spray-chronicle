SprayChronicle
==============

Event sourcing framework with immutable state pattern support.

[![Build Status](https://travis-ci.org/mhwk/spray-chronicle.svg?branch=master)](https://travis-ci.org/mhwk/spray-chronicle)

Examples
--------

Have a look at the [example project](src/SprayChronicle.Example) and [example tests](test/SprayChronicle.Example.Test) for how the framework is used.

Development
===========

For most parts of the framework just the [dotnet cli](https://docs.microsoft.com/en-us/dotnet/core/tools) is enough to get started. Parts that communicate with external services need those services running in order to be able to test them. You can either install those services on your machine or use the [docker-compose](https://docs.docker.com/compose) configuration [file](docker-compose.yml). With the following commands you can turn these services on:

  docker-compose up -d eventstore
  docker-compose up -d ravendb

Changelog
=========

Version 0.2.0
-------------

 * Embraced async
 * Implemented TPL pipelines
 * Support for [RavenDB](https://www.ravendb.net)

Version 0.1.0
-------------

 * Implementation of immutable state handling
 * Initial http support
 * Support for [EventStore](https://www.geteventstore.com)
 * Support for [MongoDB](https://www.mongodb.com)
