
up:
	docker-compose up -d mongo1 mongo2 mongo3
	docker-compose run --rm mongo-ready
	docker-compose run --rm mongo-init
.PHONY: up

test: up
	docker-compose run --rm spray_chronicle.test test
	docker-compose run --rm spray_chronicle.mongo.test test
.PHONY: test

pack:
	docker-compose run --rm spray_chronicle pack -c Release --version-suffix build-$$TRAVIS_BUILD_NUMBER --output /app/package
	docker-compose run --rm spray_chronicle.mongo pack -c Release --version-suffix build-$$TRAVIS_BUILD_NUMBER --output /app/package
	docker-compose run --rm spray_chronicle nuget push /app/package/*.nupkg --api-key $$NUGET_KEY -s https://www.nuget.org/api/v2/package
.PHONY: pack
